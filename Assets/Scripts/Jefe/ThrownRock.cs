using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;

namespace ElMinero.Jefe
{
    /// <summary>
    /// Roca con pirargirita que lanza el Coloso en arco hacia la x del jugador.
    /// Calcula la velocidad inicial para caer en el objetivo (gravityScale 2) y se rompe en polvo al tocar el piso.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class ThrownRock : MonoBehaviour
    {
        private Sprite[] cuadrosPolvo;
        private float vida;
        private bool rota;

        public static ThrownRock Lanzar(Vector3 origen, Vector3 objetivo, Sprite[] cuadros, Sprite[] polvo)
        {
            GameObject go = new GameObject("Roca_Lanzada");
            go.transform.position = origen;
            go.tag = Constantes.Tags.Hazard;
            int capa = LayerMask.NameToLayer(Constantes.Layers.Enemigo);
            if (capa != -1) go.layer = capa;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;
            sr.sortingOrder = 7;
            if (cuadros != null && cuadros.Length > 0) sr.sprite = cuadros[0];

            var anim = go.AddComponent<SpriteFrameAnimator>();
            anim.Configurar(cuadros, 12f, true, true);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            // Tiro parabólico: tiempo de vuelo según la distancia
            float g = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
            float dx = objetivo.x - origen.x;
            float dy = objetivo.y - origen.y;
            float t = Mathf.Clamp(Mathf.Abs(dx) / 8f, 0.6f, 1.3f);
            float vx = dx / t;
            float vy = (dy + 0.5f * g * t * t) / t;
            rb.linearVelocity = new Vector2(vx, vy);

            var roca = go.AddComponent<ThrownRock>();
            roca.cuadrosPolvo = polvo;
            return roca;
        }

        private void Update()
        {
            vida += Time.deltaTime;
            if (vida > 4f) Romper();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (rota) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                var salud = other.GetComponent<PlayerHealth>();
                if (salud != null && salud.TakeDamage(1) && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDanio();
                }
                Romper();
                return;
            }

            if (!other.isTrigger && other.gameObject.layer == LayerMask.NameToLayer(Constantes.Layers.Suelo))
            {
                Romper();
            }
        }

        public void Romper()
        {
            if (rota) return;
            rota = true;
            EfectoTemporal.Crear(transform.position, cuadrosPolvo, 12f, Color.white, 0.8f);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRocaImpacto();
            Destroy(gameObject);
        }
    }
}
