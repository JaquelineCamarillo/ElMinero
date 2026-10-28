using System.Collections;
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
    /// Controla la pantalla Intro narrativa (Sección 3.6).
    /// Muestra la historia contextual de Guanajuato en el siglo XIX con efecto máquina de escribir,
    /// personalizando el nombre del minero elegido por el jugador.
    /// </summary>
    public class IntroController : MonoBehaviour
    {
        [Header("Elementos de UI")]
        [SerializeField] private TextMeshProUGUI textoNarrativo;
        [SerializeField] private TextMeshProUGUI textoIndicador;
        [SerializeField] private Button botonSaltar;

        [Header("Configuración")]
        [SerializeField] private float velocidadEscritura = 0.035f;

        private List<string> paginas = new List<string>();
        private int paginaActual = 0;
        private bool escribiendo = false;
        private Coroutine corrutinaEscritura;
        private string textoCompletoActual = "";

        private void Start()
        {
            string nombre = PlayerPrefs.GetString("NombreJugador", "Refugio");
            if (string.IsNullOrWhiteSpace(nombre)) nombre = "Refugio";

            paginas.Add("Guanajuato, siglo XIX. La plata de sus minas es famosa en todo el mundo.");
            paginas.Add($"{nombre} trabaja en lo más profundo de la Mina de Rayas cuando la tierra empieza a temblar…");
            paginas.Add("¡Derrumbe! La única salida está arriba: subir por El Nopal hasta la bocamina de La Valenciana.");
            paginas.Add("Recoge toda la plata que puedas… y no mires atrás.");

            if (botonSaltar != null)
            {
                botonSaltar.onClick.AddListener(IniciarJuego);
            }

            MostrarPagina(0);
        }

        private void Update()
        {
            bool accionDetectada = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && (Keyboard.current.spaceKey.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame)) accionDetectada = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) accionDetectada = true;
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) accionDetectada = true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) accionDetectada = true;
#else
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetMouseButtonDown(0)) accionDetectada = true;
#endif

            if (accionDetectada)
            {
                AvanzarOCompletar();
            }
        }

        private void MostrarPagina(int index)
        {
            if (index >= paginas.Count)
            {
                IniciarJuego();
                return;
            }

            paginaActual = index;
            textoCompletoActual = paginas[index];
            if (textoIndicador != null) textoIndicador.gameObject.SetActive(false);

            if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
            corrutinaEscritura = StartCoroutine(EscribirTexto(textoCompletoActual));
        }

        private IEnumerator EscribirTexto(string texto)
        {
            escribiendo = true;
            textoNarrativo.text = "";

            for (int i = 0; i < texto.Length; i++)
            {
                textoNarrativo.text += texto[i];
                yield return new WaitForSeconds(velocidadEscritura);
            }

            escribiendo = false;
            if (textoIndicador != null)
            {
                textoIndicador.text = (paginaActual < paginas.Count - 1) ? "Presiona [Espacio] o Toca para continuar" : "Presiona [Espacio] o Toca para empezar";
                textoIndicador.gameObject.SetActive(true);
            }
        }

        public void AvanzarOCompletar()
        {
            if (escribiendo)
            {
                // Completar texto inmediatamente
                if (corrutinaEscritura != null) StopCoroutine(corrutinaEscritura);
                textoNarrativo.text = textoCompletoActual;
                escribiendo = false;
                if (textoIndicador != null)
                {
                    textoIndicador.text = (paginaActual < paginas.Count - 1) ? "Presiona [Espacio] o Toca para continuar" : "Presiona [Espacio] o Toca para empezar";
                    textoIndicador.gameObject.SetActive(true);
                }
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
                MostrarPagina(paginaActual + 1);
            }
        }

        public void IniciarJuego()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            SceneLoader.LoadScene("Nivel1_MinaRayas");
        }
    }
}
