using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Trigger que activa el muro de fuego perseguidor cuando el minero entra en los últimos 35 tiles de La Valenciana.
    /// </summary>
    public class FireChaseTrigger : MonoBehaviour, IResettable
    {
        [SerializeField] private FireWallChase targetFireWall;
        private bool isTriggered = false;

        public void Configurar(FireWallChase fireWall)
        {
            targetFireWall = fireWall;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                isTriggered = true;
                if (targetFireWall != null)
                {
                    targetFireWall.IniciarPersecucion();
                }
            }
        }

        public void ResetObject()
        {
            isTriggered = false;
        }
    }
}
