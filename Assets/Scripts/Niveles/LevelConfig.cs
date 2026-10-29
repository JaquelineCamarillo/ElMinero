using UnityEngine;

namespace ElMinero.Niveles
{
    /// <summary>
    /// Configuración de parámetros específicos de cada nivel mediante ScriptableObject.
    /// Define tiempo límite, cantidad de coleccionables, mineral distintivo, miniatura, dato histórico, paleta y dificultad.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "El Minero/Level Config", order = 1)]
    public class LevelConfig : ScriptableObject
    {
        [Header("Información General")]
        [Tooltip("Nombre descriptivo del nivel (ej. Mina Rayas, El Nopal, La Valenciana).")]
        [SerializeField] private string levelName = "Nivel 1 — Mina Rayas";

        [Tooltip("Índice numérico del nivel (1, 2 o 3).")]
        [SerializeField] private int levelIndex = 1;

        [Tooltip("Miniatura representativa para la tarjeta de inicio del nivel.")]
        [SerializeField] private Sprite miniatura;

        [TextArea(2, 4)]
        [Tooltip("Dato histórico de la mina para la tarjeta de inicio.")]
        [SerializeField] private string datoHistorico = "Mina de Rayas, descubierta en 1558 por Juan Rayas. Tiene el tiro más profundo de Guanajuato (425 m).";

        [Header("Tiempos y Derrumbe")]
        [Tooltip("Tiempo límite en segundos para completar el nivel antes del derrumbe.")]
        [SerializeField] private float timeLimit = 100f;

        [Header("Coleccionables")]
        [Tooltip("Cantidad esperada de vetas de plata en este nivel.")]
        [SerializeField] private int silverVeinCount = 6;

        [Tooltip("Cantidad esperada de gemas de amatista en este nivel.")]
        [SerializeField] private int amethystGemCount = 2;

        [Tooltip("Puntaje otorgado por cada veta de plata (GDD: 10).")]
        [SerializeField] private int silverVeinPoints = 10;

        [Tooltip("Puntaje otorgado por cada gema de amatista (GDD: 25).")]
        [SerializeField] private int amethystGemPoints = 25;

        [Tooltip("Bono por recolectar todas las vetas de plata del nivel (GDD: 50).")]
        [SerializeField] private int allSilverBonus = 50;

        [Header("Mineral Distintivo Histórico")]
        [SerializeField] private string distinctiveMineralName = "Aguilarita";
        [SerializeField] private int distinctiveMineralPoints = 50;
        [TextArea(2, 4)]
        [SerializeField] private string distinctiveMineralDescription = "Aguilarita: mineral de plata y selenio descubierto en Guanajuato.";

        [Header("Obstáculos y Dinámicas")]
        [Tooltip("Intervalo sugerido en segundos entre caída de rocas.")]
        [SerializeField] private float rockSpawnInterval = 4.0f;

        [Header("Ambientación")]
        [Tooltip("Color de fondo para la cámara de este nivel.")]
        [SerializeField] private Color cameraBackgroundColor = new Color(0.12f, 0.09f, 0.19f);

        // Propiedades públicas
        public string LevelName => levelName;
        public int LevelIndex => levelIndex;
        public Sprite Miniatura => miniatura;
        public string DatoHistorico => datoHistorico;
        public float TimeLimit => timeLimit;
        public float InitialTime => timeLimit;
        public int SilverVeinCount => silverVeinCount;
        public int AmethystGemCount => amethystGemCount;
        public int SilverVeinPoints => silverVeinPoints;
        public int AmethystGemPoints => amethystGemPoints;
        public int AllSilverBonus => allSilverBonus;
        public string DistinctiveMineralName => distinctiveMineralName;
        public int DistinctiveMineralPoints => distinctiveMineralPoints;
        public string DistinctiveMineralDescription => distinctiveMineralDescription;
        public float RockSpawnInterval => rockSpawnInterval;
        public Color CameraBackgroundColor => cameraBackgroundColor;

        public void Configurar(string nombre, int indice, float tiempo, int vetas, int amatistas, string minNombre, string minDesc, float intervaloRocas, Color colorFondo, Sprite mini = null, string dato = "")
        {
            levelName = nombre;
            levelIndex = indice;
            timeLimit = tiempo;
            silverVeinCount = vetas;
            amethystGemCount = amatistas;
            distinctiveMineralName = minNombre;
            distinctiveMineralDescription = minDesc;
            rockSpawnInterval = intervaloRocas;
            cameraBackgroundColor = colorFondo;
            if (mini != null) miniatura = mini;
            if (!string.IsNullOrEmpty(dato)) datoHistorico = dato;
        }
    }
}
