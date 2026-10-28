using UnityEngine;
using ElMinero.Jugador;
using ElMinero.Managers;

namespace ElMinero.Coleccionables
{
    /// <summary>
    /// Mineral distintivo único de cada mina (50 puntos):
    /// - Mina de Rayas: Aguilarita
    /// - El Nopal: Pepita de oro
    /// - La Valenciana: Pirargirita (plata roja)
    /// Al recolectarse, reproduce sonido característico, suma 50 puntos y despliega
    /// un cartel emergente de 3 s en pantalla con información mineralógica e histórica.
    /// </summary>
    public class DistinctiveMineral : Collectible
    {
        [Header("Datos del Mineral")]
        [Tooltip("Nombre del mineral (Aguilarita, Pepita de oro, Pirargirita).")]
        [SerializeField] private string mineralName = "Aguilarita";

        [Tooltip("Texto histórico descriptivo para el cartel emergente.")]
        [TextArea(2, 4)]
        [SerializeField] private string historicalDescription = "";

        [Tooltip("Número de nivel al que corresponde (1, 2 o 3).")]
        [SerializeField] private int levelIndex = 1;

        public string MineralName => mineralName;
        public string HistoricalDescription => historicalDescription;
        public int LevelIndex => levelIndex;

        protected override void Awake()
        {
            basePoints = 50;
            base.Awake();
        }

        protected override void OnCollect(PlayerController player)
        {
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayRecolectarMineral();
            }

            // Marcar en PlayerPrefs
            PlayerPrefs.SetInt($"MineralEncontrado_N{levelIndex}", 1);
            PlayerPrefs.Save();

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.RecolectarMineralDistintivo(this);
            }
        }

        public void ConfigurarMineral(string nombre, string descripcion, int nivel)
        {
            mineralName = nombre;
            historicalDescription = descripcion;
            levelIndex = nivel;
        }
    }
}
