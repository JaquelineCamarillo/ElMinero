using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Efecto de los botones del Menú Principal: al pasar el ratón o
    /// seleccionarlos con teclado/control crecen un poco, el texto se vuelve
    /// dorado y el icono se desplaza. La selección sigue al ratón para que
    /// nunca haya dos botones resaltados a la vez.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class BotonMenuAnimado : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private TextMeshProUGUI texto;
        [SerializeField] private RectTransform icono;
        [SerializeField] private float escalaResaltado = 1.06f;
        [SerializeField] private Color colorNormal = new Color(0.96f, 0.91f, 0.8f);
        [SerializeField] private Color colorResaltado = new Color(1f, 0.84f, 0.32f);
        [SerializeField] private float desplazamientoIcono = 8f;

        private Button boton;
        private bool resaltado;
        private float k;
        private Vector2 posIconoBase;
        private Image imgIcono;

        private void Awake()
        {
            boton = GetComponent<Button>();
            if (icono != null) posIconoBase = icono.anchoredPosition;
        }

        private void OnDisable()
        {
            resaltado = false;
            k = 0f;
            Aplicar();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (boton == null || !boton.interactable) return;

            // No robar el foco si el jugador está escribiendo su nombre
            var es = EventSystem.current;
            if (es != null)
            {
                GameObject actual = es.currentSelectedGameObject;
                var campo = actual != null ? actual.GetComponent<TMP_InputField>() : null;
                if (campo == null || !campo.isFocused) es.SetSelectedGameObject(gameObject);
            }
            if (!resaltado) Sonar();
            resaltado = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            var es = EventSystem.current;
            if (es == null || es.currentSelectedGameObject != gameObject) resaltado = false;
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (boton != null && !boton.interactable) return;
            if (!resaltado) Sonar();
            resaltado = true;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            resaltado = false;
        }

        private void Sonar()
        {
            if (AudioManager.Instance != null && Time.timeSinceLevelLoad > 1.5f) AudioManager.Instance.PlayHoverUI();
        }

        private void Update()
        {
            bool activo = resaltado && boton != null && boton.interactable;
            k = Mathf.MoveTowards(k, activo ? 1f : 0f, Time.unscaledDeltaTime * 8f);
            Aplicar();
        }

        private void Aplicar()
        {
            float e = Mathf.Lerp(1f, escalaResaltado, k);
            transform.localScale = new Vector3(e, e, 1f);
            if (texto != null)
            {
                Color baseColor = (boton != null && !boton.interactable) ? new Color(0.62f, 0.58f, 0.52f) : colorNormal;
                texto.color = Color.Lerp(baseColor, colorResaltado, k);
            }
            if (icono != null)
            {
                float vaiven = k > 0.99f ? Mathf.Sin(Time.unscaledTime * 7f) * 2f : 0f;
                icono.anchoredPosition = posIconoBase + new Vector2(desplazamientoIcono * k + vaiven, 0f);
                if (imgIcono == null) imgIcono = icono.GetComponent<Image>();
                if (imgIcono != null) imgIcono.color = (boton != null && !boton.interactable) ? new Color(1f, 1f, 1f, 0.4f) : Color.white;
            }
        }
    }
}
