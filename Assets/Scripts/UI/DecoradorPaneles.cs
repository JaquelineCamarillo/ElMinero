using UnityEngine;
using UnityEngine.UI;

namespace ElMinero.UI
{
    /// <summary>
    /// Da el mismo acabado a todos los paneles de madera del juego (historia de cada nivel,
    /// nivel completado, game over, pausa, opciones, créditos, niveles, panel final...):
    /// - Detrás del contenido: textura de roca, resplandor dorado arriba y un marco
    ///   interior dorado con rombos en las esquinas.
    /// - Delante: esquinas de hierro con gema, emblema dorado arriba, remate dorado abajo
    ///   y placas de hierro con remaches a los lados.
    /// Se aplica al construir (prefabs y escenas) y, por si acaso, también al iniciar la
    /// escena a cualquier panel que todavía no lo tenga. Nunca decora dos veces.
    /// </summary>
    public class DecoradorPaneles : MonoBehaviour
    {
        [System.Serializable]
        public class Piezas
        {
            public Sprite esquina;
            public Sprite emblema;
            public Sprite textura;
            public Sprite marcoInterior;
            public Sprite placaLateral;
            public Sprite remateInferior;
            public Sprite resplandor;
        }

        [SerializeField] private Piezas piezas = new Piezas();
        [SerializeField] private string nombreSpritePanel = "Panel_Madera";
        [Tooltip("Objetos con estos nombres no se decoran (por ejemplo, el letrero del menú que ya tiene antorchas).")]
        [SerializeField] private string[] excluir = { "Panel_Lateral" };

        public const string NombreMarca = "Decoracion_Panel";
        private const float Escala = 4f; // 1 px del sprite = 4 px de la interfaz (igual que los paneles)

        private void Start()
        {
            Canvas.ForceUpdateCanvases();
            foreach (Image img in GetComponentsInChildren<Image>(true))
            {
                if (img == null || img.sprite == null || img.sprite.name != nombreSpritePanel) continue;
                if (EsExcluido(img.gameObject.name)) continue;
                Decorar(img, piezas);
            }
        }

        private bool EsExcluido(string nombre)
        {
            if (excluir == null) return false;
            foreach (string e in excluir) if (!string.IsNullOrEmpty(e) && nombre == e) return true;
            return false;
        }

