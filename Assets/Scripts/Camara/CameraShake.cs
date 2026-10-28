using System.Collections;
using UnityEngine;

namespace ElMinero.Camara
{
    /// <summary>
    /// Componente complementario para sacudidas de cámara por código (CameraShake).
    /// Funciona de forma autónoma alterando el offset local de la cámara durante impactos o derrumbes.
    /// </summary>
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 originalLocalPos;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            originalLocalPos = transform.localPosition;
        }

        public void Shake(float duration = 0.5f, float magnitude = 0.3f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
            }
            shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;

                transform.localPosition = new Vector3(originalLocalPos.x + x, originalLocalPos.y + y, originalLocalPos.z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localPosition = originalLocalPos;
            shakeCoroutine = null;
        }
    }
}
