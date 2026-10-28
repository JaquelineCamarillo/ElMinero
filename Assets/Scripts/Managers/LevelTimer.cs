using System;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Managers
{
    /// <summary>
    /// Temporizador en cuenta regresiva para el nivel.
    /// Representa el derrumbe inminente. Al llegar a 0 se detiene el juego y declara derrota.
    /// No se reinicia al perder una vida individual, solo al reiniciar el nivel completo.
    /// </summary>
    public class LevelTimer : MonoBehaviour, IResettable
    {
        [Header("Configuración")]
        [Tooltip("Tiempo límite inicial en segundos.")]
        [SerializeField] private float initialTime = 90f;

        [Tooltip("Umbral de tiempo crítico en segundos (según GDD: 10 s para texto rojo y alerta).")]
        [SerializeField] private float urgentTimeThreshold = 10f;

        private float remainingTime;
        private bool isRunning = false;
        private bool isUrgent = false;

        // Eventos C#
        public event Action<float> OnTimeChanged;     // Notifica segundos restantes
        public event Action OnTimeExpired;            // Notifica tiempo agotado (0s)
        public event Action<bool> OnUrgentStateChanged; // Entra o sale de los últimos 10s

        public float RemainingTime => remainingTime;
        public float InitialTime => initialTime;
        public bool IsRunning => isRunning;
        public bool IsUrgent => isUrgent;

        public void Iniciar(float duracion)
        {
            initialTime = duracion;
            remainingTime = duracion;
            isRunning = true;
            isUrgent = false;
            OnTimeChanged?.Invoke(remainingTime);
        }

        public void Pausar(bool pausado)
        {
            isRunning = !pausado;
        }

        public void Detener()
        {
            isRunning = false;
        }

        private void Update()
        {
            if (!isRunning) return;

            remainingTime -= Time.deltaTime;

            if (remainingTime <= urgentTimeThreshold && !isUrgent)
            {
                isUrgent = true;
                OnUrgentStateChanged?.Invoke(true);
            }

            if (remainingTime <= 0f)
            {
                remainingTime = 0f;
                isRunning = false;
                OnTimeChanged?.Invoke(remainingTime);
                OnTimeExpired?.Invoke();
                return;
            }

            OnTimeChanged?.Invoke(remainingTime);
        }

        public void ResetObject()
        {
            remainingTime = initialTime;
            isRunning = true;
            isUrgent = false;
            OnUrgentStateChanged?.Invoke(false);
            OnTimeChanged?.Invoke(remainingTime);
        }
    }
}