        /// <summary>Decora un panel (si aún no está decorado). Se puede llamar desde el editor.</summary>
        public static void Decorar(Image panel, Piezas p)
        {
            if (panel == null || p == null) return;
            if (panel.transform.Find(NombreMarca) != null) return;

            RectTransform rt = panel.rectTransform;
            Vector2 tam = rt.rect.size;
            bool tamConocido = tam.x > 1f && tam.y > 1f;
            if (!tamConocido && rt.sizeDelta.x > 1f && rt.sizeDelta.y > 1f && rt.anchorMin == rt.anchorMax)
            {
                tam = rt.sizeDelta;
                tamConocido = true;
            }
            // Si no se conoce el tamaño (panel estirado aún sin calcular) se trata como grande
            bool grande = !tamConocido || (tam.x >= 300f && tam.y >= 220f);
            bool mediano = !tamConocido || (tam.x >= 120f && tam.y >= 90f);

            // ------------------------------------------------ Detrás del contenido
            GameObject atras = NuevoContenedor(NombreMarca, panel.transform);
            atras.transform.SetAsFirstSibling();

            if (p.textura != null && mediano)
            {
                Image tex = CrearImagen("Textura", atras.transform, p.textura);
                tex.type = Image.Type.Tiled;
                tex.pixelsPerUnitMultiplier = 1.5625f;
                Estirar(tex.rectTransform, 32f);
            }
            if (!grande) return;

            if (p.resplandor != null)
            {
                Image brillo = CrearImagen("Resplandor", atras.transform, p.resplandor);
                brillo.color = new Color(1f, 0.8f, 0.4f, 0.12f);
                RectTransform r = brillo.rectTransform;
                r.anchorMin = new Vector2(0.1f, 1f);
                r.anchorMax = new Vector2(0.9f, 1f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(0f, 320f);
                r.anchoredPosition = new Vector2(0f, -90f);
            }
            if (p.marcoInterior != null)
            {
                Image marco = CrearImagen("Marco_Interior", atras.transform, p.marcoInterior);
                marco.type = Image.Type.Sliced;
                marco.fillCenter = false;
                marco.pixelsPerUnitMultiplier = 1.5625f;
                Estirar(marco.rectTransform, 36f);
            }

            // ------------------------------------------------ Delante (sobre el marco)
            GameObject frente = NuevoContenedor("Decoracion_Frente", panel.transform);
            frente.transform.SetAsLastSibling();

            if (p.placaLateral != null)
            {
                Vector2 tamPlaca = new Vector2(p.placaLateral.rect.width * Escala, p.placaLateral.rect.height * Escala);
                foreach (float x in new[] { 0f, 1f })
                {
                    Image pl = CrearImagen("Placa_Lateral", frente.transform, p.placaLateral);
                    RectTransform r = pl.rectTransform;
                    r.anchorMin = r.anchorMax = new Vector2(x, 0.5f);
                    r.pivot = new Vector2(0.5f, 0.5f);
                    r.sizeDelta = tamPlaca;
                    r.anchoredPosition = new Vector2(x < 0.5f ? 16f : -16f, 0f);
                }
            }

            if (p.esquina != null)
            {
                float lado = p.esquina.rect.width * Escala;
                Vector2[] anclas = { new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f) };
                Vector3[] escalas = { new Vector3(1f, 1f, 1f), new Vector3(-1f, 1f, 1f), new Vector3(1f, -1f, 1f), new Vector3(-1f, -1f, 1f) };
                for (int i = 0; i < 4; i++)
                {
                    Image e = CrearImagen("Esquina_" + i, frente.transform, p.esquina);
                    RectTransform r = e.rectTransform;
                    r.anchorMin = r.anchorMax = anclas[i];
                    r.pivot = new Vector2(0f, 1f);
                    r.sizeDelta = new Vector2(lado, lado);
                    r.localScale = escalas[i];
                    r.anchoredPosition = new Vector2(anclas[i].x < 0.5f ? -16f : 16f, anclas[i].y > 0.5f ? 16f : -16f);
                }
            }

            if (p.emblema != null)
            {
                Image em = CrearImagen("Emblema", frente.transform, p.emblema);
                RectTransform r = em.rectTransform;
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 1f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(p.emblema.rect.width * Escala, p.emblema.rect.height * Escala);
                r.anchoredPosition = Vector2.zero; // centrado sobre el borde superior
            }

            if (p.remateInferior != null)
            {
                Image rem = CrearImagen("Remate_Inferior", frente.transform, p.remateInferior);
                RectTransform r = rem.rectTransform;
                r.anchorMin = r.anchorMax = new Vector2(0.5f, 0f);
                r.pivot = new Vector2(0.5f, 0.5f);
                r.sizeDelta = new Vector2(p.remateInferior.rect.width * Escala, p.remateInferior.rect.height * Escala);
                r.anchoredPosition = new Vector2(0f, 2f);
            }
        }

        private static GameObject NuevoContenedor(string nombre, Transform padre)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            Estirar((RectTransform)go.transform, 0f);
            var le = go.AddComponent<LayoutElement>();
            le.ignoreLayout = true;
            return go;
        }

        private static Image CrearImagen(string nombre, Transform padre, Sprite sprite)
        {
            GameObject go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            Image img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.raycastTarget = false;
            return img;
        }

        private static void Estirar(RectTransform r, float margen)
        {
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = new Vector2(0.5f, 0.5f);
            r.offsetMin = new Vector2(margen, margen);
            r.offsetMax = new Vector2(-margen, -margen);
        }
    }
}
