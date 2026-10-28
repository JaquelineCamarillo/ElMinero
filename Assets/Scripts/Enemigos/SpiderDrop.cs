using UnityEngine;
using ElMinero.Managers;

namespace ElMinero.Enemigos
{
    /// <summary>
    /// Araña de techo (Niveles 2 y 3).
    /// Cuelga de su hilo (Hilo_Arana). Cuando el jugador pasa por debajo (±1.5 u en X),
    /// desciende rápidamente a 4 u/s hasta 4-6 tiles, espera 1 s balanceándose suavemente,
    /// y asciende de vuelta a 1.5 u/s.
    /// </summary>
    public class SpiderDrop : EnemyBase
    {
        private enum SpiderState { Hanging, Dropping, WaitingAtBottom, Rising }

        [Header("Configuración de Descenso")]
        [Tooltip("Distancia vertical máxima de descenso (4 a 6 tiles).")]
        [SerializeField] private float dropDistance = 5f;

        [Tooltip("Velocidad de descenso (4 u/s).")]
        [SerializeField] private float dropSpeed = 4f;

        [Tooltip("Tiempo de espera en el punto bajo (1 s).")]
        [SerializeField] private float waitTime = 1f;

        [Tooltip("Velocidad de subida (1.5 u/s).")]
        [SerializeField] private float riseSpeed = 1.5f;

        [Tooltip("Tolerancia horizontal para activar el descenso (±1.5 u).")]
        [SerializeField] private float triggerToleranceX = 1.5f;

        [Header("Hilo de la Araña")]
        [Tooltip("SpriteRenderer del hilo de la araña (estirado en Y o modo Tiled).")]
        [SerializeField] private SpriteRenderer webRenderer;

        private SpiderState currentState = SpiderState.Hanging;
        private Vector3 bottomPosition;
        private float waitTimer = 0f;

        protected override void Awake()
        {
            base.Awake();
            bottomPosition = initialPosition - new Vector3(0f, dropDistance, 0f);
            ActualizarHilo();
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                LocalizarJugador();
            }

            switch (currentState)
            {
                case SpiderState.Hanging:
                    VerificarPasoJugador();
                    break;

                case SpiderState.Dropping:
                    ActualizarDescenso();
                    break;

                case SpiderState.WaitingAtBottom:
                    ActualizarEsperaAbajo();
                    break;

                case SpiderState.Rising:
                    ActualizarSubida();
                    break;
            }

            ActualizarHilo();
        }

        private void VerificarPasoJugador()
        {
            if (playerTransform == null) return;

            Vector3 pos = transform.position;
            Vector3 pPos = playerTransform.position;

            // Si el jugador está directamente debajo (±1.5 u en X) y a menor altura
            if (Mathf.Abs(pPos.x - pos.x) <= triggerToleranceX && pPos.y < pos.y && (pos.y - pPos.y) <= (dropDistance + 2f))
            {
                currentState = SpiderState.Dropping;
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayAranaBaja();
                }
            }
        }

        private void ActualizarDescenso()
        {
            transform.position = Vector3.MoveTowards(transform.position, bottomPosition, dropSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, bottomPosition) <= 0.05f)
            {
                transform.position = bottomPosition;
                currentState = SpiderState.WaitingAtBottom;
                waitTimer = waitTime;
            }
        }

        private void ActualizarEsperaAbajo()
        {
            waitTimer -= Time.deltaTime;

            // Balanceo sutil horizontal mientras espera
            float sway = Mathf.Sin(Time.time * 6f) * 0.12f;
            transform.position = new Vector3(bottomPosition.x + sway, bottomPosition.y, bottomPosition.z);

            if (waitTimer <= 0f)
            {
                currentState = SpiderState.Rising;
            }
        }

        private void ActualizarSubida()
        {
            Vector3 target = new Vector3(initialPosition.x, initialPosition.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, riseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, initialPosition) <= 0.05f)
            {
                transform.position = initialPosition;
                currentState = SpiderState.Hanging;
            }
        }

        private void ActualizarHilo()
        {
            if (webRenderer == null) return;

            float currentDist = initialPosition.y - transform.position.y;
            if (currentDist <= 0.05f)
            {
                webRenderer.enabled = false;
            }
            else
            {
                webRenderer.enabled = true;
                // Posicionar el hilo a medio camino entre el techo y la araña
                webRenderer.transform.position = new Vector3(initialPosition.x, initialPosition.y - (currentDist * 0.5f), transform.position.z);
                webRenderer.size = new Vector2(webRenderer.size.x, currentDist);
            }
        }

        public override void ResetObject()
        {
            base.ResetObject();
            currentState = SpiderState.Hanging;
            bottomPosition = initialPosition - new Vector3(0f, dropDistance, 0f);
            ActualizarHilo();
        }

        protected override void OnDrawGizmosSelected()
        {
            Vector3 top = Application.isPlaying ? initialPosition : transform.position;
            Vector3 bot = top - new Vector3(0f, dropDistance, 0f);

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(top, bot);
            Gizmos.DrawWireSphere(bot, 0.25f);

            // Zona de activación en X
            Gizmos.color = new Color(1f, 0f, 1f, 0.2f);
            Vector3 triggerCenter = new Vector3(top.x, top.y - (dropDistance * 0.5f), top.z);
            Vector3 triggerSize = new Vector3(triggerToleranceX * 2f, dropDistance + 2f, 0.1f);
            Gizmos.DrawWireCube(triggerCenter, triggerSize);
        }
    }
}
