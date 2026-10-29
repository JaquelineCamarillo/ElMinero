using UnityEngine;
using ElMinero.Managers;
using ElMinero.Utilidades;

namespace ElMinero.Enemigos
{
    /// <summary>
    /// IA del Alacrán (Nivel 3).
    /// Máquina de estados finita: Patrulla -> Persecución -> Ataque -> Enfriamiento.
    /// Patrulla sin caerse de plataformas a 1.5 u/s con detección de suelo desde la base de sus patas
    /// comprobando exclusivamente la capa Suelo.
    /// Persigue al jugador a 3.5 u/s si tiene línea de visión (5 u horizontal, ±1 u vertical).
    /// A 1.3 u ejecuta ataque telegrafiado de 0.35 s con daño frontal.
    /// </summary>
    public class ScorpionAI : EnemyBase
    {
        private enum ScorpionState { Patrulla, Persecucion, Ataque, Enfriamiento }

        [Header("Configuración de Movimiento")]
        [Tooltip("Velocidad en modo patrulla (1.5 u/s).")]
        [SerializeField] private float patrolSpeed = 1.5f;

        [Tooltip("Velocidad en modo persecución (3.5 u/s).")]
        [SerializeField] private float chaseSpeed = 3.5f;

        [Header("Detección y Combate")]
        [Tooltip("Rango horizontal de visión (5 u).")]
        [SerializeField] private float sightRange = 5f;

        [Tooltip("Tolerancia vertical para ver al jugador (±1 u).")]
        [SerializeField] private float heightTolerance = 1.0f;

        [Tooltip("Distancia para iniciar el ataque (1.3 u).")]
        [SerializeField] private float attackRange = 1.3f;

        [Tooltip("Tiempo de anticipación / telegrafía del golpe (0.35 s).")]
        [SerializeField] private float attackTelegraphTime = 0.35f;

        [Tooltip("Tiempo de enfriamiento tras atacar (1.5 s).")]
        [SerializeField] private float attackCooldown = 1.5f;

        [Tooltip("Tiempo sin ver al jugador antes de regresar a patrulla (2 s).")]
        [SerializeField] private float loseSightDuration = 2.0f;

        [Header("Raycasts de Entorno")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask obstacleLayer;

        private ScorpionState state = ScorpionState.Patrulla;
        private int facingDirection = -1; // -1 = izquierda (default del sprite), 1 = derecha
        private float stateTimer = 0f;
        private float loseSightTimer = 0f;
        private bool hasHitInCurrentAttack = false;
        private Collider2D selfCollider;

        protected override void Awake()
        {
            base.Awake();
            selfCollider = GetComponent<Collider2D>();
            groundLayer = LayerMask.GetMask(Constantes.Layers.Suelo);
            obstacleLayer = LayerMask.GetMask(Constantes.Layers.Suelo);
        }

        private void Update()
        {
            if (playerTransform == null)
            {
                LocalizarJugador();
            }

            switch (state)
            {
                case ScorpionState.Patrulla:
                    ActualizarPatrulla();
                    break;

                case ScorpionState.Persecucion:
                    ActualizarPersecucion();
                    break;

                case ScorpionState.Ataque:
                    ActualizarAtaque();
                    break;

                case ScorpionState.Enfriamiento:
                    ActualizarEnfriamiento();
                    break;
            }

            ActualizarOrientacionSprite();
        }

        private void ActualizarPatrulla()
        {
            // Verificar borde de plataforma y pared en la dirección actual
            if (HayParedEnFrente() || !HaySueloEnFrente())
            {
                facingDirection = -facingDirection;
            }

            transform.Translate(Vector3.right * (facingDirection * patrolSpeed * Time.deltaTime));

            if (TieneLineaDeVisionConJugador())
            {
                state = ScorpionState.Persecucion;
                loseSightTimer = 0f;
            }
        }

        private void ActualizarPersecucion()
        {
            if (playerTransform == null)
            {
                state = ScorpionState.Patrulla;
                return;
            }

            float distJugador = Vector2.Distance(transform.position, playerTransform.position);
            int dirHaciaJugador = playerTransform.position.x > transform.position.x ? 1 : -1;

            if (TieneLineaDeVisionConJugador())
            {
                loseSightTimer = 0f;
                facingDirection = dirHaciaJugador;

                // Si está en rango de ataque, comenzar ataque
                if (distJugador <= attackRange)
                {
                    IniciarAtaque();
                    return;
                }

                // Perseguir cuidando no caerse de la plataforma
                if (HaySueloEnFrente() && !HayParedEnFrente())
                {
                    transform.Translate(Vector3.right * (facingDirection * chaseSpeed * Time.deltaTime));
                }
            }
            else
            {
                loseSightTimer += Time.deltaTime;
                if (loseSightTimer >= loseSightDuration)
                {
                    state = ScorpionState.Patrulla;
                }
                else
                {
                    if (HaySueloEnFrente() && !HayParedEnFrente())
                    {
                        transform.Translate(Vector3.right * (facingDirection * chaseSpeed * Time.deltaTime));
                    }
                }
            }
        }

        private void IniciarAtaque()
        {
            state = ScorpionState.Ataque;
            stateTimer = attackTelegraphTime;
            hasHitInCurrentAttack = false;

            if (animator != null)
            {
                animator.SetTrigger("Atacar");
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayAlacranAtaque();
            }
        }

        private void ActualizarAtaque()
        {
            stateTimer -= Time.deltaTime;

            // Al cumplirse el tiempo de anticipación (0.35 s), se aplica el golpe en el área frontal
            if (!hasHitInCurrentAttack && stateTimer <= 0f)
            {
                hasHitInCurrentAttack = true;
                EjecutarGolpeFrontal();
                state = ScorpionState.Enfriamiento;
                stateTimer = attackCooldown;
            }
        }

        private void EjecutarGolpeFrontal()
        {
            Vector2 attackCenter = (Vector2)transform.position + new Vector2(facingDirection * 0.8f, 0.2f);
            Collider2D hit = Physics2D.OverlapCircle(attackCenter, 0.6f, LayerMask.GetMask(Constantes.Layers.Jugador, "Default"));

            if (hit != null && hit.CompareTag(Constantes.Tags.Player))
            {
                AplicarDanioAJugador(hit.gameObject);
            }
        }

        private void ActualizarEnfriamiento()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                state = TieneLineaDeVisionConJugador() ? ScorpionState.Persecucion : ScorpionState.Patrulla;
            }
        }

