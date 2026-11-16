using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ElMinero.UI
{
    /// <summary>
    /// Barra de vida del jefe (arriba al centro): retrato del Coloso, nombre
    /// y marco con 3 segmentos de relleno.
    /// </summary>
    public class BossHealthBarUI : MonoBehaviour
    {
        public static BossHealthBarUI Instance { get; private set; }

        [SerializeField] private CanvasGroup grupo;
        [SerializeField] private TextMeshProUGUI textoNombre;
        [SerializeField] private Image[] segmentos;
        [SerializeField] private TextMeshProUGUI textoInstruccion;

        private Coroutine rutina;

        private void Awake()
        {
            Instance = this;
            if (grupo == null) grupo = GetComponent<CanvasGroup>();
            if (grupo != null) grupo.alpha = 0f;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Mostrar(string nombre, int vida)
        {
            if (textoNombre != null && !string.IsNullOrEmpty(nombre)) textoNombre.text = nombre;
            SetVida(vida);
            Fundir(1f);
        }

        public void SetInstruccion(string texto)
        {
            if (textoInstruccion != null) textoInstruccion.text = texto;
        }

        public void Ocultar()
        {
            Fundir(0f);
        }

        public void SetVida(int vida)
        {
            if (segmentos == null) return;
            for (int i = 0; i < segmentos.Length; i++)
            {
                if (segmentos[i] != null) segmentos[i].enabled = i < vida;
            }
        }

        private void Fundir(float objetivo)
        {
            if (grupo == null) return;
            if (rutina != null) StopCoroutine(rutina);
            if (!isActiveAndEnabled) { grupo.alpha = objetivo; return; }
            rutina = StartCoroutine(RutinaFundir(objetivo));
        }

        private IEnumerator RutinaFundir(float objetivo)
        {
            float inicio = grupo.alpha;
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.unscaledDeltaTime;
                grupo.alpha = Mathf.Lerp(inicio, objetivo, t / 0.35f);
                yield return null;
            }
            grupo.alpha = objetivo;
            rutina = null;
        }
    }
}
