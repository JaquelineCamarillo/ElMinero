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
    /// Tarjeta modal informativa que aparece al iniciar un nivel por primera vez (Sección 3.6).
    /// Muestra el nombre de la mina, miniatura, dato histórico y objetivo,
    /// pausando el juego hasta que el jugador presiona "¡A escapar!".
    /// Si la miniatura no está asignada, la oculta de forma transparente (sin cuadro blanco).
    /// No se muestra en reintentos tras morir.
    /// </summary>
    public class LevelStartCard : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [SerializeField] private Image miniaturaMina;
        [SerializeField] private TextMeshProUGUI textoNombreMina;
        [SerializeField] private TextMeshProUGUI textoDatoHistorico;
        [SerializeField] private TextMeshProUGUI textoObjetivo;
        [SerializeField] private Button botonEscapar;

        // Registro estático de niveles en los que ya se mostró la tarjeta durante la sesión
        private static HashSet<string> nivelesYaIniciados = new HashSet<string>();

        private string nombreEscenaActual = "";

        private void Awake()
        {
            if (botonEscapar != null)
            {
                botonEscapar.onClick.AddListener(IniciarNivel);
            }
        }

        private void Update()
        {
            if (!gameObject.activeSelf) return;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) IniciarNivel();
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) IniciarNivel();
#else
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) IniciarNivel();
#endif
        }

        public void ConfigurarYMostrar(string nombreEscena, string nombreMina, Sprite miniatura, string datoHistorico, string objetivo = "Sube a la siguiente mina antes de que el derrumbe te alcance.")
        {
            nombreEscenaActual = nombreEscena;

            // Si ya se vio la tarjeta en este nivel durante la sesión, no frenar el ritmo
            if (nivelesYaIniciados.Contains(nombreEscenaActual))
            {
                gameObject.SetActive(false);
                return;
            }

            if (miniaturaMina != null)
            {
                if (miniatura != null)
                {
                    miniaturaMina.sprite = miniatura;
                    miniaturaMina.color = Color.white;
                }
                else
                {
                    miniaturaMina.sprite = null;
                    miniaturaMina.color = new Color(1f, 1f, 1f, 0f);
                }
            }

            if (textoNombreMina != null) textoNombreMina.text = nombreMina;
            if (textoDatoHistorico != null) textoDatoHistorico.text = datoHistorico;
            if (textoObjetivo != null) textoObjetivo.text = objetivo;

            gameObject.SetActive(true);
            Time.timeScale = 0f; // Pausa completa hasta presionar botón
        }

        public void IniciarNivel()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();

            if (!string.IsNullOrEmpty(nombreEscenaActual))
            {
                nivelesYaIniciados.Add(nombreEscenaActual);
            }

            Time.timeScale = 1f;
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Limpia el registro de niveles vistos (útil al volver al Menú Principal o reiniciar sesión).
        /// </summary>
        public static void ReiniciarRegistroNiveles()
        {
            nivelesYaIniciados.Clear();
        }
    }
}
