using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Trigger "X" del mapa del nivel 3 (x = 187, después del jefe).
    /// Cuando el jugador lo cruza, el muro de fuego aparece detrás de él.
    /// Si el muro alcanza al jugador, el muro se apaga y este trigger se rearma.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FireChaseTrigger : MonoBehaviour, IResettable
    {
        [SerializeField] private FireWallChase targetFireWall;
        private bool armado = true;

        public bool Armado => armado;

        public void Configurar(FireWallChase fireWall)
        {
            targetFireWall = fireWall;
        }

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!armado) return;
            if (!other.CompareTag(Constantes.Tags.Player)) return;

            armado = false;
            if (targetFireWall != null)
            {
                targetFireWall.Activar(other.transform);
            }
        }

        public void Rearmar()
        {
            armado = true;
        }

        public void ResetObject()
        {
            armado = true;
        }
    }
}
