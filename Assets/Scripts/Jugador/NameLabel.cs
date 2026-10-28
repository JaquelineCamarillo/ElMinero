using UnityEngine;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.Jugador
{
    /// <summary>
    /// Muestra el nombre personalizado del minero en una etiqueta sobre su cabeza (World Space).
    /// Mantiene su orientación fija sin voltearse cuando el minero cambia de dirección.
    /// </summary>
    public class NameLabel : MonoBehaviour
    {
        [Header("Referencias")]
        [Tooltip("Componente TextMeshPro donde se muestra el nombre.")]
        [SerializeField] private TextMeshPro textMeshPro;

        [Tooltip("Offset vertical respecto a la posición del jugador.")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2.2f, 0f);

        private Transform playerTransform;

        private void Awake()
        {
            if (textMeshPro == null)
            {
                textMeshPro = GetComponentInChildren<TextMeshPro>();
            }

            if (transform.parent != null)
            {
                playerTransform = transform.parent;
            }
        }

        private void Start()
        {
            ActualizarNombre();
        }

        public void ActualizarNombre()
        {
            if (textMeshPro != null)
            {
                string nombre = GameManager.Instance != null 
                    ? GameManager.Instance.PlayerName 
                    : Utilidades.Constantes.Prefs.NombrePorDefecto;

                textMeshPro.text = nombre;
            }
        }

        private void LateUpdate()
        {
            // Mantener la escala positiva en X sin importar el flip del personaje
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;

            // Mantener posición encima del personaje
            if (playerTransform != null)
            {
                transform.position = playerTransform.position + offset;
            }
        }
    }
}
