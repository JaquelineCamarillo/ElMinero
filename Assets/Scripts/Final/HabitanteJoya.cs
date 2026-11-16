using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Final
{
    /// <summary>
    /// Habitante de La Valenciana que espera su parte del tesoro.
    /// - Esperando: idle (cuadros 0–1) con una burbuja que muestra una joya y se mece.
    /// - Al recibir su joya: celebra (cuadros 2–3) un momento, la burbuja desaparece
    ///   y después da saltitos de alegría de vez en cuando.
    /// - En la fiesta final celebra sin parar.
    /// Siempre voltea a ver al minero.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HabitanteJoya : MonoBehaviour
    {
        [SerializeField] private Sprite[] cuadros;
        [SerializeField] private string frase = "¡Gracias, minero!";
        [SerializeField] private Transform burbuja;
        [Tooltip("Los sprites de los habitantes miran a la izquierda por defecto.")]
        [SerializeField] private bool miraALaIzquierda = true;

        public bool Esperando { get; private set; } = true;
        public string Frase => frase;
        public Vector3 PuntoJoya => transform.position + new Vector3(0f, 2.3f, 0f);

        private SpriteRenderer sr;
        private Transform jugador;
        private float celebrarHasta;
        private bool fiesta;
        private float proximoSalto;
        private float fase;
        private Vector3 posBurbuja;
        private float baseY;

        public void Configurar(Sprite[] hoja, string nuevaFrase, Transform nuevaBurbuja)
        {
            cuadros = hoja;
            frase = nuevaFrase;
            burbuja = nuevaBurbuja;
        }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            fase = Random.Range(0f, 3f);
            baseY = transform.position.y;
            if (burbuja != null) posBurbuja = burbuja.localPosition;
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
            if (p != null) jugador = p.transform;
        }

        /// <summary>Recibe su joya: celebra y se quita la burbuja.</summary>
        public void Recibir()
        {
            Esperando = false;
            celebrarHasta = Time.time + 2.2f;
            proximoSalto = Time.time + Random.Range(3f, 5f);
            if (burbuja != null) burbuja.gameObject.SetActive(false);
        }

        /// <summary>Fiesta final: todos celebran sin parar.</summary>
        public void Fiesta()
        {
            fiesta = true;
            if (burbuja != null) burbuja.gameObject.SetActive(false);
        }

        private void Update()
        {
            float t = Time.time + fase;

            // Voltear hacia el minero
            if (jugador != null && sr != null)
            {
                bool jugadorDerecha = jugador.position.x > transform.position.x;
                sr.flipX = miraALaIzquierda ? jugadorDerecha : !jugadorDerecha;
            }

            bool celebrando = fiesta || Time.time < celebrarHasta;
            if (celebrando) Mostrar(2 + Mathf.FloorToInt(t * 6f) % 2);
            else Mostrar(Mathf.FloorToInt(t * 2.5f) % 2);

            // Saltitos de alegría (después de recibir su joya y en la fiesta)
            float salto = 0f;
            if (fiesta)
            {
                salto = Mathf.Abs(Mathf.Sin(t * 5f)) * 0.35f;
            }
            else if (!Esperando)
            {
                if (Time.time >= proximoSalto)
                {
                    proximoSalto = Time.time + Random.Range(3f, 6f);
                    celebrarHasta = Time.time + 0.6f;
                }
                if (Time.time < celebrarHasta) salto = Mathf.Abs(Mathf.Sin((celebrarHasta - Time.time) * 10f)) * 0.3f;
            }
            Vector3 pos = transform.position;
            pos.y = baseY + salto;
            transform.position = pos;

            // La burbuja se mece suavemente
            if (Esperando && burbuja != null)
            {
                burbuja.localPosition = posBurbuja + new Vector3(0f, Mathf.Sin(t * 3f) * 0.1f, 0f);
            }
        }

        private void Mostrar(int i)
        {
            if (sr == null || cuadros == null || cuadros.Length == 0) return;
            sr.sprite = cuadros[Mathf.Clamp(i, 0, cuadros.Length - 1)];
        }
    }
}
