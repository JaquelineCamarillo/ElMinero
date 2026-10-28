using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;
using ElMinero.Jugador;
using ElMinero.Coleccionables;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del HUD principal del juego en pantalla.
    /// Muestra vidas (corazones llenos/vacíos), reloj en cuenta regresiva (formato MM:SS y rojo en <= 10s),
    /// contador de vetas recolectadas, puntaje total e insignia x2 con tiempo de racha restante.
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
        [Tooltip("Texto para el contador de vetas de plata (ej. '🪙 3 / 5').")]
        [SerializeField] private TextMeshProUGUI silverCountText;

        [Tooltip("Texto para el puntaje total acumulado.")]
        [SerializeField] private TextMeshProUGUI scoreText;

        [Header("Insignia Racha x2")]
        [Tooltip("GameObject o contenedor de la insignia x2 animada.")]
        [SerializeField] private GameObject streakBadgeObject;

        [Tooltip("Barra de progreso o relleno de la duración del multiplicador.")]
        [SerializeField] private Image streakFillImage;

        [Tooltip("Texto de segundos restantes del multiplicador.")]
        [SerializeField] private TextMeshProUGUI streakTimerText;

        private bool isTimerUrgent = false;

        private void Start()
        {
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

                // Timer
                if (LevelManager.Instance.Timer != null)
                {
                    LevelManager.Instance.Timer.OnTimeChanged += ActualizarTiempo;
                    LevelManager.Instance.Timer.OnUrgentStateChanged += EstablecerEstadoUrgente;
                    ActualizarTiempo(LevelManager.Instance.Timer.RemainingTime);
                }

                // Coleccionables
                if (LevelManager.Instance.Collectibles != null)
                {
                    LevelManager.Instance.Collectibles.OnSilverUpdated += ActualizarContadorPlata;
                    ActualizarContadorPlata(LevelManager.Instance.Collectibles.CollectedSilver, LevelManager.Instance.Collectibles.TotalSilverInLevel);
                }

                // Racha x2
                if (LevelManager.Instance.Streak != null)
                {
                    LevelManager.Instance.Streak.OnMultiplierStateChanged += ManejarEstadoMultiplicador;
                    LevelManager.Instance.Streak.OnMultiplierTimerUpdated += ActualizarBarraMultiplicador;
                    ManejarEstadoMultiplicador(LevelManager.Instance.Streak.IsMultiplierActive, 0f);
                }

                // Puntaje
                LevelManager.Instance.OnScoreUpdated += ActualizarPuntaje;
                int puntajeInicial = GameManager.Instance != null ? GameManager.Instance.CombinedScore : 0;
                ActualizarPuntaje(puntajeInicial);
            }
        }

        private void Update()
        {
            // Efecto de parpadeo del reloj en tiempo crítico (últimos 10 segundos)
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
    }
}
