using UnityEngine;
using Unity.Cinemachine;
using ElMinero.Utilidades;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Zona de derrumbe sorpresa (CaveInZone).
    /// Trigger oculto que se activa la primera vez que el jugador pasa por él.
    /// Hace caer un bloque macizo detrás de él bloqueando el regreso, y genera un temblor de cámara.
    /// Muestra un Gizmo de líneas rojas punteadas en la vista de escena.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CaveInZone : MonoBehaviour, IResettable
    {
        [Header("Bloque de Derrumbe")]
        [Tooltip("Referencia al BloqueDerrumbe que caerá para cerrar el camino.")]
        [SerializeField] private BloqueDerrumbe blockObject;

        [Tooltip("Posición final en el suelo donde cae el bloque detrás del jugador.")]
        [SerializeField] private Vector3 blockDropPosition;

        [Header("Efectos")]
        [Tooltip("Fuente de impulso de Cinemachine para sacudir la cámara.")]
        [SerializeField] private CinemachineImpulseSource impulseSource;

        [Tooltip("Prefab de partículas de polvo al caer el bloque.")]
        [SerializeField] private GameObject dustParticlesPrefab;

        private Collider2D col;
        private bool isTriggered = false;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            if (col != null) col.isTrigger = true;

            if (impulseSource == null)
            {
                impulseSource = GetComponent<CinemachineImpulseSource>();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isTriggered) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                isTriggered = true;
                ActivarDerrumbe();
            }
        }

        private void ActivarDerrumbe()
        {
            // 1. Activar bloque de derrumbe
            if (blockObject != null)
            {
                blockObject.ActivarDerrumbe(blockDropPosition);
            }

            // 2. Sacudir cámara con Cinemachine Impulse
            if (impulseSource != null)
            {
                impulseSource.GenerateImpulseWithForce(1.2f);
            }

            // 3. Partículas de polvo
            if (dustParticlesPrefab != null)
            {
                Instantiate(dustParticlesPrefab, blockDropPosition, Quaternion.identity);
            }

            Debug.Log("[El Minero] ¡Derrumbe sorpresa activado! Camino de regreso bloqueado.");
        }

        public void ResetObject()
        {
            isTriggered = false;
            if (blockObject != null)
            {
                blockObject.ResetObject();
            }
        }

        private void OnDrawGizmos()
        {
            // Gizmo de líneas punteadas rojas en vista Scene según GDD sección 4.8
            Gizmos.color = Color.red;

            Bounds bounds = GetComponent<Collider2D>() != null ? GetComponent<Collider2D>().bounds : new Bounds(transform.position, new Vector3(2f, 4f, 0f));

            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            // Dibujar rectángulo con tramos discontinuos
            DibujarLineaPunteada(new Vector3(min.x, min.y, 0f), new Vector3(max.x, min.y, 0f), 0.3f);
            DibujarLineaPunteada(new Vector3(max.x, min.y, 0f), new Vector3(max.x, max.y, 0f), 0.3f);
            DibujarLineaPunteada(new Vector3(max.x, max.y, 0f), new Vector3(min.x, max.y, 0f), 0.3f);
            DibujarLineaPunteada(new Vector3(min.x, max.y, 0f), new Vector3(min.x, min.y, 0f), 0.3f);

            // Marcador de punto de caída del bloque
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(blockDropPosition, new Vector3(2f, 3f, 0f));
        }

        private void DibujarLineaPunteada(Vector3 inicio, Vector3 fin, float longitudTramo)
        {
            float distanciaTotal = Vector3.Distance(inicio, fin);
            Vector3 direccion = (fin - inicio).normalized;
            int segmentos = Mathf.FloorToInt(distanciaTotal / (longitudTramo * 2f));

            for (int i = 0; i < segmentos; i++)
            {
                Vector3 p1 = inicio + direccion * (i * 2f * longitudTramo);
                Vector3 p2 = p1 + direccion * longitudTramo;
                Gizmos.DrawLine(p1, p2);
            }
        }
    }
}
