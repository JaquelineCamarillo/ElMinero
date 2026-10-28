using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Bloque de piedra maciza que cae durante un derrumbe sorpresa (BloqueDerrumbe).
    /// Cae desde el techo detrás del jugador y bloquea el paso de regreso.
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

            targetGroundPosition = targetPos;
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
