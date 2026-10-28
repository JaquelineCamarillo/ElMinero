using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Hueco letal (DeathPit). Trigger ubicado en fosas o precipicios.
    /// Al caer en él, el jugador pierde 1 corazón de vida y reaparece en el último checkpoint.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class DeathPit : MonoBehaviour
    {
        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
            tag = Constantes.Tags.Hazard;

            int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (layerPeligro != -1)
            {
                gameObject.layer = layerPeligro;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag(Constantes.Tags.Player))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(1);
                }
            }
        }
    }
}