        private bool TieneLineaDeVisionConJugador()
        {
            if (playerTransform == null) return false;

            Vector3 pos = transform.position;
            Vector3 pPos = playerTransform.position;

            float dy = Mathf.Abs(pPos.y - pos.y);
            if (dy > heightTolerance) return false;

            float dx = pPos.x - pos.x;
            if (Mathf.Abs(dx) > sightRange) return false;

            // Raycast para validar que no haya pared entre el alacrán y el jugador
            Vector2 origin = new Vector2(pos.x, pos.y + 0.3f);
            Vector2 dir = new Vector2(dx, 0f).normalized;
            RaycastHit2D hit = Physics2D.Raycast(origin, dir, Mathf.Abs(dx), obstacleLayer);

            return (hit.collider == null || hit.collider.CompareTag(Constantes.Tags.Player));
        }

        private bool HaySueloEnFrente()
        {
            // Disparar desde las patas (bounds.min.y + 0.05) hacia abajo exclusivamente a Suelo
            float pieY = selfCollider != null ? selfCollider.bounds.min.y + 0.05f : transform.position.y - 0.2f;
            Vector2 checkPos = new Vector2(transform.position.x + facingDirection * 0.45f, pieY);

            RaycastHit2D hit = Physics2D.Raycast(checkPos, Vector2.down, 0.45f, groundLayer);
            return hit.collider != null && hit.collider != selfCollider;
        }

        private bool HayParedEnFrente()
        {
            float centroY = selfCollider != null ? selfCollider.bounds.center.y : transform.position.y;
            Vector2 checkPos = new Vector2(transform.position.x + (facingDirection * 0.15f), centroY);

            RaycastHit2D hit = Physics2D.Raycast(checkPos, new Vector2(facingDirection, 0f), 0.45f, obstacleLayer);
            return hit.collider != null && hit.collider != selfCollider && !hit.collider.CompareTag(Constantes.Tags.Player);
        }

        private void ActualizarOrientacionSprite()
        {
            if (spriteRenderer != null)
            {
                // El sprite original mira a la izquierda. Si va a la derecha (1), flipX = true.
                spriteRenderer.flipX = (facingDirection > 0);
            }
        }

        public override void ResetObject()
        {
            base.ResetObject();
            state = ScorpionState.Patrulla;
            facingDirection = -1;
            stateTimer = 0f;
            loseSightTimer = 0f;
            hasHitInCurrentAttack = false;
        }

        protected override void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 origin = transform.position;
            Vector3 sightDir = new Vector3(facingDirection * sightRange, 0f, 0f);
            Gizmos.DrawLine(origin, origin + sightDir);

            // Hitbox de ataque frontal
            Vector3 attackCenter = origin + new Vector3(facingDirection * 0.8f, 0.2f, 0f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackCenter, 0.6f);
        }
    }
}
