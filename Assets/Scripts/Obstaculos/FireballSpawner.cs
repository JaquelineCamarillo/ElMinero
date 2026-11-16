using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ElMinero.Utilidades;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Generador de bolas de fuego en el techo (nivel 3), con pool.
    /// Muestra humo de aviso 0.8 s antes de soltar cada bola y solo lanza
    /// si el jugador está a menos de 14 u.
    /// </summary>
    public class FireballSpawner : MonoBehaviour, IResettable
    {
        [Header("Tiempos")]
        [SerializeField] private float interval = 3.2f;
        [SerializeField] private float warningTime = 0.8f;
        [SerializeField] private float initialDelay = 0.5f;

        [Header("Distancia de activación")]
        [SerializeField] private float distanciaJugador = 14f;

        [Header("Referencias")]
        [Tooltip("Plantilla desactivada (hija de este objeto) con Fireball, Rigidbody2D y CircleCollider2D.")]
        [SerializeField] private Fireball plantilla;
        [SerializeField] private GameObject smokeWarningObject;
        [SerializeField] private int tamanoPool = 3;

        private readonly Queue<Fireball> libres = new Queue<Fireball>();
        private readonly List<Fireball> todas = new List<Fireball>();
        private Coroutine spawnRoutine;
        private Transform jugador;

        public void Configurar(float nuevoIntervalo, float retrasoInicial, Fireball plantillaBola, GameObject objHumo)
        {
            interval = nuevoIntervalo;
            initialDelay = retrasoInicial;
            plantilla = plantillaBola;
            smokeWarningObject = objHumo;
        }

        private void Awake()
        {
            if (plantilla != null)
            {
                plantilla.gameObject.SetActive(false);
                for (int i = 0; i < tamanoPool; i++)
                {
                    Fireball fb = Instantiate(plantilla, transform.position, Quaternion.identity);
                    fb.name = $"Bola_Fuego_{i}";
                    fb.gameObject.SetActive(false);
                    todas.Add(fb);
                    libres.Enqueue(fb);
                }
            }
        }

        private void Start()
        {
            if (smokeWarningObject != null) smokeWarningObject.SetActive(false);
            spawnRoutine = StartCoroutine(RutinaSpawn());
        }

        private bool JugadorCerca()
        {
            if (jugador == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
                if (p != null) jugador = p.transform;
            }
            return jugador != null && Vector2.Distance(jugador.position, transform.position) < distanciaJugador;
        }

        private IEnumerator RutinaSpawn()
        {
            yield return new WaitForSeconds(initialDelay);

            while (true)
            {
                if (!JugadorCerca())
                {
                    yield return new WaitForSeconds(0.25f);
                    continue;
                }

                if (smokeWarningObject != null) smokeWarningObject.SetActive(true);
                yield return new WaitForSeconds(warningTime);
                if (smokeWarningObject != null) smokeWarningObject.SetActive(false);

                if (libres.Count > 0)
                {
                    Fireball fb = libres.Dequeue();
                    fb.Lanzar(transform.position - new Vector3(0f, 0.5f, 0f), Devolver);
                }

                yield return new WaitForSeconds(interval);
            }
        }

        private void Devolver(Fireball fb)
        {
            if (fb != null && !libres.Contains(fb)) libres.Enqueue(fb);
        }

        public void ResetObject()
        {
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
            if (smokeWarningObject != null) smokeWarningObject.SetActive(false);

            libres.Clear();
            foreach (var fb in todas)
            {
                if (fb == null) continue;
                fb.gameObject.SetActive(false);
                libres.Enqueue(fb);
            }

            spawnRoutine = StartCoroutine(RutinaSpawn());
        }
    }
}
