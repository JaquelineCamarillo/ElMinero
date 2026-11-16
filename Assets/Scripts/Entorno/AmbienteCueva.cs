using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ElMinero.Entorno
{
    /// <summary>
    /// Ambiente de la mina: polvo que flota frente a la cámara, gotas de agua que
    /// caen de las estalactitas (con salpicadura) y destellos en las vetas de plata
    /// de la roca. Solo decoración: no tiene colisiones.
    /// </summary>
    public class AmbienteCueva : MonoBehaviour
    {
        [Header("Sprites")]
        [SerializeField] private Sprite spritePolvo;
        [SerializeField] private Sprite spriteGota;
        [SerializeField] private Sprite spriteDestello;

        [Header("Polvo")]
        [SerializeField] private int cantidadPolvo = 28;
        [SerializeField] private string capaPolvo = "Primer plano";

        [Header("Gotas (x de cada estalactita, alto del techo y del piso)")]
        [SerializeField] private float[] xGoteras = new float[0];
        [SerializeField] private float yTecho = 5.3f;
        [SerializeField] private float yPiso = -1f;

        [Header("Destellos en vetas")]
        [SerializeField] private Vector2[] puntosVetas = new Vector2[0];

        private Camera cam;
        private readonly List<SpriteRenderer> polvo = new List<SpriteRenderer>();
        private readonly List<Vector3> datosPolvo = new List<Vector3>(); // x: semilla, y: velocidad, z: alfa
        private float proximaGota, proximoDestello;

        private void Start()
        {
            cam = Camera.main;
            if (spritePolvo != null && cam != null)
            {
                for (int i = 0; i < cantidadPolvo; i++)
                {
                    var sr = Crear("Polvo", spritePolvo, PuntoEnVista(), capaPolvo, 2, new Color(0.85f, 0.8f, 0.95f, 0f));
                    sr.transform.localScale = Vector3.one * Random.Range(0.4f, 0.8f);
                    polvo.Add(sr);
                    datosPolvo.Add(new Vector3(Random.Range(0f, 100f), Random.Range(0.08f, 0.25f), Random.Range(0.15f, 0.45f)));
                }
            }
            proximaGota = Time.time + Random.Range(0.5f, 2f);
            proximoDestello = Time.time + 1f;
        }

        private SpriteRenderer Crear(string nombre, Sprite sprite, Vector3 pos, string capa, int orden, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(transform, true);
            go.transform.position = pos;
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingLayerName = capa;
            sr.sortingOrder = orden;
            sr.color = color;
            return sr;
        }

        private Vector3 PuntoEnVista()
        {
            if (cam == null) return Vector3.zero;
            float alto = cam.orthographicSize;
            float ancho = alto * cam.aspect;
            Vector3 c = cam.transform.position;
            return new Vector3(c.x + Random.Range(-ancho, ancho), c.y + Random.Range(-alto, alto), 0f);
        }

        private void Update()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;

            // Polvo: sube y se mece muy despacio; se recicla al salir de la vista
            float alto = cam.orthographicSize;
            float ancho = alto * cam.aspect;
            Vector3 c = cam.transform.position;
            for (int i = 0; i < polvo.Count; i++)
            {
                Vector3 d = datosPolvo[i];
                Transform tr = polvo[i].transform;
                Vector3 p = tr.position;
                p.y += d.y * Time.deltaTime;
                p.x += (Mathf.PerlinNoise(d.x, Time.time * 0.2f) - 0.5f) * 0.6f * Time.deltaTime;
                if (p.y > c.y + alto + 0.3f) p = new Vector3(c.x + Random.Range(-ancho, ancho), c.y - alto - 0.2f, 0f);
                if (p.x < c.x - ancho - 0.5f) p.x += ancho * 2f;
                if (p.x > c.x + ancho + 0.5f) p.x -= ancho * 2f;
                tr.position = p;
                float brillo = Mathf.PerlinNoise(d.x + 50f, Time.time * 0.5f);
                polvo[i].color = new Color(0.85f, 0.8f, 0.95f, d.z * brillo);
            }

            // Gotas: solo las goteras cercanas a la cámara
            if (Time.time >= proximaGota && spriteGota != null && xGoteras.Length > 0)
            {
                proximaGota = Time.time + Random.Range(0.8f, 2.2f);
                for (int intento = 0; intento < 6; intento++)
                {
                    float x = xGoteras[Random.Range(0, xGoteras.Length)];
                    if (Mathf.Abs(x - c.x) < ancho + 1f)
                    {
                        StartCoroutine(Gota(x));
                        break;
                    }
                }
            }

            // Destellos en vetas visibles
            if (Time.time >= proximoDestello && spriteDestello != null && puntosVetas.Length > 0)
            {
                proximoDestello = Time.time + Random.Range(0.4f, 1.1f);
                for (int intento = 0; intento < 6; intento++)
                {
                    Vector2 v = puntosVetas[Random.Range(0, puntosVetas.Length)];
                    if (Mathf.Abs(v.x - c.x) < ancho && Mathf.Abs(v.y - c.y) < alto)
                    {
                        StartCoroutine(Destello(v));
                        break;
                    }
                }
            }
        }

        private IEnumerator Gota(float x)
        {
            var sr = Crear("Gota", spriteGota, new Vector3(x, yTecho, 0f), "Objetos", 5, Color.white);
            // Se forma colgando un momento
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                sr.transform.localScale = new Vector3(1f, Mathf.Lerp(0.3f, 1.2f, t / 0.5f), 1f);
                yield return null;
            }
            // Cae acelerando
            float v = 0f;
            Vector3 p = sr.transform.position;
            while (p.y > yPiso + 0.1f)
            {
                v += 22f * Time.deltaTime;
                p.y -= v * Time.deltaTime;
                sr.transform.position = p;
                yield return null;
            }
            // Salpicadura: cuatro gotitas que saltan
            Destroy(sr.gameObject);
            var gotitas = new List<SpriteRenderer>();
            var vel = new List<Vector2>();
            for (int i = 0; i < 4; i++)
            {
                var g = Crear("Salpicadura", spriteGota, new Vector3(x, yPiso + 0.05f, 0f), "Objetos", 5, Color.white);
                g.transform.localScale = Vector3.one * 0.6f;
                gotitas.Add(g);
                vel.Add(new Vector2(Random.Range(-1.6f, 1.6f), Random.Range(1.6f, 2.8f)));
            }
            t = 0f;
            while (t < 0.4f)
            {
                t += Time.deltaTime;
                for (int i = 0; i < gotitas.Count; i++)
                {
                    Vector2 vv = vel[i];
                    vv.y -= 14f * Time.deltaTime;
                    vel[i] = vv;
                    gotitas[i].transform.position += (Vector3)(vv * Time.deltaTime);
                    gotitas[i].color = new Color(1f, 1f, 1f, 1f - t / 0.4f);
                }
                yield return null;
            }
            foreach (var g in gotitas) Destroy(g.gameObject);
        }

        private IEnumerator Destello(Vector2 pos)
        {
            var sr = Crear("Destello_Veta", spriteDestello, pos, "Escenario", 20, new Color(1f, 1f, 1f, 0f));
            float t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(t / 0.5f * Mathf.PI);
                sr.color = new Color(0.9f, 0.95f, 1f, k);
                sr.transform.localScale = Vector3.one * (0.4f + k * 0.7f);
                sr.transform.rotation = Quaternion.Euler(0f, 0f, t * 120f);
                yield return null;
            }
            Destroy(sr.gameObject);
        }
    }
}
