using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ElMinero.Managers
{
    /// <summary>
    /// Administrador de audio global (Singleton, DontDestroyOnLoad).
    /// Controla la música de fondo por nivel y provee un pool de AudioSources para SFX.
    /// Es completamente seguro y tolerante a fallos si los AudioClips aún no han sido asignados (null-safe).
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
                    instance = UnityEngine.Object.FindAnyObjectByType<AudioManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("[AudioManager]");
                        instance = go.AddComponent<AudioManager>();
                    }
                }
                return instance;
            }
        }

        [Header("Fuentes de Audio")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private int sfxPoolSize = 8;

        [Header("Música")]
        [SerializeField] private AudioClip musicMenu;
        [SerializeField] private AudioClip musicNivel1;
        [SerializeField] private AudioClip musicNivel2;
        [SerializeField] private AudioClip musicNivel3;
        [SerializeField] private AudioClip musicVictoria;
        [SerializeField] private AudioClip musicDerrota;

        [Header("Efectos de Sonido (SFX)")]
        [SerializeField] private AudioClip sfxPasos;
        [SerializeField] private AudioClip sfxSalto;
        [SerializeField] private AudioClip sfxCaidaRoca;
        [SerializeField] private AudioClip sfxImpactoRoca;
        [SerializeField] private AudioClip sfxRecolectarPlata;
        [SerializeField] private AudioClip sfxRecolectarGema;
        [SerializeField] private AudioClip sfxColapsoPlataforma;
        [SerializeField] private AudioClip sfxTemblor;
        [SerializeField] private AudioClip sfxDerrumbeSorpresa;
        [SerializeField] private AudioClip sfxPerderVida;
        [SerializeField] private AudioClip sfxCheckpoint;
        [SerializeField] private AudioClip sfxClick;

        private List<AudioSource> sfxPool = new List<AudioSource>();
        private int currentSfxIndex = 0;
        private float baseMusicPitch = 1.0f;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InicializarAudioSources();
        }

        private void InicializarAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = 0.7f;
            }

            for (int i = 0; i < sfxPoolSize; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                src.volume = 0.9f;
                sfxPool.Add(src);
            }
        }

        // ====================================================================
        // MÚSICA
        // ====================================================================
        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (clip == null || musicSource == null) return;

            if (musicSource.clip == clip && musicSource.isPlaying) return;

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.pitch = baseMusicPitch;
            musicSource.Play();
        }

        public void PlayMusicNivel(int nivel)
        {
            switch (nivel)
            {
                case 1: PlayMusic(musicNivel1); break;
                case 2: PlayMusic(musicNivel2); break;
                case 3: PlayMusic(musicNivel3); break;
                default: PlayMusic(musicMenu); break;
            }
        }

        public void PlayMusicMenu() => PlayMusic(musicMenu);
        public void PlayMusicVictoria() => PlayMusic(musicVictoria, false);
        public void PlayMusicDerrota() => PlayMusic(musicDerrota, false);

        public void StopMusic()
        {
            if (musicSource != null) musicSource.Stop();
        }

        /// <summary>
        /// Aumenta la intensidad (pitch/tempo sutil) de la música en el último tramo de tiempo.
        /// </summary>
        public void SetMusicUrgency(bool urgent)
        {
            if (musicSource != null)
            {
                musicSource.pitch = urgent ? baseMusicPitch * 1.15f : baseMusicPitch;
            }
        }

        // ====================================================================
        // SFX POOL
        // ====================================================================
        public void PlaySFX(AudioClip clip, float volume = 1.0f, float pitchVariation = 0f)
        {
            if (clip == null || sfxPool.Count == 0) return;

            AudioSource src = sfxPool[currentSfxIndex];
            currentSfxIndex = (currentSfxIndex + 1) % sfxPool.Count;

            src.pitch = 1.0f + (pitchVariation > 0f ? Random.Range(-pitchVariation, pitchVariation) : 0f);
            src.PlayOneShot(clip, volume);
        }

        // Métodos de acceso rápido para SFX
        public void PlayPasos() => PlaySFX(sfxPasos, 0.4f, 0.1f);
        public void PlaySalto() => PlaySFX(sfxSalto, 0.8f, 0.05f);
        public void PlayCaidaRoca() => PlaySFX(sfxCaidaRoca, 0.7f);
        public void PlayImpactoRoca() => PlaySFX(sfxImpactoRoca, 0.8f, 0.1f);
        public void PlayRecolectarPlata() => PlaySFX(sfxRecolectarPlata, 0.85f, 0.05f);
        public void PlayRecolectarGema() => PlaySFX(sfxRecolectarGema, 1.0f, 0.05f);
        public void PlayColapsoPlataforma() => PlaySFX(sfxColapsoPlataforma, 0.9f);
        public void PlayTemblor() => PlaySFX(sfxTemblor, 1.0f);
        public void PlayDerrumbeSorpresa() => PlaySFX(sfxDerrumbeSorpresa, 1.0f);
        public void PlayPerderVida() => PlaySFX(sfxPerderVida, 0.9f);
        public void PlayCheckpoint() => PlaySFX(sfxCheckpoint, 0.85f);
        public void PlayClick() => PlaySFX(sfxClick, 0.7f);
    }
}
