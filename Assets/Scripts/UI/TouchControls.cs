using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using ElMinero.Jugador;

namespace ElMinero.UI
{
    /// <summary>
    /// Gestiona los botones táctiles en pantalla para dispositivos móviles (o WebGL en móvil).
    /// Controla izquierda, derecha y salto, comunicándose con PlayerInputHandler.
    /// Solo se muestra en plataformas móviles o si se activa el forzado en el Editor.
    /// </summary>
    public class TouchControls : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Si es verdadero, fuerza la visibilidad de los controles táctiles en el Editor.")]
        [SerializeField] private bool forceShowInEditor = false;

        [Header("Referencias de Botones")]
        [SerializeField] private GameObject leftButton;
        [SerializeField] private GameObject rightButton;
        [SerializeField] private GameObject jumpButton;

        private PlayerInputHandler inputHandler;
        private bool movingLeft = false;
        private bool movingRight = false;

        private void Awake()
        {
            // Determinar visibilidad según plataforma
            bool isMobile = Application.isMobilePlatform || (Application.isEditor && forceShowInEditor);
            gameObject.SetActive(isMobile);

            FindInputHandler();
        }

        private void Start()
        {
            if (inputHandler == null)
            {
                FindInputHandler();
            }
        }

        private void FindInputHandler()
        {
            inputHandler = UnityEngine.Object.FindAnyObjectByType<PlayerInputHandler>();
        }

        public void SetInputHandler(PlayerInputHandler handler)
        {
            inputHandler = handler;
        }

        // --- IZQUIERDA ---
        public void OnLeftDown()
        {
            movingLeft = true;
            UpdateMoveInput();
        }

        public void OnLeftUp()
        {
            movingLeft = false;
            UpdateMoveInput();
        }

        // --- DERECHA ---
        public void OnRightDown()
        {
            movingRight = true;
            UpdateMoveInput();
        }

        public void OnRightUp()
        {
            movingRight = false;
            UpdateMoveInput();
        }

        // --- SALTO ---
        public void OnJumpDown()
        {
            if (inputHandler != null)
            {
                inputHandler.OnJumpButtonDown();
            }
        }

        public void OnJumpUp()
        {
            if (inputHandler != null)
            {
                inputHandler.OnJumpButtonUp();
            }
        }

        private void UpdateMoveInput()
        {
            if (inputHandler == null)
            {
                FindInputHandler();
                if (inputHandler == null) return;
            }

            float move = 0f;
            if (movingLeft) move -= 1f;
            if (movingRight) move += 1f;

            inputHandler.SetMoveInput(move);
        }
    }
}
