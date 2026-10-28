using System;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Coleccionables
{
    /// <summary>
    /// Sistema de racha de recolección (StreakSystem).
    /// Recolectar 3 coleccionables consecutivos sin recibir daño activa un multiplicador x2 por 10 segundos.
    /// Recibir daño o reiniciar el nivel reinicia el progreso de la racha.
    /// </summary>
    public class StreakSystem : MonoBehaviour, IResettable
    {
        [Header("Configuración de Racha")]
        [Tooltip("Cantidad consecutiva de coleccionables requerida para activar el multiplicador (GDD: 3).")]
        [SerializeField] private int requiredStreak = 3;

        [Tooltip("Duración en segundos del multiplicador x2 (GDD: 10 s).")]
        [SerializeField] private float multiplierDuration = 10f;

        private int currentStreak = 0;
        private float multiplierTimer = 0f;
        private bool isMultiplierActive = false;

        // Eventos C#
        public event Action<int, int> OnStreakCountChanged;          // (actual, objetivo)
        public event Action<bool, float> OnMultiplierStateChanged;     // (activo, duracion)
        public event Action<float, float> OnMultiplierTimerUpdated;    // (tiempoRestante, duracionTotal)

        public int CurrentStreak => currentStreak;
        public int RequiredStreak => requiredStreak;
        public bool IsMultiplierActive => isMultiplierActive;
        public float RemainingMultiplierTime => multiplierTimer;
        public int MultiplierValue => isMultiplierActive ? 2 : 1;

        private void Update()
        {
            if (!isMultiplierActive) return;

            multiplierTimer -= Time.deltaTime;
            OnMultiplierTimerUpdated?.Invoke(multiplierTimer, multiplierDuration);

            if (multiplierTimer <= 0f)
            {
                DesactivarMultiplicador();
            }
        }

        /// <summary>
        /// Registra la recolección de una veta o gema.
        /// </summary>
        public void RegistrarColeccionable()
        {
            if (!isMultiplierActive)
            {
                currentStreak++;
                OnStreakCountChanged?.Invoke(currentStreak, requiredStreak);

                if (currentStreak >= requiredStreak)
                {
                    ActivarMultiplicador();
                }
            }
            else
            {
                // Si el multiplicador ya está activo, recolectar refresca el tiempo a 10s
                multiplierTimer = multiplierDuration;
            }
        }

        private void ActivarMultiplicador()
        {
            isMultiplierActive = true;
            multiplierTimer = multiplierDuration;
            currentStreak = 0;
            OnMultiplierStateChanged?.Invoke(true, multiplierDuration);
            OnStreakCountChanged?.Invoke(currentStreak, requiredStreak);
        }

        private void DesactivarMultiplicador()
        {
            isMultiplierActive = false;
            multiplierTimer = 0f;
            currentStreak = 0;
            OnMultiplierStateChanged?.Invoke(false, 0f);
            OnStreakCountChanged?.Invoke(currentStreak, requiredStreak);
        }

        /// <summary>
        /// Aplica el multiplicador al puntaje base recibido.
        /// </summary>
        public int AplicarMultiplicador(int puntajeBase)
        {
            return puntajeBase * MultiplierValue;
        }

        /// <summary>
        /// Reinicia la racha (llamado cuando el jugador recibe daño).
        /// </summary>
        public void CortarRachaPorDanio()
        {
            DesactivarMultiplicador();
        }

        public void ResetObject()
        {
            DesactivarMultiplicador();
        }
    }
}
