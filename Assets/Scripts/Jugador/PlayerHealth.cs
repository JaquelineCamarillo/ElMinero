using System;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Jugador
{
    /// <summary>
    /// Gestiona la vida del jugador (3 corazones), recepción de daño,
    /// eventos de pérdida de vida y muerte, y restauración de salud.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerHealth : MonoBehaviour, IResettable
    {
        [Header("Configuración de Vidas")]
        [Tooltip("Cantidad máxima de vidas (según GDD: 3 corazones).")]
        [SerializeField] private int maxLives = 3;

        private int currentLives;
        private PlayerController playerController;

        // Eventos C# desacoplados
        public event Action<int> OnHealthChanged;      // Notifica vidas actuales
        public event Action<int> OnLifeLost;           // Notifica vida perdida
        public event Action OnDeath;                   // Notifica muerte (0 vidas)

        public int CurrentLives => currentLives;
        public int MaxLives => maxLives;
        public bool IsDead => currentLives <= 0;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            currentLives = maxLives;
        }

        private void Start()
        {
            OnHealthChanged?.Invoke(currentLives);
        }

        /// <summary>
        /// Aplica daño al jugador si no se encuentra en estado invulnerable.
        /// </summary>
        public bool TakeDamage(int damage = 1)
        {
            if (IsDead) return false;
            if (playerController != null && playerController.IsInvulnerable) return false;

            currentLives = Mathf.Max(0, currentLives - damage);
            OnHealthChanged?.Invoke(currentLives);
            OnLifeLost?.Invoke(currentLives);

            if (currentLives <= 0)
            {
                OnDeath?.Invoke();
            }
            else
            {
                // Activar invulnerabilidad temporal y parpadeo
                if (playerController != null)
                {
                    playerController.TriggerInvulnerability();
                }
            }

            return true;
        }

        /// <summary>
        /// Restablece la salud al valor máximo (llamado al reiniciar el nivel).
        /// </summary>
        public void ResetObject()
        {
            currentLives = maxLives;
            OnHealthChanged?.Invoke(currentLives);
        }
    }
}
