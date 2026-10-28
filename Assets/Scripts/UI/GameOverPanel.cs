using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del panel de Game Over dentro de cada nivel (Sección 4.2).
    /// Muestra la causa de la derrota personalizada con el nombre del minero,
    /// estadísticas alcanzadas y opciones para reintentar o volver al menú.
    /// </summary>
    public class GameOverPanel : MonoBehaviour
    {
        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI textoTitulo;
        [SerializeField] private TextMeshProUGUI textoSubtitulo;
        [SerializeField] private TextMeshProUGUI textoMotivo;
        [SerializeField] private TextMeshProUGUI textoEstadisticas;

        [Header("Botones")]
        [SerializeField] private Button botonReintentar;
        [SerializeField] private Button botonMenu;

        private void Awake()
        {
            if (botonReintentar != null) botonReintentar.onClick.AddListener(Reintentar);
            if (botonMenu != null) botonMenu.onClick.AddListener(IrAlMenu);
        }

        public void MostrarGameOver(string motivo, int gemas, int plata, int puntos)
        {
            string nombre = PlayerPrefs.GetString("NombreJugador", "Refugio");
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Refugio";

            if (textoTitulo != null) textoTitulo.text = "¡EL DERRUMBE TE ALCANZÓ!";
            if (textoSubtitulo != null) textoSubtitulo.text = $"La mina ha atrapado a {nombre}…";
            if (textoMotivo != null) textoMotivo.text = motivo;
            if (textoEstadisticas != null)
            {
                textoEstadisticas.text = $"Plata: {plata}  |  Gemas: {gemas}  |  Puntos: {puntos:N0}";
            }

            gameObject.SetActive(true);
            Time.timeScale = 0f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicGameOver();
            }
        }

        public void Reintentar()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            SceneLoader.ReloadCurrentScene();
        }

        public void IrAlMenu()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            LevelStartCard.ReiniciarRegistroNiveles();
            SceneLoader.LoadScene("MenuPrincipal");
        }
    }
}
