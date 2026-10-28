using UnityEngine;
using UnityEngine.UI;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del panel de créditos (Sección 3.5).
    /// Muestra la autoría del proyecto, institución, docente, licencias y menciones de recursos.
    /// Soporta auto-desplazamiento suave y botón volver.
    /// </summary>
    public class CreditsPanel : MonoBehaviour
    {
        [Header("Navegación")]
        [SerializeField] private Button botonVolver;

        [Header("Desplazamiento")]
        [SerializeField] private RectTransform contenidoCreditos;
        [SerializeField] private float velocidadScroll = 20f;
        [SerializeField] private float posicionReinicioY = -250f;
        [SerializeField] private float posicionLimiteY = 450f;

        private void Awake()
        {
            if (botonVolver != null)
            {
                botonVolver.onClick.AddListener(CerrarPanel);
            }
        }

        private void OnEnable()
        {
            if (contenidoCreditos != null)
            {
                contenidoCreditos.anchoredPosition = new Vector2(contenidoCreditos.anchoredPosition.x, posicionReinicioY);
            }
        }

        private void Update()
        {
            if (contenidoCreditos != null)
            {
                Vector2 pos = contenidoCreditos.anchoredPosition;
                pos.y += velocidadScroll * Time.unscaledDeltaTime;
                if (pos.y > posicionLimiteY)
                {
                    pos.y = posicionReinicioY;
                }
                contenidoCreditos.anchoredPosition = pos;
            }
        }

        public void CerrarPanel()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            gameObject.SetActive(false);
        }
    }
}
