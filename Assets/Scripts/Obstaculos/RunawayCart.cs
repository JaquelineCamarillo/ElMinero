using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;
using ElMinero.Managers;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Vagoneta desbocada (Niveles 2 y 3).
    /// Se activa cuando el jugador entra en su trigger de aviso.
    /// Muestra un cartel de advertencia "¡!" 1 s antes y reproduce sonido ambiental.
    /// Cruza los rieles a 9 u/s infligiendo daño al jugador en caso de contacto.
    /// Al llegar al final se desactiva y se reinicia mediante IResettable.
    /// </summary>
    public class RunawayCart : MonoBehaviour, IResettable
    {
        [Header("Cinemática")]
        [Tooltip("Punto inicial sobre los rieles.")]
        [SerializeField] private Vector3 startPoint;

        [Tooltip("Punto final del recorrido.")]
        [SerializeField] private Vector3 endPoint;

        [Tooltip("Velocidad de avance a lo largo del riel (según GDD: 9 u/s).")]
        [SerializeField] private float speed = 9f;

        [Tooltip("Tiempo de aviso previo antes de lanzarse (1 s).")]
        [SerializeField] private float warningDuration = 1.0f;

        [Header("Referencias Visuales y Sonido")]
        [Tooltip("Objeto visual que muestra el aviso '¡!' sobre el riel.")]
        [SerializeField] private GameObject warningObject;

        [Tooltip("AudioSource 3D con el loop de la vagoneta en movimiento.")]
        [SerializeField] private AudioSource loopAudioSource;

        [Tooltip("Daño infligido al jugador.")]
        [SerializeField] private int damage = 1;

        private bool isRunning = false;
        private bool isTriggered = false;
        private Coroutine cartRoutine;

        private void Awake()
        {
            if (startPoint == Vector3.zero) startPoint = transform.position;
            if (endPoint == Vector3.zero) endPoint = transform.position + new Vector3(25f, 0f, 0f);

            ConfigurarAudio3D();
            if (warningObject != null) warningObject.SetActive(false);
        }

        private void ConfigurarAudio3D()
        {
            if (loopAudioSource == null)
            {
                loopAudioSource = gameObject.GetComponent<AudioSource>();
                if (loopAudioSource == null)
                {
                    loopAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (loopAudioSource != null)
            {
                loopAudioSource.loop = true;
                loopAudioSource.playOnAwake = false;
                loopAudioSource.spatialBlend = 1.0f; // 3D
                loopAudioSource.minDistance = 3f;
                loopAudioSource.maxDistance = 12f;
                loopAudioSource.rolloffMode = AudioRolloffMode.Linear;
                if (AudioManager.Instance != null)
                {
                    loopAudioSource.clip = AudioManager.Instance.GetAudioClipVagonetaLoop();
                    loopAudioSource.outputAudioMixerGroup = AudioManager.Instance.GetSFXGroup();
                }
            }
        }

        public void DispararVagoneta()
        {
            if (isTriggered || isRunning) return;
            isTriggered = true;
            cartRoutine = StartCoroutine(RutinaVagoneta());
        }

        private IEnumerator RutinaVagoneta()
        {
            // 1. Aviso 1 s antes con "¡!" y sonido de advertencia
            if (warningObject != null)
            {
                warningObject.SetActive(true);
            }

            if (loopAudioSource != null && loopAudioSource.clip != null)
            {
                loopAudioSource.volume = 0.5f;
                loopAudioSource.Play();
            }

            yield return new WaitForSeconds(warningDuration);

            if (warningObject != null)
            {
                warningObject.SetActive(false);
            }

            // 2. Recorrido a 9 u/s
            isRunning = true;
            if (loopAudioSource != null) loopAudioSource.volume = 1.0f;

            while (Vector3.Distance(transform.position, endPoint) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, endPoint, speed * Time.deltaTime);
                yield return null;
            }

            transform.position = endPoint;
            isRunning = false;

            if (loopAudioSource != null)
            {
                loopAudioSource.Stop();
            }

            gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isRunning) return;

            if (other.CompareTag("Player"))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    bool golpe = health.TakeDamage(damage);
                    if (golpe && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDanio();
                    }
                }
            }
        }

        public void ResetObject()
        {
            if (cartRoutine != null) StopCoroutine(cartRoutine);

            isRunning = false;
            isTriggered = false;
            transform.position = startPoint;

            if (warningObject != null) warningObject.SetActive(false);
            if (loopAudioSource != null) loopAudioSource.Stop();

            gameObject.SetActive(true);
        }

        public void SetPuntos(Vector3 inicio, Vector3 fin)
        {
            startPoint = inicio;
            endPoint = fin;
            transform.position = inicio;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 pIni = Application.isPlaying ? startPoint : transform.position;
            Vector3 pFin = endPoint != Vector3.zero ? endPoint : pIni + new Vector3(20f, 0f, 0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pIni, pFin);
            Gizmos.DrawWireCube(pIni, new Vector3(2f, 1.5f, 0.1f));
            Gizmos.DrawWireCube(pFin, new Vector3(2f, 1.5f, 0.1f));
        }
    }
}
