using UnityEngine;

namespace ElMinero.Managers
{
    /// <summary>
    /// Gestiona las preferencias de configuración del juego (volumen, calidad gráfica, opciones).
    /// Centraliza la persistencia en PlayerPrefs y la sincronización con AudioManager y QualitySettings.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsManager instance;
        public static SettingsManager Instance => instance;

        public const string PREF_VOL_MUSICA = "VolMusica";
        public const string PREF_VOL_EFECTOS = "VolEfectos";
        public const string PREF_MUSICA_ACTIVA = "MusicaActiva";
        public const string PREF_CALIDAD = "CalidadGrafica";
        public const string PREF_NIVEL_DESBLOQUEADO = "NivelDesbloqueado";
        public const string PREF_NOMBRE_JUGADOR = "NombreJugador";

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            AplicarConfiguracionesGuardadas();
        }

        private void Start()
        {
            AplicarConfiguracionesGuardadas();
        }

        /// <summary>
        /// Aplica los valores guardados a los sistemas correspondientes.
        /// </summary>
        public void AplicarConfiguracionesGuardadas()
        {
            float volMusica = PlayerPrefs.GetFloat(PREF_VOL_MUSICA, 0.8f);
            float volEfectos = PlayerPrefs.GetFloat(PREF_VOL_EFECTOS, 0.8f);
            bool musicaActiva = PlayerPrefs.GetInt(PREF_MUSICA_ACTIVA, 1) == 1;
            int calidad = PlayerPrefs.GetInt(PREF_CALIDAD, 2);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicActive(musicaActiva);
                AudioManager.Instance.SetMusicVolume(musicaActiva ? volMusica : 0f);
                AudioManager.Instance.SetSFXVolume(volEfectos);
            }

            QualitySettings.SetQualityLevel(Mathf.Clamp(calidad, 0, QualitySettings.names.Length - 1), true);
        }

        public float GetVolumenMusica() => PlayerPrefs.GetFloat(PREF_VOL_MUSICA, 0.8f);
        public float GetVolumenEfectos() => PlayerPrefs.GetFloat(PREF_VOL_EFECTOS, 0.8f);
        public bool GetMusicaActiva() => PlayerPrefs.GetInt(PREF_MUSICA_ACTIVA, 1) == 1;
        public int GetCalidad() => PlayerPrefs.GetInt(PREF_CALIDAD, 2);

        public void SetVolumenMusica(float valor)
        {
            valor = Mathf.Clamp01(valor);
            PlayerPrefs.SetFloat(PREF_VOL_MUSICA, valor);
            PlayerPrefs.Save();

            if (GetMusicaActiva() && AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicVolume(valor);
            }
        }

        public void SetVolumenEfectos(float valor)
        {
            valor = Mathf.Clamp01(valor);
            PlayerPrefs.SetFloat(PREF_VOL_EFECTOS, valor);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetSFXVolume(valor);
            }
        }

        public void SetMusicaActiva(bool activa)
        {
            PlayerPrefs.SetInt(PREF_MUSICA_ACTIVA, activa ? 1 : 0);
            PlayerPrefs.Save();

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.SetMusicActive(activa);
            }
        }

        public void SetCalidad(int indice)
        {
            indice = Mathf.Clamp(indice, 0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt(PREF_CALIDAD, indice);
            PlayerPrefs.Save();
            QualitySettings.SetQualityLevel(indice, true);
        }

        /// <summary>
        /// Borra todo el progreso del jugador (estrellas, récords, niveles y nombre),
        /// preservando intactos los ajustes de audio y calidad gráfica.
        /// </summary>
        public void BorrarProgreso()
        {
            float volMusica = GetVolumenMusica();
            float volEfectos = GetVolumenEfectos();
            bool musicaActiva = GetMusicaActiva();
            int calidad = GetCalidad();

            // Borrar todo PlayerPrefs
            PlayerPrefs.DeleteAll();

            // Restaurar configuración de usuario
            PlayerPrefs.SetFloat(PREF_VOL_MUSICA, volMusica);
            PlayerPrefs.SetFloat(PREF_VOL_EFECTOS, volEfectos);
            PlayerPrefs.SetInt(PREF_MUSICA_ACTIVA, musicaActiva ? 1 : 0);
            PlayerPrefs.SetInt(PREF_CALIDAD, calidad);
            PlayerPrefs.SetInt(PREF_NIVEL_DESBLOQUEADO, 1);
            PlayerPrefs.SetString(PREF_NOMBRE_JUGADOR, "Refugio");
            PlayerPrefs.Save();

            Debug.Log("[SettingsManager] Progreso del jugador restablecido exitosamente. Configuraciones conservadas.");
        }
    }
}
