using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ElMinero.Jugador
{
    /// <summary>
    /// Manejador unificado de entrada para el jugador.
    /// Soporta tanto el nuevo Input System como el Legacy Input Manager,
    /// además de entrada virtual proveniente de controles táctiles en pantalla.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Configuración y Depuración")]
        [Tooltip("Fuerza la visualización/uso de controles virtuales incluso en el Editor para pruebas.")]
        [SerializeField] private bool forceMobileControls = false;

        // Entradas procesadas
        public float MoveInput { get; private set; }
        public bool JumpTriggered { get; private set; }
        public bool JumpHeld { get; private set; }
        public bool JumpReleased { get; private set; }
        public bool PauseTriggered { get; private set; }

        // Entradas virtuales (para botones táctiles de UI)
        private float virtualMoveInput = 0f;
        private bool virtualJumpPressed = false;
        private bool virtualJumpHeld = false;
        private bool virtualJumpReleased = false;

        public bool ForceMobileControls => forceMobileControls;

        private void Update()
        {
            ReadKeyboardInput();
            CombineInputs();
        }

        private void ReadKeyboardInput()
        {
            float keyboardMove = 0f;
            bool keyboardJumpDown = false;
            bool keyboardJumpHeld = false;
            bool keyboardJumpUp = false;
            bool keyboardPause = false;

#if ENABLE_INPUT_SYSTEM
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                    keyboardMove -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                    keyboardMove += 1f;

                keyboardJumpDown = keyboard.spaceKey.wasPressedThisFrame;
                keyboardJumpHeld = keyboard.spaceKey.isPressed;
                keyboardJumpUp = keyboard.spaceKey.wasReleasedThisFrame;

                keyboardPause = keyboard.escapeKey.wasPressedThisFrame;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            keyboardMove = Input.GetAxisRaw("Horizontal");
            keyboardJumpDown = Input.GetKeyDown(KeyCode.Space);
            keyboardJumpHeld = Input.GetKey(KeyCode.Space);
            keyboardJumpUp = Input.GetKeyUp(KeyCode.Space);
            keyboardPause = Input.GetKeyDown(KeyCode.Escape);
#endif

            // Asignación combinando teclado
            MoveInput = Mathf.Clamp(keyboardMove + virtualMoveInput, -1f, 1f);
            JumpTriggered = keyboardJumpDown || virtualJumpPressed;
            JumpHeld = keyboardJumpHeld || virtualJumpHeld;
            JumpReleased = keyboardJumpUp || virtualJumpReleased;
            PauseTriggered = keyboardPause;

            // Resetear triggers virtuales consumidos en este frame
            virtualJumpPressed = false;
            virtualJumpReleased = false;
        }

        private void CombineInputs()
        {
            // Limitar valor final
            if (Mathf.Abs(MoveInput) < 0.05f)
            {
                MoveInput = 0f;
            }
        }

        // ====================================================================
        // MÉTODOS PÚBLICOS PARA CONTROLES TÁCTILES (UI EventTrigger)
        // ====================================================================

        public void SetMoveInput(float value)
        {
            virtualMoveInput = Mathf.Clamp(value, -1f, 1f);
        }

        public void OnJumpButtonDown()
        {
            virtualJumpPressed = true;
            virtualJumpHeld = true;
        }

        public void OnJumpButtonUp()
        {
            virtualJumpHeld = false;
            virtualJumpReleased = true;
        }
    }
}
