using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del panel de opciones según la especificación exacta de la sección 3.4.
    /// Gestiona sliders de volumen, toggle de música, selector de calidad gráfica,
    /// subpanel de controles, y modal de confirmación para borrar progreso.
    /// </summary>
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Elementos de Audio")]
        [SerializeField] private Slider sliderMusica;
        [SerializeField] private Slider sliderEfectos;
        [SerializeField] private Toggle toggleMusica;

        [Header("Calidad")]
        [SerializeField] private TMP_Dropdown dropdownCalidad;

        [Header("Subpanel de Controles")]
        [SerializeField] private Button botonControles;
        [SerializeField] private GameObject panelControles;
        [SerializeField] private Button botonCerrarControles;

        [Header("Modal Borrar Progreso")]
        [SerializeField] private Button botonBorrarProgreso;
        [SerializeField] private GameObject modalConfirmarBorrar;
        [SerializeField] private Button botonConfirmarBorrarSi;
        [SerializeField] private Button botonConfirmarBorrarNo;
        [SerializeField] private GameObject avisoProgresoBorrado;

        [Header("Navegación")]
        [SerializeField] private Button botonVolver;

        private void Awake()
        {
            ConfigurarListeners();
        }

        private void OnEnable()
        {
            CargarValoresActuales();
            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(false);
            if (panelControles != null) panelControles.SetActive(false);
            if (avisoProgresoBorrado != null) avisoProgresoBorrado.SetActive(false);
        }

        private void ConfigurarListeners()
        {
            if (sliderMusica != null)
            {
                sliderMusica.onValueChanged.AddListener(OnMusicaChanged);
            }

            if (sliderEfectos != null)
            {
                sliderEfectos.onValueChanged.AddListener(OnEfectosChanged);
            }

            if (toggleMusica != null)
            {
                toggleMusica.onValueChanged.AddListener(OnToggleMusicaChanged);
            }

            if (dropdownCalidad != null)
            {
                dropdownCalidad.onValueChanged.AddListener(OnCalidadChanged);
            }

            if (botonControles != null)
            {
                botonControles.onClick.AddListener(AbrirControles);
            }

            if (botonCerrarControles != null)
            {
                botonCerrarControles.onClick.AddListener(CerrarControles);
            }

            if (botonBorrarProgreso != null)
            {
                botonBorrarProgreso.onClick.AddListener(MostrarModalBorrar);
            }

            if (botonConfirmarBorrarSi != null)
            {
                botonConfirmarBorrarSi.onClick.AddListener(ConfirmarBorrado);
            }

            if (botonConfirmarBorrarNo != null)
            {
                botonConfirmarBorrarNo.onClick.AddListener(CancelarBorrado);
            }

            if (botonVolver != null)
            {
                botonVolver.onClick.AddListener(CerrarPanel);
            }
        }

        private void CargarValoresActuales()
        {
            float volMusica = PlayerPrefs.GetFloat(SettingsManager.PREF_VOL_MUSICA, 0.8f);
            float volEfectos = PlayerPrefs.GetFloat(SettingsManager.PREF_VOL_EFECTOS, 0.8f);
            bool musicaActiva = PlayerPrefs.GetInt(SettingsManager.PREF_MUSICA_ACTIVA, 1) == 1;
            int calidad = PlayerPrefs.GetInt(SettingsManager.PREF_CALIDAD, 2);

            if (sliderMusica != null) sliderMusica.value = volMusica;
            if (sliderEfectos != null) sliderEfectos.value = volEfectos;
            if (toggleMusica != null) toggleMusica.isOn = musicaActiva;

            if (dropdownCalidad != null)
            {
                dropdownCalidad.ClearOptions();
                List<string> opciones = new List<string> { "Baja", "Media", "Alta" };
                dropdownCalidad.AddOptions(opciones);
                dropdownCalidad.value = Mathf.Clamp(calidad, 0, opciones.Count - 1);
                dropdownCalidad.RefreshShownValue();
            }
        }

        private void OnMusicaChanged(float val)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetVolumenMusica(val);
            }
            else
            {
                PlayerPrefs.SetFloat(SettingsManager.PREF_VOL_MUSICA, val);
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
            }
        }

        private void OnEfectosChanged(float val)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetVolumenEfectos(val);
            }
            else
            {
                PlayerPrefs.SetFloat(SettingsManager.PREF_VOL_EFECTOS, val);
                if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);
            }
        }

        private void OnToggleMusicaChanged(bool activa)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetMusicaActiva(activa);
            }
            else
            {
                PlayerPrefs.SetInt(SettingsManager.PREF_MUSICA_ACTIVA, activa ? 1 : 0);
                if (AudioManager.Instance != null) AudioManager.Instance.SetMusicActive(activa);
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
        }

        private void OnCalidadChanged(int indice)
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.SetCalidad(indice);
            }
            else
            {
                QualitySettings.SetQualityLevel(indice, true);
                PlayerPrefs.SetInt(SettingsManager.PREF_CALIDAD, indice);
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
        }

        public void AbrirControles()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (panelControles != null) panelControles.SetActive(true);
        }

        public void CerrarControles()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (panelControles != null) panelControles.SetActive(false);
        }

        public void MostrarModalBorrar()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(true);
        }

        public void CancelarBorrado()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(false);
        }

        public void ConfirmarBorrado()
        {
            if (SettingsManager.Instance != null)
            {
                SettingsManager.Instance.BorrarProgreso();
            }
            else
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
            }

            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(false);
            if (avisoProgresoBorrado != null)
            {
                avisoProgresoBorrado.SetActive(true);
                StartCoroutine(OcultarAvisoDespues(2.5f));
            }

            if (AudioManager.Instance != null) AudioManager.Instance.PlayDanioSuave();
        }

        private IEnumerator OcultarAvisoDespues(float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (avisoProgresoBorrado != null) avisoProgresoBorrado.SetActive(false);
        }

        public void CerrarPanel()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            gameObject.SetActive(false);
        }
    }
}
