using System.Collections.Generic;
using UnityEngine;

namespace ElMinero.UI
{
    /// <summary>
    /// Vida del fondo del Menú Principal: estrellas que titilan, estrellas
    /// fugaces, luciérnagas junto a los faroles, murciélagos que cruzan frente
    /// a la luna, niebla que se desliza y destellos en las vetas del suelo.
    /// Todo se crea al iniciar a partir de unos cuantos sprites.
    /// </summary>
    public class MenuAmbiente : MonoBehaviour
    {
        [Header("Capas (padres)")]
        [SerializeField] private Transform capaCielo;
        [SerializeField] private Transform capaFrente;
        [SerializeField] private string sortingLayer = "Fondo";

        [Header("Sprites")]
        [SerializeField] private Sprite spriteDestello;
        [SerializeField] private Sprite spriteEstrellaFugaz;
        [SerializeField] private Sprite spriteLuciernaga;
        [SerializeField] private Sprite[] cuadrosMurcielago;

        [Header("Estrellas")]
        [SerializeField] private int cantidadEstrellas = 26;
        [SerializeField] private Rect zonaEstrellas = new Rect(-15f, 0.5f, 30f, 7.5f);
        [SerializeField] private Rect zonaSinEstrellas = new Rect(9.5f, 0.2f, 5.5f, 4.6f); // la luna

        [Header("Luciérnagas (centros, locales a la capa del frente)")]
        [SerializeField] private Vector2[] focosLuciernagas = new Vector2[0];
        [SerializeField] private int luciernagasPorFoco = 5;

        [Header("Vetas que destellan (locales a la capa del frente)")]
        [SerializeField] private Vector2[] puntosVetas = new Vector2[0];

        [Header("Niebla")]
        [SerializeField] private SpriteRenderer[] nieblas = new SpriteRenderer[0];
        [SerializeField] private float velocidadNiebla = 0.25f;

        private class Titilante
        {
            public SpriteRenderer sr;
            public float fase, velocidad, escalaBase, alfaMax;
        }

        private class Luciernaga
        {
            public SpriteRenderer sr;
            public Vector2 centro;
            public float semilla, radio;
        }

        private readonly List<Titilante> estrellas = new List<Titilante>();
        private readonly List<Luciernaga> luciernagas = new List<Luciernaga>();
        private readonly List<Titilante> destellosVeta = new List<Titilante>();
        private float anchoNiebla;
        private float proximaFugaz, proximoMurcielago;

        private void Start()
        {
            CrearEstrellas();
            CrearLuciernagas();
            CrearDestellosVeta();

            if (nieblas.Length > 0 && nieblas[0] != null && nieblas[0].sprite != null)
            {
                anchoNiebla = nieblas[0].sprite.bounds.size.x * nieblas[0].transform.localScale.x;
            }

            proximaFugaz = Time.time + Random.Range(1.5f, 3.5f);
            proximoMurcielago = Time.time + Random.Range(4f, 7f);
        }

        private SpriteRenderer CrearSprite(string nombre, Transform padre, Sprite sprite, Vector3 posLocal, int orden, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre != null ? padre : transform, false);
            go.transform.localPosition = posLocal;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = orden;
            sr.color = color;
            return sr;
        }

        private void CrearEstrellas()
        {
            if (spriteDestello == null) return;
            for (int i = 0; i < cantidadEstrellas; i++)
            {
                Vector2 p = Vector2.zero;
                for (int intento = 0; intento < 10; intento++)
                {
                    p = new Vector2(Random.Range(zonaEstrellas.xMin, zonaEstrellas.xMax), Random.Range(zonaEstrellas.yMin, zonaEstrellas.yMax));
                    if (!zonaSinEstrellas.Contains(p)) break;
                }
                var sr = CrearSprite("Estrella_" + i, capaCielo, spriteDestello, p, -9, Color.white);
                var t = new Titilante
                {
                    sr = sr,
                    fase = Random.Range(0f, 10f),
                    velocidad = Random.Range(0.6f, 1.8f),
                    escalaBase = Random.Range(0.45f, 0.95f),
                    alfaMax = Random.Range(0.55f, 1f)
                };
                sr.transform.localScale = Vector3.one * t.escalaBase;
                estrellas.Add(t);
            }
        }

        private void CrearLuciernagas()
        {
            if (spriteLuciernaga == null) return;
            foreach (Vector2 foco in focosLuciernagas)
            {
                for (int i = 0; i < luciernagasPorFoco; i++)
                {
                    var sr = CrearSprite("Luciernaga", capaFrente, spriteLuciernaga, foco, 3, new Color(1f, 0.82f, 0.4f, 0f));
                    luciernagas.Add(new Luciernaga
                    {
                        sr = sr,
                        centro = foco,
                        semilla = Random.Range(0f, 100f),
                        radio = Random.Range(0.7f, 1.6f)
                    });
                }
            }
        }

        private void CrearDestellosVeta()
        {
            if (spriteDestello == null) return;
            foreach (Vector2 p in puntosVetas)
            {
                var sr = CrearSprite("Destello_Veta", capaFrente, spriteDestello, p + new Vector2(0.15f, 0.15f), 4, new Color(1f, 0.92f, 0.6f, 0f));
                destellosVeta.Add(new Titilante
                {
                    sr = sr,
                    fase = Random.Range(0f, 10f),
                    velocidad = Random.Range(0.25f, 0.45f),
                    escalaBase = 0.9f,
                    alfaMax = 1f
                });
            }
        }

