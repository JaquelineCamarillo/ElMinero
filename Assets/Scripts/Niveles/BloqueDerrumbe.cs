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

            // El collider debe coincidir exactamente con el dibujo (antes tenía un offset de 1.5
            // con el sprite centrado y por eso el bloque quedaba flotando).
            var box = col as BoxCollider2D;
            if (box != null && sr != null && sr.sprite != null)
            {
                Bounds b = sr.sprite.bounds;
                box.size = new Vector2(b.size.x * 0.95f, b.size.y);
                box.offset = b.center;
            }

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

            // Detectar el suelo real bajo el punto de caída (el rayo empieza un poco arriba del
            // punto indicado para no nacer dentro de la roca del techo).
            float baseDibujo = (sr != null && sr.sprite != null) ? -sr.sprite.bounds.min.y : (col != null ? col.bounds.extents.y : 0.5f);
            int layerSueloMask = LayerMask.GetMask(Constantes.Layers.Suelo);
            bool antes = Physics2D.queriesStartInColliders;
            Physics2D.queriesStartInColliders = false;
            if (col != null) col.enabled = false;
            Vector2 origen = new Vector2(targetPos.x, targetPos.y + 2f);
            RaycastHit2D hit = Physics2D.Raycast(origen, Vector2.down, 40f, layerSueloMask);
            Physics2D.queriesStartInColliders = antes;

            transform.position = new Vector3(targetPos.x, transform.position.y, transform.position.z);
            if (hit.collider != null)
            {
                targetGroundPosition = new Vector3(targetPos.x, hit.point.y + baseDibujo, transform.position.z);
            }
            else
            {
                targetGroundPosition = new Vector3(targetPos.x, targetPos.y + baseDibujo, transform.position.z);
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
