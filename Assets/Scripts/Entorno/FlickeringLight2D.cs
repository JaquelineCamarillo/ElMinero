using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ElMinero.Entorno
{
    /// <summary>
    /// Simula el parpadeo natural de una antorcha o farol en una mina del siglo XIX
    /// modulando suavemente la intensidad y radio de una Light2D de URP.
    /// </summary>
    [RequireComponent(typeof(Light2D))]
    public class FlickeringLight2D : MonoBehaviour
    {
        [Header("Configuración de Parpadeo")]
        [Tooltip("Amplitud de la variación de intensidad.")]
        [SerializeField] private float intensityVariation = 0.25f;

        [Tooltip("Velocidad del parpadeo.")]
        [SerializeField] private float flickerSpeed = 5.0f;

        [Tooltip("Si es verdadero, también varía sutilmente el radio externo.")]
        [SerializeField] private bool modulateRadius = true;

        private Light2D light2D;
        private float baseIntensity;
        private float baseOuterRadius;
        private float seed;

        private void Awake()
        {
            light2D = GetComponent<Light2D>();
            if (light2D != null)
            {
                baseIntensity = light2D.intensity;
                baseOuterRadius = light2D.pointLightOuterRadius;
            }
            seed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (light2D == null) return;

            float noise = Mathf.PerlinNoise(seed, Time.time * flickerSpeed);
            // Mapear de 0..1 a -1..1
            float offset = (noise * 2f) - 1f;

            light2D.intensity = Mathf.Max(0.1f, baseIntensity + offset * intensityVariation);

            if (modulateRadius)
            {
                light2D.pointLightOuterRadius = Mathf.Max(0.5f, baseOuterRadius + offset * (intensityVariation * 0.5f));
            }
        }
    }
}
