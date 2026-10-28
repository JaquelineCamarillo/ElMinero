using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using ElMinero.Utilidades;
using ElMinero.Jugador;
using ElMinero.Coleccionables;
using ElMinero.Niveles;
using ElMinero.UI;

namespace ElMinero.Managers
{
    /// <summary>
    /// Administrador central de la escena del nivel actual.
    /// Coordina tiempo, coleccionables, vidas, reaparición en checkpoints,
    /// reinicio integral del nivel y finalización exitosa.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Configuración del Nivel")]
        [Tooltip("Asset de configuración ScriptableObject para este nivel.")]
        [SerializeField] private LevelConfig levelConfig;

        [Header("Subsistemas y Componentes")]
        [SerializeField] private LevelTimer timer;
        [SerializeField] private CollectibleManager collectibleManager;
        [SerializeField] private StreakSystem streakSystem;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private HUDController hudController;

        [Header("Puntos de Aparición")]
        [Tooltip("Posición inicial del jugador.")]
        [SerializeField] private Vector3 initialSpawnPoint;

        private Vector3 currentCheckpoint;
        private readonly List<IResettable> resettableObjects = new List<IResettable>();

        // Eventos C#
        public event Action<int> OnScoreUpdated;       // Notifica puntaje total combinado
        public event Action<string> OnBonusEarned;      // Notifica bono obtenido ("¡Bono todas las vetas: +50!")

