using UnityEngine;
using UnityEngine.Rendering.Universal;
using ElMinero.Utilidades;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Punto de control en el nivel (Checkpoint).
    /// Guarda la posición de reaparición del jugador. Al tocarlo se enciende visualmente
    /// cambiando su sprite y activando su luz 2D.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour, IResettable
    {
        [Header("Sprites de Estado")]
        [Tooltip("Sprite cuando el checkpoint está apagado.")]
        [SerializeField] private Sprite spriteOff;

        [Tooltip("Sprite cuando el checkpoint está encendido.")]
        [SerializeField] private Sprite spriteOn;

        [Header("Iluminación 2D")]
        [Tooltip("Luz 2D que se activa al encender el checkpoint.")]
        [SerializeField] private Light2D checkpointLight;

        private Collider2D col;
        private SpriteRenderer sr;
        private bool isActivated = false;

        public bool IsActivated => isActivated;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();

            if (col != null) col.isTrigger = true;
            tag = Constantes.Tags.Checkpoint;

            if (checkpointLight == null)
            {
                checkpointLight = GetComponentInChildren<Light2D>();
            }

            ActualizarVisuales(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                Activar();
            }
        }

        public void Activar()
        {
            if (isActivated) return;

            isActivated = true;
            ActualizarVisuales(true);

            if (Managers.LevelManager.Instance != null)
            {
                Managers.LevelManager.Instance.EstablecerCheckpoint(transform.position);
            }
        }

        private void ActualizarVisuales(bool activa)
        {
            if (sr != null)
            {
                sr.sprite = activa ? (spriteOn != null ? spriteOn : sr.sprite) : (spriteOff != null ? spriteOff : sr.sprite);
            }

            if (checkpointLight != null)
            {
                checkpointLight.enabled = activa;
            }
        }

        public void ResetObject()
        {
            isActivated = false;
            ActualizarVisuales(false);
        }
    }
}
