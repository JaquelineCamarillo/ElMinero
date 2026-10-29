using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace ElMinero.Managers
{
    /// <summary>
    /// Administrador de audio global (Singleton, DontDestroyOnLoad).
    /// Controla la música por escena con fundido cruzado (crossfade), aceleración de intensidad
    /// en el derrumbe final, control directo y limpio de volumen (0-1) sin depender de AudioMixer exposed parameters,
    /// pool de SFX y registro de AudioSources 3D para peligros ambientales.
    /// Carga todos los AudioClips mediante AudioLibrary en Resources para garantizar compatibilidad
    /// tanto en el Editor como en builds ejecutables (.exe, WebGL, Android).
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Object.FindAnyObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("[AudioManager]");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Biblioteca de Audio")]
        [SerializeField] private AudioLibrary audioLibrary;

        [Header("Fuentes de Música (Crossfade)")]
        [SerializeField] private AudioSource musicSourceA;
        [SerializeField] private AudioSource musicSourceB;
        private AudioSource activeMusicSource;
        private Coroutine crossfadeCoroutine;

        [Header("Pool de Efectos (SFX)")]
        [SerializeField] private int sfxPoolSize = 16;
        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentSfxIndex = 0;

        [Header("Fuentes 3D Registradas")]
        private readonly HashSet<AudioSource> fuentesEfectos3D = new HashSet<AudioSource>();

        [Header("Música")]
        [SerializeField] private AudioClip musicMenu;
        [SerializeField] private AudioClip musicNivel1;
        [SerializeField] private AudioClip musicNivel2;
        [SerializeField] private AudioClip musicNivel3;
        [SerializeField] private AudioClip musicVictoria;
        [SerializeField] private AudioClip musicGameOver;

        [Header("Efectos del Jugador")]
        [SerializeField] private AudioClip sfxSalto;
        [SerializeField] private AudioClip sfxAterrizar;
        [SerializeField] private AudioClip sfxPaso1;
        [SerializeField] private AudioClip sfxPaso2;
        [SerializeField] private AudioClip sfxDanio;
        [SerializeField] private AudioClip sfxPerderVida;

        [Header("Efectos de Coleccionables")]
        [SerializeField] private AudioClip sfxRecolectarPlata;
        [SerializeField] private AudioClip sfxRecolectarGema;
        [SerializeField] private AudioClip sfxRecolectarMineral;
        [SerializeField] private AudioClip sfxRachaX2;

        [Header("Efectos de Obstáculos y Entorno")]
        [SerializeField] private AudioClip sfxRocaCae;
        [SerializeField] private AudioClip sfxRocaImpacto;
        [SerializeField] private AudioClip sfxPlataformaCruje;
        [SerializeField] private AudioClip sfxPlataformaColapso;
        [SerializeField] private AudioClip sfxTemblor;
        [SerializeField] private AudioClip sfxDerrumbe;
        [SerializeField] private AudioClip sfxCheckpoint;
        [SerializeField] private AudioClip sfxNivelCompletado;
        [SerializeField] private AudioClip sfxTicReloj;

        [Header("Efectos de Enemigos")]
        [SerializeField] private AudioClip sfxMurcielagoChillido;
        [SerializeField] private AudioClip sfxAranaBaja;
        [SerializeField] private AudioClip sfxAlacranAtaque;

        [Header("Loops 3D")]
        [SerializeField] private AudioClip sfxVagonetaLoop;
        [SerializeField] private AudioClip sfxFuegoLoop;
        [SerializeField] private AudioClip sfxMalacateLoop;

        [Header("Efectos de UI")]
        [SerializeField] private AudioClip sfxClickUI;
        [SerializeField] private AudioClip sfxHoverUI;
        [SerializeField] private AudioClip sfxPausa;
        [SerializeField] private AudioClip sfxEstrella1;
        [SerializeField] private AudioClip sfxEstrella2;
        [SerializeField] private AudioClip sfxEstrella3;
        [SerializeField] private AudioClip sfxDesbloqueoNivel;

        private bool pasoAlternado = false;
        private bool isUrgent = false;
        private float pauseMuffleFactor = 1.0f;
        private float volumenMusica = 0.8f;
        private float volumenEfectos = 0.8f;
        private bool musicActive = true;

        public float VolumenMusica => volumenMusica;
        public float VolumenEfectos => volumenEfectos;
        public bool MusicaActiva => musicActive;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            CargarPreferenciasVolumen();
            InicializarAudioSources();
            CargarRecursosDeAudio();
        }

        private void CargarPreferenciasVolumen()
        {
            volumenMusica = PlayerPrefs.GetFloat("VolMusica", 0.8f);
            volumenEfectos = PlayerPrefs.GetFloat("VolEfectos", 0.8f);
            musicActive = PlayerPrefs.GetInt("MusicaActiva", 1) == 1;
        }

        private void InicializarAudioSources()
        {
            // Dos fuentes para crossfade
            if (musicSourceA == null)
            {
                musicSourceA = gameObject.AddComponent<AudioSource>();
                musicSourceA.loop = true;
                musicSourceA.playOnAwake = false;
            }

            if (musicSourceB == null)
            {
                musicSourceB = gameObject.AddComponent<AudioSource>();
                musicSourceB.loop = true;
                musicSourceB.playOnAwake = false;
            }

            activeMusicSource = musicSourceA;

            // Pool de SFX
            if (sfxPool.Count == 0)
            {
                for (int i = 0; i < sfxPoolSize; i++)
                {
                    AudioSource src = gameObject.AddComponent<AudioSource>();
                    src.loop = false;
                    src.playOnAwake = false;
                    sfxPool.Add(src);
                }
            }
        }

        public void CargarRecursosDeAudio()
        {
            if (audioLibrary == null)
            {
                audioLibrary = Resources.Load<AudioLibrary>("AudioLibrary");
            }

            if (audioLibrary != null)
            {
                musicMenu = audioLibrary.musicMenu;
                musicNivel1 = audioLibrary.musicNivel1;
                musicNivel2 = audioLibrary.musicNivel2;
                musicNivel3 = audioLibrary.musicNivel3;
                musicVictoria = audioLibrary.musicVictoria;
                musicGameOver = audioLibrary.musicGameOver;

                sfxSalto = audioLibrary.sfxSalto;
                sfxAterrizar = audioLibrary.sfxAterrizar;
                sfxPaso1 = audioLibrary.sfxPaso1;
                sfxPaso2 = audioLibrary.sfxPaso2;
                sfxDanio = audioLibrary.sfxDanio;
                sfxPerderVida = audioLibrary.sfxPerderVida;

                sfxRecolectarPlata = audioLibrary.sfxRecolectarPlata;
                sfxRecolectarGema = audioLibrary.sfxRecolectarGema;
                sfxRecolectarMineral = audioLibrary.sfxRecolectarMineral;
                sfxRachaX2 = audioLibrary.sfxRachaX2;

                sfxRocaCae = audioLibrary.sfxRocaCae;
                sfxRocaImpacto = audioLibrary.sfxRocaImpacto;
                sfxPlataformaCruje = audioLibrary.sfxPlataformaCruje;
                sfxPlataformaColapso = audioLibrary.sfxPlataformaColapso;
                sfxTemblor = audioLibrary.sfxTemblor;
                sfxDerrumbe = audioLibrary.sfxDerrumbe;
                sfxCheckpoint = audioLibrary.sfxCheckpoint;
                sfxNivelCompletado = audioLibrary.sfxNivelCompletado;
                sfxTicReloj = audioLibrary.sfxTicReloj;

                sfxMurcielagoChillido = audioLibrary.sfxMurcielagoChillido;
                sfxAranaBaja = audioLibrary.sfxAranaBaja;
                sfxAlacranAtaque = audioLibrary.sfxAlacranAtaque;

                sfxVagonetaLoop = audioLibrary.sfxVagonetaLoop;
                sfxFuegoLoop = audioLibrary.sfxFuegoLoop;
                sfxMalacateLoop = audioLibrary.sfxMalacateLoop;

                sfxClickUI = audioLibrary.sfxClickUI;
                sfxHoverUI = audioLibrary.sfxHoverUI;
                sfxPausa = audioLibrary.sfxPausa;
                sfxEstrella1 = audioLibrary.sfxEstrella1;
                sfxEstrella2 = audioLibrary.sfxEstrella2;
                sfxEstrella3 = audioLibrary.sfxEstrella3;
                sfxDesbloqueoNivel = audioLibrary.sfxDesbloqueoNivel;
            }
#if UNITY_EDITOR
            else
            {
                // Respaldo en editor si AudioLibrary.asset aún no se ha generado
                string pMusic = "Assets/Audios/Musica/";
                string pSfx = "Assets/Audios/SFX/";

                if (musicMenu == null) musicMenu = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pMusic + "Musica_Menu.ogg");
                if (musicNivel1 == null) musicNivel1 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pMusic + "Musica_Nivel1_MinaRayas.ogg");
                if (musicNivel2 == null) musicNivel2 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pMusic + "Musica_Nivel2_ElNopal.ogg");
                if (musicNivel3 == null) musicNivel3 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pMusic + "Musica_Nivel3_LaValenciana.ogg");
                if (musicVictoria == null) musicVictoria = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pMusic + "Musica_Victoria.ogg");
                if (musicGameOver == null) musicGameOver = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pMusic + "Musica_GameOver.ogg");

                if (sfxSalto == null) sfxSalto = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Salto.wav");
                if (sfxAterrizar == null) sfxAterrizar = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Aterrizar.wav");
                if (sfxPaso1 == null) sfxPaso1 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Paso_1.wav");
                if (sfxPaso2 == null) sfxPaso2 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Paso_2.wav");
                if (sfxDanio == null) sfxDanio = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Danio.wav");
                if (sfxPerderVida == null) sfxPerderVida = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Perder_Vida.wav");

                if (sfxRecolectarPlata == null) sfxRecolectarPlata = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Recolectar_Plata.wav");
                if (sfxRecolectarGema == null) sfxRecolectarGema = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Recolectar_Gema.wav");
                if (sfxRecolectarMineral == null) sfxRecolectarMineral = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Recolectar_Mineral.wav");
                if (sfxRachaX2 == null) sfxRachaX2 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Racha_x2.wav");

                if (sfxRocaCae == null) sfxRocaCae = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Roca_Cae.wav");
                if (sfxRocaImpacto == null) sfxRocaImpacto = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Roca_Impacto.wav");
                if (sfxPlataformaCruje == null) sfxPlataformaCruje = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Plataforma_Cruje.wav");
                if (sfxPlataformaColapso == null) sfxPlataformaColapso = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Plataforma_Colapso.wav");
                if (sfxTemblor == null) sfxTemblor = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Temblor.wav");
                if (sfxDerrumbe == null) sfxDerrumbe = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Derrumbe.wav");
                if (sfxCheckpoint == null) sfxCheckpoint = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Checkpoint.wav");
                if (sfxNivelCompletado == null) sfxNivelCompletado = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Nivel_Completado.wav");
                if (sfxTicReloj == null) sfxTicReloj = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Tic_Reloj.wav");

                if (sfxMurcielagoChillido == null) sfxMurcielagoChillido = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Murcielago_Chillido.wav");
                if (sfxAranaBaja == null) sfxAranaBaja = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Arana_Baja.wav");
                if (sfxAlacranAtaque == null) sfxAlacranAtaque = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Alacran_Ataque.wav");

                if (sfxVagonetaLoop == null) sfxVagonetaLoop = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Vagoneta_Rodando_Loop.wav");
                if (sfxFuegoLoop == null) sfxFuegoLoop = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Fuego_Loop.wav");
                if (sfxMalacateLoop == null) sfxMalacateLoop = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Malacate_Loop.wav");

                if (sfxClickUI == null) sfxClickUI = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Click_UI.wav");
                if (sfxHoverUI == null) sfxHoverUI = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Hover_UI.wav");
                if (sfxPausa == null) sfxPausa = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Pausa.wav");
                if (sfxEstrella1 == null) sfxEstrella1 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Estrella_1.wav");
                if (sfxEstrella2 == null) sfxEstrella2 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Estrella_2.wav");
                if (sfxEstrella3 == null) sfxEstrella3 = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Estrella_3.wav");
                if (sfxDesbloqueoNivel == null) sfxDesbloqueoNivel = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>(pSfx + "Desbloqueo_Nivel.wav");
            }
