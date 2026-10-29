using System.IO;
using UnityEngine;
using UnityEditor;
using ElMinero.Managers;

namespace ElMinero.Editor
{
    /// <summary>
    /// Generador e inicializador del asset AudioLibrary en Assets/Resources/.
    /// Asigna todos los AudioClips del proyecto de forma idempotente para su uso
    /// en tiempo de ejecución sin depender de AssetDatabase.
    /// </summary>
    public static class AudioLibraryBuilder
    {
        private const string ResourcesFolder = "Assets/Resources";
        private const string AssetPath = "Assets/Resources/AudioLibrary.asset";

        [MenuItem("Herramientas/El Minero/Generar Audio Library", priority = 35)]
        public static void GenerarAudioLibrary()
        {
            if (!Directory.Exists(ResourcesFolder))
            {
                Directory.CreateDirectory(ResourcesFolder);
                AssetDatabase.Refresh();
            }

            AudioLibrary library = AssetDatabase.LoadAssetAtPath<AudioLibrary>(AssetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<AudioLibrary>();
                AssetDatabase.CreateAsset(library, AssetPath);
            }

            string pMusica = "Assets/Audios/Musica/";
            string pSfx = "Assets/Audios/SFX/";

            // Música
            library.musicMenu = AssetDatabase.LoadAssetAtPath<AudioClip>(pMusica + "Musica_Menu.ogg");
            library.musicNivel1 = AssetDatabase.LoadAssetAtPath<AudioClip>(pMusica + "Musica_Nivel1_MinaRayas.ogg");
            library.musicNivel2 = AssetDatabase.LoadAssetAtPath<AudioClip>(pMusica + "Musica_Nivel2_ElNopal.ogg");
            library.musicNivel3 = AssetDatabase.LoadAssetAtPath<AudioClip>(pMusica + "Musica_Nivel3_LaValenciana.ogg");
            library.musicVictoria = AssetDatabase.LoadAssetAtPath<AudioClip>(pMusica + "Musica_Victoria.ogg");
            library.musicGameOver = AssetDatabase.LoadAssetAtPath<AudioClip>(pMusica + "Musica_GameOver.ogg");

            // SFX Jugador
            library.sfxSalto = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Salto.wav");
            library.sfxAterrizar = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Aterrizar.wav");
            library.sfxPaso1 = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Paso_1.wav");
            library.sfxPaso2 = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Paso_2.wav");
            library.sfxDanio = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Danio.wav");
            library.sfxPerderVida = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Perder_Vida.wav");

            // SFX Coleccionables
            library.sfxRecolectarPlata = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Recolectar_Plata.wav");
            library.sfxRecolectarGema = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Recolectar_Gema.wav");
            library.sfxRecolectarMineral = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Recolectar_Mineral.wav");
            library.sfxRachaX2 = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Racha_x2.wav");

            // SFX Obstáculos y Entorno
            library.sfxRocaCae = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Roca_Cae.wav");
            library.sfxRocaImpacto = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Roca_Impacto.wav");
            library.sfxPlataformaCruje = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Plataforma_Cruje.wav");
            library.sfxPlataformaColapso = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Plataforma_Colapso.wav");
            library.sfxTemblor = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Temblor.wav");
            library.sfxDerrumbe = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Derrumbe.wav");
            library.sfxCheckpoint = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Checkpoint.wav");
            library.sfxNivelCompletado = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Nivel_Completado.wav");
            library.sfxTicReloj = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Tic_Reloj.wav");

            // SFX Enemigos
            library.sfxMurcielagoChillido = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Murcielago_Chillido.wav");
            library.sfxAranaBaja = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Arana_Baja.wav");
            library.sfxAlacranAtaque = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Alacran_Ataque.wav");

            // Loops 3D
            library.sfxVagonetaLoop = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Vagoneta_Rodando_Loop.wav");
            library.sfxFuegoLoop = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Fuego_Loop.wav");
            library.sfxMalacateLoop = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Malacate_Loop.wav");

            // SFX UI
            library.sfxClickUI = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Click_UI.wav");
            library.sfxHoverUI = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Hover_UI.wav");
            library.sfxPausa = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Pausa.wav");
            library.sfxEstrella1 = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Estrella_1.wav");
            library.sfxEstrella2 = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Estrella_2.wav");
            library.sfxEstrella3 = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Estrella_3.wav");
            library.sfxDesbloqueoNivel = AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Desbloqueo_Nivel.wav");

            EditorUtility.SetDirty(library);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[El Minero] AudioLibrary generada y asignada exitosamente en " + AssetPath);
        }
    }
}
