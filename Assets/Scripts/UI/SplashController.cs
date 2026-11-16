using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Pantalla de carga al abrir el juego (escena Splash).
    /// Carga el Menú Principal en segundo plano mientras muestra el logo, una barra
    /// de progreso sobre la que el minero empuja su vagoneta, el porcentaje, consejos
    /// que van cambiando y polvo flotando sobre el fondo de la mina.
    /// La barra dura al menos <see cref="duracionMinima"/> segundos para que se alcance a ver.
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("Escena a cargar")]
        [SerializeField] private string escenaSiguiente = "MenuPrincipal";
        [SerializeField] private float duracionMinima = 3.5f;

        [Header("Barra")]
        [SerializeField] private Image barraRelleno;
        [SerializeField] private RectTransform zonaBarra;
        [SerializeField] private RectTransform minero;
        [SerializeField] private Image imagenMinero;
        [SerializeField] private Sprite[] cuadrosMinero;
        [SerializeField] private RectTransform vagoneta;

        [Header("Textos")]
        [SerializeField] private TextMeshProUGUI textoCargando;
        [SerializeField] private TextMeshProUGUI textoPorcentaje;
        [SerializeField] private TextMeshProUGUI textoConsejo;
        [TextArea]
        [SerializeField] private string[] consejos =
        {
            "Consejo: junta vetas de plata seguidas para activar la racha x2.",
            "Consejo: cuando la mina tiembla, viene un derrumbe. ¡No te detengas!",
            "Consejo: las capillas son checkpoints: ahí vuelves si pierdes una vida.",
            "Consejo: los murciélagos vuelan en patrón. Espera tu momento y pasa.",
            "Dato: La Valenciana fue una de las minas de plata más ricas del mundo."
        };

        [Header("Logo, polvo y fundidos")]
        [SerializeField] private RectTransform logo;
        [SerializeField] private CanvasGroup grupoContenido;
        [SerializeField] private Image fundidoNegro;
        [SerializeField] private Sprite spriteMota;
        [SerializeField] private RectTransform zonaMotas;

        private AsyncOperation operacion;
        private float inicio;
        private float mostrado;
        private bool terminando;
        private float finTerminando;
        private int consejoActual;
        private float proximoConsejo;
        private Vector2 posLogo;
        private readonly List<RectTransform> motas = new List<RectTransform>();
        private readonly List<Vector3> datosMotas = new List<Vector3>();

        private void Start()
        {
            Time.timeScale = 1f;
            inicio = Time.unscaledTime;
            if (logo != null) posLogo = logo.anchoredPosition;
            if (grupoContenido != null) grupoContenido.alpha = 0f;
            if (fundidoNegro != null) fundidoNegro.color = new Color(0f, 0f, 0f, 0f);
            if (barraRelleno != null) barraRelleno.enabled = false;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicMenu();

            if (consejos != null && consejos.Length > 0)
            {
                consejoActual = Random.Range(0, consejos.Length);
                if (textoConsejo != null) textoConsejo.text = consejos[consejoActual];
            }
            proximoConsejo = Time.unscaledTime + 3.2f;

            CrearMotas();

            operacion = SceneManager.LoadSceneAsync(escenaSiguiente);
            if (operacion != null) operacion.allowSceneActivation = false;
        }

        private void CrearMotas()
        {
            if (spriteMota == null || zonaMotas == null) return;
            for (int i = 0; i < 26; i++)
            {
                GameObject go = new GameObject("Mota");
                go.transform.SetParent(zonaMotas, false);
                var rt = go.AddComponent<RectTransform>();
                float tam = Random.Range(6f, 12f);
                rt.sizeDelta = new Vector2(tam, tam);
                rt.anchoredPosition = new Vector2(Random.Range(-960f, 960f), Random.Range(-540f, 540f));
                var img = go.AddComponent<Image>();
                img.sprite = spriteMota;
                img.raycastTarget = false;
                img.color = new Color(1f, 0.85f, 0.6f, 0f);
                motas.Add(rt);
                datosMotas.Add(new Vector3(Random.Range(0f, 100f), Random.Range(18f, 45f), Random.Range(0.2f, 0.6f)));
            }
        }

        private void Update()
        {
            float t = Time.unscaledTime - inicio;
            float dt = Time.unscaledDeltaTime;

            // Aparece el contenido
            if (grupoContenido != null) grupoContenido.alpha = Mathf.Clamp01(t / 0.6f);

            // Progreso: el menor entre la carga real y el tiempo mínimo, suavizado
            float real = operacion != null ? Mathf.Clamp01(operacion.progress / 0.9f) : 1f;
            float porTiempo = Mathf.Clamp01(t / duracionMinima);
            float objetivo = Mathf.Min(real, porTiempo);
            mostrado = Mathf.MoveTowards(mostrado, objetivo, dt * 0.9f);
            float visual = mostrado;

            if (barraRelleno != null)
            {
                // El relleno se estira (9-slice) en lugar de recortarse: así conserva sus orillas
                RectTransform rt = barraRelleno.rectTransform;
                rt.anchorMax = new Vector2(visual, 1f);
                barraRelleno.enabled = visual > 0.025f;
            }
            if (textoPorcentaje != null) textoPorcentaje.text = Mathf.RoundToInt(visual * 100f) + "%";

            // Minero empujando la vagoneta sobre la barra
            if (zonaBarra != null)
            {
                float ancho = zonaBarra.rect.width;
                float x = -ancho * 0.5f + ancho * visual;
                if (vagoneta != null) vagoneta.anchoredPosition = new Vector2(x + 6f, vagoneta.anchoredPosition.y);
                if (minero != null) minero.anchoredPosition = new Vector2(x - 66f, minero.anchoredPosition.y);
            }
            if (imagenMinero != null && cuadrosMinero != null && cuadrosMinero.Length > 0)
            {
                bool caminando = !terminando && visual < 0.999f;
                int f = caminando ? Mathf.FloorToInt(t * 10f) % cuadrosMinero.Length : 0;
                imagenMinero.sprite = cuadrosMinero[f];
            }

            // Texto "CARGANDO..." con puntos animados
            if (textoCargando != null && !terminando)
            {
                int puntos = Mathf.FloorToInt(t * 2.5f) % 4;
                textoCargando.text = "CARGANDO" + new string('.', puntos);
            }

            // Consejos que cambian
            if (textoConsejo != null && consejos != null && consejos.Length > 1)
            {
                float resto = proximoConsejo - Time.unscaledTime;
                float alfa = resto < 0.3f ? Mathf.Clamp01(resto / 0.3f) : 1f;
                if (resto <= 0f)
                {
                    consejoActual = (consejoActual + 1) % consejos.Length;
                    textoConsejo.text = consejos[consejoActual];
                    proximoConsejo = Time.unscaledTime + 3.2f;
                }
                float desde = 3.2f - (proximoConsejo - Time.unscaledTime);
                if (desde < 0.3f) alfa = Mathf.Clamp01(desde / 0.3f);
                Color c = textoConsejo.color;
                textoConsejo.color = new Color(c.r, c.g, c.b, alfa);
            }

            // Logo que respira
            if (logo != null)
            {
                logo.anchoredPosition = posLogo + new Vector2(0f, Mathf.Sin(t * 2f) * 6f);
            }

            // Polvo que sube despacio
            for (int i = 0; i < motas.Count; i++)
            {
                Vector3 d = datosMotas[i];
                Vector2 p = motas[i].anchoredPosition;
                p.y += d.y * dt;
                p.x += (Mathf.PerlinNoise(d.x, t * 0.3f) - 0.5f) * 30f * dt;
                if (p.y > 560f) p = new Vector2(Random.Range(-960f, 960f), -560f);
                motas[i].anchoredPosition = p;
                var img = motas[i].GetComponent<Image>();
                img.color = new Color(1f, 0.85f, 0.6f, d.z * Mathf.PerlinNoise(d.x + 30f, t * 0.6f));
            }

            // Final: "¡LISTO!", fundido a negro y se activa el menú
            if (!terminando && visual >= 0.999f && real >= 1f)
            {
                terminando = true;
                finTerminando = Time.unscaledTime + 0.9f;
                if (textoCargando != null) textoCargando.text = "¡LISTO!";
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            }
            if (terminando)
            {
                float k = Mathf.Clamp01(1f - (finTerminando - Time.unscaledTime) / 0.5f);
                if (Time.unscaledTime > finTerminando - 0.5f && fundidoNegro != null)
                {
                    fundidoNegro.color = new Color(0f, 0f, 0f, k);
                }
                if (Time.unscaledTime >= finTerminando && operacion != null && !operacion.allowSceneActivation)
                {
                    operacion.allowSceneActivation = true;
                }
                else if (operacion == null && Time.unscaledTime >= finTerminando)
                {
                    SceneLoader.LoadScene(escenaSiguiente);
                    enabled = false;
                }
            }
        }
    }
}
