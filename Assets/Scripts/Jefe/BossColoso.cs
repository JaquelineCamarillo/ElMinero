using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;
using ElMinero.Camara;

namespace ElMinero.Jefe
{
    /// <summary>
    /// Jefe final del nivel 3: "El Coloso de Plata Roja" (leyenda ficticia creada para el juego).
    ///
    /// Máquina de estados con 3 fases según su vida (3, 2 y 1):
    ///   Fase 1: Embestida → Pisotón → Embestida (pausa 1.0 s, embestida 9 u/s)
    ///   Fase 2: Embestida → Lanzar ×2 → Pisotón (pausa 0.8 s, embestida 11 u/s, rocas del techo cada 3 s)
    ///   Fase 3: Embestida → Pisotón doble → Lanzar ×3 (pausa 0.6 s, embestida 12 u/s, rocas cada 2 s, ondas a 9 u/s)
    ///
    /// Solo recibe daño cuando está Aturdido (después de chocar con la pared) y el jugador le cae encima.
    /// Tocar su cuerpo cuando no está aturdido quita 1 vida y empuja al jugador.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public class BossColoso : MonoBehaviour, IResettable
    {
        private enum Ataque { Embestida, Pisoton, PisotonDoble, Lanzar2, Lanzar3 }
        private enum EstadoJefe { Dormido, Combate, Derrotado }

        [Header("Arena")]
        [SerializeField] private BossArena arena;
        [SerializeField] private float pisoY = 34f;
        [Tooltip("Límites del centro del jefe al embestir (pared izquierda y derecha de la arena).")]
        [SerializeField] private float limiteIzquierdo = 155.9f;
        [SerializeField] private float limiteDerecho = 181.1f;
        [Tooltip("Paredes de la arena para las ondas de choque.")]
        [SerializeField] private float paredIzquierda = 154.6f;
        [SerializeField] private float paredDerecha = 182.4f;

        [Header("Componentes")]
        [SerializeField] private Animator anim;
        [SerializeField] private SpriteRenderer sr;
        [SerializeField] private Collider2D puntoDebil;
        [SerializeField] private GameObject estrellas;
        [SerializeField] private AudioSource audioEmbestida;

        [Header("Sprites de efectos")]
        [SerializeField] private Sprite[] cuadrosOnda;
        [SerializeField] private Sprite[] cuadrosRoca;
        [SerializeField] private Sprite[] cuadrosPolvoGrande;
        [SerializeField] private Sprite[] cuadrosPolvo;

        [Header("Efectos de golpe")]
        [SerializeField] private TMP_FontAsset fuenteGolpe;

        [Header("Vida")]
        [SerializeField] private int vidaMaxima = 3;

        private Rigidbody2D rb;
        private Vector3 posicionInicial;
        private int vida;
        private EstadoJefe estado = EstadoJefe.Dormido;
        private bool aturdido;
        private bool golpeRecibido;
        private float invulnerableHasta;
        private float direccion = -1f; // -1 = mira a la izquierda (como el sprite)
        private Transform jugador;
        private PlayerController controlJugador;
        private PlayerHealth saludJugador;
        private Coroutine rutinaCombate;

        private static readonly string[] Triggers = { "Idle", "Caminar", "Preparar", "Embestir", "Aturdir", "Golpe", "Pisoton", "Lanzar", "Morir" };

        public int Vida => vida;
        public int Fase => vida >= 3 ? 1 : (vida == 2 ? 2 : 3);
        public bool Derrotado => estado == EstadoJefe.Derrotado;

        public void Configurar(BossArena arenaJefe, float piso, float limIzq, float limDer, float paredIzq, float paredDer)
        {
            arena = arenaJefe;
            pisoY = piso;
            limiteIzquierdo = limIzq;
            limiteDerecho = limDer;
            paredIzquierda = paredIzq;
            paredDerecha = paredDer;
        }

        public void ConfigurarEfectos(Sprite[] onda, Sprite[] roca, Sprite[] polvoGrande, Sprite[] polvo)
        {
            cuadrosOnda = onda;
            cuadrosRoca = roca;
            cuadrosPolvoGrande = polvoGrande;
            cuadrosPolvo = polvo;
        }

        public void ConfigurarFuente(TMP_FontAsset fuente)
        {
            fuenteGolpe = fuente;
        }

        /// <summary>El jugador perdió (Game Over): el jefe deja de atacar sin reiniciarse.</summary>
        public void Detener()
        {
            StopAllCoroutines();
            rutinaCombate = null;
            if (estado != EstadoJefe.Derrotado) estado = EstadoJefe.Dormido;
            aturdido = false;
            if (audioEmbestida != null) audioEmbestida.Stop();
            if (puntoDebil != null) puntoDebil.enabled = false;
            if (estrellas != null) estrellas.SetActive(false);
            Animar("Idle");
        }

        public void ConfigurarComponentes(Animator animator, SpriteRenderer render, Collider2D debil, GameObject objEstrellas, AudioSource audioLoop)
        {
            anim = animator;
            sr = render;
            puntoDebil = debil;
            estrellas = objEstrellas;
            audioEmbestida = audioLoop;
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            if (anim == null) anim = GetComponent<Animator>();
            if (sr == null) sr = GetComponent<SpriteRenderer>();

            tag = Constantes.Tags.Hazard;
            int capa = LayerMask.NameToLayer(Constantes.Layers.Enemigo);
            if (capa != -1) gameObject.layer = capa;

            posicionInicial = transform.position;
            vida = vidaMaxima;
            EstadoVisualInicial();
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
            if (p != null)
            {
                jugador = p.transform;
                controlJugador = p.GetComponent<PlayerController>();
                saludJugador = p.GetComponent<PlayerHealth>();
            }

            if (audioEmbestida != null && AudioManager.Instance != null)
            {
                audioEmbestida.clip = AudioManager.Instance.ObtenerClip("Coloso_Embestida_Loop");
                AudioManager.Instance.RegistrarFuenteEfectos(audioEmbestida);
            }
        }

        private void OnDestroy()
        {
            if (audioEmbestida != null && AudioManager.Instance != null)
            {
                AudioManager.Instance.DesregistrarFuenteEfectos(audioEmbestida);
            }
        }

        private void EstadoVisualInicial()
        {
            aturdido = false;
            golpeRecibido = false;
            direccion = -1f;
            if (sr != null) { sr.flipX = false; sr.color = Color.white; }
            if (puntoDebil != null) puntoDebil.enabled = false;
            if (estrellas != null) estrellas.SetActive(false);
            if (audioEmbestida != null) audioEmbestida.Stop();
            var cuerpo = GetComponent<BoxCollider2D>();
            if (cuerpo != null) cuerpo.enabled = true;
            Animar("Idle");
        }

        // ====================================================================
        // API PARA LA ARENA
        // ====================================================================

        /// <summary>Intro: el Coloso se levanta.</summary>
        public void Levantarse()
        {
            Animar("Caminar");
            EfectoTemporal.Crear(new Vector3(transform.position.x, pisoY + 0.4f, 0f), cuadrosPolvoGrande, 12f, Color.white, 1.5f);
        }

        public void IniciarCombate()
        {
            if (estado == EstadoJefe.Derrotado) return;
            estado = EstadoJefe.Combate;
            if (rutinaCombate != null) StopCoroutine(rutinaCombate);
            rutinaCombate = StartCoroutine(CicloCombate());
        }

        /// <summary>El jugador perdió una vida durante la pelea: el jefe vuelve a 3 de vida y a su lugar.</summary>
        public void Reiniciar()
        {
            StopAllCoroutines();
            rutinaCombate = null;
            estado = EstadoJefe.Dormido;
            vida = vidaMaxima;
            transform.position = posicionInicial;
            if (rb != null) rb.position = posicionInicial;
            EstadoVisualInicial();
        }

        public void ResetObject()
        {
            Reiniciar();
        }

        // ====================================================================
        // CICLO DE COMBATE
        // ====================================================================

        private float PausaIdle => Fase == 1 ? 1.0f : (Fase == 2 ? 0.8f : 0.6f);
        private float VelocidadEmbestida => Fase == 1 ? 9f : (Fase == 2 ? 11f : 12f);
        private float VelocidadOnda => Fase == 3 ? 9f : 7f;
        private float DuracionAturdido => Fase == 3 ? 2f : 2.5f;

        private List<Ataque> SecuenciaFase(int fase)
        {
            switch (fase)
            {
                case 1: return new List<Ataque> { Ataque.Embestida, Ataque.Pisoton, Ataque.Embestida };
                case 2: return new List<Ataque> { Ataque.Embestida, Ataque.Lanzar2, Ataque.Pisoton };
                default: return new List<Ataque> { Ataque.Embestida, Ataque.PisotonDoble, Ataque.Lanzar3 };
            }
        }

        private IEnumerator CicloCombate()
        {
            while (estado == EstadoJefe.Combate && vida > 0)
            {
                int faseActual = Fase;
                foreach (Ataque ataque in SecuenciaFase(faseActual))
                {
                    Animar("Idle");
                    yield return new WaitForSeconds(PausaIdle);
                    if (estado != EstadoJefe.Combate) yield break;

                    yield return EjecutarAtaque(ataque);

                    if (estado != EstadoJefe.Combate) yield break;
                    if (Fase != faseActual) break; // cambió de fase: empieza la secuencia nueva
                }
            }
        }

        private IEnumerator EjecutarAtaque(Ataque ataque)
        {
            switch (ataque)
            {
                case Ataque.Embestida: yield return Embestida(); break;
                case Ataque.Pisoton: yield return Pisoton(); break;
                case Ataque.PisotonDoble:
                    yield return Pisoton();
                    yield return new WaitForSeconds(0.25f);
                    yield return Pisoton();
                    break;
                case Ataque.Lanzar2: yield return Lanzar(2); break;
                case Ataque.Lanzar3: yield return Lanzar(3); break;
            }
        }

        // ---------------------------------------------------------------- Embestida
        private IEnumerator Embestida()
        {
            MirarHaciaJugador();

            // 1. Aviso: se agacha, ojos rojos, polvo y temblor leve
            Animar("Preparar");
            EfectoTemporal.Crear(new Vector3(transform.position.x - direccion * 1.2f, pisoY + 0.3f, 0f), cuadrosPolvo, 12f, Color.white, 1.4f);
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.8f, 0.06f);
            yield return new WaitForSeconds(0.8f);

            // 2. Embiste hasta chocar con la pared
            Animar("Embestir");
            if (audioEmbestida != null && audioEmbestida.clip != null) audioEmbestida.Play();

            float destinoX = direccion > 0f ? limiteDerecho : limiteIzquierdo;
            float vel = VelocidadEmbestida;
            while (Mathf.Abs(rb.position.x - destinoX) > 0.01f)
            {
                float nx = Mathf.MoveTowards(rb.position.x, destinoX, vel * Time.fixedDeltaTime);
                rb.MovePosition(new Vector2(nx, pisoY));
                yield return new WaitForFixedUpdate();
                if (estado != EstadoJefe.Combate) yield break;
            }
            if (audioEmbestida != null) audioEmbestida.Stop();

            // 3. Choque con la pared
            Sonido("Coloso_ChoquePared");
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.45f, 0.35f);
            EfectoTemporal.Crear(new Vector3(destinoX + direccion * 1.4f, pisoY + 1.2f, 0f), cuadrosPolvoGrande, 12f, Color.white, 1.6f);
            if (arena != null) arena.SoltarRocas(2);

