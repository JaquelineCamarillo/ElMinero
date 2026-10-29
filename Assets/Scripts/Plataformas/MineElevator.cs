using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;

namespace ElMinero.Plataformas
{
    /// <summary>
    /// Elevador de tiro de mina con malacate y cable continuo (Nivel 3).
    /// El malacate está anclado a la repisa superior.
    /// Al subirse el minero, tras 0.5 s sube suavemente hasta quedar a ras de la repisa superior.
    /// Al salir el minero y pasar 3.0 s, el elevador desciende de nuevo.
    /// Transporta al jugador mediante traslación física directa sin emparentar (evita SetParent).
    /// </summary>
    public class MineElevator : MonoBehaviour, IResettable
    {
        public enum EstadoElevador
        {
            Abajo,
            EsperandoParaSubir,
            Subiendo,
            Arriba,
            EsperandoParaBajar,
            Bajando
        }

        [Header("Puntos de Recorrido")]
        [SerializeField] private Vector3 bottomPoint;
        [SerializeField] private Vector3 topPoint;
        [SerializeField] private float speed = 2.4f;

        [Header("Tiempos")]
        [SerializeField] private float delaySubida = 0.5f;
        [SerializeField] private float delayBajada = 3.0f;

        [Header("Malacate Superior y Cable")]
        [SerializeField] private Transform malacateWheel;
        [SerializeField] private Transform cableAnchorPoint;
        [SerializeField] private LineRenderer cableLine;
        [SerializeField] private float wheelRotationSpeed = 180f;

        [Header("Audio")]
        [SerializeField] private AudioSource loopAudioSource;

        private Rigidbody2D rb;
        private EstadoElevador estado = EstadoElevador.Abajo;
        private bool jugadorEnPlataforma = false;
        private Rigidbody2D playerRb;
        private Coroutine corrutinaTemporizador;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.useFullKinematicContacts = true;
            }

            if (bottomPoint == Vector3.zero) bottomPoint = transform.position;
            if (topPoint == Vector3.zero) topPoint = transform.position + new Vector3(0f, 10f, 0f);