#endif
        }

        // ====================================================================
        // GESTIÓN DE MÚSICA CON CROSSFADE
        // ====================================================================

        private float CalcularVolumenObjetivoMusica()
        {
            if (!musicActive) return 0f;
            float target = volumenMusica * pauseMuffleFactor;
            if (isUrgent) target = Mathf.Min(1.0f, target * 1.2f);
            return target;
        }

        public void PlayMusic(AudioClip clip, bool loop = true, float fadeDuration = 0.8f)
        {
            if (clip == null) return;

            if (activeMusicSource != null && activeMusicSource.clip == clip && activeMusicSource.isPlaying)
                return;

            if (crossfadeCoroutine != null)
            {
                StopCoroutine(crossfadeCoroutine);
            }

            crossfadeCoroutine = StartCoroutine(RutinaCrossfade(clip, loop, fadeDuration));
        }

        private IEnumerator RutinaCrossfade(AudioClip newClip, bool loop, float duration)
        {
            AudioSource outgoingSource = activeMusicSource;
            AudioSource incomingSource = (activeMusicSource == musicSourceA) ? musicSourceB : musicSourceA;

            incomingSource.clip = newClip;
            incomingSource.loop = loop;
            incomingSource.pitch = isUrgent ? 1.08f : 1.0f;
            incomingSource.volume = 0f;
            incomingSource.Play();

            float elapsed = 0f;
            float initialOutVol = outgoingSource != null ? outgoingSource.volume : 0f;
            float targetInVol = CalcularVolumenObjetivoMusica();

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);

                if (outgoingSource != null)
                {
                    outgoingSource.volume = Mathf.Lerp(initialOutVol, 0f, t);
                }
                incomingSource.volume = Mathf.Lerp(0f, targetInVol, t);

                yield return null;
            }

            if (outgoingSource != null)
            {
                outgoingSource.Stop();
                outgoingSource.volume = 0f;
            }

            incomingSource.volume = targetInVol;
            activeMusicSource = incomingSource;
            crossfadeCoroutine = null;
        }

        public void PlayMusicMenu()
        {
            Debug.Log("[AudioManager] Reproduciendo pista de música: Menú Principal (Vals)");
            SetMusicUrgency(false);
            PlayMusic(musicMenu, true);
        }

        public void PlayMusicIntro()
        {
            Debug.Log("[AudioManager] Reproduciendo pista de música: Intro (Vals)");
            SetMusicUrgency(false);
            PlayMusic(musicMenu, true);
        }

        public void PlayMusicCreditos()
        {
            Debug.Log("[AudioManager] Reproduciendo pista de música: Créditos (Vals)");
            SetMusicUrgency(false);
            PlayMusic(musicMenu, true);
        }

        public void PlayMusicNivel(int nivel)
        {
            SetMusicUrgency(false);
            Debug.Log($"[AudioManager] Reproduciendo pista de música para Nivel {nivel}");
            switch (nivel)
            {
                case 1:
                    Debug.Log("[AudioManager] Pista Nivel 1: Misteriosa (Mina Rayas)");
                    PlayMusic(musicNivel1, true);
                    break;
                case 2:
                    Debug.Log("[AudioManager] Pista Nivel 2: Rítmica (El Nopal)");
                    PlayMusic(musicNivel2, true);
                    break;
                case 3:
                    Debug.Log("[AudioManager] Pista Nivel 3: Urgente (La Valenciana)");
                    PlayMusic(musicNivel3, true);
                    break;
                default:
                    PlayMusic(musicMenu, true);
                    break;
            }
        }

        public void PlayMusicVictoria()
        {
            Debug.Log("[AudioManager] Reproduciendo pista de música: Victoria Fanfarria");
            SetMusicUrgency(false);
            PlayMusic(musicVictoria, false, 0.4f);
        }

        public void PlayMusicGameOver()
        {
            Debug.Log("[AudioManager] Reproduciendo pista de música: Game Over");
            SetMusicUrgency(false);
            PlayMusic(musicGameOver, false, 0.4f);
        }

        public void StopMusic()
        {
            if (crossfadeCoroutine != null) StopCoroutine(crossfadeCoroutine);
            if (musicSourceA != null) musicSourceA.Stop();
            if (musicSourceB != null) musicSourceB.Stop();
        }

        public void SetPauseMusicMuffle(bool paused)
        {
            pauseMuffleFactor = paused ? 0.35f : 1.0f;
            ActualizarVolumenMusicaActiva();
        }

        public void SetMusicUrgency(bool urgent)
        {
            if (isUrgent == urgent) return;
            isUrgent = urgent;

            if (activeMusicSource != null)
            {
                activeMusicSource.pitch = isUrgent ? 1.08f : 1.0f;
            }
            ActualizarVolumenMusicaActiva();
        }

        private void ActualizarVolumenMusicaActiva()
        {
            if (activeMusicSource != null && crossfadeCoroutine == null)
            {
                activeMusicSource.volume = CalcularVolumenObjetivoMusica();
            }
        }

        // ====================================================================
        // CONTROL DIRECTO DE VOLUMEN (Sin AudioMixer SetFloat)
        // ====================================================================

        public void SetMusicVolume(float normalizedVolume)
        {
            volumenMusica = Mathf.Clamp01(normalizedVolume);
            PlayerPrefs.SetFloat("VolMusica", volumenMusica);
            PlayerPrefs.Save();
            ActualizarVolumenMusicaActiva();
        }

        public void SetSFXVolume(float normalizedVolume)
        {
            volumenEfectos = Mathf.Clamp01(normalizedVolume);
            PlayerPrefs.SetFloat("VolEfectos", volumenEfectos);
            PlayerPrefs.Save();
            ActualizarFuentes3D();
        }

        public void SetMusicActive(bool active)
        {
            musicActive = active;
            PlayerPrefs.SetInt("MusicaActiva", active ? 1 : 0);
            PlayerPrefs.Save();
            ActualizarVolumenMusicaActiva();
        }

        // ====================================================================
        // REGISTRO DE FUENTES 3D AMBIENTALES
        // ====================================================================

        public void RegistrarFuenteEfectos(AudioSource src)
        {
            if (src == null) return;
            if (!fuentesEfectos3D.Contains(src))
            {
                fuentesEfectos3D.Add(src);
            }
            src.volume = volumenEfectos;
        }

        public void DesregistrarFuenteEfectos(AudioSource src)
        {
            if (src == null) return;
            if (fuentesEfectos3D.Contains(src))
            {
                fuentesEfectos3D.Remove(src);
            }
        }

        private void ActualizarFuentes3D()
        {
            fuentesEfectos3D.RemoveWhere(s => s == null);
            foreach (var src in fuentesEfectos3D)
            {
                if (src != null)
                {
                    src.volume = volumenEfectos;
                }
            }
        }

        // ====================================================================
        // EFECTOS DE SONIDO (SFX POOL)
        // ====================================================================

        public void PlaySFX(AudioClip clip, float volume = 1.0f, float pitchVariation = 0f)
        {
            if (clip == null || sfxPool.Count == 0) return;

            AudioSource src = sfxPool[currentSfxIndex];
            currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Count;

            src.pitch = 1.0f + (pitchVariation > 0f ? Random.Range(-pitchVariation, pitchVariation) : 0f);
            src.PlayOneShot(clip, Mathf.Clamp01(volume * volumenEfectos));
        }

        // Métodos de acceso rápido para SFX del juego
        public void PlaySalto() => PlaySFX(sfxSalto, 0.85f, 0.05f);
        public void PlayAterrizar() => PlaySFX(sfxAterrizar, 0.75f, 0.05f);
        public void PlayPasos()
        {
            pasoAlternado = !pasoAlternado;
            PlaySFX(pasoAlternado ? sfxPaso1 : sfxPaso2, 0.45f, 0.05f);
        }
        public void PlayDanio() => PlaySFX(sfxDanio, 0.9f);
        public void PlayDanioSuave() => PlaySFX(sfxDanio, 0.3f);
        public void PlayPerderVida() => PlaySFX(sfxPerderVida, 1.0f);

        public void PlayRecolectarPlata() => PlaySFX(sfxRecolectarPlata, 0.85f, 0.05f);
        public void PlayRecolectarGema() => PlaySFX(sfxRecolectarGema, 1.0f, 0.05f);
        public void PlayRecolectarMineral() => PlaySFX(sfxRecolectarMineral, 1.0f);
        public void PlayRachaX2() => PlaySFX(sfxRachaX2, 1.0f);

        public void PlayRocaCae() => PlaySFX(sfxRocaCae, 0.75f);
        public void PlayRocaImpacto() => PlaySFX(sfxRocaImpacto, 0.85f, 0.05f);
        public void PlayPlataformaCruje() => PlaySFX(sfxPlataformaCruje, 0.85f);
        public void PlayPlataformaColapso() => PlaySFX(sfxPlataformaColapso, 0.95f);
        public void PlayTemblor() => PlaySFX(sfxTemblor, 1.0f);
        public void PlayDerrumbe() => PlaySFX(sfxDerrumbe, 1.0f);
        public void PlayCheckpoint() => PlaySFX(sfxCheckpoint, 0.9f);
        public void PlayNivelCompletado() => PlaySFX(sfxNivelCompletado, 1.0f);
        public void PlayTicReloj() => PlaySFX(sfxTicReloj, 0.8f);

        public void PlayMurcielagoChillido() => PlaySFX(sfxMurcielagoChillido, 0.8f, 0.1f);
        public void PlayAranaBaja() => PlaySFX(sfxAranaBaja, 0.85f);
        public void PlayAlacranAtaque() => PlaySFX(sfxAlacranAtaque, 0.9f);

        public void PlayClickUI() => PlaySFX(sfxClickUI, 0.75f);
        public void PlayHoverUI() => PlaySFX(sfxHoverUI, 0.45f);
        public void PlayPausa() => PlaySFX(sfxPausa, 0.8f);
        public void PlayEstrella(int estrellaIndex)
        {
            switch (estrellaIndex)
            {
                case 1: PlaySFX(sfxEstrella1, 1.0f); break;
                case 2: PlaySFX(sfxEstrella2, 1.0f); break;
                case 3: PlaySFX(sfxEstrella3, 1.0f); break;
                default: PlaySFX(sfxEstrella1, 1.0f); break;
            }
        }
        public void PlayDesbloqueoNivel() => PlaySFX(sfxDesbloqueoNivel, 1.0f);

        // ====================================================================
        // SOPORTE PARA AUDIOSOURCES 3D AMBIENTALES
        // ====================================================================

        public AudioSource ConfigurarAudio3D(GameObject target, AudioClip clip, float minDistance = 3f, float maxDistance = 12f)
        {
            if (target == null || clip == null) return null;

            AudioSource src = target.AddComponent<AudioSource>();
            src.clip = clip;
            src.loop = true;
            src.playOnAwake = true;
            src.spatialBlend = 1.0f; // 3D espacial
            src.minDistance = minDistance;
            src.maxDistance = maxDistance;
            src.rolloffMode = AudioRolloffMode.Linear;
            src.volume = volumenEfectos;
            RegistrarFuenteEfectos(src);
            src.Play();
            return src;
        }

        public AudioClip GetAudioClipVagonetaLoop() => sfxVagonetaLoop;
        public AudioClip GetAudioClipFuegoLoop() => sfxFuegoLoop;
        public AudioClip GetAudioClipMalacateLoop() => sfxMalacateLoop;
        public AudioMixerGroup GetSFXGroup() => null;
    }
}
