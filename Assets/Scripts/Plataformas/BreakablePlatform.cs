using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Plataformas
{
    /// <summary>
    /// Plataforma quebradiza (BreakablePlatform).
    /// Al ser pisada por el jugador, comienza a temblar, muestra grietas progresivas
    /// y colapsa exactamente a los 2 segundos. Se regenera automáticamente tras 4 segundos
    /// o de forma inmediata si el jugador reaparece o reinicia el nivel.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BreakablePlatform : MonoBehaviour, IResettable
    {
        [Header("Sprites de Estado")]
        [Tooltip("Cuadro 0: Normal / intacta.")]
        [SerializeField] private Sprite spriteNormal;

        [Tooltip("Cuadro 1: Agrietada.")]
        [SerializeField] private Sprite spriteCracked;

        [Tooltip("Cuadro 2: Rompiéndose.")]
        [SerializeField] private Sprite spriteBreaking;

        [Header("Tiempos de Colapso y Restauración")]
        [Tooltip("Tiempo en segundos antes de colapsar al ser pisada (GDD: 2 s).")]
        [SerializeField] private float collapseDelay = 2.0f;

        [Tooltip("Tiempo en segundos antes de regenerarse automáticamente (GDD: 4 s).")]
        [SerializeField] private float restoreDelay = 4.0f;

        [Tooltip("Intensidad de la vibración al temblar.")]
        [SerializeField] private float shakeIntensity = 0.05f;

        private Collider2D col;
        private SpriteRenderer sr;
        private Vector3 originalPosition;
        private bool isTriggered = false;
        private bool isCollapsed = false;
        private Coroutine collapseRoutine;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
            originalPosition = transform.position;

            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo != -1)
            {
                gameObject.layer = layerSuelo;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isTriggered || isCollapsed) return;

            if (collision.gameObject.CompareTag(Constantes.Tags.Player))
            {
                foreach (var contact in collision.contacts)
                {
                    // Si el jugador se para encima
                    if (contact.normal.y < -0.5f)
                    {
                        isTriggered = true;
                        collapseRoutine = StartCoroutine(RutinaColapso());
                        break;
                    }
                }
            }
        }

        private IEnumerator RutinaColapso()
        {
            float elapsed = 0f;

            // Etapa 1: Agrietarse y temblar
            if (spriteCracked != null && sr != null) sr.sprite = spriteCracked;

            while (elapsed < collapseDelay)
            {
                // Mitad del tiempo: cambio a estado rompiéndose
                if (elapsed > collapseDelay * 0.5f && spriteBreaking != null && sr != null)
                {
                    sr.sprite = spriteBreaking;
                }

                // Efecto de temblor horizontal
                float xOffset = Random.Range(-shakeIntensity, shakeIntensity);
                transform.position = originalPosition + new Vector3(xOffset, 0f, 0f);

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Colapsar
            transform.position = originalPosition;
            isCollapsed = true;
            if (col != null) col.enabled = false;
            if (sr != null) sr.enabled = false;

            // Esperar tiempo de restauración
            yield return new WaitForSeconds(restoreDelay);

            Restaurar();
        }

        public void Restaurar()
        {
            if (collapseRoutine != null)
            {
                StopCoroutine(collapseRoutine);
                collapseRoutine = null;
            }

            isTriggered = false;
            isCollapsed = false;
            transform.position = originalPosition;

            if (col != null) col.enabled = true;
            if (sr != null)
            {
                sr.enabled = true;
                if (spriteNormal != null) sr.sprite = spriteNormal;
            }
        }

        public void ResetObject()
        {
            Restaurar();
        }
    }
}
