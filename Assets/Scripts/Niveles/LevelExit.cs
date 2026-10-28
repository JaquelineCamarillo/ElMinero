using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Salida del nivel (Túnel o Superficie).
    /// Al ser tocada por el jugador, finaliza el nivel, evalúa el bono de plata completa
    /// y transiciona al siguiente nivel o a la pantalla de victoria.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class LevelExit : MonoBehaviour
    {
        private Collider2D col;
        private bool isTriggered = false;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;
            tag = Constantes.Tags.Exit;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                isTriggered = true;
                if (LevelManager.Instance != null)
                {
                    LevelManager.Instance.CompletarNivel();
                }
            }
        }

        public void ResetExit()
        {
            isTriggered = false;
        }
    }
}
