using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Jugador;

namespace ElMinero.Jefe
{
    /// <summary>
    /// Trigger sobre la cabeza del Coloso (2.4 × 0.6). Solo está activo mientras el jefe está aturdido:
    /// si el jugador le cae encima, el jefe recibe el golpe y el jugador rebota.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PuntoDebilColoso : MonoBehaviour
    {
        [SerializeField] private BossColoso jefe;

        public void Configurar(BossColoso coloso)
        {
            jefe = coloso;
        }

        private void Awake()
        {
            if (jefe == null) jefe = GetComponentInParent<BossColoso>();
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other) => Revisar(other);
        private void OnTriggerStay2D(Collider2D other) => Revisar(other);

        private void Revisar(Collider2D other)
        {
            if (jefe == null || !other.CompareTag(Constantes.Tags.Player)) return;

            var rb = other.attachedRigidbody;
            bool cayendo = rb == null || rb.linearVelocity.y <= 0.5f;
            bool encima = other.transform.position.y >= transform.position.y - 0.5f;
            if (cayendo && encima)
            {
                jefe.Pisado(other.GetComponent<PlayerController>());
            }
        }
    }
}