        private void Update()
        {
            float tiempo = Time.time;

            // Estrellas que titilan
            foreach (var e in estrellas)
            {
                float s = 0.5f + 0.5f * Mathf.Sin((tiempo + e.fase) * e.velocidad * 2.2f);
                e.sr.color = new Color(1f, 0.97f, 0.88f, Mathf.Lerp(0.15f, e.alfaMax, s));
                e.sr.transform.localScale = Vector3.one * e.escalaBase * Mathf.Lerp(0.7f, 1.1f, s);
            }

            // Destellos breves en las vetas del suelo (un "ping" cada pocos segundos)
            foreach (var d in destellosVeta)
            {
                float ciclo = Mathf.Repeat((tiempo + d.fase) * d.velocidad, 1f);
                float k = ciclo < 0.12f ? Mathf.Sin(ciclo / 0.12f * Mathf.PI) : 0f;
                d.sr.color = new Color(1f, 0.92f, 0.6f, k);
                d.sr.transform.localScale = Vector3.one * d.escalaBase * (0.4f + k * 0.8f);
                d.sr.transform.localRotation = Quaternion.Euler(0f, 0f, k * 45f);
            }

            // Luciérnagas: vuelo suave con ruido Perlin y brillo pulsante
            foreach (var l in luciernagas)
            {
                float nx = Mathf.PerlinNoise(l.semilla, tiempo * 0.25f) * 2f - 1f;
                float ny = Mathf.PerlinNoise(l.semilla + 37f, tiempo * 0.25f) * 2f - 1f;
                l.sr.transform.localPosition = l.centro + new Vector2(nx * l.radio * 1.4f, ny * l.radio);
                float brillo = Mathf.PerlinNoise(l.semilla + 91f, tiempo * 0.9f);
                l.sr.color = new Color(1f, 0.82f, 0.4f, Mathf.SmoothStep(0f, 0.95f, brillo));
            }

            // Niebla que se desliza (dos copias que se turnan)
            if (anchoNiebla > 0.01f)
            {
                foreach (var n in nieblas)
                {
                    if (n == null) continue;
                    Vector3 p = n.transform.localPosition;
                    p.x -= velocidadNiebla * Time.deltaTime;
                    if (p.x < -anchoNiebla) p.x += anchoNiebla * nieblas.Length;
                    n.transform.localPosition = p;
                }
            }

            if (tiempo >= proximaFugaz)
            {
                proximaFugaz = tiempo + Random.Range(4f, 8f);
                if (spriteEstrellaFugaz != null) StartCoroutine(EstrellaFugaz());
            }

            if (tiempo >= proximoMurcielago)
            {
                proximoMurcielago = tiempo + Random.Range(9f, 15f);
                if (cuadrosMurcielago != null && cuadrosMurcielago.Length > 0) StartCoroutine(Murcielagos());
            }
        }

        private System.Collections.IEnumerator EstrellaFugaz()
        {
            Vector2 inicio = new Vector2(Random.Range(-13f, 6f), Random.Range(4.5f, 7.5f));
            Vector2 dir = new Vector2(1f, -0.42f).normalized;
            var sr = CrearSprite("EstrellaFugaz", capaCielo, spriteEstrellaFugaz, inicio, -8, Color.white);
            sr.transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
            float dur = Random.Range(0.6f, 0.9f);
            float vel = Random.Range(14f, 18f);
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                sr.transform.localPosition = inicio + dir * vel * t;
                float a = k < 0.2f ? k / 0.2f : 1f - (k - 0.2f) / 0.8f;
                sr.color = new Color(1f, 1f, 0.95f, a);
                yield return null;
            }
            Destroy(sr.gameObject);
        }

        private System.Collections.IEnumerator Murcielagos()
        {
            // De 1 a 3 murciélagos cruzan el cielo (a veces frente a la luna)
            int n = Random.Range(1, 4);
            bool haciaIzquierda = Random.value < 0.5f;
            float y0 = Random.Range(2.2f, 5.5f);
            var grupo = new List<SpriteRenderer>();
            var desfase = new List<Vector2>();
            for (int i = 0; i < n; i++)
            {
                var sr = CrearSprite("Murcielago_Menu", capaCielo, cuadrosMurcielago[0], Vector3.zero, -6, new Color(0.12f, 0.08f, 0.18f, 1f));
                sr.flipX = haciaIzquierda;
                sr.transform.localScale = Vector3.one * Random.Range(0.55f, 0.8f);
                grupo.Add(sr);
                desfase.Add(new Vector2(-i * Random.Range(0.8f, 1.3f), Random.Range(-0.6f, 0.6f)));
            }

            float x = haciaIzquierda ? 17f : -17f;
            float dirX = haciaIzquierda ? -1f : 1f;
            float vel = Random.Range(3.2f, 4.5f);
            float t = 0f;
            while (Mathf.Abs(x) <= 19f + n * 1.5f)
            {
                t += Time.deltaTime;
                x += dirX * vel * Time.deltaTime;
                for (int i = 0; i < grupo.Count; i++)
                {
                    float xi = x + desfase[i].x * dirX;
                    float yi = y0 + desfase[i].y + Mathf.Sin((t + i) * 5f) * 0.25f;
                    grupo[i].transform.localPosition = new Vector3(xi, yi, 0f);
                    grupo[i].sprite = cuadrosMurcielago[Mathf.FloorToInt((t + i * 0.13f) * 10f) % cuadrosMurcielago.Length];
                }
                yield return null;
            }
            foreach (var sr in grupo) Destroy(sr.gameObject);
        }
    }
}
