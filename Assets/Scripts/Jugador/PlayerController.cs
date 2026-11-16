using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;

namespace ElMinero.Jugador
{
    /// <summary>
    /// Controlador físico del jugador (El Minero).
    /// Maneja el movimiento horizontal a 5 u/s, cálculo exacto de salto para 2.5 u,
    /// coyote time, jump buffer, gravedad dinámica, detección de suelo y parpadeo por invulnerabilidad.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(PlayerInputHandler))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimiento Horizontal")]
        [Tooltip("Velocidad horizontal constante en unidades por segundo (según GDD: 5 u/s).")]
        [SerializeField] private float moveSpeed = 5.2f;

        [Header("Salto y Gravedad")]
        [Tooltip("Altura máxima del salto en unidades (ajustada a 3.2 u para alcanzar holgadamente todas las plataformas).")]
        [SerializeField] private float jumpHeight = 3.2f;

        [Tooltip("Escala de gravedad personalizada aplicada al Rigidbody2D (sugerido: 3).")]
        [SerializeField] private float customGravityScale = 3f;

        [Tooltip("Multiplicador de gravedad al caer para caída más ágil (según GDD: ~1.5).")]
        [SerializeField] private float fallGravityMultiplier = 1.5f;

        [Tooltip("Corte de velocidad ascendente al soltar el botón de salto prematuramente (según GDD: ~0.5).")]
        [SerializeField] private float jumpCutMultiplier = 0.5f;

        [Tooltip("Ventana de tiempo coyote en segundos (según GDD: 0.1 s).")]
        [SerializeField] private float coyoteTime = 0.1f;

        [Tooltip("Ventana de buffer para presionar el salto antes de tocar suelo (según GDD: 0.1 s).")]
        [SerializeField] private float jumpBufferTime = 0.1f;

