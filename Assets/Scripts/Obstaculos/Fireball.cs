using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Bola de fuego que cae desde el techo de la mina (Nivel 3).
    /// Cae con aceleración gravitacional (gravityScale = 2.5).
    /// Al colisionar con el suelo o el jugador, inflige daño y estalla desapareciendo.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public class Fireball : MonoBehaviour
    {
        [Tooltip("Daño infligido al jugador.")]
        [SerializeField] private int damage = 1;

        private Rigidbody2D rb;
        private CircleCollider2D col;
        private bool hasExploded = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();

            tag = Constantes.Tags.Hazard;
            int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (layerPeligro != -1) gameObject.layer = layerPeligro;

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.gravityScale = 2.5f;
            }

            if (col != null)
            {
                col.radius = 0.45f;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasExploded) return;

            if (collision.gameObject.CompareTag(Constantes.Tags.Player))
            {
                var health = collision.gameObject.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    bool golpe = health.TakeDamage(damage);
                    if (golpe && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDanio();
                    }
                }
            }

            Explotar();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasExploded) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    bool golpe = health.TakeDamage(damage);
                    if (golpe && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDanio();
                    }
                }
                Explotar();
            }
        }

        private void Explotar()
        {
            hasExploded = true;
            Destroy(gameObject);
        }
    }
}
