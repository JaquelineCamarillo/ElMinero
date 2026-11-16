using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Obstaculos;

namespace ElMinero.Plataformas
{
    /// <summary>
    /// Elevador (jaula) del tiro del nivel 3.
    ///
    /// - Abajo: el jugador se para en la jaula → 0.5 s → sube a 3 u/s hasta quedar al ras del piso de arriba.
    /// - Arriba: si el jugador se baja, a los 3 s la jaula baja vacía.
    /// - Si el jugador muere o reaparece, la jaula vuelve abajo al instante.
    /// - Lleva al jugador con rb.MovePosition y le suma el delta vertical (sin emparentar).
    /// - El piso es un collider delgado con PlatformEffector2D (one-way); las barras no tienen collider.
    /// - Mientras sube con el jugador, el generador de rocas del tiro suelta rocas (sombra sobre el piso de la jaula).
    /// - La rueda del castillete se anima solo mientras la jaula se mueve, con Malacate_Loop.
    /// - El cable (sprite Tiled) va del centro de la rueda al techo de la jaula y cambia de largo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
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

        [Header("Recorrido (posición del piso de la jaula)")]
        [SerializeField] private Vector3 bottomPoint;
        [SerializeField] private Vector3 topPoint;
        [SerializeField] private float speed = 3f;

        [Header("Tiempos")]
        [SerializeField] private float delaySubida = 0.5f;
        [SerializeField] private float delayBajada = 3.0f;

        [Header("Detección del jugador (relativa al piso de la jaula)")]
        [SerializeField] private float anchoJaula = 3f;

        [Header("Castillete y cable")]
        [SerializeField] private SpriteFrameAnimator ruedaCastillete;
        [SerializeField] private Transform centroRueda;
        [SerializeField] private SpriteRenderer cable;
        [SerializeField] private float altoJaula = 1.25f;

        [Header("Peligro durante la subida")]
        [SerializeField] private RockSpawner rocasDelTiro;

        [Header("Audio")]
        [SerializeField] private AudioSource loopAudioSource;

        // Compatibilidad con el prefab antiguo (Elevador_Malacate)
        [SerializeField] private Transform malacateWheel;

        private Rigidbody2D rb;
        private EstadoElevador estado = EstadoElevador.Abajo;
        private Transform jugador;
        private Rigidbody2D playerRb;
        private Coroutine corrutinaTemporizador;
        private bool jugadorEnJaula;

        public EstadoElevador Estado => estado;

        public void Configurar(Vector3 abajo, Vector3 arriba, float velocidad, SpriteFrameAnimator rueda, Transform centro, SpriteRenderer spriteCable, RockSpawner rocas)
        {
            bottomPoint = abajo;
            topPoint = arriba;
            speed = velocidad;
            ruedaCastillete = rueda;
            centroRueda = centro;
            cable = spriteCable;
            rocasDelTiro = rocas;
            transform.position = abajo;
        }

        /// <summary>Firma antigua usada por el prefab Elevador_Malacate (niveles anteriores).</summary>
        public void ConfigurarPuntos(Vector3 abajo, Vector3 arriba, Transform rueda, Transform castillete = null)
        {
            bottomPoint = abajo;
            topPoint = arriba;
            transform.position = abajo;
            if (rueda != null) malacateWheel = rueda;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (bottomPoint == Vector3.zero) bottomPoint = transform.position;
            if (topPoint == Vector3.zero) topPoint = transform.position + new Vector3(0f, 10f, 0f);

            if (loopAudioSource == null) loopAudioSource = GetComponent<AudioSource>();
            if (loopAudioSource == null) loopAudioSource = gameObject.AddComponent<AudioSource>();
            loopAudioSource.loop = true;
            loopAudioSource.playOnAwake = false;
            loopAudioSource.spatialBlend = 0.5f;
            loopAudioSource.minDistance = 4f;
            loopAudioSource.maxDistance = 22f;
            loopAudioSource.rolloffMode = AudioRolloffMode.Linear;

            transform.position = bottomPoint;
            rb.position = bottomPoint;
        }

        private void Start()
        {
            if (AudioManager.Instance != null)
            {
                if (loopAudioSource.clip == null) loopAudioSource.clip = AudioManager.Instance.GetAudioClipMalacateLoop();
                AudioManager.Instance.RegistrarFuenteEfectos(loopAudioSource);
            }
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnJugadorReaparecio += ResetObject;
            }

            GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
            if (p != null)
            {
                jugador = p.transform;
                playerRb = p.GetComponent<Rigidbody2D>();
            }

