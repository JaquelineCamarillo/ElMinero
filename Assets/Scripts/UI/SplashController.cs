using System.Collections;
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
    /// Controla la secuencia de inicio de la pantalla Splash (SecciÃ³n 3.1).
    /// Muestra el logo de la autora y la instituciÃ³n (UTNG), seguido del logo del juego,
    /// con transiciones suaves y posibilidad de omitir en cualquier momento.
    /// </summary>
    public class SplashController : MonoBehaviour
    {
        [Header("Grupos de Pantalla")]
        [SerializeField] private CanvasGroup grupoAutora;
        [SerializeField] private CanvasGroup grupoLogoJuego;

        [Header("ConfiguraciÃ³n de Tiempos")]
        [SerializeField] private float duracionFade = 0.6f;
        [SerializeField] private float duracionEspera = 1.0f;

        private bool saltando = false;

        private void Start()
        {
            if (grupoAutora != null) grupoAutora.alpha = 0f;
            if (grupoLogoJuego != null) grupoLogoJuego.alpha = 0f;

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicMenu();
            }

            StartCoroutine(SecuenciaSplash());
        }

        private void Update()
        {
            if (saltando) return;

            bool inputDetectado = false;

#if ENABLE_INPUT_SYSTEM
            if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame) inputDetectado = true;
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) inputDetectado = true;
            if (Gamepad.current != null && (Gamepad.current.buttonSouth.wasPressedThisFrame || Gamepad.current.startButton.wasPressedThisFrame)) inputDetectado = true;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame) inputDetectado = true;
#else
            if (Input.anyKeyDown) inputDetectado = true;
#endif

            if (inputDetectado)
            {
                SaltarAlMenu();
            }
        }

        private IEnumerator SecuenciaSplash()
        {
            // Fase 1: Logo Autora e InstituciÃ³n
            if (grupoAutora != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(grupoAutora, 0f, 1f, duracionFade));
                yield return new WaitForSeconds(duracionEspera);
                yield return StartCoroutine(FadeCanvasGroup(grupoAutora, 1f, 0f, duracionFade));
            }

            if (saltando) yield break;

            // Fase 2: Logo El Minero
            if (grupoLogoJuego != null)
            {
                yield return StartCoroutine(FadeCanvasGroup(grupoLogoJuego, 0f, 1f, duracionFade));
                yield return new WaitForSeconds(duracionEspera);
                yield return StartCoroutine(FadeCanvasGroup(grupoLogoJuego, 1f, 0f, duracionFade));
            }

            if (!saltando)
            {
                SaltarAlMenu();
            }
        }

        private IEnumerator FadeCanvasGroup(CanvasGroup cg, float inicio, float fin, float duracion)
        {
            float t = 0f;
            while (t < duracion)
            {
                if (saltando) yield break;
                t += Time.deltaTime;
                cg.alpha = Mathf.Lerp(inicio, fin, t / duracion);
                yield return null;
            }
            cg.alpha = fin;
        }

        public void SaltarAlMenu()
        {
            if (saltando) return;
            saltando = true;
            StopAllCoroutines();
            SceneLoader.LoadScene("MenuPrincipal");
        }
    }
}
