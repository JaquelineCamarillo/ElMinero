using UnityEngine;
using ElMinero.Managers;

namespace ElMinero.Enemigos
{
    /// <summary>
    /// Murciélago estándar (Niveles 1 y 2).
    /// Patrulla horizontalmente entre dos puntos a 2 u/s con ondulación vertical (amplitud 0.5 u).
    /// Emite chillido cuando el jugador se encuentra a menos de 6 u (cooldown de 3 s).
    /// </summary>
    public class BatPatrol : EnemyBase
    {
        [Header("Configuración de Patrulla")]
        [Tooltip("Distancia horizontal total de patrulla desde la posición inicial.")]
        [SerializeField] private float patrolDistance = 4f;

        [Tooltip("Velocidad de desplazamiento horizontal (según GDD: 2 u/s).")]
        [SerializeField] private float speed = 2f;

        [Tooltip("Amplitud de la ondulación senoidal vertical (0.5 u).")]
        [SerializeField] private float waveAmplitude = 0.5f;

        [Tooltip("Frecuencia de la ondulación senoidal.")]
        [SerializeField] private float waveFrequency = 3f;

        [Header("Chillido")]
        [Tooltip("Distancia de detección para reproducir el chillido (< 6 u).")]
        [SerializeField] private float screechDistance = 6f;

        [Tooltip("Tiempo mínimo entre chillidos (3 s).")]
        [SerializeField] private float screechCooldown = 3f;

        private float leftX;
        private float rightX;
        private int movingDirection = 1; // 1 = derecha, -1 = izquierda
        private float nextScreechTime = 0f;

        protected override void Awake()
        {
            base.Awake();
            CalcularLimites();
        }

        private void CalcularLimites()
        {
            leftX = initialPosition.x - (patrolDistance * 0.5f);
            rightX = initialPosition.x + (patrolDistance * 0.5f);
        }

        private void Update()
        {
            MoverMurcielago();
            VerificarChillido();
        }

        private void MoverMurcielago()
        {
            // Movimiento horizontal
            float newX = transform.position.x + (movingDirection * speed * Time.deltaTime);

            if (newX >= rightX)
            {
                newX = rightX;
                movingDirection = -1;
            }
            else if (newX <= leftX)
            {
                newX = leftX;
                movingDirection = 1;
            }

            // Ondulación senoidal vertical sobre la altura inicial
            float newY = initialPosition.y + Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

            transform.position = new Vector3(newX, newY, transform.position.z);

            // Orientación visual
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (movingDirection > 0);
            }
        }

        private void VerificarChillido()
        {
            if (playerTransform == null)
            {
                LocalizarJugador();
                return;
            }

            if (Time.time >= nextScreechTime)
            {
                float dist = Vector2.Distance(transform.position, playerTransform.position);
                if (dist < screechDistance)
                {
                    nextScreechTime = Time.time + screechCooldown;
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayMurcielagoChillido();
                    }
                }
            }
        }

        public override void ResetObject()
        {
            base.ResetObject();
            movingDirection = 1;
            nextScreechTime = 0f;
            CalcularLimites();
        }

        protected override void OnDrawGizmosSelected()
        {
            Vector3 center = Application.isPlaying ? initialPosition : transform.position;
            Vector3 pLeft = new Vector3(center.x - (patrolDistance * 0.5f), center.y, center.z);
            Vector3 pRight = new Vector3(center.x + (patrolDistance * 0.5f), center.y, center.z);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pLeft, pRight);
            Gizmos.DrawWireSphere(pLeft, 0.2f);
            Gizmos.DrawWireSphere(pRight, 0.2f);

            // Rango de chillido
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, screechDistance);
        }
    }
}
