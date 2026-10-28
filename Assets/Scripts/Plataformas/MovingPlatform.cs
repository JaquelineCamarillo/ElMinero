using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;

namespace ElMinero.Plataformas
{
    /// <summary>
    /// Plataforma móvil horizontal (MovingPlatform).
    /// Se desplaza suavemente entre dos puntos A y B usando Rigidbody2D Kinemático con MovePosition,
    /// pausando en los extremos y sincronizando la velocidad con el PlayerController.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class MovingPlatform : MonoBehaviour, IResettable
    {
        [Header("Trayectoria y Movimiento")]
        [Tooltip("Punto inicial de movimiento (si es nulo, usa la posición inicial).")]
        [SerializeField] private Transform pointA;

        [Tooltip("Punto final de movimiento.")]
        [SerializeField] private Transform pointB;

        [Tooltip("Distancia horizontal relativa si no se asignan transforms A y B.")]
        [SerializeField] private float moveDistance = 5f;

        [Tooltip("Velocidad de traslación en unidades por segundo.")]
        [SerializeField] private float speed = 2f;

        [Tooltip("Pausa en segundos al llegar a cada extremo antes de regresar.")]
        [SerializeField] private float waitTimeAtEnds = 0.5f;

        private Rigidbody2D rb;
        private Vector3 startPos;
        private Vector3 targetPos;
        private Vector3 posA;
        private Vector3 posB;
        private bool movingToB = true;
        private bool isWaiting = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo != -1)
            {
                gameObject.layer = layerSuelo;
            }

            CalcularPuntos();
        }

        private void CalcularPuntos()
        {
            startPos = transform.position;
            posA = pointA != null ? pointA.position : startPos;
            posB = pointB != null ? pointB.position : startPos + new Vector3(moveDistance, 0f, 0f);
            targetPos = posB;
            movingToB = true;
        }

        private void FixedUpdate()
        {
            if (isWaiting) return;

            Vector2 current = rb.position;
            Vector2 target = targetPos;
            Vector2 newPos = Vector2.MoveTowards(current, target, speed * Time.fixedDeltaTime);
            Vector2 vel = (newPos - current) / Time.fixedDeltaTime;

            rb.linearVelocity = vel;
            rb.MovePosition(newPos);

            if (Vector2.Distance(newPos, target) < 0.02f)
            {
                StartCoroutine(EsperarEnExtremo());
            }
        }

        private IEnumerator EsperarEnExtremo()
        {
            isWaiting = true;
            rb.linearVelocity = Vector2.zero;

            yield return new WaitForSeconds(waitTimeAtEnds);

            movingToB = !movingToB;
            targetPos = movingToB ? posB : posA;
            isWaiting = false;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Constantes.Tags.Player))
            {
                // Solo si el contacto ocurre desde arriba
                foreach (var contact in collision.contacts)
                {
                    if (contact.normal.y < -0.5f)
                    {
                        var player = collision.gameObject.GetComponent<PlayerController>();
                        if (player != null)
                        {
                            player.SetMovingPlatform(rb);
                        }
                        break;
                    }
                }
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag(Constantes.Tags.Player))
            {
                var player = collision.gameObject.GetComponent<PlayerController>();
                if (player != null)
                {
                    player.SetMovingPlatform(null);
                }
            }
        }

        public void ResetObject()
        {
            StopAllCoroutines();
            isWaiting = false;
            movingToB = true;
            targetPos = posB;
            transform.position = posA;
            rb.position = posA;
            rb.linearVelocity = Vector2.zero;
        }

        private void OnDrawGizmos()
        {
            Vector3 a = pointA != null ? pointA.position : transform.position;
            Vector3 b = pointB != null ? pointB.position : transform.position + new Vector3(moveDistance, 0f, 0f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawWireSphere(a, 0.25f);
            Gizmos.DrawWireSphere(b, 0.25f);
        }
    }
}
