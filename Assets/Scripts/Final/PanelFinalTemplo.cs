using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;
using ElMinero.UI;

namespace ElMinero.Final
{
    /// <summary>
    /// Panel final de la escena Final_Templo: "¡{nombre} es libre!", texto de cierre,
    /// resumen (puntaje total, estrellas / 9, minerales distintivos / 3 con iconos y Coloso vencido)
    /// y botones Créditos y Menú. Los créditos se desplazan sobre FondoFinal_Completo.
    /// </summary>
    public class PanelFinalTemplo : MonoBehaviour
    {
        [Header("Panel final")]
        [SerializeField] private CanvasGroup grupo;
        [SerializeField] private TextMeshProUGUI textoTitulo;
        [SerializeField] private TextMeshProUGUI textoCuerpo;
        [SerializeField] private TextMeshProUGUI textoPuntaje;
        [SerializeField] private TextMeshProUGUI textoEstrellas;
        [SerializeField] private TextMeshProUGUI textoMinerales;
        [SerializeField] private TextMeshProUGUI textoColoso;
        [SerializeField] private Image iconoAguilarita;
        [SerializeField] private Image iconoOro;
        [SerializeField] private Image iconoPirargirita;
        [SerializeField] private Button botonCreditos;
        [SerializeField] private Button botonMenu;

        [Header("Créditos")]
        [SerializeField] private GameObject panelCreditos;
        [SerializeField] private RectTransform contenidoCreditos;
        [SerializeField] private Button botonMenuCreditos;
        [SerializeField] private float velocidadCreditos = 60f;

        private bool creditosActivos;
        private float inicioCreditosY;

        private void Awake()
        {
            if (grupo == null) grupo = GetComponent<CanvasGroup>();
            if (grupo != null) { grupo.alpha = 0f; grupo.interactable = false; grupo.blocksRaycasts = false; }
            if (panelCreditos != null) panelCreditos.SetActive(false);
            if (contenidoCreditos != null) inicioCreditosY = contenidoCreditos.anchoredPosition.y;

            if (botonCreditos != null) botonCreditos.onClick.AddListener(MostrarCreditos);
            if (botonMenu != null) botonMenu.onClick.AddListener(IrAlMenu);
            if (botonMenuCreditos != null) botonMenuCreditos.onClick.AddListener(IrAlMenu);
        }

        public void Mostrar()
        {
            string nombre = PlayerPrefs.GetString("NombreJugador", "Refugio");
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Refugio";

            if (textoTitulo != null) textoTitulo.text = $"¡{nombre} es libre!";
            if (textoCuerpo != null)
            {
                textoCuerpo.text = $"{nombre} salió de la mina con las joyas del Coloso y las repartió entre la gente de La Valenciana. Las campanas repican, los cohetes iluminan el Templo de San Cayetano y el pueblo celebra: hoy nadie se quedó sin su parte del tesoro.";
            }

            int puntaje = PlayerPrefs.GetInt("MejorPuntaje_Nivel1_MinaRayas", 0)
                        + PlayerPrefs.GetInt("MejorPuntaje_Nivel2_ElNopal", 0)
                        + PlayerPrefs.GetInt("MejorPuntaje_Nivel3_LaValenciana", 0);
            int estrellas = PlayerPrefs.GetInt("Estrellas_Nivel1_MinaRayas", 0)
                          + PlayerPrefs.GetInt("Estrellas_Nivel2_ElNopal", 0)
                          + PlayerPrefs.GetInt("Estrellas_Nivel3_LaValenciana", 0);
            bool m1 = PlayerPrefs.GetInt("Mineral_Nivel1_MinaRayas", 0) == 1;
            bool m2 = PlayerPrefs.GetInt("Mineral_Nivel2_ElNopal", 0) == 1;
            bool m3 = PlayerPrefs.GetInt("Mineral_Nivel3_LaValenciana", 0) == 1;
            int minerales = (m1 ? 1 : 0) + (m2 ? 1 : 0) + (m3 ? 1 : 0);
            bool coloso = PlayerPrefs.GetInt("JefeVencido_N3", 0) == 1;

            if (textoPuntaje != null) textoPuntaje.text = $"Puntaje total: {puntaje:N0}";
            if (textoEstrellas != null) textoEstrellas.text = $"Estrellas: {estrellas} / 9";
            if (textoMinerales != null) textoMinerales.text = $"Minerales distintivos: {minerales} / 3";
            if (textoColoso != null) textoColoso.text = coloso ? "Coloso vencido: sí" : "Coloso vencido: no";

            Color apagado = new Color(0.25f, 0.25f, 0.25f, 0.55f);
            if (iconoAguilarita != null) iconoAguilarita.color = m1 ? Color.white : apagado;
            if (iconoOro != null) iconoOro.color = m2 ? Color.white : apagado;
            if (iconoPirargirita != null) iconoPirargirita.color = m3 ? Color.white : apagado;

            gameObject.SetActive(true);
            StartCoroutine(FundirEntrada());
        }

        private IEnumerator FundirEntrada()
        {
            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.unscaledDeltaTime;
                if (grupo != null) grupo.alpha = t / 0.6f;
                yield return null;
            }
            if (grupo != null) { grupo.alpha = 1f; grupo.interactable = true; grupo.blocksRaycasts = true; }
            if (botonCreditos != null) botonCreditos.Select();
        }

        private void MostrarCreditos()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (panelCreditos == null) return;
            panelCreditos.SetActive(true);
            panelCreditos.transform.SetAsLastSibling();
            if (contenidoCreditos != null)
            {
                contenidoCreditos.anchoredPosition = new Vector2(contenidoCreditos.anchoredPosition.x, inicioCreditosY);
            }
            creditosActivos = true;
            if (botonMenuCreditos != null) botonMenuCreditos.Select();
        }

        private void Update()
        {
            if (!creditosActivos || contenidoCreditos == null) return;
            contenidoCreditos.anchoredPosition += new Vector2(0f, velocidadCreditos * Time.unscaledDeltaTime);

            // Cuando el texto termina de pasar, vuelve a empezar
            float alto = contenidoCreditos.rect.height;
            if (contenidoCreditos.anchoredPosition.y > inicioCreditosY + alto + 1200f)
            {
                contenidoCreditos.anchoredPosition = new Vector2(contenidoCreditos.anchoredPosition.x, inicioCreditosY);
            }
        }

        private void IrAlMenu()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayClickUI();
                AudioManager.Instance.PlayMusicMenu();
            }
            LevelStartCard.ReiniciarRegistroNiveles();
            SceneLoader.LoadScene(ElMinero.Utilidades.Constantes.Escenas.MenuPrincipal);
        }
    }
}