        [Header("Detección de Suelo")]
        [Tooltip("Offset del centro de la caja de detección respecto a la posición del jugador.")]
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.05f);

        [Tooltip("Dimensiones de la caja para detectar si está sobre el suelo.")]
        [SerializeField] private Vector2 groundCheckSize = new Vector2(0.6f, 0.1f);

        [Tooltip("Capa que representa el suelo transitable.")]
        [SerializeField] private LayerMask groundLayer;

        [Header("Efectos Visuales e Invulnerabilidad")]
        [Tooltip("Referencia al SpriteRenderer del personaje para voltear y parpadear.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Duración de la invulnerabilidad tras recibir un impacto (según GDD: 1.5 s).")]
        [SerializeField] private float invulnerabilityDuration = 1.5f;

        [Tooltip("Intervalo entre parpadeos durante la invulnerabilidad.")]
        [SerializeField] private float blinkInterval = 0.1f;

        // Componentes requeridos
        private Rigidbody2D rb;
        private CapsuleCollider2D capsuleCollider;
        private PlayerInputHandler inputHandler;

        // Variables de estado
        private bool isGrounded;
        private float tiempoEnAire;
        private float temporizadorPasos;
        private float coyoteCounter;
        private float jumpBufferCounter;
        private bool isFacingRight = true;
        private bool isInvulnerable = false;
        private Coroutine invulnerabilityCoroutine;

        // Soporte para plataformas móviles
        private Rigidbody2D activeMovingPlatformRb;

        // Control bloqueado (intro del jefe, escenas cinemáticas) y empujes
        private bool controlBloqueado = false;
        private float movimientoForzado = 0f;
        private float knockbackTimer = 0f;

        // Propiedades públicas para animador, HUD y otros sistemas
        public bool IsGrounded => isGrounded;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public float MoveSpeed => moveSpeed;
        public float JumpHeight => jumpHeight;
        public bool IsInvulnerable => isInvulnerable;
        public bool IsFacingRight => isFacingRight;
        public bool ControlBloqueado => controlBloqueado;

        /// <summary>Entrada horizontal efectiva (la del jugador o la forzada durante una cinemática).</summary>
        private float EntradaHorizontal => controlBloqueado ? movimientoForzado : inputHandler.MoveInput;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            capsuleCollider = GetComponent<CapsuleCollider2D>();
            inputHandler = GetComponent<PlayerInputHandler>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            ConfigurePhysics();
        }

        private void Start()
        {
            // Asegurar que la máscara de suelo siempre incluya la capa Suelo y Default si está vacía
            int maskSuelo = LayerMask.GetMask(Constantes.Layers.Suelo);
            if (maskSuelo != 0)
            {
                groundLayer |= maskSuelo;
            }
            if (groundLayer.value == 0)
            {
                groundLayer = LayerMask.GetMask(Constantes.Layers.Default);
            }
        }

        private void ConfigurePhysics()
        {
            // Configuración obligatoria según sección 4.1
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.gravityScale = customGravityScale;

            // Asegurar que el collider sea vertical
            capsuleCollider.direction = CapsuleDirection2D.Vertical;
        }

        private void Update()
        {
            CheckGround();
            HandleJumpTimers();
            HandleFacingDirection();
            SonidoPasos();
        }

        /// <summary>Pasos alternados mientras camina sobre el suelo.</summary>
        private void SonidoPasos()
        {
            float vx = rb != null ? rb.linearVelocity.x : 0f;
            if (activeMovingPlatformRb != null) vx -= activeMovingPlatformRb.linearVelocity.x;
            if (!isGrounded || Mathf.Abs(vx) < 1f || Time.timeScale <= 0f)
            {
                temporizadorPasos = 0.05f;
                return;
            }
            temporizadorPasos -= Time.deltaTime;
            if (temporizadorPasos <= 0f)
            {
                temporizadorPasos = 0.3f;
                if (AudioManager.Instance != null) AudioManager.Instance.PlayPasos();
            }
        }

        private void FixedUpdate()
        {
            ApplyHorizontalMovement();
            ApplyVerticalPhysics();
        }

        private void CheckGround()
        {
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            bool wasGrounded = isGrounded;
            isGrounded = Physics2D.OverlapBox(checkPosition, groundCheckSize, 0f, groundLayer);

            if (isGrounded)
            {
                // Aterrizaje: solo si de verdad estuvo en el aire (no en pequeños escalones)
                if (!wasGrounded && tiempoEnAire > 0.15f && Time.timeSinceLevelLoad > 0.5f)
                {
                    if (AudioManager.Instance != null) AudioManager.Instance.PlayAterrizar();
                    temporizadorPasos = 0.25f;
                }
                tiempoEnAire = 0f;
                coyoteCounter = coyoteTime;
            }
            else
            {
                tiempoEnAire += Time.deltaTime;
                coyoteCounter -= Time.deltaTime;
            }
        }

        private void HandleJumpTimers()
        {
            if (controlBloqueado)
            {
                jumpBufferCounter = 0f;
                return;
            }

            // Buffer de salto al pulsar
            if (inputHandler.JumpTriggered)
            {
                jumpBufferCounter = jumpBufferTime;
            }
            else
            {
                jumpBufferCounter -= Time.deltaTime;
            }

            // Ejecución del salto si cumple condiciones de coyote y buffer
            if (jumpBufferCounter > 0f && coyoteCounter > 0f)
            {
                ExecuteJump();
                jumpBufferCounter = 0f;
                coyoteCounter = 0f;
            }

            // Salto variable: cortar subida al soltar el botón
            if (inputHandler.JumpReleased && rb.linearVelocity.y > 0f)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }

        private void ExecuteJump()
        {
            // Fórmula física del GDD: v = sqrt(2 * g * h)
            // donde g = |Physics2D.gravity.y| * rb.gravityScale
            float effectiveGravity = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;
            float initialJumpVelocity = Mathf.Sqrt(2f * effectiveGravity * jumpHeight);

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, initialJumpVelocity);
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySalto();
        }

        private void ApplyHorizontalMovement()
        {
            if (knockbackTimer > 0f)
            {
                knockbackTimer -= Time.fixedDeltaTime;
                return;
            }

            float targetVelocityX = EntradaHorizontal * moveSpeed;

            // Si está sobre una plataforma móvil, sumar su velocidad horizontal
            if (activeMovingPlatformRb != null)
            {
                targetVelocityX += activeMovingPlatformRb.linearVelocity.x;
            }

            rb.linearVelocity = new Vector2(targetVelocityX, rb.linearVelocity.y);
        }

        private void ApplyVerticalPhysics()
        {
            // Caída más rápida cuando va descendiendo
            if (rb.linearVelocity.y < -0.01f)
            {
                rb.gravityScale = customGravityScale * fallGravityMultiplier;
            }
            else
            {
                rb.gravityScale = customGravityScale;
            }
        }

        private void HandleFacingDirection()
        {
            float entrada = EntradaHorizontal;
            if (entrada > 0.05f && !isFacingRight)
            {
                Flip(true);
            }
            else if (entrada < -0.05f && isFacingRight)
            {
                Flip(false);
            }
        }

        private void Flip(bool faceRight)
        {
            isFacingRight = faceRight;
            if (spriteRenderer != null)
            {
                // Solo volteamos el sprite con flipX para no afectar etiquetas hijas (nombre) ni colliders
                spriteRenderer.flipX = !faceRight;
            }
        }

        // ====================================================================
        // GESTIÓN DE DAÑO E INVULNERABILIDAD
        // ====================================================================

        public void TriggerInvulnerability(float duration = -1f)
        {
            if (duration < 0f)
            {
                duration = invulnerabilityDuration;
            }

            if (invulnerabilityCoroutine != null)
            {
                StopCoroutine(invulnerabilityCoroutine);
            }

            invulnerabilityCoroutine = StartCoroutine(InvulnerabilityRoutine(duration));
        }

        private IEnumerator InvulnerabilityRoutine(float duration)
        {
            isInvulnerable = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                }
                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
            }

            isInvulnerable = false;
            invulnerabilityCoroutine = null;
        }

        // ====================================================================
        // CONTROL BLOQUEADO, REBOTE Y EMPUJE
        // ====================================================================

        /// <summary>
        /// Quita o devuelve el control al jugador. Con control bloqueado se puede forzar
        /// una caminata automática (-1 izquierda, 0 quieto, 1 derecha).
        /// </summary>
        public void BloquearControl(bool bloquear, float caminarForzado = 0f)
        {
            controlBloqueado = bloquear;
            movimientoForzado = bloquear ? Mathf.Clamp(caminarForzado, -1f, 1f) : 0f;
            if (bloquear && Mathf.Approximately(caminarForzado, 0f) && rb != null)
            {
                rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            }
        }

        /// <summary>Rebote vertical (por ejemplo, al caerle encima al jefe).</summary>
        public void Rebotar(float factor = 1f)
        {
            if (rb == null) return;
            float g = Mathf.Abs(Physics2D.gravity.y) * customGravityScale;
            float v = Mathf.Sqrt(2f * g * jumpHeight) * factor;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, v);
            coyoteCounter = 0f;
        }

        /// <summary>Empuje con velocidad fija durante un tiempo corto (ignora la entrada horizontal).</summary>
        public void Empujar(Vector2 velocidad, float duracion = 0.35f)
        {
            if (rb == null) return;
            rb.linearVelocity = velocidad;
            knockbackTimer = duracion;
        }

        // ====================================================================
        // PLATAFORMAS MÓVILES
        // ====================================================================

        public void SetMovingPlatform(Rigidbody2D platformRb)
        {
            activeMovingPlatformRb = platformRb;
        }

        // ====================================================================
        // GIZMOS PARA INSPECCIÓN EN EDITOR
        // ====================================================================

        private void OnDrawGizmosSelected()
        {
            // 1. Caja de detección de suelo
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Vector2 checkPosition = (Vector2)transform.position + groundCheckOffset;
            Gizmos.DrawWireCube(checkPosition, groundCheckSize);

            // 2. Gizmo de altura máxima de salto (2.5 u) según criterio 12
            Gizmos.color = Color.cyan;
            Vector3 jumpPeak = transform.position + Vector3.up * jumpHeight;
            Gizmos.DrawLine(transform.position, jumpPeak);
            Gizmos.DrawWireCube(jumpPeak, new Vector3(0.5f, 0.05f, 0f));
        }
    }
}
