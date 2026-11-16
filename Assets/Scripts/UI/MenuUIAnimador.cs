using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace ElMinero.UI
{
    /// <summary>
    /// Animaciones de la interfaz del Menú Principal: fundido desde negro,
    /// caída con rebote del logo (y un vaivén suave después), el letrero de
    /// madera que sube desde el suelo, botones que aparecen uno tras otro,
    /// antorchas encendidas y el récord del jugador en la barra inferior.
    /// </summary>
    public class MenuUIAnimador : MonoBehaviour
    {
        [Header("Fundido")]
        [SerializeField] private CanvasGroup fundidoNegro;
        [SerializeField] private float duracionFundido = 1.1f;

        [Header("Logo")]
        [SerializeField] private RectTransform logo;
        [SerializeField] private Image brilloLogo;
        [SerializeField] private float retrasoLogo = 0.25f;

        [Header("Letrero de opciones")]
        [SerializeField] private RectTransform letrero;
        [SerializeField] private float retrasoLetrero = 0.55f;
        [SerializeField] private CanvasGroup[] elementosLetrero = new CanvasGroup[0];

        [Header("Antorchas del letrero")]
        [SerializeField] private Image[] antorchas = new Image[0];
        [SerializeField] private Sprite[] cuadrosAntorcha = new Sprite[0];
        [SerializeField] private Image[] resplandores = new Image[0];

        [Header("Barra inferior")]
        [SerializeField] private TextMeshProUGUI textoRecord;

        [Header("Navegación")]
        [SerializeField] private GameObject primerSeleccionado;

        private Vector2 posLogo, posLetrero;
        private float inicio;
        private bool seleccionHecha;

        private static readonly string[] Escenas = { "Nivel1_MinaRayas", "Nivel2_ElNopal", "Nivel3_LaValenciana", "Final_Templo" };

        private void Start()
        {
            inicio = Time.unscaledTime;
            if (logo != null) posLogo = logo.anchoredPosition;
            if (letrero != null) posLetrero = letrero.anchoredPosition;
            if (fundidoNegro != null)
            {
                fundidoNegro.alpha = 1f;
                fundidoNegro.blocksRaycasts = false;
                fundidoNegro.interactable = false;
            }
            foreach (var cg in elementosLetrero) if (cg != null) cg.alpha = 0f;
            ActualizarRecord();
            Update();
        }

        private void ActualizarRecord()
        {
            if (textoRecord == null) return;
            int mejor = 0;
            foreach (string e in Escenas) mejor = Mathf.Max(mejor, PlayerPrefs.GetInt("MejorPuntaje_" + e, 0));
            textoRecord.text = mejor > 0 ? "MEJOR PUNTAJE: " + mejor.ToString("N0") : "EL TESORO TE ESPERA";
        }

        private void Update()
        {
            float t = Time.unscaledTime - inicio;

            // 1. Fundido desde negro
            if (fundidoNegro != null)
            {
                float a = 1f - Mathf.Clamp01(t / duracionFundido);
                fundidoNegro.alpha = a * a;
                if (a <= 0f && fundidoNegro.gameObject.activeSelf) fundidoNegro.gameObject.SetActive(false);
            }

            // 2. Logo: cae con rebote y después se mece suavemente
            if (logo != null)
            {
                float k = Mathf.Clamp01((t - retrasoLogo) / 0.9f);
                float caida = (1f - EaseOutBounce(k)) * 420f;
                float vaiven = k >= 1f ? Mathf.Sin((t - retrasoLogo - 0.9f) * 2.1f) * 6f : 0f;
                logo.anchoredPosition = posLogo + new Vector2(0f, caida + vaiven);
            }
            if (brilloLogo != null)
            {
                Color c = brilloLogo.color;
                float aparicion = Mathf.Clamp01((t - retrasoLogo - 0.6f) / 0.8f);
                c.a = aparicion * (0.2f + 0.08f * Mathf.Sin(t * 1.7f));
                brilloLogo.color = c;
            }

            // 3. Letrero: sube desde abajo con un pequeño rebote
            if (letrero != null)
            {
                float k = Mathf.Clamp01((t - retrasoLetrero) / 0.75f);
                float sube = (1f - EaseOutBack(k)) * -900f;
                letrero.anchoredPosition = posLetrero + new Vector2(0f, sube);
            }

            // 4. Elementos del letrero aparecen uno tras otro
            for (int i = 0; i < elementosLetrero.Length; i++)
            {
                if (elementosLetrero[i] == null) continue;
                float inicioElem = retrasoLetrero + 0.45f + i * 0.08f;
                elementosLetrero[i].alpha = Mathf.Clamp01((t - inicioElem) / 0.25f);
            }

            // 5. Antorchas y su resplandor
            if (cuadrosAntorcha != null && cuadrosAntorcha.Length > 0)
            {
                for (int i = 0; i < antorchas.Length; i++)
                {
                    if (antorchas[i] == null) continue;
                    int f = Mathf.FloorToInt(t * 8f + i * 1.5f) % cuadrosAntorcha.Length;
                    antorchas[i].sprite = cuadrosAntorcha[f];
                }
            }
            for (int i = 0; i < resplandores.Length; i++)
            {
                if (resplandores[i] == null) continue;
                float ruido = Mathf.PerlinNoise(i * 13.1f, t * 3.5f);
                Color c = resplandores[i].color;
                c.a = 0.22f + ruido * 0.22f;
                resplandores[i].color = c;
                float e = 0.92f + ruido * 0.16f;
                resplandores[i].rectTransform.localScale = new Vector3(e, e, 1f);
            }

            // 6. Foco inicial para teclado/control cuando el letrero ya llegó
            if (!seleccionHecha && t > retrasoLetrero + 1.1f)
            {
                seleccionHecha = true;
                var es = EventSystem.current;
                if (es != null && primerSeleccionado != null && es.currentSelectedGameObject == null && primerSeleccionado.activeInHierarchy)
                {
                    es.SetSelectedGameObject(primerSeleccionado);
                }
            }
        }

        private static float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(x - 1f, 3f) + c1 * Mathf.Pow(x - 1f, 2f);
        }

        private static float EaseOutBounce(float x)
        {
            const float n1 = 7.5625f;
            const float d1 = 2.75f;
            if (x < 1f / d1) return n1 * x * x;
            if (x < 2f / d1) { x -= 1.5f / d1; return n1 * x * x + 0.75f; }
            if (x < 2.5f / d1) { x -= 2.25f / d1; return n1 * x * x + 0.9375f; }
            x -= 2.625f / d1;
            return n1 * x * x + 0.984375f;
        }
    }
}
