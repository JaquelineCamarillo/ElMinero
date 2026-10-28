using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controla la pantalla de Victoria al escapar con éxito de las minas tras el Nivel 3.
    /// Muestra felicitación personalizada, puntaje final, récord y plata total.
    /// </summary>
    public class VictoryScreen : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;
        [SerializeField] private TextMeshProUGUI silverText;

        [Header("Botones")]
        [SerializeField] private Button playAgainButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            string nombre = GameManager.Instance != null ? GameManager.Instance.PlayerName : "Refugio";
            int finalScore = GameManager.Instance != null ? GameManager.Instance.TotalScore : 0;
            int highScore = GameManager.Instance != null ? GameManager.Instance.HighScore : 0;
            int totalSilver = GameManager.Instance != null ? GameManager.Instance.TotalSilverCollected : 0;

            if (titleText != null)
            {
                titleText.text = $"¡Lo lograste, {nombre}!";
            }

            if (scoreText != null)
            {
                scoreText.text = $"Puntaje Final: {finalScore} pts";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"Mejor Récord: {highScore} pts";
            }

            if (silverText != null)
            {
                silverText.text = $"Plata Total Recolectada: {totalSilver}";
            }

            if (playAgainButton != null)
            {
                playAgainButton.onClick.AddListener(() =>
                {
                    if (GameManager.Instance != null) GameManager.Instance.ResetScore();
                    SceneLoader.LoadScene("Nivel1_MinaRayas");
                });
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(() =>
                {
                    SceneLoader.LoadScene("MenuPrincipal");
                });
            }
        }
    }
}
