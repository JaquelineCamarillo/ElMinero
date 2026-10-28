using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Trigger que dispara la vagoneta desbocada cuando el jugador lo cruza.
    /// Se reinicia con IResettable si el jugador reaparece en el checkpoint.
    /// </summary>
    public class RunawayCartTrigger : MonoBehaviour, IResettable
    {
        [Tooltip("Referencia a la vagoneta desbocada que debe activarse.")]
        [SerializeField] private RunawayCart targetCart;

        private bool triggered = false;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (triggered) return;

            if (other.CompareTag("Player"))
            {
                triggered = true;
                if (targetCart != null)
                {
                    targetCart.DispararVagoneta();
                }
            }
        }

        public void ResetObject()
        {
            triggered = false;
        }

        public void SetTargetCart(RunawayCart cart)
        {
            targetCart = cart;
        }
    }
}
