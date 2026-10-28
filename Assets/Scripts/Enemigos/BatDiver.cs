using System.Collections;
using UnityEngine;
using ElMinero.Managers;

namespace ElMinero.Enemigos
{
    /// <summary>
    /// Murciélago rojo agresivo (Nivel 3).
    /// Cuelga en el techo. Si el jugador entra en su cono de detección inferior (6 u horizontal, 7 u vertical),
    /// telegrafía durante 0.4 s (temblor visual y chillido), desciende en picada a 7 u/s,
    /// regresa a su posición a 3 u/s y reposa 2 s antes de volver a atacar.
    /// </summary>
    public class BatDiver : EnemyBase
    {
        private enum BatState { Hanging, Telegraphing, Diving, Returning, Cooldown }

        [Header("Cono de Detección Inferior")]
        [Tooltip("Ancho horizontal del área de detección (6 u).")]
        [SerializeField] private float detectionWidth = 6f;

        [Tooltip("Profundidad vertical hacia abajo (7 u).")]
        [SerializeField] private float detectionDepth = 7f;

        [Header("Cinemática de Picada")]
        [Tooltip("Duración del aviso previo / telegrafía (0.4 s).")]
        [SerializeField] private float telegraphTime = 0.4f;

        [Tooltip("Velocidad de la picada (7 u/s).")]
        [SerializeField] private float diveSpeed = 7f;

        [Tooltip("Velocidad de retorno al techo (3 u/s).")]
        [SerializeField] private float returnSpeed = 3f;

        [Tooltip("Tiempo de enfriamiento en el techo tras atacar (2 s).")]
        [SerializeField] private float cooldownTime = 2f;

        private BatState currentState = BatState.Hanging;
        private Vector3 targetDivePosition;
        private float stateTimer = 0f;

        private void Update()
        {
            if (playerTransform == null)
            {
                LocalizarJugador();
                return;
            }

            switch (currentState)
            {
                case BatState.Hanging:
                    VerificarDeteccionJugador();
                    break;

                case BatState.Telegraphing:
                    ActualizarTelegrafia();
                    break;

                case BatState.Diving:
                    ActualizarPicada();
                    break;

                case BatState.Returning:
                    ActualizarRetorno();
                    break;

                case BatState.Cooldown:
                    ActualizarEnfriamiento();
                    break;
            }
        }

        private void VerificarDeteccionJugador()
        {
            Vector3 pos = transform.position;
            Vector3 pPos = playerTransform.position;

            // Cono hacia abajo: horizontal dentro de [-3, 3] y vertical dentro de [-7, 0]
            float dx = Mathf.Abs(pPos.x - pos.x);
            float dy = pos.y - pPos.y;

            if (dx <= (detectionWidth * 0.5f) && dy > 0f && dy <= detectionDepth)
            {
                IniciarTelegrafia(pPos);
            }
        }

        private void IniciarTelegrafia(Vector3 targetPos)
        {
            currentState = BatState.Telegraphing;
            stateTimer = telegraphTime;
            targetDivePosition = targetPos;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMurcielagoChillido();
            }
        }

        private void ActualizarTelegrafia()
        {
            stateTimer -= Time.deltaTime;

            // Pequeño temblor visual
            float shake = Mathf.Sin(Time.time * 40f) * 0.08f;
            transform.position = initialPosition + new Vector3(shake, 0f, 0f);

            if (stateTimer <= 0f)
            {
                currentState = BatState.Diving;
            }
        }

        private void ActualizarPicada()
        {
            transform.position = Vector3.MoveTowards(transform.position, targetDivePosition, diveSpeed * Time.deltaTime);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (targetDivePosition.x > transform.position.x);
            }

            if (Vector3.Distance(transform.position, targetDivePosition) <= 0.1f)
            {
                currentState = BatState.Returning;
            }
        }

        private void ActualizarRetorno()
        {
            transform.position = Vector3.MoveTowards(transform.position, initialPosition, returnSpeed * Time.deltaTime);

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = (initialPosition.x > transform.position.x);
            }

            if (Vector3.Distance(transform.position, initialPosition) <= 0.05f)
            {
                transform.position = initialPosition;
                currentState = BatState.Cooldown;
                stateTimer = cooldownTime;
            }
        }

        private void ActualizarEnfriamiento()
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                currentState = BatState.Hanging;
            }
        }

        public override void ResetObject()
        {
            base.ResetObject();
            currentState = BatState.Hanging;
            stateTimer = 0f;
        }

        protected override void OnDrawGizmosSelected()
        {
            Vector3 origin = Application.isPlaying ? initialPosition : transform.position;

            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.35f);
            Vector3 boxCenter = new Vector3(origin.x, origin.y - (detectionDepth * 0.5f), origin.z);
            Vector3 boxSize = new Vector3(detectionWidth, detectionDepth, 0.1f);
            Gizmos.DrawWireCube(boxCenter, boxSize);

            // Línea de referencia inicial
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(origin, 0.15f);
        }
    }
}
