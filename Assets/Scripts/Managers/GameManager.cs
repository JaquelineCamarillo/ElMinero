using UnityEngine;
using UnityEngine.SceneManagement;
using ElMinero.Utilidades;

namespace ElMinero.Managers
{
    /// <summary>
    /// Administrador global persistente del juego (Singleton, DontDestroyOnLoad).
    /// Almacena el nombre del jugador, puntaje acumulado, récord histórico y flujo de escenas.
    /// Se autocrea si no existe al iniciar cualquier escena en el Editor.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;

        public static GameManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = UnityEngine.Object.FindAnyObjectByType<GameManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("[GameManager]");
                        instance = go.AddComponent<GameManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Datos del Jugador")]
        [SerializeField] private string playerName = Constantes.Prefs.NombrePorDefecto;

        [Header("Puntajes y Estadísticas")]
        [SerializeField] private int totalScore = 0;
        [SerializeField] private int currentLevelScore = 0;
        [SerializeField] private int highScore = 0;
        [SerializeField] private int totalSilverCollected = 0;
        [SerializeField] private int currentLevelIndex = 1;

        // Motivo de derrota (para mostrar en pantalla de Derrota)
        public string DefeatReason { get; set; } = "El derrumbe te alcanzó...";
        public string LastDefeatReason => DefeatReason;
        public string LastLevelSceneName { get; set; } = Constantes.Escenas.Nivel1;

        public string PlayerName => playerName;
        public int TotalScore => totalScore;
        public int CurrentLevelScore => currentLevelScore;
        public int CombinedScore => totalScore + currentLevelScore;
        public int HighScore => highScore;
        public int TotalSilverCollected => totalSilverCollected;
        public int CurrentLevelIndex => currentLevelIndex;

        public void ResetScore()
        {
            totalScore = 0;
            currentLevelScore = 0;
            totalSilverCollected = 0;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            CargarDatosGuardados();
        }

        private void CargarDatosGuardados()
        {
            playerName = PlayerPrefs.GetString(Constantes.Prefs.NombreJugador, Constantes.Prefs.NombrePorDefecto);
            if (string.IsNullOrWhiteSpace(playerName))
            {
                playerName = Constantes.Prefs.NombrePorDefecto;
            }

            highScore = PlayerPrefs.GetInt(Constantes.Prefs.MejorPuntaje, 0);
        }

        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                playerName = Constantes.Prefs.NombrePorDefecto;
            }
            else
            {
                playerName = newName.Trim();
                if (playerName.Length > 12)
                {
                    playerName = playerName.Substring(0, 12);
                }
            }

            PlayerPrefs.SetString(Constantes.Prefs.NombreJugador, playerName);
            PlayerPrefs.Save();
        }

        public void AddScore(int points)
        {
            currentLevelScore += points;
        }

        public void AddSilver()
        {
            totalSilverCollected++;
        }

        /// <summary>
        /// Confirma el puntaje del nivel al completarlo exitosamente y lo suma al total.
        /// </summary>
        public void CommitLevelScore()
        {
            totalScore += currentLevelScore;
            currentLevelScore = 0;
            CheckAndSaveHighScore();
        }

        /// <summary>
        /// Descarta el puntaje del nivel actual si se reinicia tras perder vidas o tiempo.
        /// </summary>
        public void DiscardLevelScore()
        {
            currentLevelScore = 0;
        }

        public void CheckAndSaveHighScore()
        {
            int currentTotal = totalScore + currentLevelScore;
            if (currentTotal > highScore)
            {
                highScore = currentTotal;
                PlayerPrefs.SetInt(Constantes.Prefs.MejorPuntaje, highScore);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// Inicia una nueva partida desde el Nivel 1 reseteando el puntaje total.
        /// </summary>
        public void IniciarNuevaPartida()
        {
            totalScore = 0;
            currentLevelScore = 0;
            totalSilverCollected = 0;
            currentLevelIndex = 1;
            CargarEscena(Constantes.Escenas.Nivel1);
        }

        public void AvanzarNivel()
        {
            CommitLevelScore();
            currentLevelIndex++;

            if (currentLevelIndex == 2)
            {
                CargarEscena(Constantes.Escenas.Nivel2);
            }
            else if (currentLevelIndex == 3)
            {
                CargarEscena(Constantes.Escenas.Nivel3);
            }
            else
            {
                // Victoria tras completar el Nivel 3
                CargarEscena(Constantes.Escenas.Victoria);
            }
        }

        public void CargarEscena(string nombreEscena)
        {
            SceneLoader.LoadScene(nombreEscena);
        }
    }
}
