using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controla la pantalla de Derrota cuando el minero se queda sin vidas o se acaba el tiempo.
    /// Muestra mensaje personalizado, motivo de la derrota y opciones para reintentar.
    /// </summary>
    public class DefeatScreen : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI reasonText;

        [Header("Botones")]
        [SerializeField] private Button retryButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            string nombre = GameManager.Instance != null ? GameManager.Instance.PlayerName : "Refugio";
            string motivo = GameManager.Instance != null ? GameManager.Instance.LastDefeatReason : "El tiempo se ha agotado...";
            string failedScene = GameManager.Instance != null ? GameManager.Instance.LastLevelSceneName : "Nivel1_MinaRayas";

            if (titleText != null)
            {
                titleText.text = $"El derrumbe alcanzó a {nombre}...";
            }

            if (reasonText != null)
            {
                reasonText.text = motivo;
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(() =>
                {
                    SceneLoader.LoadScene(failedScene);
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
