using System.Collections;
using UnityEngine;
using TMPro;

namespace ElMinero.UI
{
    /// <summary>
    /// Cartel grande centrado en pantalla para avisos del nivel:
    /// "¡LA MINA SE INCENDIA!", la presentación del jefe y "¡VENCISTE AL COLOSO!".
    /// Usa tiempo sin escala para funcionar aunque el juego esté en pausa.
    /// </summary>
    public class AvisoPantalla : MonoBehaviour
    {
        public static AvisoPantalla Instance { get; private set; }

        [SerializeField] private TextMeshProUGUI textoTitulo;
        [SerializeField] private TextMeshProUGUI textoSubtitulo;
        [SerializeField] private CanvasGroup grupo;

        private Coroutine rutina;

        private void Awake()
        {
            Instance = this;
            if (grupo == null) grupo = GetComponent<CanvasGroup>();
            OcultarInmediato();
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public static void Mostrar(string titulo, string subtitulo, float duracion, float tamanoTitulo = 56f, float tamanoSubtitulo = 34f, Color? color = null)
        {
            if (Instance == null) return;
            Instance.MostrarInterno(titulo, subtitulo, duracion, tamanoTitulo, tamanoSubtitulo, color ?? new Color(1f, 0.85f, 0.4f));
        }

        public static void Ocultar()
        {
            if (Instance == null) return;
            if (Instance.rutina != null) Instance.StopCoroutine(Instance.rutina);
            Instance.OcultarInmediato();
        }

        private void MostrarInterno(string titulo, string subtitulo, float duracion, float tamTitulo, float tamSub, Color color)
        {
            if (rutina != null) StopCoroutine(rutina);
            gameObject.SetActive(true);

            if (textoTitulo != null)
            {
                textoTitulo.text = titulo;
                textoTitulo.fontSize = tamTitulo;
                textoTitulo.fontSizeMax = tamTitulo;
                textoTitulo.color = color;
            }

            if (textoSubtitulo != null)
            {
                textoSubtitulo.text = subtitulo ?? "";
                textoSubtitulo.fontSize = tamSub;
                textoSubtitulo.fontSizeMax = tamSub;
                textoSubtitulo.gameObject.SetActive(!string.IsNullOrEmpty(subtitulo));
            }

            rutina = StartCoroutine(RutinaMostrar(duracion));
        }

        private IEnumerator RutinaMostrar(float duracion)
        {
            float fade = 0.25f;
            float t = 0f;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                if (grupo != null) grupo.alpha = t / fade;
                yield return null;
            }
            if (grupo != null) grupo.alpha = 1f;

            yield return new WaitForSecondsRealtime(Mathf.Max(0f, duracion - fade * 2f));

            t = 0f;
            while (t < fade)
            {
                t += Time.unscaledDeltaTime;
                if (grupo != null) grupo.alpha = 1f - t / fade;
                yield return null;
            }
            OcultarInmediato();
            rutina = null;
        }

        private void OcultarInmediato()
        {
            if (grupo != null) grupo.alpha = 0f;
            if (textoTitulo != null) textoTitulo.text = "";
            if (textoSubtitulo != null) textoSubtitulo.text = "";
        }
    }
}
