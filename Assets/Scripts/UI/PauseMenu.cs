using UnityEngine;
using UnityEngine.UI;
using ElMinero.Managers;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ElMinero.UI
{
    /// <summary>
    /// Gestiona el menú de pausa durante el gameplay.
    /// Detiene el tiempo (Time.timeScale = 0) y ofrece Continuar, Reiniciar Nivel, Opciones o Volver al Menú.
    /// Transiciona limpiamente entre el panel de pausa y el panel de opciones.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panel y Botones")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button pauseIconButton; // Botón en la esquina del HUD
        [SerializeField] private OptionsPanel optionsPanel;

        private bool isPaused = false;

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionsPanel != null)
            {
                optionsPanel.gameObject.SetActive(false);
                optionsPanel.AlCerrar += OnOptionsClosed;
            }

            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
            if (optionsButton != null) optionsButton.onClick.AddListener(OpenOptions);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
            if (pauseIconButton != null) pauseIconButton.onClick.AddListener(TogglePause);
        }

        private void OnDestroy()
        {
            if (optionsPanel != null)
            {
                optionsPanel.AlCerrar -= OnOptionsClosed;
            }
        }

        private void Update()
        {
            bool escapePressed = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                escapePressed = true;
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                escapePressed = true;
            }
#endif

            if (escapePressed)
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            // Si el panel de opciones está abierto dentro de la pausa, cerrarlo restaura el menú de pausa
            if (optionsPanel != null && optionsPanel.gameObject.activeSelf)
            {
                optionsPanel.CerrarPanel();
                return;
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            if (pausePanel != null) pausePanel.SetActive(true);
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            if (pausePanel != null) pausePanel.SetActive(false);
            if (optionsPanel != null) optionsPanel.gameObject.SetActive(false);
        }

        public void OpenOptions()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (optionsPanel != null)
            {
                if (pausePanel != null) pausePanel.SetActive(false);
                optionsPanel.gameObject.SetActive(true);
                optionsPanel.transform.SetAsLastSibling();
            }
        }

        private void OnOptionsClosed()
        {
            if (isPaused && pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneLoader.ReloadCurrentScene();
        }

        public void GoToMainMenu()
        {
            Time.timeScale = 1f;
            isPaused = false;
            SceneLoader.LoadScene("MenuPrincipal");
        }
    }
}