            // 4. Aturdido (vulnerable)
            yield return Aturdido();
        }

        private IEnumerator Aturdido()
        {
            aturdido = true;
            golpeRecibido = false;
            Animar("Aturdir");
            if (arena != null) arena.AlAturdirse();
            if (estrellas != null) estrellas.SetActive(true);
            if (puntoDebil != null) puntoDebil.enabled = true;

            float t = 0f;
            float duracion = DuracionAturdido;
            while (t < duracion && !golpeRecibido)
            {
                t += Time.deltaTime;
                yield return null;
                if (estado != EstadoJefe.Combate) yield break;
            }

            if (puntoDebil != null) puntoDebil.enabled = false;
            if (estrellas != null) estrellas.SetActive(false);

            if (golpeRecibido)
            {
                yield return RecibirGolpe();
            }
            aturdido = false;
        }

        /// <summary>Llamado por el punto débil cuando el jugador cae sobre la cabeza del jefe aturdido.</summary>
        public void Pisado(PlayerController quien)
        {
            if (!aturdido || golpeRecibido || estado != EstadoJefe.Combate) return;
            if (Time.time < invulnerableHasta) return;

            golpeRecibido = true;
            if (puntoDebil != null) puntoDebil.enabled = false;
            if (quien != null) quien.Rebotar(1f);
        }

        private IEnumerator RecibirGolpe()
        {
            vida = Mathf.Max(0, vida - 1);
            invulnerableHasta = Time.time + 1f;

            Sonido("Coloso_Golpeado");
            Animar("Golpe");

            // Impacto: congelación breve, chispas, texto flotante y sacudida
            Vector3 cabeza = new Vector3(transform.position.x, pisoY + 3.6f, 0f);
            EfectoTemporal.Crear(cabeza, cuadrosPolvoGrande, 16f, new Color(1f, 0.9f, 0.6f), 1.8f, null, 30);
            EfectoTemporal.Crear(cabeza + new Vector3(-0.9f, 0.3f, 0f), cuadrosPolvo, 16f, new Color(1f, 0.6f, 0.3f), 2.2f, null, 31);
            EfectoTemporal.Crear(cabeza + new Vector3(0.9f, 0.3f, 0f), cuadrosPolvo, 16f, new Color(1f, 0.6f, 0.3f), 2.2f, null, 31);
            CrearTextoFlotante(cabeza + Vector3.up * 0.8f, vida > 0 ? "¡GOLPE!" : "¡ZAS!", new Color(1f, 0.9f, 0.3f));
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.35f, 0.3f);

            float escalaTiempo = Time.timeScale;
            Time.timeScale = 0.05f;
            yield return new WaitForSecondsRealtime(0.12f);
            if (Time.timeScale < 0.1f) Time.timeScale = escalaTiempo > 0f ? escalaTiempo : 1f;

            // Parpadeo blanco/rojo y temblor del sprite
            Vector3 basePos = transform.position;
            for (int i = 0; i < 6; i++)
            {
                if (sr != null) sr.color = (i % 2 == 0) ? new Color(1f, 0.45f, 0.45f) : Color.white;
                float dx = (i % 2 == 0 ? 0.12f : -0.12f);
                rb.MovePosition(new Vector2(basePos.x + dx, pisoY));
                yield return new WaitForSeconds(0.06f);
            }
            rb.MovePosition(new Vector2(basePos.x, pisoY));
            if (sr != null) sr.color = Color.white;

            if (arena != null) arena.AlRecibirGolpe(vida);

            if (vida <= 0)
            {
                yield return Derrota();
                yield break;
            }

            // Ruge, caen 3 rocas y pasa a la siguiente fase
            Animar("Idle");
            Sonido("Coloso_Rugido");
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.6f, 0.2f);
            if (arena != null)
            {
                arena.SoltarRocas(3);
                arena.AlCambiarFase(Fase);
            }
            yield return new WaitForSeconds(0.8f);
        }

        // ---------------------------------------------------------------- Pisotón
        private IEnumerator Pisoton()
        {
            MirarHaciaJugador();
            Animar("Pisoton");

            // Se agacha 0.4 s
            yield return new WaitForSeconds(0.4f);

            // Salta 0.4 s (sube y cae)
            float t = 0f;
            float duracion = 0.4f;
            float altura = 2.2f;
            while (t < duracion)
            {
                t += Time.fixedDeltaTime;
                float k = Mathf.Clamp01(t / duracion);
                float y = pisoY + altura * 4f * k * (1f - k);
                rb.MovePosition(new Vector2(rb.position.x, y));
                yield return new WaitForFixedUpdate();
                if (estado != EstadoJefe.Combate) yield break;
            }
            rb.MovePosition(new Vector2(rb.position.x, pisoY));

            // Cae: sonido, polvo, sacudida y 2 ondas de choque
            Sonido("Coloso_Pisoton");
            Sonido("Onda_Choque", 0.8f);
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.35f, 0.3f);
            EfectoTemporal.Crear(new Vector3(rb.position.x, pisoY + 0.4f, 0f), cuadrosPolvoGrande, 12f, Color.white, 1.8f);

            float vel = VelocidadOnda;
            Vector3 origen = new Vector3(rb.position.x, pisoY, 0f);
            ShockwaveProjectile.Crear(origen + Vector3.left * 1.4f, -1f, vel, paredIzquierda, cuadrosOnda);
            ShockwaveProjectile.Crear(origen + Vector3.right * 1.4f, 1f, vel, paredDerecha, cuadrosOnda);

            yield return new WaitForSeconds(0.3f);
        }

        // ---------------------------------------------------------------- Lanzar
        private IEnumerator Lanzar(int cantidad)
        {
            for (int i = 0; i < cantidad; i++)
            {
                MirarHaciaJugador();
                Animar("Lanzar");
                yield return new WaitForSeconds(i == 0 ? 0.5f : 0.15f);
                if (estado != EstadoJefe.Combate) yield break;

                Vector3 origen = new Vector3(rb.position.x + direccion * 1.2f, pisoY + 3.6f, 0f);
                Vector3 objetivo = jugador != null ? new Vector3(jugador.position.x, pisoY + 0.3f, 0f) : origen + Vector3.right * direccion * 6f;
                ThrownRock.Lanzar(origen, objetivo, cuadrosRoca, cuadrosPolvo);
                Sonido("Roca_Lanzada");

                yield return new WaitForSeconds(0.35f);
            }
            Animar("Idle");
        }

        // ---------------------------------------------------------------- Derrota
        private IEnumerator Derrota()
        {
            estado = EstadoJefe.Derrotado;
            aturdido = false;
            if (arena != null) arena.AlIniciarDerrota();

            Animar("Morir");
            Sonido("Coloso_Derrotado");
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(1.2f, 0.45f);
            EfectoTemporal.Crear(new Vector3(rb.position.x, pisoY + 1.5f, 0f), cuadrosPolvoGrande, 10f, Color.white, 2.2f);

            yield return new WaitForSeconds(1.2f);

            var cuerpo = GetComponent<BoxCollider2D>();
            if (cuerpo != null) cuerpo.enabled = false;

            if (arena != null) arena.AlDerrotarJefe();
        }

        // ====================================================================
        // DAÑO POR CONTACTO
        // ====================================================================

        private void OnCollisionEnter2D(Collision2D c) => RevisarContacto(c);
        private void OnCollisionStay2D(Collision2D c) => RevisarContacto(c);

        private void RevisarContacto(Collision2D c)
        {
            if (estado != EstadoJefe.Combate || aturdido) return;
            if (!c.gameObject.CompareTag(Constantes.Tags.Player)) return;

            var salud = c.gameObject.GetComponent<PlayerHealth>();
            if (salud == null) return;

            if (salud.TakeDamage(1))
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayDanio();
                var ctrl = c.gameObject.GetComponent<PlayerController>();
                EfectoTemporal.Crear(c.transform.position + Vector3.up * 0.8f, cuadrosPolvo, 16f, new Color(1f, 0.7f, 0.6f), 1.6f, null, 30);
                if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.25f, 0.2f);
                if (ctrl != null && !salud.IsDead)
                {
                    float lado = Mathf.Sign(c.transform.position.x - transform.position.x);
                    if (Mathf.Approximately(lado, 0f)) lado = -direccion;
                    ctrl.Empujar(new Vector2(lado * 15f, 7f), 0.4f);
                }
            }
        }

        // ====================================================================
        // UTILIDADES
        // ====================================================================

        private void MirarHaciaJugador()
        {
            if (jugador == null) return;
            direccion = jugador.position.x >= transform.position.x ? 1f : -1f;
            if (sr != null) sr.flipX = direccion > 0f; // el sprite mira a la izquierda
        }

        private void Animar(string trigger)
        {
            if (anim == null || anim.runtimeAnimatorController == null) return;
            foreach (var t in Triggers) anim.ResetTrigger(t);
            anim.SetTrigger(trigger);
        }

        private void CrearTextoFlotante(Vector3 posicion, string texto, Color color)
        {
            GameObject go = new GameObject("Texto_Golpe");
            go.transform.position = posicion;
            var tmp = go.AddComponent<TextMeshPro>();
            if (fuenteGolpe != null) tmp.font = fuenteGolpe;
            tmp.text = texto;
            tmp.fontSize = 6f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color32(40, 10, 5, 255);
            tmp.sortingLayerID = SortingLayer.NameToID(Constantes.SortingLayers.UI);
            tmp.sortingOrder = 50;
            go.AddComponent<TextoFlotante>();
        }

        private void Sonido(string nombre, float volumen = 1f)
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre(nombre, volumen);
        }
    }
}