            DetenerMovimientoVisual();
            ActualizarCable();
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnJugadorReaparecio -= ResetObject;
            }
            if (AudioManager.Instance != null && loopAudioSource != null)
            {
                AudioManager.Instance.DesregistrarFuenteEfectos(loopAudioSource);
            }
        }

        /// <summary>El jugador está de pie (o casi) sobre el piso de la jaula.</summary>
        private bool DetectarJugador()
        {
            if (jugador == null) return false;
            Vector2 piso = rb.position;
            Vector2 pj = jugador.position;
            bool dentroX = Mathf.Abs(pj.x - piso.x) <= anchoJaula * 0.5f;
            bool alturaPiso = pj.y >= piso.y - 0.25f && pj.y <= piso.y + 0.6f;
            bool noSube = playerRb == null || playerRb.linearVelocity.y <= speed + 0.5f;
            return dentroX && alturaPiso && noSube;
        }

        private void FixedUpdate()
        {
            bool antes = jugadorEnJaula;
            jugadorEnJaula = DetectarJugador();

            if (jugadorEnJaula && !antes) AlSubirJugador();
            else if (!jugadorEnJaula && antes) AlBajarJugador();

            if (estado == EstadoElevador.Subiendo)
            {
                MoverHacia(topPoint, () =>
                {
                    estado = EstadoElevador.Arriba;
                    DetenerMovimientoVisual();
                    if (!jugadorEnJaula) ProgramarBajada();
                });
            }
            else if (estado == EstadoElevador.Bajando)
            {
                MoverHacia(bottomPoint, () =>
                {
                    estado = EstadoElevador.Abajo;
                    DetenerMovimientoVisual();
                    if (jugadorEnJaula) AlSubirJugador();
                });
            }
        }

        private void LateUpdate()
        {
            ActualizarCable();
        }

        private void MoverHacia(Vector3 target, System.Action alLlegar)
        {
            Vector2 posActual = rb.position;
            Vector2 posDestino = Vector2.MoveTowards(posActual, target, speed * Time.fixedDeltaTime);
            Vector2 delta = posDestino - posActual;

            rb.MovePosition(posDestino);

            // Arrastrar físicamente al jugador sin emparentar
            if (jugadorEnJaula && playerRb != null)
            {
                playerRb.position += new Vector2(0f, delta.y);
                if (delta.y > 0f && playerRb.linearVelocity.y < 0f)
                {
                    playerRb.linearVelocity = new Vector2(playerRb.linearVelocity.x, 0f);
                }
            }

            if (Vector2.Distance(posDestino, target) < 0.01f)
            {
                rb.MovePosition(target);
                alLlegar?.Invoke();
            }
        }

        private void AlSubirJugador()
        {
            if (estado == EstadoElevador.Abajo)
            {
                if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
                corrutinaTemporizador = StartCoroutine(RutinaEsperarYSubir());
            }
            else if (estado == EstadoElevador.EsperandoParaBajar)
            {
                if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
                estado = EstadoElevador.Arriba;
            }
            else if (estado == EstadoElevador.Subiendo && rocasDelTiro != null)
            {
                rocasDelTiro.SetActivo(true);
            }
        }

        private void AlBajarJugador()
        {
            if (rocasDelTiro != null) rocasDelTiro.SetActivo(false);
            if (estado == EstadoElevador.Arriba) ProgramarBajada();
        }

        private void ProgramarBajada()
        {
            if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
            corrutinaTemporizador = StartCoroutine(RutinaEsperarYDescender());
        }

        private IEnumerator RutinaEsperarYSubir()
        {
            estado = EstadoElevador.EsperandoParaSubir;
            yield return new WaitForSeconds(delaySubida);

            if (jugadorEnJaula)
            {
                estado = EstadoElevador.Subiendo;
                IniciarMovimientoVisual();
                if (rocasDelTiro != null) rocasDelTiro.SetActivo(true);
            }
            else
            {
                estado = EstadoElevador.Abajo;
            }
            corrutinaTemporizador = null;
        }

        private IEnumerator RutinaEsperarYDescender()
        {
            estado = EstadoElevador.EsperandoParaBajar;
            yield return new WaitForSeconds(delayBajada);

            if (!jugadorEnJaula)
            {
                estado = EstadoElevador.Bajando;
                IniciarMovimientoVisual();
            }
            else
            {
                estado = EstadoElevador.Arriba;
            }
            corrutinaTemporizador = null;
        }

        private void IniciarMovimientoVisual()
        {
            if (ruedaCastillete != null) ruedaCastillete.Play();
            if (loopAudioSource != null && loopAudioSource.clip != null && !loopAudioSource.isPlaying) loopAudioSource.Play();
        }

        private void DetenerMovimientoVisual()
        {
            if (ruedaCastillete != null) ruedaCastillete.Stop(0);
            if (loopAudioSource != null && loopAudioSource.isPlaying) loopAudioSource.Stop();
            if (rocasDelTiro != null) rocasDelTiro.SetActivo(false);
        }

        private void ActualizarCable()
        {
            if (cable == null || centroRueda == null) return;
            float yTecho = transform.position.y + altoJaula;
            float yRueda = centroRueda.position.y;
            float largo = Mathf.Max(0.1f, yRueda - yTecho);
            cable.transform.position = new Vector3(transform.position.x, yTecho + largo * 0.5f, 0f);
            cable.size = new Vector2(cable.size.x, largo);
        }

        public void ResetObject()
        {
            if (corrutinaTemporizador != null) StopCoroutine(corrutinaTemporizador);
            corrutinaTemporizador = null;
            DetenerMovimientoVisual();

            transform.position = bottomPoint;
            if (rb != null) rb.position = bottomPoint;
            estado = EstadoElevador.Abajo;
            jugadorEnJaula = false;
            ActualizarCable();
        }
    }
}
