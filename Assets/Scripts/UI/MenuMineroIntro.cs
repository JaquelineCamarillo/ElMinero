using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.UI
{
    /// <summary>
    /// Pequeña escena animada del Menú Principal: el minero sale caminando
    /// de la oscuridad de la bocamina, salta a las vías, levanta polvo al
    /// caer y se queda esperando (con gestos de vez en cuando).
    /// Las posiciones son locales a la capa del frente del fondo.
    /// </summary>
    public class MenuMineroIntro : MonoBehaviour
    {
        [Header("Cuadros de animación")]
        [SerializeField] private Sprite[] cuadrosCaminar;
        [SerializeField] private Sprite[] cuadrosIdle;
        [SerializeField] private Sprite[] cuadrosSaltar;
        [SerializeField] private Sprite[] cuadrosCelebrar;
        [SerializeField] private Sprite[] cuadrosPolvo;

        [Header("Recorrido (posiciones locales)")]
        [SerializeField] private Vector2 puntoInicio = new Vector2(-12.4f, -4.5f);
        [SerializeField] private Vector2 puntoBorde = new Vector2(-9.7f, -4.5f);
        [SerializeField] private Vector2 puntoAterrizaje = new Vector2(-9.0f, -6.375f);
        [SerializeField] private Vector2 puntoEspera = new Vector2(-8.9f, -6.375f);
        [Tooltip("Mientras x sea menor que esto, el minero sigue en la sombra de la mina.")]
        [SerializeField] private float xFinSombra = -10.4f;

        [Header("Ajustes")]
        [SerializeField] private float retrasoInicial = 0.8f;
        [SerializeField] private float velocidad = 2.2f;
        [SerializeField] private float alturaSalto = 1.1f;
        [SerializeField] private float duracionSalto = 0.5f;

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            transform.localPosition = new Vector3(puntoInicio.x, puntoInicio.y, 0f);
            if (sr != null)
            {
                sr.flipX = false;
                sr.color = Color.black;
                if (cuadrosIdle != null && cuadrosIdle.Length > 0) sr.sprite = cuadrosIdle[0];
            }
            StartCoroutine(Rutina());
        }

        private IEnumerator Rutina()
        {
            // Respira en la oscuridad mientras se funde la pantalla
            float t = 0f;
            while (t < retrasoInicial)
            {
                t += Time.deltaTime;
                Animar(cuadrosIdle, 3f);
                yield return null;
            }

            // 1. Sale caminando de la bocamina
            yield return Caminar(puntoBorde);

            // 2. Salta hacia las vías
            Vector3 desde = transform.localPosition;
            Vector3 hasta = new Vector3(puntoAterrizaje.x, puntoAterrizaje.y, 0f);
            t = 0f;
            if (cuadrosSaltar != null && cuadrosSaltar.Length > 0) sr.sprite = cuadrosSaltar[0];
            while (t < duracionSalto)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duracionSalto);
                Vector3 p = Vector3.Lerp(desde, hasta, k);
                p.y += Mathf.Sin(k * Mathf.PI) * alturaSalto;
                transform.localPosition = p;
                ActualizarSombra();
                yield return null;
            }
            transform.localPosition = hasta;
            sr.color = Color.white;

            // 3. Aterriza: polvo y un pequeño "aplastón"
            CrearPolvo();
            float a = 0f;
            Vector3 escala = transform.localScale;
            while (a < 0.18f)
            {
                a += Time.deltaTime;
                float k = a / 0.18f;
                float sy = Mathf.Lerp(0.8f, 1f, k);
                transform.localScale = new Vector3(escala.x * (2f - sy), escala.y * sy, escala.z);
                if (cuadrosIdle != null && cuadrosIdle.Length > 0) sr.sprite = cuadrosIdle[0];
                yield return null;
            }
            transform.localScale = escala;

            // 4. Unos pasos más y espera
            yield return Caminar(puntoEspera);

            // 5. Gestos ocasionales
            while (true)
            {
                float espera = Random.Range(4.5f, 8f);
                t = 0f;
                while (t < espera)
                {
                    t += Time.deltaTime;
                    Animar(cuadrosIdle, 3f);
                    yield return null;
                }

                if (Random.value < 0.5f && cuadrosCelebrar != null && cuadrosCelebrar.Length > 0)
                {
                    // Celebra (levanta el pico)
                    t = 0f;
                    while (t < 1.4f)
                    {
                        t += Time.deltaTime;
                        Animar(cuadrosCelebrar, 6f);
                        yield return null;
                    }
                }
                else
                {
                    // Voltea a ver la mina y regresa la mirada
                    sr.flipX = true;
                    t = 0f;
                    while (t < 1.3f)
                    {
                        t += Time.deltaTime;
                        Animar(cuadrosIdle, 3f);
                        yield return null;
                    }
                    sr.flipX = false;
                }
            }
        }

        private IEnumerator Caminar(Vector2 destino)
        {
            Vector3 meta = new Vector3(destino.x, destino.y, 0f);
            sr.flipX = meta.x < transform.localPosition.x;
            while ((transform.localPosition - meta).sqrMagnitude > 0.0004f)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, meta, velocidad * Time.deltaTime);
                Animar(cuadrosCaminar, 9f);
                ActualizarSombra();
                yield return null;
            }
            transform.localPosition = meta;
            sr.flipX = false;
        }

        private void Animar(Sprite[] cuadros, float fps)
        {
            if (sr == null || cuadros == null || cuadros.Length == 0) return;
            int i = Mathf.FloorToInt(Time.time * fps) % cuadros.Length;
            sr.sprite = cuadros[i];
        }

        private void ActualizarSombra()
        {
            if (sr == null) return;
            float x = transform.localPosition.x;
            float k = Mathf.InverseLerp(puntoInicio.x, xFinSombra, x);
            sr.color = Color.Lerp(Color.black, Color.white, k * k);
        }

        private void CrearPolvo()
        {
            if (cuadrosPolvo == null || cuadrosPolvo.Length == 0 || sr == null) return;
            Vector3 pos = transform.position + new Vector3(0f, 0.25f, 0f);
            GameObject polvo = EfectoTemporal.Crear(pos, cuadrosPolvo, 14f, new Color(0.85f, 0.78f, 0.7f, 0.9f), 0.6f, sr.sortingLayerName, sr.sortingOrder + 1);
            if (polvo != null && transform.parent != null) polvo.transform.SetParent(transform.parent, true);
        }
    }
}
