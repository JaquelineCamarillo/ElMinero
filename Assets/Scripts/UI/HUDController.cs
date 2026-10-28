using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;
using ElMinero.Jugador;
using ElMinero.Coleccionables;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del HUD principal del juego en pantalla (1920x1080 match 0.5).
    /// Muestra vidas (corazones llenos/vacíos), reloj en cuenta regresiva (formato MM:SS y rojo en <= 10s),
    /// contador de vetas recolectadas, contador de gemas, icono del mineral distintivo (gris/color),
    /// puntaje total, insignia x2 animada y cartel emergente informativo de 3 s para el mineral distintivo.
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        [Header("Vidas (Corazones)")]
        [Tooltip("Imágenes de los 3 corazones en orden.")]
        [SerializeField] private Image[] heartImages;
        [SerializeField] private Sprite heartFullSprite;
        [SerializeField] private Sprite heartEmptySprite;

        [Header("Reloj y Tiempo")]
        [Tooltip("Texto TextMeshPro para el tiempo restante (formato MM:SS).")]
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private Color normalTimerColor = Color.white;
        [SerializeField] private Color urgentTimerColor = new Color(1f, 0.25f, 0.25f); // Rojo alerta
        [SerializeField] private float blinkSpeed = 4f;

        [Header("Coleccionables y Puntaje")]
        [Tooltip("Texto para el contador de vetas de plata (ej. '3 / 6').")]
        [SerializeField] private TextMeshProUGUI silverCountText;

        [Tooltip("Texto para el contador de gemas (ej. '1 / 2').")]
        [SerializeField] private TextMeshProUGUI gemCountText;

        [Tooltip("Icono del mineral distintivo (gris hasta encontrarlo, a color después).")]
        [SerializeField] private Image distinctiveMineralIcon;

        [Tooltip("Texto para el puntaje total acumulado.")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Insignia Racha x2")]
        [Tooltip("GameObject o contenedor de la insignia x2 animada.")]
        [SerializeField] private GameObject streakBadgeObject;

        [Tooltip("Barra de progreso o relleno de la duración del multiplicador.")]
        [SerializeField] private Image streakFillImage;

        [Tooltip("Texto de segundos restantes del multiplicador.")]
        [SerializeField] private TextMeshProUGUI streakTimerText;

        [Header("Cartel Emergente de Mineral Distintivo")]
        [SerializeField] private GameObject mineralPopupPanel;
        [SerializeField] private TextMeshProUGUI mineralPopupTitle;
        [SerializeField] private TextMeshProUGUI mineralPopupDescription;

        [Header("Controles Móviles")]
        [SerializeField] private GameObject touchControlsContainer;

        private bool isTimerUrgent = false;
        private Coroutine mineralPopupCoroutine;

        private void Start()
        {
            if (mineralPopupPanel != null) mineralPopupPanel.SetActive(false);

            // En móvil se muestran los controles táctiles, en PC se ocultan
            if (touchControlsContainer != null)
            {
                touchControlsContainer.SetActive(Application.isMobilePlatform);
            }

            // Inicializar icono de mineral distintivo en gris (0.35f)
            if (distinctiveMineralIcon != null)
            {
                distinctiveMineralIcon.color = new Color(0.35f, 0.35f, 0.35f, 0.7f);
            }

            ConectarConManagers();
        }

        private void ConectarConManagers()
        {
            if (LevelManager.Instance != null)
            {
                // Vidas
                if (LevelManager.Instance.Health != null)
                {
                    LevelManager.Instance.Health.OnHealthChanged += ActualizarCorazones;
                    ActualizarCorazones(LevelManager.Instance.Health.CurrentLives);
                }

                // Reloj
                if (LevelManager.Instance.Timer != null)
                {
                    LevelManager.Instance.Timer.OnTimeChanged += ActualizarTiempo;
                    LevelManager.Instance.Timer.OnUrgentStateChanged += EstablecerEstadoUrgente;
                }

                // Coleccionables
                if (LevelManager.Instance.Collectibles != null)
                {
                    LevelManager.Instance.Collectibles.OnSilverUpdated += ActualizarContadorPlata;
                    LevelManager.Instance.Collectibles.OnGemsUpdated += ActualizarContadorGemas;
                }

                // Puntaje
                LevelManager.Instance.OnScoreUpdated += ActualizarPuntaje;
                if (GameManager.Instance != null)
                {
                    ActualizarPuntaje(GameManager.Instance.CombinedScore);
                }

                // Racha x2
                if (LevelManager.Instance.Streak != null)
                {
                    LevelManager.Instance.Streak.OnMultiplierStateChanged += ManejarEstadoMultiplicador;
                    LevelManager.Instance.Streak.OnMultiplierTimerUpdated += ActualizarBarraMultiplicador;
                }
            }
        }

        private void Update()
        {
            if (isTimerUrgent && timerText != null)
            {
                float alpha = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
                timerText.color = Color.Lerp(normalTimerColor, urgentTimerColor, alpha);
            }
        }

        public void ActualizarCorazones(int vidasRestantes)
        {
            if (heartImages == null) return;

            for (int i = 0; i < heartImages.Length; i++)
            {
                if (heartImages[i] != null)
                {
                    heartImages[i].sprite = (i < vidasRestantes) ? heartFullSprite : heartEmptySprite;
                }
            }
        }

        public void ActualizarTiempo(float segundosRestantes)
        {
            if (timerText == null) return;

            int minutos = Mathf.FloorToInt(segundosRestantes / 60f);
            int segundos = Mathf.FloorToInt(segundosRestantes % 60f);
            timerText.text = $"{minutos:00}:{segundos:00}";
        }

        public void EstablecerEstadoUrgente(bool urgente)
        {
            isTimerUrgent = urgente;
            if (!urgente && timerText != null)
            {
                timerText.color = normalTimerColor;
            }
        }

        public void ActualizarContadorPlata(int recolectadas, int total)
        {
            if (silverCountText != null)
            {
                silverCountText.text = $"{recolectadas} / {total}";
            }
        }

        public void ActualizarContadorGemas(int recolectadas, int total)
        {
            if (gemCountText != null)
            {
                gemCountText.text = $"{recolectadas} / {total}";
            }
        }

        public void ActualizarPuntaje(int puntajeTotal)
        {
            if (scoreText != null)
            {
                scoreText.text = $"{puntajeTotal} pts";
            }
        }

        public void ManejarEstadoMultiplicador(bool activo, float duracionTotal)
        {
            if (streakBadgeObject != null)
            {
                streakBadgeObject.SetActive(activo);
            }
        }

        public void ActualizarBarraMultiplicador(float tiempoRestante, float duracionTotal)
        {
            if (streakFillImage != null && duracionTotal > 0f)
            {
                streakFillImage.fillAmount = Mathf.Clamp01(tiempoRestante / duracionTotal);
            }

            if (streakTimerText != null)
            {
                streakTimerText.text = $"{tiempoRestante:0.0}s";
            }
        }

        /// <summary>
        /// Muestra el cartel emergente de 3 segundos con información del mineral distintivo sin pausar el juego.
        /// </summary>
        public void MostrarNotificacionMineral(string nombre, string descripcion)
        {
            // Encender el icono del mineral a todo color
            if (distinctiveMineralIcon != null)
            {
                distinctiveMineralIcon.color = Color.white;
            }

            if (mineralPopupPanel == null) return;

            if (mineralPopupCoroutine != null)
            {
                StopCoroutine(mineralPopupCoroutine);
            }

            mineralPopupCoroutine = StartCoroutine(RutinaNotificacionMineral(nombre, descripcion));
        }

        private IEnumerator RutinaNotificacionMineral(string nombre, string descripcion)
        {
            if (mineralPopupTitle != null) mineralPopupTitle.text = $"¡{nombre} encontrado!";
            if (mineralPopupDescription != null) mineralPopupDescription.text = descripcion;

            mineralPopupPanel.SetActive(true);

            yield return new WaitForSeconds(3.0f);

            mineralPopupPanel.SetActive(false);
            mineralPopupCoroutine = null;
        }
    }
}
