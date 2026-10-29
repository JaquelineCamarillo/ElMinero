using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace ElMinero.UI
{
    /// <summary>
    /// Efecto sutil de Parallax para el fondo de 3 capas en el Menú Principal.
    /// Desplaza Capa0_Cielo (0.2), Capa1_Ciudad (0.5) y Capa2_Frente (1.0)
    /// mediante el movimiento del cursor y una suave oscilación senoidal.
    /// </summary>
    public class MenuParallax : MonoBehaviour
    {
        [Header("Capas de Fondo")]
        [SerializeField] private Transform capaCielo;
        [SerializeField] private Transform capaCiudad;
        [SerializeField] private Transform capaFrente;

        [Header("Factores de Parallax")]
        [SerializeField] private float factorCielo = 0.2f;
        [SerializeField] private float factorCiudad = 0.5f;
        [SerializeField] private float factorFrente = 1.0f;

        [Header("Ajustes de Movimiento")]
        [SerializeField] private float maxOffset = 0.8f;
        [SerializeField] private float suavizado = 3.0f;

        private Vector3 posInicialCielo;
        private Vector3 posInicialCiudad;
        private Vector3 posInicialFrente;

        private Vector2 offsetActual;

        private void Start()
        {
            if (capaCielo != null) posInicialCielo = capaCielo.position;
            if (capaCiudad != null) posInicialCiudad = capaCiudad.position;
            if (capaFrente != null) posInicialFrente = capaFrente.position;
        }

        private void Update()
        {
            Vector2 targetOffset = Vector2.zero;

#if ENABLE_INPUT_SYSTEM
            if (Mouse.current != null)
            {
                Vector2 mousePos = Mouse.current.position.ReadValue();
                float nx = (mousePos.x / Mathf.Max(1, Screen.width) - 0.5f) * 2f;
                float ny = (mousePos.y / Mathf.Max(1, Screen.height) - 0.5f) * 2f;
                targetOffset = new Vector2(nx, ny) * maxOffset;
            }
#endif
            // Oscilación suave constante por si no se mueve el ratón
            float idleX = Mathf.Sin(Time.time * 0.4f) * (maxOffset * 0.25f);
            float idleY = Mathf.Cos(Time.time * 0.3f) * (maxOffset * 0.15f);
            targetOffset += new Vector2(idleX, idleY);

            offsetActual = Vector2.Lerp(offsetActual, targetOffset, Time.deltaTime * suavizado);

            if (capaCielo != null)
            {
                capaCielo.position = posInicialCielo + new Vector3(offsetActual.x * factorCielo, offsetActual.y * factorCielo * 0.5f, 0f);
            }
            if (capaCiudad != null)
            {
                capaCiudad.position = posInicialCiudad + new Vector3(offsetActual.x * factorCiudad, offsetActual.y * factorCiudad * 0.5f, 0f);
            }
            if (capaFrente != null)
            {
                capaFrente.position = posInicialFrente + new Vector3(offsetActual.x * factorFrente, offsetActual.y * factorFrente * 0.5f, 0f);
            }
        }
    }
}
