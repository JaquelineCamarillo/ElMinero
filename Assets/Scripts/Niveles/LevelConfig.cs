using UnityEngine;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Configuración de parámetros específicos de cada nivel mediante ScriptableObject.
    /// Define tiempo límite, cantidad de coleccionables, paleta y dificultad.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "El Minero/Level Config", order = 1)]
    public class LevelConfig : ScriptableObject
    {
        [Header("Información General")]
        [Tooltip("Nombre descriptivo del nivel (ej. Mina Rayas, El Nopal, La Valenciana).")]
        [SerializeField] private string levelName = "Nivel 1 — Mina Rayas";

        [Tooltip("Índice numérico del nivel (1, 2 o 3).")]
        [SerializeField] private int levelIndex = 1;

        [Header("Tiempos y Derrumbe")]
        [Tooltip("Tiempo límite en segundos para completar el nivel antes del derrumbe.")]
        [SerializeField] private float timeLimit = 90f;

        [Header("Coleccionables")]
        [Tooltip("Cantidad esperada de vetas de plata en este nivel.")]
        [SerializeField] private int silverVeinCount = 4;

        [Tooltip("Cantidad esperada de gemas de amatista en este nivel.")]
        [SerializeField] private int amethystGemCount = 1;

        [Tooltip("Puntaje otorgado por cada veta de plata (GDD: 10).")]
        [SerializeField] private int silverVeinPoints = 10;

        [Tooltip("Puntaje otorgado por cada gema de amatista (GDD: 25).")]
        [SerializeField] private int amethystGemPoints = 25;

        [Tooltip("Bono por recolectar todas las vetas de plata del nivel (GDD: 50).")]
        [SerializeField] private int allSilverBonus = 50;

        [Header("Obstáculos y Dinámicas")]
        [Tooltip("Intervalo sugerido en segundos entre caída de rocas.")]
        [SerializeField] private float rockSpawnInterval = 4.0f;

        [Header("Ambientación")]
        [Tooltip("Color de fondo para la cámara de este nivel.")]
        [SerializeField] private Color cameraBackgroundColor = new Color(0.12f, 0.09f, 0.19f); // Violeta Mina Rayas

        // Propiedades públicas
        public string LevelName => levelName;
        public int LevelIndex => levelIndex;
        public float TimeLimit => timeLimit;
        public int SilverVeinCount => silverVeinCount;
        public int AmethystGemCount => amethystGemCount;
        public int SilverVeinPoints => silverVeinPoints;
        public int AmethystGemPoints => amethystGemPoints;
        public int AllSilverBonus => allSilverBonus;
        public float RockSpawnInterval => rockSpawnInterval;
        public Color CameraBackgroundColor => cameraBackgroundColor;
    }
}
