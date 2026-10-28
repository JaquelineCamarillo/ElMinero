using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;

namespace ElMinero.Coleccionables
{
    /// <summary>
    /// Clase base abstracta para todos los coleccionables (Veta de Plata, Gema de Amatista).
    /// Maneja el trigger de detección, desactivación visual y física, e implementación de IResettable.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public abstract class Collectible : MonoBehaviour, IResettable
    {
        [Header("Propiedades Base")]
        [Tooltip("Puntaje base otorgado por este coleccionable.")]
        [SerializeField] protected int basePoints = 10;

        protected Collider2D col;
        protected SpriteRenderer sr;
        protected bool isCollected = false;

        public bool IsCollected => isCollected;
        public int BasePoints => basePoints;

        protected virtual void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();

            // Forzar que sea Trigger
            if (col != null) col.isTrigger = true;

            // Asignar Tag y Layer apropiados
            tag = Constantes.Tags.Collectible;
            int layerColeccionable = LayerMask.NameToLayer(Constantes.Layers.Coleccionable);
            if (layerColeccionable != -1)
            {
                gameObject.layer = layerColeccionable;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isCollected) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                var player = other.GetComponent<PlayerController>();
                Collect(player);
            }
        }

        public void Collect(PlayerController player)
        {
            if (isCollected) return;

            isCollected = true;
            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;

            OnCollect(player);
        }

        /// <summary>
        /// Lógica específica al recolectar (sumar puntos, efectos, etc.).
        /// </summary>
        protected abstract void OnCollect(PlayerController player);

        public virtual void ResetObject()
        {
            isCollected = false;
            if (col != null) col.enabled = true;
            if (sr != null) sr.enabled = true;
        }
    }
}
