using UnityEngine;

namespace ElMinero.Entorno
{
    /// <summary>
    /// Controla el efecto parallax de una capa de fondo en 2D.
    /// Soporta repetición infinita horizontal para cubrir todo el nivel.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [Header("Efecto Parallax")]
        [Tooltip("Multiplicador de seguimiento (0 = pegado a la cámara, 1 = completamente estático en el mundo).")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxEffectX = 0.5f;

        [Tooltip("Multiplicador vertical (usualmente 0 o muy bajo para plataformeros).")]
        [Range(0f, 1f)]
        [SerializeField] private float parallaxEffectY = 0.05f;

        [Header("Repetición Horizontal")]
        [Tooltip("Si es verdadero, recoloca el fondo cuando la cámara sobrepasa su longitud.")]
        [SerializeField] private bool infiniteHorizontal = true;

        private Transform cameraTransform;
        private Vector3 lastCameraPosition;
        private float textureUnitSizeX;

        private void Start()
        {
            if (Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
                lastCameraPosition = cameraTransform.position;
            }

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                // En modo Tiled con tamaño definido
                textureUnitSizeX = spriteRenderer.bounds.size.x;
                if (textureUnitSizeX <= 0f)
                {
                    textureUnitSizeX = spriteRenderer.sprite.rect.width / spriteRenderer.sprite.pixelsPerUnit;
                }
            }
            else
            {
                textureUnitSizeX = 20f; // Valor por defecto seguro (320px / 16ppu = 20u)
            }
        }

        private void LateUpdate()
        {
            if (cameraTransform == null)
            {
                if (Camera.main != null)
                {
                    cameraTransform = Camera.main.transform;
                    lastCameraPosition = cameraTransform.position;
                }
                return;
            }

            Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

            // Movimiento inverso del parallax: cuanto menor sea el efecto, más se mueve con la cámara
            float moveX = deltaMovement.x * (1f - parallaxEffectX);
            float moveY = deltaMovement.y * (1f - parallaxEffectY);

            transform.position += new Vector3(moveX, moveY, 0f);
            lastCameraPosition = cameraTransform.position;

            if (infiniteHorizontal && textureUnitSizeX > 0f)
            {
                float diffX = cameraTransform.position.x - transform.position.x;
                if (Mathf.Abs(diffX) >= textureUnitSizeX)
                {
                    float offsetPositionX = (diffX > 0 ? 1 : -1) * textureUnitSizeX;
                    transform.position = new Vector3(transform.position.x + offsetPositionX, transform.position.y, transform.position.z);
                }
            }
        }
    }
}
