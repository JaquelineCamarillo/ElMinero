using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Generador de bolas de fuego en el techo de la mina (Nivel 3).
    /// Muestra humo de advertencia (Humo.png) en el techo durante 0.8 s antes de soltar la bola de fuego.
    /// Funciona de forma periódica y se reinicia mediante IResettable.
    /// </summary>
    public class FireballSpawner : MonoBehaviour, IResettable
    {
        [Header("Configuración de Tiempos")]
        [Tooltip("Intervalo entre bolas de fuego.")]
        [SerializeField] private float interval = 3.5f;

        [Tooltip("Tiempo de advertencia previa con humo (0.8 s).")]
        [SerializeField] private float warningTime = 0.8f;

        [Tooltip("Desfase inicial para no sincronizar todos los spawners.")]
        [SerializeField] private float initialDelay = 0.5f;

        [Header("Prefabs y Referencias")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject smokeWarningObject;

        private Coroutine spawnRoutine;
        private List<GameObject> activeFireballs = new List<GameObject>();

        private void Start()
        {
            if (smokeWarningObject != null)
            {
                smokeWarningObject.SetActive(false);
            }

            spawnRoutine = StartCoroutine(RutinaSpawn());
        }

        public void Configurar(float nuevoIntervalo, float retrasoInicial, GameObject prefabBola, GameObject objHumo)
        {
            interval = nuevoIntervalo;
            initialDelay = retrasoInicial;
            fireballPrefab = prefabBola;
            smokeWarningObject = objHumo;
        }

        private IEnumerator RutinaSpawn()
        {
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                // 1. Advertencia de humo en el techo (0.8 s)
                if (smokeWarningObject != null)
                {
                    smokeWarningObject.SetActive(true);
                }

                yield return new WaitForSeconds(warningTime);

                if (smokeWarningObject != null)
                {
                    smokeWarningObject.SetActive(false);
                }

                // 2. Generar bola de fuego cayendo
                if (fireballPrefab != null)
                {
                    GameObject fb = Instantiate(fireballPrefab, transform.position - new Vector3(0f, 0.4f, 0f), Quaternion.identity);
                    activeFireballs.Add(fb);
                }

                // 3. Pausa hasta la siguiente bola
                yield return new WaitForSeconds(interval);
            }
        }

        public void ResetObject()
        {
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);

            if (smokeWarningObject != null)
            {
                smokeWarningObject.SetActive(false);
            }

            // Limpiar bolas de fuego huérfanas
            for (int i = activeFireballs.Count - 1; i >= 0; i--)
            {
                if (activeFireballs[i] != null)
                {
                    Destroy(activeFireballs[i]);
                }
            }
            activeFireballs.Clear();

            spawnRoutine = StartCoroutine(RutinaSpawn());
        }
    }
}
