using System;
using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Bola de fuego que cae del techo (nivel 3).
    /// Rigidbody2D Dynamic (gravityScale 2.5) con collider trigger.
    /// Al tocar el Suelo explota (polvo y chispas) y regresa al pool de su generador.
    /// Vive como máximo 4 s para que ninguna se quede en la escena.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
    public class Fireball : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        [SerializeField] private float vidaMaxima = 4f;
        [SerializeField] private Sprite[] cuadrosExplosion;

        private Rigidbody2D rb;
        private CircleCollider2D col;
        private float vida;
        private bool explotada;
        private Action<Fireball> alTerminar;

        public void Configurar(Sprite[] explosion)
        {
            cuadrosExplosion = explosion;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();

            tag = Constantes.Tags.Hazard;
            int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (layerPeligro != -1) gameObject.layer = layerPeligro;

            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            col.isTrigger = true;
            col.radius = 0.4f;
        }

        /// <summary>Saca la bola del pool en la posición indicada.</summary>
        public void Lanzar(Vector3 posicion, Action<Fireball> callbackTerminar)
        {
            alTerminar = callbackTerminar;
            transform.position = posicion;
            vida = 0f;
            explotada = false;
            gameObject.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        private void Update()
        {
            vida += Time.deltaTime;
            if (vida >= vidaMaxima) Terminar(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (explotada) return;

            if (other.CompareTag(Constantes.Tags.Player))
            {
                var health = other.GetComponent<PlayerHealth>();
                if (health != null && health.TakeDamage(damage) && AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayDanio();
                }
                Terminar(true);
                return;
            }

            if (other.gameObject.layer == LayerMask.NameToLayer(Constantes.Layers.Suelo) && !other.isTrigger)
            {
                Terminar(true);
            }
        }

        private void Terminar(bool conExplosion)
        {
            if (explotada) return;
            explotada = true;

            if (conExplosion)
            {
                EfectoTemporal.Crear(transform.position, cuadrosExplosion, 14f, new Color(1f, 0.6f, 0.25f), 1.4f);
                if (AudioManager.Instance != null) AudioManager.Instance.PlayRocaImpacto();
            }

            rb.linearVelocity = Vector2.zero;
            gameObject.SetActive(false);
            alTerminar?.Invoke(this);
        }
    }
}
