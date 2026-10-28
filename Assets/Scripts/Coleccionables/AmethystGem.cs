using UnityEngine;
using ElMinero.Jugador;
using ElMinero.Managers;

namespace ElMinero.Coleccionables
{
    /// <summary>
    /// Gema de amatista (25 puntos). Coleccionable de alto riesgo situado junto a precipicios
    /// o plataformas quebradizas.
    /// </summary>
    public class AmethystGem : Collectible
    {
        protected override void Awake()
        {
            basePoints = 25;
            base.Awake();
        }

        protected override void OnCollect(PlayerController player)
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RecolectarGema(this);
            }
        }
    }
}
