using UnityEngine;
using TMPro;

namespace ElMinero.Utilidades
{
    /// <summary>
    /// Texto que sube y se desvanece (por ejemplo "¡GOLPE!" al pegarle al jefe).
    /// </summary>
    public class TextoFlotante : MonoBehaviour
    {
        [SerializeField] private float duracion = 0.9f;
        [SerializeField] private float velocidad = 2.2f;

        private TMP_Text texto;
        private float t;
        private Color colorInicial;
        private Vector3 escalaInicial;

        private void Awake()
        {
            texto = GetComponent<TMP_Text>();
            if (texto != null) colorInicial = texto.color;
            escalaInicial = transform.localScale;
        }

        /// <summary>Cambia la duración y la velocidad de subida (por ejemplo, frases largas).</summary>
        public void Configurar(float nuevaDuracion, float nuevaVelocidad)
        {
            duracion = Mathf.Max(0.1f, nuevaDuracion);
            velocidad = nuevaVelocidad;
        }

        private void Update()
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracion);
            transform.position += Vector3.up * velocidad * Time.deltaTime;
            float pop = k < 0.15f ? Mathf.Lerp(0.6f, 1.25f, k / 0.15f) : Mathf.Lerp(1.25f, 1f, (k - 0.15f) / 0.85f);
            transform.localScale = escalaInicial * pop;
            if (texto != null) texto.color = new Color(colorInicial.r, colorInicial.g, colorInicial.b, 1f - k * k);
            if (t >= duracion) Destroy(gameObject);
        }
    }
}
