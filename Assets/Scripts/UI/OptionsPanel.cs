using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del panel de opciones según la especificación exacta de la sección 3.4 y correcciones D/E.
    /// Gestiona sliders de volumen directo, toggle de música, selector de calidad con flechas (◀  Alta  ▶),
    /// subpanel de controles, modal de borrado de progreso y evento AlCerrar.
    /// </summary>
    public class OptionsPanel : MonoBehaviour
    {
        [Header("Elementos de Audio")]
        [SerializeField] private Slider sliderMusica;
        [SerializeField] private Slider sliderEfectos;
        [SerializeField] private Toggle toggleMusica;

        [Header("Selector de Calidad")]
        [SerializeField] private Button botonCalidadAnterior;
        [SerializeField] private Button botonCalidadSiguiente;
        [SerializeField] private TextMeshProUGUI textoCalidad;

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

        private int indiceCalidadActual = 2;

        public event Action AlCerrar;

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

        private void Update()
        {
            bool volver = false;
#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) volver = true;
            if (Gamepad.current != null && (Gamepad.current.buttonEast.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) volver = true;
#endif
            if (volver)
            {
                if (modalConfirmarBorrar != null && modalConfirmarBorrar.activeSelf)
                {
                    CancelarBorrado();
                }
                else if (panelControles != null && panelControles.activeSelf)
                {
                    CerrarControles();
                }
                else
                {
                    CerrarPanel();
                }
            }
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

            if (botonCalidadAnterior != null)
            {
                botonCalidadAnterior.onClick.AddListener(() => CambiarCalidad(-1));
            }

            if (botonCalidadSiguiente != null)
            {
                botonCalidadSiguiente.onClick.AddListener(() => CambiarCalidad(1));
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

            AplicarCalidad(calidad);
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

        private void CambiarCalidad(int delta)
        {
            string[] nombres = QualitySettings.names;
            int total = nombres.Length;
            if (total == 0) return;

            indiceCalidadActual = (indiceCalidadActual + delta + total) % total;
            AplicarCalidad(indiceCalidadActual);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
        }

        private void AplicarCalidad(int indice)
        {
            string[] nombres = QualitySettings.names;
            if (nombres == null || nombres.Length == 0) return;

            indiceCalidadActual = Mathf.Clamp(indice, 0, nombres.Length - 1);
            QualitySettings.SetQualityLevel(indiceCalidadActual, true);
            PlayerPrefs.SetInt(SettingsManager.PREF_CALIDAD, indiceCalidadActual);
            PlayerPrefs.Save();

            if (textoCalidad != null)
            {
                textoCalidad.text = nombres[indiceCalidadActual];
            }
        }

        private void AbrirControles()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (panelControles != null) panelControles.SetActive(true);
        }

        private void CerrarControles()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (panelControles != null) panelControles.SetActive(false);
        }

        private void MostrarModalBorrar()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(true);
        }

        private void CancelarBorrado()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(false);
        }

        private void ConfirmarBorrado()
        {
            if (modalConfirmarBorrar != null) modalConfirmarBorrar.SetActive(false);

            PlayerPrefs.DeleteKey(SettingsManager.PREF_NIVEL_DESBLOQUEADO);
            for (int i = 1; i <= 3; i++)
            {
                PlayerPrefs.DeleteKey($"Nivel_{i}_Estrellas");
                PlayerPrefs.DeleteKey($"Nivel_{i}_Score");
                PlayerPrefs.DeleteKey($"Nivel_{i}_Tiempo");
                PlayerPrefs.DeleteKey($"Nivel_{i}_Completado");
            }

            PlayerPrefs.DeleteKey("Mineral_Nivel1_MinaRayas");
            PlayerPrefs.DeleteKey("Mineral_Nivel2_ElNopal");
            PlayerPrefs.DeleteKey("Mineral_Nivel3_LaValenciana");

            PlayerPrefs.Save();

            if (AudioManager.Instance != null) AudioManager.Instance.PlayDanioSuave();
            if (avisoProgresoBorrado != null)
            {
                avisoProgresoBorrado.SetActive(true);
                StopAllCoroutines();
                StartCoroutine(OcultarAviso());
            }
        }

        private System.Collections.IEnumerator OcultarAviso()
        {
            yield return new WaitForSecondsRealtime(2f);
            if (avisoProgresoBorrado != null) avisoProgresoBorrado.SetActive(false);
        }

        public void CerrarPanel()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            gameObject.SetActive(false);
            AlCerrar?.Invoke();
        }
    }
}
