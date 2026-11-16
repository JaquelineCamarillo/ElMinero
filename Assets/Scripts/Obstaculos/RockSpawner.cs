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

        [Header("Activación")]
        [Tooltip("Si es verdadero, el generador no suelta rocas solo: espera SetActivo(true) o SoltarAhora() (tiro del elevador, arena del jefe).")]
        [SerializeField] private bool requiereActivacion = false;

        [Tooltip("Si se asigna, la sombra de aviso aparece sobre este objeto (por ejemplo, el piso de la jaula del elevador).")]
        [SerializeField] private Transform sombraSobre;

        private float spawnTimer = 0f;
        private bool activo = true;
        private Coroutine spawnCoroutine;
        private readonly List<GameObject> activeRocks = new List<GameObject>();

        public bool Activo => activo;

        public void Configurar(bool soloConActivacion, Transform objetivoSombra)
        {
            requiereActivacion = soloConActivacion;
            sombraSobre = objetivoSombra;
            activo = !requiereActivacion;
        }

        public void SetActivo(bool valor)
        {
            if (valor && !activo) spawnTimer = Mathf.Min(spawnTimer, spawnInterval);
            activo = valor;
        }

        /// <summary>Suelta una roca de inmediato (con su aviso), aunque el generador esté inactivo.</summary>
        public void SoltarAhora(float retraso = 0f)
        {
            StartCoroutine(RutinaAdvertenciaYSoltarRoca(retraso));
        }

        private void Awake()
        {
            activo = !requiereActivacion;
        }

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
            if (!activo) return;

            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                spawnTimer = spawnInterval;
                if (spawnCoroutine == null)
                {
                    spawnCoroutine = StartCoroutine(RutinaAdvertenciaYSoltarRoca(0f));
                }
            }
        }

        private IEnumerator RutinaAdvertenciaYSoltarRoca(float retraso)
        {
            if (retraso > 0f) yield return new WaitForSeconds(retraso);

            GameObject indicator = null;

            // 1. Mostrar advertencia visual antes de soltar la roca
            if (warningIndicatorPrefab != null)
            {
                Vector3 warningPos = transform.position + warningOffset;
                if (sombraSobre != null)
                {
                    warningPos = new Vector3(transform.position.x, sombraSobre.position.y + 0.1f, 0f);
                }
                indicator = Instantiate(warningIndicatorPrefab, warningPos, Quaternion.identity);
                indicator.SetActive(true); // la plantilla está desactivada dentro del generador
                if (sombraSobre != null) indicator.transform.SetParent(sombraSobre, true);
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
                rock.SetActive(true); // la plantilla está desactivada dentro del generador
                if (ElMinero.Managers.AudioManager.Instance != null) ElMinero.Managers.AudioManager.Instance.PlayRocaCae(transform.position);
                activeRocks.Add(rock);
                activeRocks.RemoveAll(r => r == null);
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

            StopAllCoroutines();
            spawnTimer = spawnInterval;
            activo = !requiereActivacion;

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
