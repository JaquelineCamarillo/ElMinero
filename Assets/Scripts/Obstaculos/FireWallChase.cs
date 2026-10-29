using System.Collections;
using UnityEngine;
using TMPro;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Muro de fuego perseguidor para los últimos 35 tiles del Nivel 3 (La Valenciana).
    /// Al activarse por trigger, muestra en pantalla "¡LA MINA SE INCENDIA!" (56 pt, 2 s).
    /// Avanza de izquierda a derecha a 2.4 u/s persiguiendo al jugador con sonido de fuego rugiente.
    /// Si alcanza a Refugio, le quita 1 vida y lo devuelve al último checkpoint.
    /// Se detiene al completar el nivel y se reinicia mediante IResettable.
    /// </summary>
    public class FireWallChase : MonoBehaviour, IResettable
    {
        [Header("Cinemática de Persecución")]
        [Tooltip("Velocidad de avance implacable del muro de fuego (2.4 u/s).")]
        [SerializeField] private float speed = 2.4f;

        [Tooltip("Punto inicial en X.")]
        [SerializeField] private float startX = 172f;

        [Tooltip("Punto final en X donde se detiene la persecución.")]
        [SerializeField] private float endX = 212f;

        [Header("Aviso de Incendio")]
        [Tooltip("Texto flotante o en HUD para advertir del incendio.")]
        [SerializeField] private TextMeshProUGUI textoAvisoIncendio;

        [Tooltip("Duración del aviso en pantalla (2 s).")]
        [SerializeField] private float avisoDuration = 2.0f;

        [Header("Audio")]
        [SerializeField] private AudioSource loopAudioSource;

        private bool isChasing = false;
        private bool isTriggered = false;
        private Vector3 initialPosition;

        private void Awake()
        {
            initialPosition = new Vector3(startX, transform.position.y, transform.position.z);
            transform.position = initialPosition;

            ConfigurarAudio();

            if (textoAvisoIncendio != null)
            {
                textoAvisoIncendio.gameObject.SetActive(false);
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
                loopAudioSource.minDistance = 4f;
                loopAudioSource.maxDistance = 22f;
                loopAudioSource.rolloffMode = AudioRolloffMode.Linear;

                if (AudioManager.Instance != null)
                {
                    loopAudioSource.clip = AudioManager.Instance.GetAudioClipFuegoLoop();
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

        public void IniciarPersecucion()
        {
            if (isTriggered) return;
            isTriggered = true;
            isChasing = true;

            if (loopAudioSource != null && !loopAudioSource.isPlaying)
            {
                loopAudioSource.Play();
            }

            StartCoroutine(RutinaAvisoIncendio());
        }

        private IEnumerator RutinaAvisoIncendio()
        {
            if (textoAvisoIncendio != null)
            {
                textoAvisoIncendio.text = "¡LA MINA SE INCENDIA!";
                textoAvisoIncendio.fontSize = 56f;
                textoAvisoIncendio.color = new Color(1.0f, 0.35f, 0.1f, 1f);
                textoAvisoIncendio.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(avisoDuration);

            if (textoAvisoIncendio != null)
            {
                textoAvisoIncendio.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (!isChasing) return;

            // Avanzar hacia la derecha a 2.4 u/s
            transform.Translate(Vector3.right * (speed * Time.deltaTime));

            if (transform.position.x >= endX)
            {
                DetenerFuego();
            }
        }

        public void DetenerFuego()
        {
            isChasing = false;
            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                loopAudioSource.Stop();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constantes.Tags.Player))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(1);
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDanio();
                    }
                }

                // Regresar a posición de inicio para darle oportunidad tras reaparecer en checkpoint
                transform.position = initialPosition;
            }
        }

        public void ResetObject()
        {
            StopAllCoroutines();
            isChasing = false;
            isTriggered = false;
            transform.position = initialPosition;

            if (loopAudioSource != null && loopAudioSource.isPlaying)
            {
                loopAudioSource.Stop();
            }

            if (textoAvisoIncendio != null)
            {
                textoAvisoIncendio.gameObject.SetActive(false);
            }
        }
    }
}
