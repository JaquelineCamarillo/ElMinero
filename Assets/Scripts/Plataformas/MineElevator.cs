using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;

namespace ElMinero.Plataformas
{
    /// <summary>
    /// Elevador del malacate (Niveles 1 y 3).
    /// Plataforma kinemática vertical que se traslada suavemente entre el fondo y la cima del tiro a 2 u/s,
    /// pausándose 1 s en cada extremo. Transporta al jugador sin deslizamiento indeseado.
    /// La rueda del malacate arriba gira y reproduce el loop 3D (Malacate_Loop) únicamente mientras se mueve.
    /// </summary>
    public class MineElevator : MonoBehaviour, IResettable
    {
        [Header("Cinemática Vertical")]
        [Tooltip("Punto inferior (A).")]
        [SerializeField] private Vector3 bottomPoint;

        [Tooltip("Punto superior (B).")]
        [SerializeField] private Vector3 topPoint;

        [Tooltip("Velocidad de elevación (según GDD: 2 u/s).")]
        [SerializeField] private float speed = 2f;

        [Tooltip("Tiempo de pausa en cada extremo (1 s).")]
        [SerializeField] private float waitTime = 1f;

        [Header("Malacate Superior y Sonido")]
        [Tooltip("Transform de la rueda del malacate que gira al moverse.")]
        [SerializeField] private Transform malacateWheel;

        [Tooltip("Velocidad angular de giro del malacate (grados/s).")]
        [SerializeField] private float wheelRotationSpeed = 180f;

        [Tooltip("AudioSource 3D para el bucle del malacate.")]
        [SerializeField] private AudioSource loopAudioSource;

        private Rigidbody2D rb;
        private Vector3 targetPoint;
        private bool isMoving = false;
        private Coroutine movementRoutine;
        private Vector3 initialPos;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.useFullKinematicContacts = true;
            }

            initialPos = transform.position;
            if (bottomPoint == Vector3.zero) bottomPoint = transform.position;
            if (topPoint == Vector3.zero) topPoint = transform.position + new Vector3(0f, 8f, 0f);

            targetPoint = topPoint;
            ConfigurarAudio3D();
        }

        private void ConfigurarAudio3D()
        {
            if (loopAudioSource == null)
            {
                loopAudioSource = GetComponent<AudioSource>();
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
                loopAudioSource.maxDistance = 14f;
                loopAudioSource.rolloffMode = AudioRolloffMode.Linear;
                if (AudioManager.Instance != null)
                {
                    loopAudioSource.clip = AudioManager.Instance.GetAudioClipMalacateLoop();
                    loopAudioSource.outputAudioMixerGroup = AudioManager.Instance.GetSFXGroup();
                }
            }
        }

        private void Start()
        {
            movementRoutine = StartCoroutine(RutinaMovimiento());
        }

        private void Update()
        {
            // Girar rueda del malacate si la plataforma se está moviendo
            if (isMoving && malacateWheel != null)
            {
                float rotDir = (targetPoint == topPoint) ? -1f : 1f;
                malacateWheel.Rotate(0f, 0f, rotDir * wheelRotationSpeed * Time.deltaTime);
            }
        }

        private IEnumerator RutinaMovimiento()
        {
            while (true)
            {
                // Iniciar movimiento hacia el objetivo
                isMoving = true;
                if (loopAudioSource != null && !loopAudioSource.isPlaying)
                {
                    loopAudioSource.Play();
                }

                while (Vector3.Distance(transform.position, targetPoint) > 0.05f)
                {
                    Vector3 newPos = Vector3.MoveTowards(transform.position, targetPoint, speed * Time.fixedDeltaTime);
                    rb.MovePosition(newPos);
                    yield return new WaitForFixedUpdate();
                }

                rb.MovePosition(targetPoint);
                isMoving = false;
                if (loopAudioSource != null)
                {
                    loopAudioSource.Pause();
                }

                // Pausa en el extremo
                yield return new WaitForSeconds(waitTime);

                // Alternar dirección
                targetPoint = (targetPoint == topPoint) ? bottomPoint : topPoint;
            }
        }

        // Transporte suave del jugador para evitar que resbale
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && collision.contacts[0].normal.y < -0.5f)
            {
                collision.transform.SetParent(transform);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player") && collision.transform.parent == transform)
            {
                collision.transform.SetParent(null);
            }
        }

        public void ResetObject()
        {
            if (movementRoutine != null) StopCoroutine(movementRoutine);

            transform.position = bottomPoint;
            targetPoint = topPoint;
            isMoving = false;

            if (loopAudioSource != null) loopAudioSource.Stop();

            movementRoutine = StartCoroutine(RutinaMovimiento());
        }

        public void ConfigurarPuntos(Vector3 abajo, Vector3 arriba, Transform ruedaMalacate = null)
        {
            bottomPoint = abajo;
            topPoint = arriba;
            transform.position = abajo;
            targetPoint = arriba;
            malacateWheel = ruedaMalacate;
        }

        private void OnDrawGizmosSelected()
        {
            Vector3 pA = bottomPoint != Vector3.zero ? bottomPoint : transform.position;
            Vector3 pB = topPoint != Vector3.zero ? topPoint : transform.position + new Vector3(0f, 8f, 0f);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(pA, pB);
            Gizmos.DrawWireCube(pA, new Vector3(3f, 0.5f, 0.1f));
            Gizmos.DrawWireCube(pB, new Vector3(3f, 0.5f, 0.1f));
        }
    }
}
