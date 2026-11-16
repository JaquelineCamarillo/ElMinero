using UnityEngine;

namespace ElMinero.Utilidades
{
    /// <summary>
    /// Efecto visual de un solo uso (polvo, chispas, polvo grande del jefe).
    /// Reproduce sus cuadros una vez y se destruye solo.
    /// </summary>
    public class EfectoTemporal : MonoBehaviour
    {
        private SpriteFrameAnimator anim;
        private float vidaMaxima = 2f;
        private float t;

        /// <summary>
        /// Crea el efecto en la posición indicada. Si no hay cuadros, no crea nada.
        /// </summary>
        public static GameObject Crear(Vector3 posicion, Sprite[] cuadros, float fps, Color color, float escala = 1f, string sortingLayer = null, int orden = 20)
        {
            if (cuadros == null || cuadros.Length == 0) return null;

            GameObject go = new GameObject("Efecto_" + cuadros[0].name);
            go.transform.position = posicion;
            go.transform.localScale = new Vector3(escala, escala, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = cuadros[0];
            sr.color = color;
            sr.sortingLayerName = string.IsNullOrEmpty(sortingLayer) ? Constantes.SortingLayers.Objetos : sortingLayer;
            sr.sortingOrder = orden;

            var anim = go.AddComponent<SpriteFrameAnimator>();
            anim.Configurar(cuadros, fps, false, false);

            var efecto = go.AddComponent<EfectoTemporal>();
            efecto.anim = anim;
            efecto.vidaMaxima = cuadros.Length / Mathf.Max(1f, fps) + 0.1f;
            anim.Play();
            return go;
        }

        private void Update()
        {
            t += Time.deltaTime;
            if (t >= vidaMaxima || (anim != null && anim.Terminado))
            {
                Destroy(gameObject);
            }
        }
    }
}
