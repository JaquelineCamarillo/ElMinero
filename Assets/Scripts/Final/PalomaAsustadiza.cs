using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Final
{
    /// <summary>
    /// Paloma del atrio: picotea (cuadros 4–5) y, si el minero pasa a menos de 3 u,
    /// cambia a vuelo (cuadros 0–3) y se va hacia arriba a la derecha hasta salir de la cámara.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class PalomaAsustadiza : MonoBehaviour
    {
        [SerializeField] private Sprite[] cuadros;
        [SerializeField] private float distancia = 3f;
        [SerializeField] private Vector2 velocidadVuelo = new Vector2(4f, 3.2f);

        private SpriteFrameAnimator anim;
        private Transform jugador;
        private bool volando;

        public void Configurar(Sprite[] hoja)
        {
            cuadros = hoja;
        }

        private void Awake()
        {
            anim = GetComponent<SpriteFrameAnimator>();
            if (anim == null) anim = gameObject.AddComponent<SpriteFrameAnimator>();
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
            if (p != null) jugador = p.transform;
            anim.Play(Sub(4, 2), Random.Range(2.5f, 4f), true);
        }

        private void Update()
        {
            if (!volando)
            {
                if (jugador != null && Vector2.Distance(jugador.position, transform.position) < distancia)
                {
                    volando = true;
                    anim.Play(Sub(0, 4), 12f, true);
                }
                return;
            }

            transform.position += (Vector3)(velocidadVuelo * Time.deltaTime);

            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 vp = cam.WorldToViewportPoint(transform.position);
                if (vp.x > 1.1f || vp.y > 1.1f) gameObject.SetActive(false);
            }
        }

        private Sprite[] Sub(int inicio, int cantidad)
        {
            if (cuadros == null || cuadros.Length == 0) return new Sprite[0];
            var r = new Sprite[cantidad];
            for (int i = 0; i < cantidad; i++) r[i] = cuadros[Mathf.Min(inicio + i, cuadros.Length - 1)];
            return r;
        }
    }
}
