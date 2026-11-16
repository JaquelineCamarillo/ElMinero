using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;

namespace ElMinero.Jefe
{
    /// <summary>
    /// Onda de choque del pisotón del Coloso: recorre el piso hacia un lado
    /// a velocidad constante hasta la pared de la arena. Se esquiva saltando.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class ShockwaveProjectile : MonoBehaviour
    {
        private float velocidad;
        private float direccion;
        private float limiteX;
        private bool viva;

        public static ShockwaveProjectile Crear(Vector3 posicion, float dir, float velocidad, float limiteX, Sprite[] cuadros)
        {
            GameObject go = new GameObject("Onda_Choque");
            go.transform.position = posicion;
            go.tag = Constantes.Tags.Hazard;
            int capa = LayerMask.NameToLayer(Constantes.Layers.Enemigo);
            if (capa != -1) go.layer = capa;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;
            sr.sortingOrder = 6;
            sr.flipX = dir < 0f;
            if (cuadros != null && cuadros.Length > 0) sr.sprite = cuadros[0];

            var anim = go.AddComponent<SpriteFrameAnimator>();
            anim.Configurar(cuadros, 10f, true, true);

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.6f, 0.6f);
            col.offset = new Vector2(0f, 0.3f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            var onda = go.AddComponent<ShockwaveProjectile>();
            onda.direccion = Mathf.Sign(dir);
            onda.velocidad = velocidad;
            onda.limiteX = limiteX;
            onda.viva = true;
            return onda;
        }

        private void Update()
        {
            if (!viva) return;
            transform.position += new Vector3(direccion * velocidad * Time.deltaTime, 0f, 0f);

            bool llego = direccion > 0f ? transform.position.x >= limiteX : transform.position.x <= limiteX;
            if (llego) Destruir();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!viva || !other.CompareTag(Constantes.Tags.Player)) return;

            var salud = other.GetComponent<PlayerHealth>();
            if (salud != null && salud.TakeDamage(1) && AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayDanio();
            }
        }

        public void Destruir()
        {
            viva = false;
            Destroy(gameObject);
        }
    }
}
