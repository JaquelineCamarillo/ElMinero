using UnityEngine;
using UnityEngine.Rendering.Universal;
using ElMinero.Utilidades;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Punto de control en el nivel (Checkpoint).
    /// Guarda la posición de reaparición del jugador.
    /// Se planta firmemente en el suelo mediante Raycast para evitar flotar.
    /// Al tocarlo se enciende visualmente cambiando su sprite y activando su luz 2D cálida (#FFD27A, intensidad 1.2, radio 4).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Checkpoint : MonoBehaviour, IResettable
    {
        [Header("Sprites de Estado")]
        [Tooltip("Sprite cuando el checkpoint está apagado.")]
        [SerializeField] private Sprite spriteOff;

        [Tooltip("Sprite cuando el checkpoint está encendido.")]
        [SerializeField] private Sprite spriteOn;

        [Header("Iluminación 2D")]
        [Tooltip("Luz 2D que se activa al encender el checkpoint.")]
        [SerializeField] private Light2D checkpointLight;

        [Header("Ajuste de Suelo")]
        [Tooltip("Planta automáticamente la base del checkpoint en el suelo mediante raycast al iniciar.")]
        [SerializeField] private bool autoPlantarEnSuelo = true;

        private Collider2D col;
        private SpriteRenderer sr;
        private bool isActivated = false;

        public bool IsActivated => isActivated;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();

            if (col != null) col.isTrigger = true;
            tag = Constantes.Tags.Checkpoint;

            ConfigurarLuz2D();
            ActualizarVisuales(false);
        }

        private void Start()
        {
            if (autoPlantarEnSuelo)
            {
                PlantarEnSuelo();
            }
        }

        public void ConfigurarLuz2D()
        {
            if (checkpointLight == null)
            {
                checkpointLight = GetComponentInChildren<Light2D>();
            }

            if (checkpointLight == null)
            {
                GameObject luzGo = new GameObject("Luz_Checkpoint");
                luzGo.transform.SetParent(transform, false);
                luzGo.transform.localPosition = new Vector3(0f, 0.6f, 0f);

                checkpointLight = luzGo.AddComponent<Light2D>();
                checkpointLight.lightType = Light2D.LightType.Point;
                checkpointLight.color = new Color(1.0f, 0.824f, 0.48f, 1f); // #FFD27A
                checkpointLight.intensity = 1.2f;
                checkpointLight.pointLightInnerRadius = 0.5f;
                checkpointLight.pointLightOuterRadius = 4.0f;
            }
        }

        public void ConfigurarSprites(Sprite off, Sprite on)
        {
            spriteOff = off;
            spriteOn = on;
            ActualizarVisuales(isActivated);
        }

        public void PlantarEnSuelo()
        {
            int layerSuelo = LayerMask.GetMask(Constantes.Layers.Suelo);
            RaycastHit2D hit = Physics2D.Raycast(transform.position + Vector3.up * 0.5f, Vector2.down, 15f, layerSuelo);
            if (hit.collider != null)
            {
                // Dado que el pivote de los sprites es Bottom Center (0.5, 0), hit.point.y sitúa la base en el suelo exacto
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isActivated) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                Activar();
            }
        }

        public void Activar()
        {
            if (isActivated) return;

            isActivated = true;
            ActualizarVisuales(true);

            if (Managers.LevelManager.Instance != null)
            {
                Managers.LevelManager.Instance.EstablecerCheckpoint(transform.position);
            }
        }

        private void ActualizarVisuales(bool activa)
        {
            if (sr != null)
            {
                sr.sprite = activa ? (spriteOn != null ? spriteOn : sr.sprite) : (spriteOff != null ? spriteOff : sr.sprite);
            }

            if (checkpointLight != null)
            {
                checkpointLight.enabled = activa;
                checkpointLight.intensity = activa ? 1.2f : 0f;
            }
        }

        public void ResetObject()
        {
            isActivated = false;
            ActualizarVisuales(false);
        }
    }
}
