using System.Collections;
using UnityEngine;
using Unity.Cinemachine;
using ElMinero.Utilidades;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Temblor periódico de la mina (MineTremor).
    /// Ocurre aleatoriamente cada 20 a 25 segundos con una duración de 1.5 segundos.
    /// Utiliza Cinemachine Impulse para sacudir la cámara y reduce temporalmente el control aéreo.
    /// </summary>
    public class MineTremor : MonoBehaviour, IResettable
    {
        [Header("Intervalos de Temblor")]
        [Tooltip("Intervalo mínimo en segundos entre temblores.")]
        [SerializeField] private float minInterval = 10f;

        [Tooltip("Intervalo máximo en segundos entre temblores.")]
        [SerializeField] private float maxInterval = 16f;

        [Tooltip("Duración en segundos del temblor (GDD: 1.5 s).")]
        [SerializeField] private float tremorDuration = 1.5f;

        [Header("Cinemachine")]
        [Tooltip("Fuente de impulso para sacudir la cámara.")]
        [SerializeField] private CinemachineImpulseSource impulseSource;

        private float nextTremorTime;
        private bool isTremoring = false;
        private Coroutine tremorCoroutine;

        public bool IsTremoring => isTremoring;

        private void Awake()
        {
            if (impulseSource == null)
            {
                impulseSource = GetComponent<CinemachineImpulseSource>();
                if (impulseSource == null)
                {
                    impulseSource = gameObject.AddComponent<CinemachineImpulseSource>();
                }
            }

            ProgramarSiguienteTemblor();
        }

        private void ProgramarSiguienteTemblor()
        {
            nextTremorTime = Random.Range(minInterval, maxInterval);
        }

        private void Update()
        {
            if (isTremoring) return;

            nextTremorTime -= Time.deltaTime;
            if (nextTremorTime <= 0f)
            {
                tremorCoroutine = StartCoroutine(EjecutarTemblor());
            }
        }

        private IEnumerator EjecutarTemblor()
        {
            isTremoring = true;

            // 1. Generar impulso continuo o pulsos durante 1.5s
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(0.8f);
            }

            Debug.Log("[El Minero] ¡Temblor de mina en curso!");

            yield return new WaitForSeconds(tremorDuration);

            isTremoring = false;
            tremorCoroutine = null;
            ProgramarSiguienteTemblor();
        }

        public void ResetObject()
        {
            if (tremorCoroutine != null)
            {
                StopCoroutine(tremorCoroutine);
                tremorCoroutine = null;
            }

            isTremoring = false;
            ProgramarSiguienteTemblor();
        }
    }
}