            ConfigurarCable();
            ConfigurarAudio();
        }

        private void ConfigurarCable()
        {
            if (cableLine == null)
            {
                cableLine = GetComponent<LineRenderer>();
                if (cableLine == null) cableLine = gameObject.AddComponent<LineRenderer>();
            }

            if (cableLine != null)
            {
                cableLine.positionCount = 2;
                cableLine.startWidth = 0.08f;
                cableLine.endWidth = 0.08f;
                cableLine.useWorldSpace = true;
                cableLine.sortingLayerName = Constantes.SortingLayers.Escenario;
                cableLine.sortingOrder = 1;

                Material lineMat = new Material(Shader.Find("Sprites/Default"));
                lineMat.color = new Color(0.25f, 0.22f, 0.20f, 1f);
                cableLine.material = lineMat;
            }

            if (cableAnchorPoint == null)
            {
                GameObject anchor = new GameObject("CableAnchor");
                anchor.transform.SetParent(transform, false);
                anchor.transform.localPosition = new Vector3(0f, 0.8f, 0f);
                cableAnchorPoint = anchor.transform;
            }
        }

        private void ConfigurarAudio()
        {
            if (loopAudioSource == null)
            {
                loopAudioSource = GetComponent<AudioSource>();
                if (loopAudioSource == null) loopAudioSource = gameObject.AddComponent<AudioSource>();
            }

            if (loopAudioSource != null)
            {
                loopAudioSource.loop = true;
                loopAudioSource.playOnAwake = false;
                loopAudioSource.spatialBlend = 1.0f;
                loopAudioSource.minDistance = 3f;
                loopAudioSource.maxDistance = 18f;
                loopAudioSource.rolloffMode = AudioRolloffMode.Linear;

                if (AudioManager.Instance != null)
                {
                    loopAudioSource.clip = AudioManager.Instance.GetAudioClipMalacateLoop();
                    AudioManager.Instance.RegistrarFuenteEfectos(loopAudioSource);
                }
            }
        }

        private void OnDisable()
        {
            if (AudioManager.Instance != null && loopAudioSource != null)
            {
                AudioManager.Instance.DesregistrarFuenteEfectos(loopAudioSource);
            }
        }

        public void ConfigurarPuntos(Vector3 abajo, Vector3 arriba, Transform rueda, Transform castillete = null)
        {
            bottomPoint = abajo;
            topPoint = arriba;
            transform.position = abajo;
            if (rueda != null) malacateWheel = rueda;
        }

        private void Update()
        {
            // Actualizar línea de cable continuo
            if (cableLine != null && malacateWheel != null && cableAnchorPoint != null)
            {
                cableLine.SetPosition(0, malacateWheel.position);
                cableLine.SetPosition(1, cableAnchorPoint.position);
            }

            // Girar rueda del malacate durante el movimiento
            if ((estado == EstadoElevador.Subiendo || estado == EstadoElevador.Bajando) && malacateWheel != null)
            {
                float dir = (estado == EstadoElevador.Subiendo) ? -1f : 1f;
                malacateWheel.Rotate(0f, 0f, dir * wheelRotationSpeed * Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (estado == EstadoElevador.Subiendo)
            {
                MoverHacia(topPoint, () =>
                {
                    estado = EstadoElevador.Arriba;
                    DetenerAudio();
                });
            }
            else if (estado == EstadoElevador.Bajando)
            {
                MoverHacia(bottomPoint, () =>
                {
                    estado = EstadoElevador.Abajo;
                    DetenerAudio();
                });
            }
        }

        private void MoverHacia(Vector3 target, System.Action alLlegar)
        {
            Vector2 posActual = rb.position;
            Vector2 posDestino = Vector2.MoveTowards(posActual, target, speed * Time.fixedDeltaTime);
            Vector2 delta = posDestino - posActual;

            rb.MovePosition(posDestino);

            // Arrastrar físicamente al jugador sin emparentar
            if (jugadorEnPlataforma && playerRb != null)
            {
                playerRb.position += new Vector2(0f, delta.y);
            }

            if (Vector2.Distance(rb.position, target) < 0.02f)
            {
                rb.MovePosition(target);
                alLlegar?.Invoke();
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y < -0.4f)
            {
                jugadorEnPlataforma = true;
                playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

                if (estado == EstadoElevador.Abajo)
                {
                    if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
                    corrutinaTemporizador = StartCoroutine(RutinaEsperarYSubir());
                }
                else if (estado == EstadoElevador.EsperandoParaBajar)
                {
                    // Si el jugador volvió antes de que baje, cancelar descenso
                    if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
                    estado = EstadoElevador.Arriba;
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                jugadorEnPlataforma = false;
                playerRb = null;

                if (estado == EstadoElevador.Arriba)
                {
                    if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
                    corrutinaTemporizador = StartCoroutine(RutinaEsperarYDescender());
                }
            }
        }

        private IEnumerator RutinaEsperarYSubir()
        {
            estado = EstadoElevador.EsperandoParaSubir;
            yield return new WaitForSeconds(delaySubida);

            if (jugadorEnPlataforma)
            {
                IniciarAudio();
                estado = EstadoElevador.Subiendo;
            }
            else
            {
                estado = EstadoElevador.Abajo;
            }
        }

        private IEnumerator RutinaEsperarYDescender()
        {
            estado = EstadoElevador.EsperandoParaBajar;
            yield return new WaitForSeconds(delayBajada);

            if (!jugadorEnPlataforma)
            {
                IniciarAudio();
                estado = EstadoElevador.Bajando;
            }
            else
            {
                estado = EstadoElevador.Arriba;
            }
        }

        private void IniciarAudio()
        {
            if (loopAudioSource != null && !loopAudioSource.isPlaying)
            {
                loopAudioSource.Play();
            }
        }

        private void DetenerAudio()
        {
            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                loopAudioSource.Pause();
            }
        }

        public void ResetObject()
        {
            if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
            DetenerAudio();

            transform.position = bottomPoint;
            if (rb != null) rb.position = bottomPoint;
            estado = EstadoElevador.Abajo;
            jugadorEnPlataforma = false;
            playerRb = null;
        }
    }
}
