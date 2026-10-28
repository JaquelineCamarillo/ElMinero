using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ElMinero.Managers
{
    /// <summary>
    /// Gestiona las transiciones con fundido a negro (fade) entre escenas.
    /// Es un Singleton persistente que se autocrea si no existe en la escena.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        private static SceneLoader instance;
        public static SceneLoader Instance => instance;

        [Header("Configuración de Transición")]
        [SerializeField] private float fadeDuration = 0.5f;

        private Canvas fadeCanvas;
        private Image fadeImage;
        private bool isTransitioning = false;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            CrearCanvasFade();
        }

        private void CrearCanvasFade()
        {
            GameObject canvasObj = new GameObject("[SceneLoader_FadeCanvas]");
            canvasObj.transform.SetParent(transform);

            fadeCanvas = canvasObj.AddComponent<Canvas>();
            fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeCanvas.sortingOrder = 9999; // Siempre por encima de todo

            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();

            GameObject imgObj = new GameObject("FadeImage");
            imgObj.transform.SetParent(canvasObj.transform, false);

            var rect = imgObj.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            fadeImage = imgObj.AddComponent<Image>();
            fadeImage.color = new Color(0f, 0f, 0f, 0f); // Transparente inicialmente
            fadeImage.raycastTarget = false;
        }

        /// <summary>
        /// Carga una escena por nombre con transición de fundido.
        /// </summary>
        public static void LoadScene(string sceneName)
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("[SceneLoader]");
                instance = obj.AddComponent<SceneLoader>();
            }

            instance.StartCoroutine(instance.DoFadeAndLoad(sceneName));
        }

        /// <summary>
        /// Recarga la escena activa actual.
        /// </summary>
        public static void ReloadCurrentScene()
        {
            string current = SceneManager.GetActiveScene().name;
            LoadScene(current);
        }

        private IEnumerator DoFadeAndLoad(string sceneName)
        {
            if (isTransitioning) yield break;
            isTransitioning = true;

            Time.timeScale = 1f; // Restaurar timescale si venía de pausa

            // 1. Fade a negro
            fadeImage.raycastTarget = true;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }
            fadeImage.color = Color.black;

            // 2. Cargar escena
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
            while (op != null && !op.isDone)
            {
                yield return null;
            }

            // Pequeña pausa opcional para estabilidad
            yield return new WaitForSecondsRealtime(0.1f);

            // 3. Fade de negro a transparente
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(1f - (elapsed / fadeDuration));
                fadeImage.color = new Color(0f, 0f, 0f, alpha);
                yield return null;
            }

            fadeImage.color = new Color(0f, 0f, 0f, 0f);
            fadeImage.raycastTarget = false;
            isTransitioning = false;
        }
    }
}
