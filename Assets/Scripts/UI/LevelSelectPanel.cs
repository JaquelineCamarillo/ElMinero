using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ElMinero.Managers;

namespace ElMinero.UI
{
    /// <summary>
    /// Controlador del panel de selección de niveles (Sección 3.3).
    /// Muestra las tarjetas de las 3 minas con sus miniaturas, récords, estrellas obtenidas
    /// y estado de bloqueo según el progreso del jugador en PlayerPrefs.
    /// </summary>
    public class LevelSelectPanel : MonoBehaviour
    {
        [System.Serializable]
        public class TarjetaNivelUI
        {
            public int numeroNivel;
            public string nombreEscena;
            public Button botonSeleccion;
            public Image miniatura;
            public GameObject contenedorCandado;
            public TextMeshProUGUI textoEstado;
            public Image[] iconosEstrellas; // 3 estrellas
            public TextMeshProUGUI textoRecord;
        }

        [Header("Tarjetas de Niveles")]
        [SerializeField] private TarjetaNivelUI[] tarjetas;

        [Header("Sprites de Estrellas")]
        [SerializeField] private Sprite spriteEstrellaLlena;
        [SerializeField] private Sprite spriteEstrellaVacia;

        [Header("Navegación")]
        [SerializeField] private Button botonVolver;

        private void Awake()
        {
            if (botonVolver != null)
            {
                botonVolver.onClick.AddListener(CerrarPanel);
            }
        }

        private void OnEnable()
        {
            ActualizarEstadoNiveles();
        }

        public void ActualizarEstadoNiveles()
        {
            int nivelDesbloqueado = PlayerPrefs.GetInt(SettingsManager.PREF_NIVEL_DESBLOQUEADO, 1);

            if (tarjetas == null) return;

            foreach (var tarjeta in tarjetas)
            {
                if (tarjeta == null) continue;

                bool estaDesbloqueado = tarjeta.numeroNivel <= nivelDesbloqueado;

                // Desbloqueado vs Bloqueado
                if (tarjeta.contenedorCandado != null)
                {
                    tarjeta.contenedorCandado.SetActive(!estaDesbloqueado);
                }

                if (tarjeta.miniatura != null)
                {
                    tarjeta.miniatura.color = estaDesbloqueado ? Color.white : new Color(0.35f, 0.35f, 0.35f, 0.9f);
                }

                if (tarjeta.textoEstado != null)
                {
                    tarjeta.textoEstado.text = estaDesbloqueado ? "Disponible" : "Completa el nivel anterior";
                    tarjeta.textoEstado.color = estaDesbloqueado ? new Color(0.4f, 1f, 0.5f) : new Color(0.8f, 0.4f, 0.4f);
                }

                // Cargar récords y estrellas
                int estrellas = PlayerPrefs.GetInt($"Estrellas_{tarjeta.nombreEscena}", 0);
                int mejorPuntaje = PlayerPrefs.GetInt($"MejorPuntaje_{tarjeta.nombreEscena}", 0);
                float mejorTiempo = PlayerPrefs.GetFloat($"MejorTiempo_{tarjeta.nombreEscena}", 0f);

                if (tarjeta.iconosEstrellas != null)
                {
                    for (int i = 0; i < tarjeta.iconosEstrellas.Length; i++)
                    {
                        if (tarjeta.iconosEstrellas[i] != null)
                        {
                            bool llena = (i < estrellas) && estaDesbloqueado;
                            tarjeta.iconosEstrellas[i].sprite = llena ? spriteEstrellaLlena : spriteEstrellaVacia;
                            tarjeta.iconosEstrellas[i].color = estaDesbloqueado ? Color.white : new Color(0.4f, 0.4f, 0.4f, 0.5f);
                        }
                    }
                }

                if (tarjeta.textoRecord != null)
                {
                    if (estaDesbloqueado && (mejorPuntaje > 0 || mejorTiempo > 0f))
                    {
                        tarjeta.textoRecord.text = $"Récord: {mejorPuntaje:N0} pts | {mejorTiempo:F1}s";
                    }
                    else if (estaDesbloqueado)
                    {
                        tarjeta.textoRecord.text = "Sin récord aún";
                    }
                    else
                    {
                        tarjeta.textoRecord.text = "Bloqueado";
                    }
                }

                // Asignar listeners al botón
                if (tarjeta.botonSeleccion != null)
                {
                    tarjeta.botonSeleccion.onClick.RemoveAllListeners();
                    string escenaDestino = tarjeta.nombreEscena;
                    bool desbloqueadoLocal = estaDesbloqueado;

                    tarjeta.botonSeleccion.onClick.AddListener(() =>
                    {
                        if (desbloqueadoLocal)
                        {
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
                            SceneLoader.LoadScene(escenaDestino);
                        }
                        else
                        {
                            if (AudioManager.Instance != null) AudioManager.Instance.PlayDanioSuave();
                        }
                    });
                }
            }
        }

        public void CerrarPanel()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayClickUI();
            gameObject.SetActive(false);
        }
    }
}
