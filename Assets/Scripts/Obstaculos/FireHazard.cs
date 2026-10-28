using System.Collections;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;
using ElMinero.Managers;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Zona de fuego intermitente por maquinaria quemada (Nivel 3).
    /// Ciclo:
    /// - Encendido: 2.0 s (daña al jugador).
    /// - Humo / chispas de advertencia: 0.7 s (aviso visual no letal).
    /// - Apagado: 1.5 s (inactivo).
    /// Cuenta con desfase configurable (initialDelay) para escalonar zonas contiguas
    /// y loop de audio 3D (Fuego_Loop, rango 3-12 u).
    /// </summary>
    public class FireHazard : MonoBehaviour, IResettable
    {
        public enum FireState { Apagado, Aviso, Encendido }

        [Header("Tiempos del Ciclo")]
        [Tooltip("Duración de la fase encendida que causa daño (2 s).")]
        [SerializeField] private float activeTime = 2.0f;

        [Tooltip("Duración del aviso con chispas/humo previo (0.7 s).")]
        [SerializeField] private float warningTime = 0.7f;

        [Tooltip("Duración de la fase apagada (1.5 s).")]
        [SerializeField] private float offTime = 1.5f;

        [Tooltip("Desfase inicial para alternar fuegos contiguos.")]
        [SerializeField] private float initialDelay = 0.0f;

        [Header("Daño")]
        [SerializeField] private int damage = 1;

        [Header("Referencias Visuales y Sonido")]
        [SerializeField] private SpriteRenderer flameRenderer;
        [SerializeField] private Collider2D damageCollider;
        [SerializeField] private AudioSource loopAudioSource;

        private FireState currentState = FireState.Apagado;
        private Coroutine cycleRoutine;

        private void Awake()
        {
            if (flameRenderer == null) flameRenderer = GetComponent<SpriteRenderer>();
            if (damageCollider == null) damageCollider = GetComponent<Collider2D>();

            ConfigurarAudio3D();
        }

        private void ConfigurarAudio3D()
        {
            if (loopAudioSource == null)
            {
                loopAudioSource = GetComponent<AudioSource>();
                if (loopAudioSource == null)
                {
                    loopAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            if (loopAudioSource != null)
            {
                loopAudioSource.loop = true;
                loopAudioSource.playOnAwake = false;
                loopAudioSource.spatialBlend = 1.0f; // 3D
                loopAudioSource.minDistance = 3f;
                loopAudioSource.maxDistance = 12f;
                loopAudioSource.rolloffMode = AudioRolloffMode.Linear;
                if (AudioManager.Instance != null)
                {
                    loopAudioSource.clip = AudioManager.Instance.GetAudioClipFuegoLoop();
                    loopAudioSource.outputAudioMixerGroup = AudioManager.Instance.GetSFXGroup();
                }
            }
        }

        private void Start()
        {
            cycleRoutine = StartCoroutine(RutinaCicloFuego());
        }

        private IEnumerator RutinaCicloFuego()
        {
            if (initialDelay > 0f)
            {
                SetState(FireState.Apagado);
                yield return new WaitForSeconds(initialDelay);
            }

            while (true)
            {
                // 1. Aviso previo (0.7 s): llamas tenues o chispas
                SetState(FireState.Aviso);
                yield return new WaitForSeconds(warningTime);

                // 2. Encendido total (2.0 s): llamas vivas y daño activo
                SetState(FireState.Encendido);
                yield return new WaitForSeconds(activeTime);

                // 3. Apagado (1.5 s): brasas frías, sin daño
                SetState(FireState.Apagado);
                yield return new WaitForSeconds(offTime);
            }
        }

        private void SetState(FireState newState)
        {
            currentState = newState;

            switch (currentState)
            {
                case FireState.Apagado:
                    if (flameRenderer != null) flameRenderer.enabled = false;
                    if (damageCollider != null) damageCollider.enabled = false;
                    if (loopAudioSource != null) loopAudioSource.volume = 0f;
                    break;

                case FireState.Aviso:
                    if (flameRenderer != null)
                    {
                        flameRenderer.enabled = true;
                        flameRenderer.color = new Color(1f, 0.5f, 0.2f, 0.4f); // Traslúcido de advertencia
                    }
                    if (damageCollider != null) damageCollider.enabled = false;
                    if (loopAudioSource != null)
                    {
                        loopAudioSource.volume = 0.3f;
                        if (!loopAudioSource.isPlaying) loopAudioSource.Play();
                    }
                    break;

                case FireState.Encendido:
                    if (flameRenderer != null)
                    {
                        flameRenderer.enabled = true;
                        flameRenderer.color = Color.white; // Fuego pleno
                    }
                    if (damageCollider != null) damageCollider.enabled = true;
                    if (loopAudioSource != null)
                    {
                        loopAudioSource.volume = 0.9f;
                        if (!loopAudioSource.isPlaying) loopAudioSource.Play();
                    }
                    break;
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (currentState != FireState.Encendido) return;

            if (other.CompareTag("Player"))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null)
                {
                    bool golpe = health.TakeDamage(damage);
                    if (golpe && AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayDanio();
                    }
                }
            }
        }

        public void ResetObject()
        {
            if (cycleRoutine != null) StopCoroutine(cycleRoutine);
            cycleRoutine = StartCoroutine(RutinaCicloFuego());
        }

        public void SetDesfase(float delay)
        {
            initialDelay = delay;
        }
    }
}
