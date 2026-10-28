using UnityEngine;
using ElMinero.Jugador;
using ElMinero.Managers;

namespace ElMinero.Coleccionables
{
    /// <summary>
    /// Veta de plata (10 puntos). Coleccionable principal situado a lo largo de la ruta.
    /// Recolectar todas las del nivel otorga un bono de 50 puntos en la salida.
    /// </summary>
    public class SilverVein : Collectible
    {
        protected override void Awake()
        {
            basePoints = 10;
            base.Awake();
        }

        protected override void OnCollect(PlayerController player)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RecolectarVeta(this);
            }
        }
    }
}
