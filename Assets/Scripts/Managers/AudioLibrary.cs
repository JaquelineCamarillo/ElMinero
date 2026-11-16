using UnityEngine;

namespace ElMinero.Managers
{
    /// <summary>
    /// ScriptableObject que almacena las referencias directas a todos los AudioClips del juego.
    /// Permite cargarse en tiempo de ejecución tanto en Editor como en Standalone Builds
    /// mediante Resources.Load<AudioLibrary>("AudioLibrary"), evitando dependencias de AssetDatabase.
    /// </summary>
    [CreateAssetMenu(fileName = "AudioLibrary", menuName = "El Minero/Audio Library", order = 2)]
    public class AudioLibrary : ScriptableObject
    {
        [Header("Música de Escenas")]
        public AudioClip musicMenu;
        public AudioClip musicNivel1;
        public AudioClip musicNivel2;
        public AudioClip musicNivel3;
        public AudioClip musicVictoria;
        public AudioClip musicGameOver;

        [Header("Efectos del Jugador")]
        public AudioClip sfxSalto;
        public AudioClip sfxAterrizar;
        public AudioClip sfxPaso1;
        public AudioClip sfxPaso2;
        public AudioClip sfxDanio;
        public AudioClip sfxPerderVida;

        [Header("Efectos de Coleccionables")]
        public AudioClip sfxRecolectarPlata;
        public AudioClip sfxRecolectarGema;
        public AudioClip sfxRecolectarMineral;
        public AudioClip sfxRachaX2;

        [Header("Efectos de Obstáculos y Entorno")]
        public AudioClip sfxRocaCae;
        public AudioClip sfxRocaImpacto;
        public AudioClip sfxPlataformaCruje;
        public AudioClip sfxPlataformaColapso;
        public AudioClip sfxTemblor;
        public AudioClip sfxDerrumbe;
        public AudioClip sfxCheckpoint;
        public AudioClip sfxNivelCompletado;
        public AudioClip sfxTicReloj;

        [Header("Efectos de Enemigos")]
        public AudioClip sfxMurcielagoChillido;
        public AudioClip sfxAranaBaja;
        public AudioClip sfxAlacranAtaque;

        [Header("Loops 3D Ambientales")]
        public AudioClip sfxVagonetaLoop;
        public AudioClip sfxFuegoLoop;
        public AudioClip sfxMalacateLoop;

        [Header("Efectos de Interfaz de Usuario")]
        public AudioClip sfxClickUI;
        public AudioClip sfxHoverUI;
        public AudioClip sfxPausa;
        public AudioClip sfxEstrella1;
        public AudioClip sfxEstrella2;
        public AudioClip sfxEstrella3;
        public AudioClip sfxDesbloqueoNivel;

        [Header("Jefe final, nivel 3 y escena del Templo")]
        public AudioClip musicJefeColoso;
        public AudioClip musicJefeDerrotado;
        public AudioClip musicFinalAmanecer;
        [Tooltip("Efectos nuevos (Coloso, compuertas, campanas, aves, aplausos...). Se buscan por nombre de archivo.")]
        public AudioClip[] clipsExtra;

        /// <summary>Busca un clip por su nombre de archivo (sin extensión).</summary>
        public AudioClip BuscarClip(string nombre)
        {
            if (string.IsNullOrEmpty(nombre)) return null;
            if (musicJefeColoso != null && musicJefeColoso.name == nombre) return musicJefeColoso;
            if (musicJefeDerrotado != null && musicJefeDerrotado.name == nombre) return musicJefeDerrotado;
            if (musicFinalAmanecer != null && musicFinalAmanecer.name == nombre) return musicFinalAmanecer;
            if (clipsExtra != null)
            {
                foreach (var c in clipsExtra)
                {
                    if (c != null && c.name == nombre) return c;
                }
            }
            return null;
        }
    }
}
