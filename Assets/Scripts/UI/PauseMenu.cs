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
    /// Detiene el tiempo (Time.timeScale = 0) y ofrece Continuar, Reiniciar Nivel o Volver al Menú.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        [Header("Panel y Botones")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button pauseIconButton; // Botón en la esquina del HUD

        private bool isPaused = false;

        private void Start()
        {
            if (pausePanel != null) pausePanel.SetActive(false);

            if (resumeButton != null) resumeButton.onClick.AddListener(ResumeGame);
            if (restartButton != null) restartButton.onClick.AddListener(RestartLevel);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(GoToMainMenu);
            if (pauseIconButton != null) pauseIconButton.onClick.AddListener(TogglePause);
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