        public LevelConfig Config => levelConfig;
        public LevelTimer Timer => timer;
        public CollectibleManager Collectibles => collectibleManager;
        public StreakSystem Streak => streakSystem;
        public PlayerHealth Health => playerHealth;
        public PlayerController Player => playerController;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            GarantizarColisionesTilemaps();
            LocalizarComponentes();
        }

        private void GarantizarColisionesTilemaps()
        {
            var colliders = FindObjectsByType<TilemapCollider2D>();
            foreach (var tmCol in colliders)
            {
                tmCol.ProcessTilemapChanges();
                var comp = tmCol.GetComponent<CompositeCollider2D>();
                if (comp != null)
                {
                    comp.GenerateGeometry();
                    if (comp.pathCount == 0)
                    {
                        tmCol.compositeOperation = Collider2D.CompositeOperation.None;
                    }
                }
            }
        }

        private void Update()
        {
            // Seguridad: si el jugador cae al abismo por debajo del nivel
            if (playerController != null && playerController.transform.position.y < -12f)
            {
                if (playerHealth != null && !playerHealth.IsDead)
                {
                    playerHealth.TakeDamage(1);
                }
            }
        }

        private void Start()
        {
            RegistrarObjetosReiniciables();
            ConfigurarPuntoAparicion();
            SuscribirEventos();
            IniciarNivel();
        }

        private void LocalizarComponentes()
        {
            if (timer == null) timer = GetComponentInChildren<LevelTimer>();
            if (timer == null) timer = gameObject.AddComponent<LevelTimer>();

            if (collectibleManager == null) collectibleManager = GetComponentInChildren<CollectibleManager>();
            if (collectibleManager == null) collectibleManager = gameObject.AddComponent<CollectibleManager>();

            if (streakSystem == null) streakSystem = GetComponentInChildren<StreakSystem>();
            if (streakSystem == null) streakSystem = gameObject.AddComponent<StreakSystem>();

            if (playerHealth == null) playerHealth = FindAnyObjectByType<PlayerHealth>();
            if (playerController == null) playerController = FindAnyObjectByType<PlayerController>();
            if (hudController == null) hudController = FindAnyObjectByType<HUDController>();
        }

        private void ConfigurarPuntoAparicion()
        {
            if (playerController != null)
            {
                initialSpawnPoint = playerController.transform.position;
            }
            currentCheckpoint = initialSpawnPoint;
        }

        private void RegistrarObjetosReiniciables()
        {
            resettableObjects.Clear();
            var monos = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);
            foreach (var m in monos)
            {
                if (m is IResettable resettable && !resettableObjects.Contains(resettable))
                {
                    resettableObjects.Add(resettable);
                }
            }
        }

        private void SuscribirEventos()
        {
            if (playerHealth != null)
            {
                playerHealth.OnLifeLost += ManejarVidaPerdida;
                playerHealth.OnDeath += ManejarMuerte;
            }

            if (timer != null)
            {
                timer.OnTimeExpired += ManejarTiempoAgotado;
            }

            // Notificación inicial de puntaje
            NotificarPuntaje();
        }

        private void IniciarNivel()
        {
            float tiempo = levelConfig != null ? levelConfig.TimeLimit : 90f;
            if (timer != null)
            {
                timer.Iniciar(tiempo);
            }
        }

        // ====================================================================
        // GESTIÓN DE DAÑO Y CHECKPOINTS
        // ====================================================================

        public void EstablecerCheckpoint(Vector3 posicion)
        {
            currentCheckpoint = posicion + new Vector3(0f, 0.5f, 0f);
            Debug.Log($"[El Minero] Nuevo Checkpoint registrado en: {currentCheckpoint}");
        }

        private void ManejarVidaPerdida(int vidasRestantes)
        {
            // Cortar racha de recolección
            if (streakSystem != null)
            {
                streakSystem.CortarRachaPorDanio();
            }

            // Si aún le quedan vidas, reaparece en el último checkpoint
            if (vidasRestantes > 0)
            {
                ReaparecerJugadorEnCheckpoint();
            }
        }

        public void ReaparecerJugadorEnCheckpoint()
        {
            if (playerController != null)
            {
                playerController.transform.position = currentCheckpoint;
                var rb = playerController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                playerController.TriggerInvulnerability(1.5f);
            }
        }

        private void ManejarMuerte()
        {
            Debug.Log("[El Minero] ¡Sin vidas! Derrota.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DefeatReason = "Te has quedado sin vidas bajo la mina...";
                GameManager.Instance.LastLevelSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                GameManager.Instance.DiscardLevelScore();
                SceneLoader.LoadScene(Constantes.Escenas.Derrota);
            }
            else
            {
                ReiniciarNivelCompleto("El derrumbe te venció. Sin vidas.");
            }
        }

        private void ManejarTiempoAgotado()
        {
            Debug.Log("[El Minero] ¡Tiempo agotado! Derrota inmediata.");
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DefeatReason = "Se acabó el tiempo y el derrumbe sepultó la mina.";
                GameManager.Instance.LastLevelSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                GameManager.Instance.DiscardLevelScore();
                SceneLoader.LoadScene(Constantes.Escenas.Derrota);
            }
            else
            {
                ReiniciarNivelCompleto("El tiempo se agotó.");
            }
        }

        // ====================================================================
        // COLECCIÓN Y PUNTAJE
        // ====================================================================

        public void RecolectarVeta(SilverVein veta)
        {
            int basePts = levelConfig != null ? levelConfig.SilverVeinPoints : veta.BasePoints;
            int finalPts = streakSystem != null ? streakSystem.AplicarMultiplicador(basePts) : basePts;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(finalPts);
                GameManager.Instance.AddSilver();
            }

            if (collectibleManager != null)
            {
                collectibleManager.RegistrarVetaRecolectada();
            }

            if (streakSystem != null)
            {
                streakSystem.RegistrarColeccionable();
            }

            NotificarPuntaje();
        }

        public void RecolectarGema(AmethystGem gema)
        {
            int basePts = levelConfig != null ? levelConfig.AmethystGemPoints : gema.BasePoints;
            int finalPts = streakSystem != null ? streakSystem.AplicarMultiplicador(basePts) : basePts;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(finalPts);
            }

            if (collectibleManager != null)
            {
                collectibleManager.RegistrarGemaRecolectada();
            }

            if (streakSystem != null)
            {
                streakSystem.RegistrarColeccionable();
            }

            NotificarPuntaje();
        }

        private void NotificarPuntaje()
        {
            int puntaje = GameManager.Instance != null ? GameManager.Instance.CombinedScore : 0;
            OnScoreUpdated?.Invoke(puntaje);
        }

        // ====================================================================
        // SALIDA Y FINALIZACIÓN
        // ====================================================================

        public void CompletarNivel()
        {
            if (timer != null) timer.Detener();

            // Bono por recolectar todas las vetas de plata
            if (collectibleManager != null && collectibleManager.AllSilverCollected)
            {
                int bono = levelConfig != null ? levelConfig.AllSilverBonus : 50;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(bono);
                }
                OnBonusEarned?.Invoke($"¡Bono todas las vetas: +{bono} pts!");
                NotificarPuntaje();
            }

            Debug.Log("[El Minero] ¡Nivel completado exitosamente!");

            // Avanzar al siguiente nivel mediante el GameManager
            if (GameManager.Instance != null)
            {
                GameManager.Instance.AvanzarNivel();
            }
        }

        // ====================================================================
        // REINICIO INTEGRAL DEL NIVEL
        // ====================================================================

        public void ReiniciarNivelCompleto(string motivo = "")
        {
            // 1. Descartar puntaje acumulado en este nivel
            if (GameManager.Instance != null)
            {
                GameManager.Instance.DiscardLevelScore();
            }

            // 2. Restaurar todos los objetos que implementan IResettable
            foreach (var r in resettableObjects)
            {
                if (r != null)
                {
                    r.ResetObject();
                }
            }

            // 3. Regresar al jugador al punto de inicio inicial
            currentCheckpoint = initialSpawnPoint;
            if (playerController != null)
            {
                playerController.transform.position = initialSpawnPoint;
                var rb = playerController.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                }
                playerController.TriggerInvulnerability(1.5f);
            }

            // 4. Reiniciar salida si existe
            var exit = FindAnyObjectByType<LevelExit>();
            if (exit != null) exit.ResetExit();

            NotificarPuntaje();
            Debug.Log($"[El Minero] Nivel reiniciado completamente. Motivo: {motivo}");
        }
    }
}
