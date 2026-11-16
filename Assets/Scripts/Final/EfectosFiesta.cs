using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElMinero.Final
{
    /// <summary>
    /// Efectos de la fiesta del pueblo en la escena final: cohetes que estallan
    /// sobre el templo, chispas al entregar una joya, corazones que suben y
    /// confeti que cae frente a la cámara. Todo con sprites pequeños y sin partículas.
    /// </summary>
    public class EfectosFiesta : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite chispa;
        [SerializeField] private Sprite destello;
        [SerializeField] private Sprite confeti;
        [SerializeField] private Sprite corazon;

        [Header("Capas")]
        [SerializeField] private string capaCielo = "Fondo";
        [SerializeField] private int ordenCielo = 0;
        [SerializeField] private string capaFrente = "Primer plano";

        [Header("Cohetes")]
        [Tooltip("Los cohetes salen a los lados del templo para que no queden ocultos detrás de él.")]
        [SerializeField] private Vector2 rangoIzquierdo = new Vector2(31f, 39.5f);
        [SerializeField] private Vector2 rangoDerecho = new Vector2(52.5f, 60f);

        private static readonly Color[] Colores =
        {
            new Color(1f, 0.35f, 0.55f), new Color(1f, 0.82f, 0.3f), new Color(0.35f, 0.85f, 1f),
            new Color(0.55f, 1f, 0.45f), new Color(0.85f, 0.5f, 1f), new Color(1f, 0.55f, 0.2f)
        };

        private Coroutine rutinaCohetes;
        private Coroutine rutinaConfeti;

        private SpriteRenderer Crear(string nombre, Sprite sprite, Vector3 pos, string capa, int orden, Color color, float escala = 1f)
        {
            GameObject go = new GameObject(nombre);
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * escala;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = capa;
            sr.sortingOrder = orden;
            sr.color = color;
            return sr;
        }

        // ------------------------------------------------------------------ Entrega de joya

        /// <summary>Chispas doradas en círculo y un destello al recibir una joya.</summary>
        public void ChispasEntrega(Vector3 pos)
        {
            StartCoroutine(Estallido(pos, new Color(1f, 0.9f, 0.45f), 12, 3.2f, 0.55f, capaFrente, 4, false));
            if (destello != null) StartCoroutine(Destello(pos));
        }

        /// <summary>Corazón que sube y se desvanece sobre un habitante.</summary>
        public void Corazon(Vector3 pos)
        {
            if (corazon == null) return;
            StartCoroutine(RutinaCorazon(pos));
        }

        private IEnumerator RutinaCorazon(Vector3 pos)
        {
            var sr = Crear("Corazon", corazon, pos, capaFrente, 6, Color.white, 0.6f);
            float t = 0f;
            while (t < 1.3f)
            {
                t += Time.deltaTime;
                float k = t / 1.3f;
                sr.transform.position = pos + new Vector3(Mathf.Sin(t * 6f) * 0.15f, k * 1.4f, 0f);
                float pop = k < 0.15f ? Mathf.Lerp(0.3f, 0.75f, k / 0.15f) : 0.65f;
                sr.transform.localScale = Vector3.one * pop;
                sr.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((k - 0.6f) / 0.4f));
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        private IEnumerator Destello(Vector3 pos)
        {
            var sr = Crear("Destello", destello, pos, capaFrente, 5, Color.white, 0.5f);
            float t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = t / 0.45f;
                sr.transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 2.2f, k);
                sr.transform.rotation = Quaternion.Euler(0f, 0f, k * 90f);
                sr.color = new Color(1f, 0.95f, 0.7f, 1f - k);
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        // ------------------------------------------------------------------ Cohetes

        public void IniciarCohetes()
        {
            if (rutinaCohetes == null) rutinaCohetes = StartCoroutine(RutinaCohetes());
        }

        private IEnumerator RutinaCohetes()
        {
            while (true)
            {
                StartCoroutine(Cohete());
                if (Random.value < 0.35f) StartCoroutine(Cohete());
                yield return new WaitForSeconds(Random.Range(0.55f, 1.1f));
            }
        }

        private IEnumerator Cohete()
        {
            if (chispa == null) yield break;
            Color color = Colores[Random.Range(0, Colores.Length)];
            Vector2 rango = Random.value < 0.5f ? rangoIzquierdo : rangoDerecho;
            Vector3 desde = new Vector3(Random.Range(rango.x, rango.y), 1f, 0f);
            Vector3 hasta = desde + new Vector3(Random.Range(-2f, 2f), Random.Range(9f, 14f), 0f);

            // Estela que sube
            var cabeza = Crear("Cohete", chispa, desde, capaCielo, ordenCielo + 1, new Color(1f, 0.95f, 0.8f), 1.2f);
            float dur = Random.Range(0.8f, 1.1f);
            float t = 0f;
            float proximaEstela = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = 1f - Mathf.Pow(1f - t / dur, 2f);
                cabeza.transform.position = Vector3.Lerp(desde, hasta, k) + new Vector3(Mathf.Sin(t * 30f) * 0.04f, 0f, 0f);
                if (t >= proximaEstela)
                {
                    proximaEstela = t + 0.04f;
                    StartCoroutine(Desvanecer(Crear("Estela", chispa, cabeza.transform.position, capaCielo, ordenCielo, new Color(1f, 0.75f, 0.4f, 0.8f), 0.8f), 0.35f));
                }
                yield return null;
            }
            Destroy(cabeza.gameObject);

            // Estallido: anillo de chispas con gravedad y un segundo anillo más pequeño
            int n = Random.Range(22, 32);
            StartCoroutine(Estallido(hasta, color, n, Random.Range(4.5f, 6f), Random.Range(1.1f, 1.5f), capaCielo, ordenCielo + 2, true));
            Color segundo = Colores[Random.Range(0, Colores.Length)];
            StartCoroutine(Estallido(hasta, segundo, n / 2, 2.2f, 0.9f, capaCielo, ordenCielo + 3, true));
        }

        private IEnumerator Desvanecer(SpriteRenderer sr, float duracion)
        {
            Color c = sr.color;
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                sr.color = new Color(c.r, c.g, c.b, c.a * (1f - t / duracion));
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        private IEnumerator Estallido(Vector3 centro, Color color, int n, float velocidad, float duracion, string capa, int orden, bool gravedad)
        {
            if (chispa == null) yield break;
            var chispas = new List<SpriteRenderer>(n);
            var vel = new List<Vector2>(n);
            float giro = Random.Range(0f, Mathf.PI * 2f);
            for (int i = 0; i < n; i++)
            {
                float a = giro + i * Mathf.PI * 2f / n;
                float v = velocidad * Random.Range(0.85f, 1.1f);
                vel.Add(new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * v);
                chispas.Add(Crear("Chispa", chispa, centro, capa, orden, color, Random.Range(1f, 1.5f)));
            }

            float t = 0f;
            while (t < duracion)
            {
                float dt = Time.deltaTime;
                t += dt;
                float k = t / duracion;
                for (int i = 0; i < chispas.Count; i++)
                {
                    Vector2 v = vel[i];
                    v *= 1f - 2.2f * dt;                 // frenado del aire
                    if (gravedad) v.y -= 3.5f * dt;     // caen un poco
                    vel[i] = v;
                    chispas[i].transform.position += (Vector3)(v * dt);
                    float parpadeo = k > 0.6f ? (Mathf.Sin(t * 40f + i) > 0f ? 1f : 0.4f) : 1f;
                    chispas[i].color = new Color(color.r, color.g, color.b, (1f - k * k) * parpadeo);
                }
                yield return null;
            }
            foreach (var c in chispas) if (c != null) Destroy(c.gameObject);
        }

        // ------------------------------------------------------------------ Confeti

        public void IniciarConfeti(Camera cam)
        {
            if (rutinaConfeti == null && confeti != null && cam != null) rutinaConfeti = StartCoroutine(RutinaConfeti(cam));
        }

        private IEnumerator RutinaConfeti(Camera cam)
        {
            const int cantidad = 70;
            var piezas = new List<SpriteRenderer>(cantidad);
            var datos = new List<Vector4>(cantidad); // x: vel caída, y: fase, z: amplitud, w: giro
            for (int i = 0; i < cantidad; i++)
            {
                Color c = Colores[i % Colores.Length];
                var sr = Crear("Confeti", confeti, Vector3.zero, capaFrente, 8, c, Random.Range(0.9f, 1.4f));
                piezas.Add(sr);
                datos.Add(new Vector4(Random.Range(1.2f, 2.4f), Random.Range(0f, 6.28f), Random.Range(0.3f, 0.9f), Random.Range(-360f, 360f)));
                sr.transform.position = PuntoArriba(cam, Random.Range(0f, cam.orthographicSize * 2.2f));
            }

            while (true)
            {
                float alto = cam.orthographicSize;
                float ancho = alto * cam.aspect;
                Vector3 c = cam.transform.position;
                for (int i = 0; i < piezas.Count; i++)
                {
                    Vector4 d = datos[i];
                    Transform tr = piezas[i].transform;
                    Vector3 p = tr.position;
                    p.y -= d.x * Time.deltaTime;
                    p.x += Mathf.Sin(Time.time * 2.5f + d.y) * d.z * Time.deltaTime;
                    tr.position = p;
                    tr.Rotate(0f, 0f, d.w * Time.deltaTime);
                    // Efecto de "volteo" del papelito
                    float s = Mathf.Abs(Mathf.Sin(Time.time * 4f + d.y));
                    tr.localScale = new Vector3(Mathf.Max(0.2f, s) * 1.2f, 1.2f, 1f);

                    if (p.y < c.y - alto - 0.5f || Mathf.Abs(p.x - c.x) > ancho + 1.5f)
                    {
                        tr.position = PuntoArriba(cam, 0f);
                    }
                }
                yield return null;
            }
        }

        private static Vector3 PuntoArriba(Camera cam, float bajar)
        {
            float alto = cam.orthographicSize;
            float ancho = alto * cam.aspect;
            Vector3 c = cam.transform.position;
            return new Vector3(c.x + Random.Range(-ancho, ancho), c.y + alto + 0.5f - bajar, 0f);
        }
    }
}
