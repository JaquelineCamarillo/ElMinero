using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del Menú Principal.
    /// Permite ingresar el nombre del minero, iniciar el juego, ver instrucciones y salir.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [Tooltip("Campo de texto para ingresar el nombre del jugador.")]
        [SerializeField] private TMP_InputField nameInputField;

        [Tooltip("Botón para iniciar el juego (Nivel 1).")]
        [SerializeField] private Button playButton;

        [Tooltip("Botón para abrir el panel de 'Cómo Jugar'.")]
        [SerializeField] private Button howToPlayButton;

        [Tooltip("Botón para salir de la aplicación.")]
        [SerializeField] private Button quitButton;

        [Header("Panel Cómo Jugar")]
        [SerializeField] private GameObject howToPlayPanel;
        [SerializeField] private Button closeHowToPlayButton;

        private const string PrefKeyNombre = "JugadorNombre";
        private const string NombrePorDefecto = "Refugio";

        private void Start()
        {
            // Cargar último nombre usado
            string nombreGuardado = PlayerPrefs.GetString(PrefKeyNombre, NombrePorDefecto);
            if (nameInputField != null)
            {
                nameInputField.characterLimit = 12;
                nameInputField.text = nombreGuardado;
            }

            // Ocultar botón salir en WebGL
#if UNITY_WEBGL
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(false);
            }
#endif

            // Asignar listeners
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (howToPlayButton != null) howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
            if (closeHowToPlayButton != null) closeHowToPlayButton.onClick.AddListener(OnCloseHowToPlayClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }

        private void OnPlayClicked()
        {
            string nombre = NombrePorDefecto;
            if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            {
                nombre = nameInputField.text.Trim();
            }

            // Guardar en PlayerPrefs
            PlayerPrefs.SetString(PrefKeyNombre, nombre);
            PlayerPrefs.Save();

            // Guardar en GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPlayerName(nombre);
                GameManager.Instance.ResetScore();
            }

            // Transición a Nivel 1
            SceneLoader.LoadScene("Nivel1_MinaRayas");
        }

        private void OnHowToPlayClicked()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(true);
        }

        private void OnCloseHowToPlayClicked()
        {
            if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        }

        private void OnQuitClicked()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
