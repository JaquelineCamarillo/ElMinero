using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del panel de resultados al completar un nivel (Sección 4).
    /// Calcula de 1 a 3 estrellas, anima contadores con rebote y SFX,
    /// guarda mejores récords en PlayerPrefs y ofrece navegación.
    /// </summary>
    public class ResultsPanel : MonoBehaviour
    {
        [Header("Títulos y Frases")]
        [SerializeField] private TextMeshProUGUI textoTitulo;
        [SerializeField] private TextMeshProUGUI textoFraseEstrellas;
        [SerializeField] private GameObject bannerNuevoDesbloqueo;
        [SerializeField] private TextMeshProUGUI textoNuevoDesbloqueo;

        [Header("Estrellas")]
        [SerializeField] private Image[] imagenesEstrellas; // 3 estrellas
        [SerializeField] private Sprite spriteEstrellaLlena;
        [SerializeField] private Sprite spriteEstrellaVacia;

        [Header("Estadísticas del Nivel")]
        [SerializeField] private TextMeshProUGUI textoGemas;
        [SerializeField] private TextMeshProUGUI textoPlata;
        [SerializeField] private TextMeshProUGUI textoTiempo;
        [SerializeField] private TextMeshProUGUI textoVidas;
        [SerializeField] private TextMeshProUGUI textoPuntajeNivel;
        [SerializeField] private TextMeshProUGUI textoPuntajeTotal;

        [Header("Botones")]
        [SerializeField] private Button botonSiguiente;
        [SerializeField] private TextMeshProUGUI textoBotonSiguiente;
        [SerializeField] private Button botonRepetir;
        [SerializeField] private Button botonMenu;

        private int estrellasGanadas = 1;
        private string escenaActual = "";
        private string escenaSiguiente = "";
        private int numeroNivelActual = 1;

        private void Awake()
        {
            if (botonSiguiente != null) botonSiguiente.onClick.AddListener(IrSiguienteNivel);
            if (botonRepetir != null) botonRepetir.onClick.AddListener(RepetirNivel);
            if (botonMenu != null) botonMenu.onClick.AddListener(IrAlMenu);
        }

        public void MostrarResultados(
            string nombreEscena,
            int numeroNivel,
            int gemasRecolectadas,
            int totalGemas,
            bool mineralDistintivoRecolectado,
            int plataRecolectada,
            int totalPlata,
            float tiempoRestante,
            float tiempoInicial,
            int vidasRestantes,
            int puntajeNivel)
        {
            escenaActual = nombreEscena;
            numeroNivelActual = numeroNivel;

            // Determinar siguiente escena
            if (numeroNivelActual == 1)
            {
                escenaSiguiente = "Nivel2_ElNopal";
                if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Siguiente nivel";
            }
            else if (numeroNivelActual == 2)
            {
                escenaSiguiente = "Nivel3_LaValenciana";
                if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Siguiente nivel";
            }
            else
            {
                escenaSiguiente = "Final";
                if (textoBotonSiguiente != null) textoBotonSiguiente.text = "Salir de la mina";
            }

            // Cálculo de Estrellas (Sección 4.1):
            // 1 estrella: completar nivel
            // 2 estrellas: completar + >= 50% gemas + >= 15% tiempo restante
            // 3 estrellas: completar + 100% gemas (incluyendo mineral distintivo) + >= 30% tiempo restante
            float ratioGemas = (totalGemas > 0) ? (float)gemasRecolectadas / totalGemas : 1f;
            float ratioTiempo = (tiempoInicial > 0f) ? (tiempoRestante / tiempoInicial) : 0f;

            estrellasGanadas = 1;

            bool cumpleGemas2 = ratioGemas >= 0.5f;
            bool cumpleTiempo2 = ratioTiempo >= 0.15f;
            if (cumpleGemas2 && cumpleTiempo2)
            {
                estrellasGanadas = 2;
            }

            bool cumpleGemas3 = (gemasRecolectadas >= totalGemas) && mineralDistintivoRecolectado;
            bool cumpleTiempo3 = ratioTiempo >= 0.30f;
            if (cumpleGemas3 && cumpleTiempo3)
            {
                estrellasGanadas = 3;
            }

            // Desbloqueo de nivel
            int desbloqueadoActual = PlayerPrefs.GetInt(SettingsManager.PREF_NIVEL_DESBLOQUEADO, 1);
            bool nuevoNivelDesbloqueado = false;
            if (numeroNivelActual < 3 && desbloqueadoActual <= numeroNivelActual)
            {
                PlayerPrefs.SetInt(SettingsManager.PREF_NIVEL_DESBLOQUEADO, numeroNivelActual + 1);
                nuevoNivelDesbloqueado = true;
            }

            // Guardar mejores récords
            int mejorPuntajePrevio = PlayerPrefs.GetInt($"MejorPuntaje_{nombreEscena}", 0);
            if (puntajeNivel > mejorPuntajePrevio)
            {
                PlayerPrefs.SetInt($"MejorPuntaje_{nombreEscena}", puntajeNivel);
            }

            float mejorTiempoPrevio = PlayerPrefs.GetFloat($"MejorTiempo_{nombreEscena}", 0f);
            if (tiempoRestante > mejorTiempoPrevio)
            {
                PlayerPrefs.SetFloat($"MejorTiempo_{nombreEscena}", tiempoRestante);
            }

            int estrellasPrevias = PlayerPrefs.GetInt($"Estrellas_{nombreEscena}", 0);
            if (estrellasGanadas > estrellasPrevias)
            {
                PlayerPrefs.SetInt($"Estrellas_{nombreEscena}", estrellasGanadas);
            }

            PlayerPrefs.Save();

            // Configurar textos
            if (textoGemas != null) textoGemas.text = $"Gemas: {gemasRecolectadas} / {totalGemas}";
            if (textoPlata != null)
            {
                bool bono = (totalPlata > 0 && plataRecolectada >= totalPlata);
                textoPlata.text = bono ? $"Plata: {plataRecolectada} / {totalPlata} <color=#FFD700>¡+50 BONO!</color>" : $"Plata: {plataRecolectada} / {totalPlata}";
            }
            if (textoTiempo != null) textoTiempo.text = $"Tiempo restante: {tiempoRestante:F1}s";
            if (textoVidas != null) textoVidas.text = $"Vidas restantes: {vidasRestantes} ♥";
            if (textoPuntajeNivel != null) textoPuntajeNivel.text = $"Puntos del nivel: {puntajeNivel:N0}";

            int p1 = PlayerPrefs.GetInt("MejorPuntaje_Nivel1_MinaRayas", 0);
            int p2 = PlayerPrefs.GetInt("MejorPuntaje_Nivel2_ElNopal", 0);
            int p3 = PlayerPrefs.GetInt("MejorPuntaje_Nivel3_LaValenciana", 0);
            if (textoPuntajeTotal != null) textoPuntajeTotal.text = $"Total acumulado: {(p1 + p2 + p3):N0}";

            if (bannerNuevoDesbloqueo != null)
            {
                bannerNuevoDesbloqueo.SetActive(nuevoNivelDesbloqueado);
                if (nuevoNivelDesbloqueado && textoNuevoDesbloqueo != null)
                {
                    textoNuevoDesbloqueo.text = $"¡Nivel {numeroNivelActual + 1} desbloqueado!";
                }
            }

            gameObject.SetActive(true);
            Time.timeScale = 0f;

            if (nuevoNivelDesbloqueado && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDesbloqueoNivel();
            }

            StartCoroutine(SecuenciaAnimacionEstrellas());
        }

        private IEnumerator SecuenciaAnimacionEstrellas()
        {
            // Inicializar estrellas vacías
            for (int i = 0; i < imagenesEstrellas.Length; i++)
            {
                if (imagenesEstrellas[i] != null)
                {
                    imagenesEstrellas[i].sprite = spriteEstrellaVacia;
                    imagenesEstrellas[i].transform.localScale = Vector3.one;
                }
            }

            if (textoFraseEstrellas != null) textoFraseEstrellas.text = "";

            // Revelación secuencial cada 0.4s
            for (int i = 0; i < estrellasGanadas && i < imagenesEstrellas.Length; i++)
            {
                yield return new WaitForSecondsRealtime(0.4f);

                if (imagenesEstrellas[i] != null)
                {
                    imagenesEstrellas[i].sprite = spriteEstrellaLlena;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayEstrella(i + 1);

                    // Pequeño efecto rebote
                    float elapsed = 0f;
                    float dur = 0.2f;
                    while (elapsed < dur)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float scale = Mathf.Lerp(1.5f, 1.0f, elapsed / dur);
                        imagenesEstrellas[i].transform.localScale = new Vector3(scale, scale, 1f);
                        yield return null;
                    }
                    imagenesEstrellas[i].transform.localScale = Vector3.one;
                }
            }

            yield return new WaitForSecondsRealtime(0.2f);

            // Frase según estrellas
            if (textoFraseEstrellas != null)
            {
                switch (estrellasGanadas)
                {
                    case 1: textoFraseEstrellas.text = "¡Escapaste por poco!"; break;
                    case 2: textoFraseEstrellas.text = "¡Buen trabajo, minero!"; break;
                    case 3: textoFraseEstrellas.text = "¡Minero legendario!"; break;
                    default: textoFraseEstrellas.text = "¡Bien hecho!"; break;
                }
            }
        }

        public void IrSiguienteNivel()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            SceneLoader.LoadScene(escenaSiguiente);
        }

        public void RepetirNivel()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            SceneLoader.ReloadCurrentScene();
        }

        public void IrAlMenu()
        {
            Time.timeScale = 1f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            SceneLoader.LoadScene("MenuPrincipal");
        }
    }
}
