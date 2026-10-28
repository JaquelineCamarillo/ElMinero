using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador de la escena Final (Sección 3.7).
    /// Muestra al minero en la superficie frente al Templo de San Cayetano,
    /// texto narrativo con el nombre personalizado, resumen acumulado de los 3 niveles,
    /// minerales distintivos encontrados, música de victoria y créditos desplazables.
    /// </summary>
    public class FinalController : MonoBehaviour
    {
        [Header("Texto Narrativo")]
        [SerializeField] private TextMeshProUGUI textoNarrativo;

        [Header("Estadísticas Totales")]
        [SerializeField] private TextMeshProUGUI textoPuntajeTotal;
        [SerializeField] private TextMeshProUGUI textoEstrellasTotales;
        [SerializeField] private TextMeshProUGUI textoMineralesTotales;

        [Header("Iconos de Minerales Distintivos")]
        [SerializeField] private Image iconoAguilarita;
        [SerializeField] private Image iconoPepitaOro;
        [SerializeField] private Image iconoPirargirita;

        [Header("Créditos y Navegación")]
        [SerializeField] private RectTransform contenedorCreditos;
        [SerializeField] private float velocidadCreditos = 22f;
        [SerializeField] private Button botonMenuPrincipal;

        private void Start()
        {
            Time.timeScale = 1f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicVictoria();
            }

            ConfigurarNarrativaYStats();

            if (botonMenuPrincipal != null)
            {
                botonMenuPrincipal.onClick.AddListener(VolverAlMenu);
            }
        }

        private void Update()
        {
            if (contenedorCreditos != null)
            {
                contenedorCreditos.anchoredPosition += new Vector2(0f, velocidadCreditos * Time.deltaTime);
            }
        }

        private void ConfigurarNarrativaYStats()
        {
            string nombre = PlayerPrefs.GetString("NombreJugador", "Refugio");
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Refugio";

            if (textoNarrativo != null)
            {
                textoNarrativo.text = $"<b>{nombre}</b> sale a la luz junto al Templo de San Cayetano. Detrás, la mina se cierra para siempre… pero la plata que llevas cuenta otra historia.";
            }

            int p1 = PlayerPrefs.GetInt("MejorPuntaje_Nivel1_MinaRayas", 0);
            int p2 = PlayerPrefs.GetInt("MejorPuntaje_Nivel2_ElNopal", 0);
            int p3 = PlayerPrefs.GetInt("MejorPuntaje_Nivel3_LaValenciana", 0);
            int puntajeTotal = p1 + p2 + p3;

            int e1 = PlayerPrefs.GetInt("Estrellas_Nivel1_MinaRayas", 0);
            int e2 = PlayerPrefs.GetInt("Estrellas_Nivel2_ElNopal", 0);
            int e3 = PlayerPrefs.GetInt("Estrellas_Nivel3_LaValenciana", 0);
            int estrellasTotal = e1 + e2 + e3;

            bool mineral1 = PlayerPrefs.GetInt("Mineral_Nivel1_MinaRayas", 0) == 1;
            bool mineral2 = PlayerPrefs.GetInt("Mineral_Nivel2_ElNopal", 0) == 1;
            bool mineral3 = PlayerPrefs.GetInt("Mineral_Nivel3_LaValenciana", 0) == 1;
            int mineralesTotal = (mineral1 ? 1 : 0) + (mineral2 ? 1 : 0) + (mineral3 ? 1 : 0);

            if (textoPuntajeTotal != null) textoPuntajeTotal.text = $"PUNTAJE TOTAL: {puntajeTotal:N0} pts";
            if (textoEstrellasTotales != null) textoEstrellasTotales.text = $"ESTRELLAS: {estrellasTotal} / 9 ★";
            if (textoMineralesTotales != null) textoMineralesTotales.text = $"MINERALES HISTÓRICOS: {mineralesTotal} / 3";

            Color colorApagado = new Color(0.25f, 0.25f, 0.25f, 0.5f);
            Color colorActivo = Color.white;

            if (iconoAguilarita != null) iconoAguilarita.color = mineral1 ? colorActivo : colorApagado;
            if (iconoPepitaOro != null) iconoPepitaOro.color = mineral2 ? colorActivo : colorApagado;
            if (iconoPirargirita != null) iconoPirargirita.color = mineral3 ? colorActivo : colorApagado;
        }

        public void VolverAlMenu()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            LevelStartCard.ReiniciarRegistroNiveles();
            SceneLoader.LoadScene("MenuPrincipal");
        }
    }
}
