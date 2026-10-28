using UnityEngine;
using Unity.Cinemachine;
using ElMinero.Utilidades;

namespace ElMinero.Camara
{
    /// <summary>
    /// Configura y controla el seguimiento de cámara hacia el jugador.
    /// Soporta Cinemachine 3.x (CinemachineCamera en Unity 6) y provee
    /// un seguimiento suave directo en caso de no utilizar Cinemachine.
    /// </summary>
    public class CameraFollowSetup : MonoBehaviour
    {
        [Header("Objetivo")]
        [Tooltip("Transform del jugador a seguir. Si está vacío, se busca automáticamente por Tag 'Player'.")]
        [SerializeField] private Transform target;

        [Header("Configuración de Vista")]
        [Tooltip("Tamaño ortográfico sugerido por el GDD (~6 unidades).")]
        [SerializeField] private float orthographicSize = 6f;

        [Header("Seguimiento Directo (Fallback sin Cinemachine)")]
        [Tooltip("Suavizado de interpolación para el movimiento de la cámara.")]
        [SerializeField] private float smoothTime = 0.2f;

        [Tooltip("Offset horizontal y vertical de la cámara respecto al jugador.")]
        [SerializeField] private Vector3 cameraOffset = new Vector3(1f, 1f, -10f);

        [Header("Límites del Nivel (Confiner)")]
        [Tooltip("Si es verdadero, la cámara no sobrepasará los límites min/max especificados.")]
        [SerializeField] private bool useBounds = false;
        [SerializeField] private Vector2 minBounds = new Vector2(-10f, -5f);
        [SerializeField] private Vector2 maxBounds = new Vector2(100f, 20f);

        private Vector3 currentVelocity = Vector3.zero;
        private Camera targetCamera;
        private CinemachineCamera cinemachineCam;

        private void Awake()
        {
            targetCamera = GetComponent<Camera>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null && targetCamera.orthographic)
            {
                targetCamera.orthographicSize = orthographicSize;
            }

            cinemachineCam = GetComponent<CinemachineCamera>();
            if (cinemachineCam == null)
            {
                cinemachineCam = UnityEngine.Object.FindAnyObjectByType<CinemachineCamera>();
            }
        }

        private void Start()
        {
            FindTargetIfNeeded();
            ConfigureCinemachine();
        }

        private void FindTargetIfNeeded()
        {
            if (target == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
                if (playerObj != null)
                {
                    target = playerObj.transform;
                }
            }
        }

        private void ConfigureCinemachine()
        {
            if (cinemachineCam == null)
            {
                cinemachineCam = GetComponent<CinemachineCamera>();
                if (cinemachineCam == null)
                {
                    cinemachineCam = UnityEngine.Object.FindAnyObjectByType<CinemachineCamera>();
                }
            }

            if (cinemachineCam != null && target != null)
            {
                cinemachineCam.Follow = target;
                var follow = cinemachineCam.GetComponent<CinemachineFollow>();
                if (follow != null)
                {
                    follow.FollowOffset = new Vector3(0f, 1f, -10f);
                }
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
            ConfigureCinemachine();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                FindTargetIfNeeded();
                if (target == null) return;
            }

            // Comprobar si hay un CinemachineBrain activo en la cámara principal
            CinemachineBrain brain = targetCamera != null ? targetCamera.GetComponent<CinemachineBrain>() : null;
            if (brain != null && brain.isActiveAndEnabled)
            {
                // Asegurar que la cámara 2D siempre permanezca en Z = -10 para no cortar sprites en Z = 0
                if (Mathf.Abs(transform.position.z) < 5f)
                {
                    Vector3 posZ = transform.position;
                    posZ.z = -10f;
                    transform.position = posZ;
                }
                return;
            }

            // Fallback: seguimiento suave SmoothDamp
            Vector3 targetPosition = target.position + cameraOffset;

            if (useBounds)
            {
                targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
                targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
            }

            targetPosition.z = cameraOffset.z;
            transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
        }

        public void SetBounds(Vector2 min, Vector2 max)
        {
            minBounds = min;
            maxBounds = max;
            useBounds = true;
        }

        private void OnDrawGizmosSelected()
        {
            if (useBounds)
            {
                Gizmos.color = Color.yellow;
                Vector3 center = new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f);
                Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1f);
                Gizmos.DrawWireCube(center, size);
            }
        }
    }
}
