using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;
using ElMinero.Managers;

namespace ElMinero.Enemigos
{
    /// <summary>
    /// Clase base abstracta para todos los enemigos del juego.
    /// Implementa IResettable para restaurar estado en checkpoints o reinicios.
    /// Realiza daño al jugador respetando invulnerabilidad, dibuja Gizmos y maneja animación.
    /// </summary>
    public abstract class EnemyBase : MonoBehaviour, IResettable
    {
        [Header("Configuración Base de Enemigo")]
        [Tooltip("Daño infligido al jugador al hacer contacto (por defecto 1 corazón).")]
        [SerializeField] protected int damage = 1;

        [Tooltip("Referencia al Animator del enemigo.")]
        [SerializeField] protected Animator animator;

        [Tooltip("Referencia al SpriteRenderer para orientación visual.")]
        [SerializeField] protected SpriteRenderer spriteRenderer;

        protected Vector3 initialPosition;
        protected Quaternion initialRotation;
        protected Vector3 initialScale;
        protected Transform playerTransform;
        protected PlayerHealth playerHealth;

        protected virtual void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();

            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialScale = transform.localScale;
        }

        protected virtual void Start()
        {
            LocalizarJugador();
        }

        protected void LocalizarJugador()
        {
            if (playerTransform == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                    playerHealth = player.GetComponent<PlayerHealth>();
                }
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                AplicarDanioAJugador(other.gameObject);
            }
        }

        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                AplicarDanioAJugador(collision.gameObject);
            }
        }

        protected virtual void AplicarDanioAJugador(GameObject playerObj)
        {
            if (playerHealth == null)
            {
                playerHealth = playerObj.GetComponent<PlayerHealth>();
            }

            if (playerHealth != null)
            {
                bool danioAplicado = playerHealth.TakeDamage(damage);
                if (danioAplicado && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDanio();
                }
            }
        }

        /// <summary>
        /// Restablece el enemigo a su posición y estado inicial.
        /// </summary>
        public virtual void ResetObject()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            transform.localScale = initialScale;
            gameObject.SetActive(true);
        }

        protected abstract void OnDrawGizmosSelected();
    }
}
