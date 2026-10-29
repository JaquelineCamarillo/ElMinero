using UnityEngine;
using ElMinero.Managers;
using ElMinero.Utilidades;

namespace ElMinero.Enemigos
{
    /// <summary>
    /// Araña de techo (Niveles 2 y 3).
    /// Mediante un Raycast hacia arriba encuentra el techo de la mina y ancla Telarana_Techo.png.
    /// Conecta su hilo continuo desde el techo hasta la araña.
    /// Cuando el jugador pasa por debajo (±1.5 u en X), desciende rápidamente a 4 u/s hasta 4-6 tiles,
    /// espera 1 s balanceándose suavemente, y asciende de vuelta a 1.5 u/s sin desconectarse jamás de su hilo.
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

        [Header("Anclaje al Techo y Telaraña")]
        [SerializeField] private Sprite spriteTelaranaTecho;
        [SerializeField] private LineRenderer webLine;

        private SpiderState currentState = SpiderState.Hanging;
        private Vector3 ceilingPosition;
        private Vector3 hangingPosition;
        private Vector3 bottomPosition;
        private float waitTimer = 0f;
        private GameObject goTelaranaTecho;

        protected override void Awake()
        {
            base.Awake();

            // 1. Raycast hacia arriba buscando el techo en la capa Suelo
            int layerSuelo = LayerMask.GetMask(Constantes.Layers.Suelo);
            RaycastHit2D hitTecho = Physics2D.Raycast(transform.position, Vector2.up, 25f, layerSuelo);

            if (hitTecho.collider != null)
            {
                ceilingPosition = new Vector3(transform.position.x, hitTecho.point.y, transform.position.z);
            }
            else
            {
                ceilingPosition = transform.position + new Vector3(0f, 4f, 0f);
            }

            // 2. Anclar telaraña en el techo
            CrearTelaranaTecho();

            // 3. Posición colgando inicial (ligeramente bajo el techo)
            hangingPosition = ceilingPosition - new Vector3(0f, 0.6f, 0f);
            transform.position = hangingPosition;
            bottomPosition = hangingPosition - new Vector3(0f, dropDistance, 0f);

            ConfigurarHilo();
        }

        private void CrearTelaranaTecho()
        {
            goTelaranaTecho = new GameObject("Telarana_Techo_Ancla");
            goTelaranaTecho.transform.position = ceilingPosition;

            var sr = goTelaranaTecho.AddComponent<SpriteRenderer>();
            if (spriteTelaranaTecho == null)
            {
                spriteTelaranaTecho = Resources.Load<Sprite>("Telarana_Techo");
#if UNITY_EDITOR
                if (spriteTelaranaTecho == null)
                {
                    spriteTelaranaTecho = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Telarana_Techo.png");
                }
#endif
            }

            sr.sprite = spriteTelaranaTecho;
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
            sr.sortingOrder = 2;
        }

        private void ConfigurarHilo()
        {
            if (webLine == null)
            {
                webLine = GetComponent<LineRenderer>();
                if (webLine == null) webLine = gameObject.AddComponent<LineRenderer>();
            }

            if (webLine != null)
            {
                webLine.positionCount = 2;
                webLine.startWidth = 0.05f;
                webLine.endWidth = 0.05f;
                webLine.useWorldSpace = true;
                webLine.sortingLayerName = Constantes.SortingLayers.Escenario;
                webLine.sortingOrder = 1;

                Material lineMat = new Material(Shader.Find("Sprites/Default"));
                lineMat.color = new Color(0.85f, 0.85f, 0.9f, 0.85f);
                webLine.material = lineMat;
            }
        }

        private void LateUpdate()
        {
            // El hilo siempre conecta desde la telaraña en el techo hasta el lomo de la araña
            if (webLine != null)
            {
                webLine.SetPosition(0, ceilingPosition);
                webLine.SetPosition(1, transform.position + new Vector3(0f, 0.15f, 0f));
            }
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
            Vector3 target = new Vector3(hangingPosition.x, hangingPosition.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, target, riseSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, hangingPosition) <= 0.05f)
            {
                transform.position = hangingPosition;
                currentState = SpiderState.Hanging;
            }
        }

        public override void ResetObject()
        {
            base.ResetObject();
            currentState = SpiderState.Hanging;
            transform.position = hangingPosition;
            bottomPosition = hangingPosition - new Vector3(0f, dropDistance, 0f);
        }

        protected override void OnDrawGizmosSelected()
        {
            Vector3 top = Application.isPlaying ? ceilingPosition : transform.position;
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
