using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del Menú Principal según las especificaciones de las secciones 3.2 a 3.5.
    /// Administra el inicio de nueva partida con nombre personalizado, continuación de progreso,
    /// apertura de paneles modales (Niveles, Opciones, Créditos), versión y salida.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [Header("Entrada de Nombre")]
        [Tooltip("Campo de texto para ingresar el nombre del minero.")]
        [SerializeField] private TMP_InputField nameInputField;

        [Header("Botones Principales")]
        [SerializeField] private Button playButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button levelsButton;
        [SerializeField] private Button optionsButton;
        [SerializeField] private Button creditsButton;
        [SerializeField] private Button quitButton;

        [Header("Paneles Modales")]
        [SerializeField] private LevelSelectPanel levelSelectPanel;
        [SerializeField] private OptionsPanel optionsPanel;
        [SerializeField] private CreditsPanel creditsPanel;

        [Header("Información de Versión")]
        [SerializeField] private TextMeshProUGUI versionText;

        public const string PrefKeyNombre = "NombreJugador";
        public const string NombrePorDefecto = "Refugio";

        private void Start()
        {
            Time.timeScale = 1f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicMenu();
            }

            // Mostrar versión
            if (versionText != null)
            {
                versionText.text = $"v{Application.version}";
            }

            // Cargar nombre guardado
            string nombreGuardado = PlayerPrefs.GetString(PrefKeyNombre, NombrePorDefecto);
            if (nameInputField != null)
            {
                nameInputField.characterLimit = 12;
                nameInputField.text = nombreGuardado;
            }

            // Configurar botón Continuar según progreso
            ConfigurarBotonContinuar();

            // Ocultar botón Salir en WebGL
#if UNITY_WEBGL
            if (quitButton != null)
            {
                quitButton.gameObject.SetActive(false);
            }
#endif

            // Asignar listeners a botones principales
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);
            if (levelsButton != null) levelsButton.onClick.AddListener(OnLevelsClicked);
            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);

            // Asegurar que los paneles modales inicien desactivados
            if (levelSelectPanel != null) levelSelectPanel.gameObject.SetActive(false);
            if (optionsPanel != null) optionsPanel.gameObject.SetActive(false);
            if (creditsPanel != null) creditsPanel.gameObject.SetActive(false);
        }

        private void ConfigurarBotonContinuar()
        {
            if (continueButton == null) return;

            int nivelDesbloqueado = PlayerPrefs.GetInt(SettingsManager.PREF_NIVEL_DESBLOQUEADO, 1);
            int estrellasN1 = PlayerPrefs.GetInt("Estrellas_Nivel1_MinaRayas", 0);
            int recordN1 = PlayerPrefs.GetInt("MejorPuntaje_Nivel1_MinaRayas", 0);

            // Hay progreso si nivelDesbloqueado > 1 o si completó el Nivel 1 alguna vez
            bool hayProgreso = (nivelDesbloqueado > 1) || (estrellasN1 > 0) || (recordN1 > 0);

            continueButton.interactable = hayProgreso;
            var img = continueButton.GetComponent<Image>();
            if (img != null)
            {
                img.color = hayProgreso ? Color.white : new Color(0.45f, 0.45f, 0.45f, 0.6f);
            }
        }

        private void GuardarNombreActual()
        {
            string nombre = NombrePorDefecto;
            if (nameInputField != null && !string.IsNullOrWhiteSpace(nameInputField.text))
            {
                nombre = nameInputField.text.Trim();
            }

            PlayerPrefs.SetString(PrefKeyNombre, nombre);
            PlayerPrefs.SetString("JugadorNombre", nombre); // Compatibilidad
            PlayerPrefs.Save();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetPlayerName(nombre);
            }
        }

        private void OnPlayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            GuardarNombreActual();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetScore();
            }

            // Iniciar flujo narrativo: Intro -> Nivel 1
            SceneLoader.LoadScene("Intro");
        }

        private void OnContinueClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            GuardarNombreActual();

            int nivelDesbloqueado = PlayerPrefs.GetInt(SettingsManager.PREF_NIVEL_DESBLOQUEADO, 1);
            string escena = "Nivel1_MinaRayas";
            if (nivelDesbloqueado == 2) escena = "Nivel2_ElNopal";
            else if (nivelDesbloqueado >= 3) escena = "Nivel3_LaValenciana";

            SceneLoader.LoadScene(escena);
        }

        private void OnLevelsClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (levelSelectPanel != null)
            {
                levelSelectPanel.gameObject.SetActive(true);
                levelSelectPanel.ActualizarEstadoNiveles();
            }
        }

        private void OnOptionsClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (optionsPanel != null)
            {
                optionsPanel.gameObject.SetActive(true);
            }
        }

        private void OnCreditsClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (creditsPanel != null)
            {
                creditsPanel.gameObject.SetActive(true);
            }
        }

        private void OnQuitClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}
