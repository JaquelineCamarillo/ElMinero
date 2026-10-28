using UnityEngine;

namespace ElMinero.Jugador
{
    /// <summary>
    /// Vincula las mecánicas de PlayerController y PlayerHealth con el Animator del Minero.
    /// Controla los parámetros Speed, IsGrounded, VelocityY, Hurt, Dead y Win.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerAnimator : MonoBehaviour
    {
        private Animator animator;
        private PlayerController controller;
        private PlayerHealth health;
        private Rigidbody2D rb;

        // Hashes para optimizar llamadas al Animator
        private static readonly int HashSpeed = Animator.StringToHash("Speed");
        private static readonly int HashIsGrounded = Animator.StringToHash("IsGrounded");
        private static readonly int HashVelocityY = Animator.StringToHash("VelocityY");
        private static readonly int HashHurt = Animator.StringToHash("Hurt");
        private static readonly int HashDead = Animator.StringToHash("Dead");
        private static readonly int HashWin = Animator.StringToHash("Win");

        private void Awake()
        {
            animator = GetComponent<Animator>();
            controller = GetComponent<PlayerController>();
            health = GetComponent<PlayerHealth>();
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnLifeLost += HandleLifeLost;
                health.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnLifeLost -= HandleLifeLost;
                health.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            if (animator == null || rb == null) return;

#if UNITY_6000_0_OR_NEWER
            Vector2 currentVel = rb.linearVelocity;
#else
            Vector2 currentVel = rb.velocity;
#endif

            // Velocidad horizontal absoluta
            animator.SetFloat(HashSpeed, Mathf.Abs(currentVel.x));

            // Velocidad vertical
            animator.SetFloat(HashVelocityY, currentVel.y);

            // Contacto con el suelo
            bool isGrounded = controller != null ? controller.IsGrounded : true;
            animator.SetBool(HashIsGrounded, isGrounded);
        }

        private void HandleLifeLost(int currentLives)
        {
            if (animator != null && currentLives > 0)
            {
                animator.SetTrigger(HashHurt);
            }
        }

        private void HandleDeath()
        {
            if (animator != null)
            {
                animator.SetTrigger(HashDead);
            }
        }

        /// <summary>
        /// Activa la animación de celebración al completar un nivel.
        /// </summary>
        public void PlayWinAnimation()
        {
            if (animator != null)
            {
                animator.SetTrigger(HashWin);
            }
        }
    }
}
