using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Generador periódico de rocas que caen (RockSpawner).
    /// Muestra un indicador de advertencia (polvo del techo o sombra en el suelo)
    /// 0.6 segundos antes del desprendimiento para dar tiempo de reacción al jugador.
    /// </summary>
    public class RockSpawner : MonoBehaviour, IResettable
    {
        [Header("Configuración de Desprendimiento")]
        [Tooltip("Prefab de la roca que caerá.")]
        [SerializeField] private GameObject rockPrefab;

        [Tooltip("Prefab o Sprite del indicador de advertencia (sombra o polvo).")]
        [SerializeField] private GameObject warningIndicatorPrefab;

        [Tooltip("Intervalo en segundos entre cada desprendimiento de roca.")]
        [SerializeField] private float spawnInterval = 4.0f;

        [Tooltip("Anticipación en segundos para mostrar la advertencia antes de soltar la roca.")]
        [SerializeField] private float warningDuration = 0.35f;

        [Tooltip("Offset vertical del punto de advertencia en el techo.")]
        [SerializeField] private Vector3 warningOffset = new Vector3(0f, -0.5f, 0f);

        private float spawnTimer = 0f;
        private Coroutine spawnCoroutine;
        private readonly List<GameObject> activeRocks = new List<GameObject>();

        public void ConfigurarIntervalo(float nuevoIntervalo)
        {
            spawnInterval = nuevoIntervalo;
        }

        private void Start()
        {
            spawnTimer = spawnInterval;
        }

        private void Update()
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                if (spawnCoroutine == null)
                {
                    spawnCoroutine = StartCoroutine(RutinaAdvertenciaYSoltarRoca());
                }
            }
        }

        private IEnumerator RutinaAdvertenciaYSoltarRoca()
        {
            GameObject indicator = null;

            // 1. Mostrar advertencia visual 0.6 segundos antes
            if (warningIndicatorPrefab != null)
            {
                Vector3 warningPos = transform.position + warningOffset;
                indicator = Instantiate(warningIndicatorPrefab, warningPos, Quaternion.identity);
            }

            yield return new WaitForSeconds(warningDuration);

            // 2. Destruir advertencia
            if (indicator != null)
            {
                Destroy(indicator);
            }

            // 3. Soltar la roca
            if (rockPrefab != null)
            {
                GameObject rock = Instantiate(rockPrefab, transform.position, Quaternion.identity);
                activeRocks.Add(rock);
            }

            spawnCoroutine = null;
        }

        public void ResetObject()
        {
            if (spawnCoroutine != null)
            {
                StopCoroutine(spawnCoroutine);
                spawnCoroutine = null;
            }

            spawnTimer = spawnInterval;

            // Limpiar rocas activas remanentes
            for (int i = activeRocks.Count - 1; i >= 0; i--)
            {
                if (activeRocks[i] != null)
                {
                    Destroy(activeRocks[i]);
                }
            }
            activeRocks.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
            Gizmos.DrawLine(transform.position, transform.position + Vector3.down * 8f);
        }
    }
}
