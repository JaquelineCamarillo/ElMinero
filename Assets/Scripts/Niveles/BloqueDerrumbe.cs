using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Bloque de piedra maciza que cae durante un derrumbe sorpresa (BloqueDerrumbe).
    /// Cae desde el techo detrás del jugador y se asienta firmemente en el suelo (mediante raycast)
    /// bloqueando por completo el paso de regreso de piso a techo sin quedar flotando.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BloqueDerrumbe : MonoBehaviour, IResettable
    {
        [Header("Configuración de Caída")]
        [Tooltip("Posición final en el suelo donde se asienta el bloque.")]
        [SerializeField] private Vector3 targetGroundPosition;

        [Tooltip("Velocidad de caída veloz del derrumbe.")]
        [SerializeField] private float fallSpeed = 36f;

        private Vector3 initialPosition;
        private bool isTriggered = false;
        private bool isSettled = false;

        public bool IsSettled => isSettled;
        private Collider2D col;
        private SpriteRenderer sr;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            initialPosition = transform.position;

            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo != -1)
            {
                gameObject.layer = layerSuelo;
            }

            // Oculto e inactivo al inicio
            DesactivarInicial();
        }

        private void DesactivarInicial()
        {
            isTriggered = false;
            isSettled = false;
            transform.position = initialPosition;
            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;
        }

        public void ActivarDerrumbe(Vector3 targetPos)
        {
            if (isTriggered) return;

            // Detectar el suelo real mediante raycast hacia abajo para evitar que quede flotando
            int layerSueloMask = LayerMask.GetMask(Constantes.Layers.Suelo);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 40f, layerSueloMask);

            if (hit.collider != null)
            {
                float extY = col != null ? col.bounds.extents.y : 0.5f;
                targetGroundPosition = new Vector3(transform.position.x, hit.point.y + extY, transform.position.z);
            }
            else
            {
                targetGroundPosition = targetPos;
            }

            isTriggered = true;
            if (col != null) col.enabled = true;
            if (sr != null) sr.enabled = true;

            StartCoroutine(RutinaCaida());
        }

        private IEnumerator RutinaCaida()
        {
            while (Vector3.Distance(transform.position, targetGroundPosition) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetGroundPosition, fallSpeed * Time.deltaTime);
                yield return null;
            }

            transform.position = targetGroundPosition;
            isSettled = true;
        }

        public void ResetObject()
        {
            StopAllCoroutines();
            DesactivarInicial();
        }
    }
}
