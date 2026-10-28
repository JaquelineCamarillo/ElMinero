using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Roca que cae desde el techo de la mina (FallingRock).
    /// Daña al jugador (-1 vida) al impactarlo y se destruye al chocar con el suelo
    /// generando partículas o efecto de polvo.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class FallingRock : MonoBehaviour
    {
        [Header("Configuración")]
        [Tooltip("Velocidad de rotación visual mientras cae.")]
        [SerializeField] private float rotationSpeed = 360f;

        [Tooltip("Prefab o sprite de polvo que se genera al destruirse.")]
        [SerializeField] private GameObject dustEffectPrefab;

        private Rigidbody2D rb;
        private bool hasCollided = false;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            tag = Constantes.Tags.Hazard;

            int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (layerPeligro != -1)
            {
                gameObject.layer = layerPeligro;
            }

            // Física reactiva y desprendimiento veloz
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 4.5f;
        }

        private void Update()
        {
            // Rotación visual durante la caída
            transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

            if (transform.position.y < -20f)
            {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (hasCollided) return;

            // Chocar contra el jugador
            if (collision.gameObject.CompareTag(Constantes.Tags.Player))
            {
                hasCollided = true;
                var health = collision.gameObject.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(1);
                }
                DestruirConEfecto();
                return;
            }

            // Chocar contra el suelo o escenario
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (collision.gameObject.layer == layerSuelo || collision.gameObject.CompareTag("Untagged"))
            {
                hasCollided = true;
                DestruirConEfecto();
            }
        }

        private void DestruirConEfecto()
        {
            if (dustEffectPrefab != null)
            {
                Instantiate(dustEffectPrefab, transform.position, Quaternion.identity);
            }
            Destroy(gameObject);
        }
    }
}
