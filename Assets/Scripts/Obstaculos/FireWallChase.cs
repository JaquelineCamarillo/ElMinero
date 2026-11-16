using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;
using ElMinero.UI;

namespace ElMinero.Obstaculos
{
    /// <summary>
    /// Muro de fuego que persigue al minero en la huida final del nivel 3 (La Valenciana).
    ///
    /// - Al inicio NO existe: sin sprites, collider ni sonido.
    /// - Se activa cuando el jugador cruza el trigger X (FireChaseTrigger): aparece 9 u detrás
    ///   del jugador y avanza a la derecha a 2.6 u/s, siguiendo suavemente la altura del jugador.
    /// - Es una columna de 7 llamas animadas (Llamas_Incendio, 4 cuadros a 10 FPS, desfase aleatorio)
    ///   que cubre de y − 2 a y + 12.
    /// - Si toca al jugador: −1 vida, el jugador reaparece en el checkpoint C4, el muro se apaga
    ///   y el trigger se rearma.
    /// - Se detiene y se desvanece al llegar a x = 206 o cuando el jugador toca la salida.
    /// </summary>
    public class FireWallChase : MonoBehaviour, IResettable
    {
        [Header("Persecución")]
        [SerializeField] private float velocidad = 2.6f;
        [SerializeField] private float distanciaAparicion = 9f;
        [SerializeField] private float finX = 206f;
        [SerializeField] private float suavizadoAltura = 2.5f;

        [Header("Llamas")]
        [SerializeField] private Sprite[] cuadrosLlama;
        [SerializeField] private int numeroLlamas = 7;
        [SerializeField] private float alturaLlama = 2f;
        [SerializeField] private float bordeInferior = -2f;

        [Header("Reaparición")]
        [Tooltip("Posición del checkpoint C4 (después del jefe).")]
        [SerializeField] private Vector3 checkpointPersecucion = new Vector3(185.5f, 34f, 0f);

        [Header("Referencias")]
        [SerializeField] private FireChaseTrigger trigger;
        [SerializeField] private AudioSource audioLoop;

        private enum Estado { Inactivo, Persiguiendo, Apagandose }
        private Estado estado = Estado.Inactivo;

        private SpriteRenderer[] llamas;
        private SpriteFrameAnimator[] animLlamas;
        private BoxCollider2D col;
        private Light2D luz;
        private Transform jugador;
        private PlayerHealth saludJugador;
        private Coroutine rutinaFade;

        public bool Activo => estado == Estado.Persiguiendo;

        public void Configurar(Sprite[] cuadros, FireChaseTrigger triggerAsociado, Vector3 posicionC4, float xFinal)
        {
            cuadrosLlama = cuadros;
            trigger = triggerAsociado;
            checkpointPersecucion = posicionC4;
            finX = xFinal;
        }

        private void Awake()
        {
            tag = Constantes.Tags.Hazard;
            int capaPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (capaPeligro != -1) gameObject.layer = capaPeligro;

            ConstruirLlamas();
            luz = GetComponent<Light2D>();

            col = GetComponent<BoxCollider2D>();
            if (col == null) col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            float alto = numeroLlamas * alturaLlama;
            col.size = new Vector2(1.4f, alto);
            col.offset = new Vector2(0f, bordeInferior + alto * 0.5f);

            var rb = GetComponent<Rigidbody2D>();
            if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            if (audioLoop == null) audioLoop = GetComponent<AudioSource>();
            if (audioLoop == null) audioLoop = gameObject.AddComponent<AudioSource>();
            audioLoop.loop = true;
            audioLoop.playOnAwake = false;
            audioLoop.spatialBlend = 0.6f;
            audioLoop.minDistance = 4f;
            audioLoop.maxDistance = 24f;
            audioLoop.rolloffMode = AudioRolloffMode.Linear;

            ApagarInmediato();
        }

        private void Start()
        {
            if (AudioManager.Instance != null && audioLoop != null)
            {
                if (audioLoop.clip == null) audioLoop.clip = AudioManager.Instance.GetAudioClipFuegoLoop();
                AudioManager.Instance.RegistrarFuenteEfectos(audioLoop);
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnJugadorReaparecio += AlReaparecerJugador;
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnJugadorReaparecio -= AlReaparecerJugador;
            }
            if (AudioManager.Instance != null && audioLoop != null)
            {
                AudioManager.Instance.DesregistrarFuenteEfectos(audioLoop);
            }
        }

        private void ConstruirLlamas()
        {
            llamas = new SpriteRenderer[numeroLlamas];
            animLlamas = new SpriteFrameAnimator[numeroLlamas];

            for (int i = 0; i < numeroLlamas; i++)
            {
                Transform existente = transform.Find($"Llama_{i}");
                GameObject go = existente != null ? existente.gameObject : new GameObject($"Llama_{i}");
                go.transform.SetParent(transform, false);
                go.transform.localPosition = new Vector3(0f, bordeInferior + i * alturaLlama, 0f);

                var sr = go.GetComponent<SpriteRenderer>();
                if (sr == null) sr = go.AddComponent<SpriteRenderer>();
                sr.sortingLayerName = Constantes.SortingLayers.PrimerPlano;
                sr.sortingOrder = 5;
                if (cuadrosLlama != null && cuadrosLlama.Length > 0) sr.sprite = cuadrosLlama[0];
                llamas[i] = sr;

                var anim = go.GetComponent<SpriteFrameAnimator>();
                if (anim == null) anim = go.AddComponent<SpriteFrameAnimator>();
                anim.Configurar(cuadrosLlama, 10f, true, false, true);
                animLlamas[i] = anim;
            }
        }

        /// <summary>Llamado por el trigger X: el muro aparece detrás del jugador y empieza a avanzar.</summary>
        public void Activar(Transform objetivo)
        {
            if (estado == Estado.Persiguiendo) return;
            if (LevelManager.Instance != null && LevelManager.Instance.NivelCompletado) return;

            jugador = objetivo;
            saludJugador = objetivo != null ? objetivo.GetComponent<PlayerHealth>() : null;
            if (rutinaFade != null) { StopCoroutine(rutinaFade); rutinaFade = null; }

            // El checkpoint C4 debe ser el punto de reaparición durante la huida
            if (LevelManager.Instance != null && LevelManager.Instance.CheckpointActual.x < checkpointPersecucion.x - 1f)
            {
                LevelManager.Instance.EstablecerCheckpoint(checkpointPersecucion);
            }

            Vector3 p = objetivo != null ? objetivo.position : transform.position;
            transform.position = new Vector3(p.x - distanciaAparicion, p.y, 0f);

            for (int i = 0; i < llamas.Length; i++)
            {
                llamas[i].enabled = true;
                llamas[i].color = Color.white;
                animLlamas[i].Play();
            }
            col.enabled = true;
            if (luz != null) luz.enabled = true;
            if (audioLoop != null && audioLoop.clip != null) audioLoop.Play();

            estado = Estado.Persiguiendo;

            AvisoPantalla.Mostrar("¡LA MINA SE INCENDIA!", "", 2f, 56f, 34f, new Color(1f, 0.45f, 0.15f));
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDerrumbe();
            if (ElMinero.Camara.CameraShake.Instance != null) ElMinero.Camara.CameraShake.Instance.Shake(0.5f, 0.25f);
        }

        private void Update()
        {
            if (estado != Estado.Persiguiendo) return;

            if (LevelManager.Instance != null && LevelManager.Instance.NivelCompletado)
            {
                Desvanecer();
                return;
            }

            float x = transform.position.x + velocidad * Time.deltaTime;
            float y = transform.position.y;
            if (jugador != null)
            {
                y = Mathf.Lerp(y, jugador.position.y, Time.deltaTime * suavizadoAltura);
            }
            transform.position = new Vector3(x, y, 0f);

            if (x >= finX)
            {
                Desvanecer();
            }
        }

        private void OnTriggerEnter2D(Collider2D other) => RevisarContacto(other);
        private void OnTriggerStay2D(Collider2D other) => RevisarContacto(other);

        private void RevisarContacto(Collider2D other)
        {
            if (estado != Estado.Persiguiendo) return;
            if (!other.CompareTag(Constantes.Tags.Player)) return;

            var salud = other.GetComponent<PlayerHealth>();
            if (salud == null) return;

            bool golpe = salud.TakeDamage(1);
            if (!golpe) return;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayDanio();

            // El jugador reaparece en C4 (LevelManager lo mueve al checkpoint actual);
            // el muro se apaga y el trigger se rearma.
            ApagarYRearmar();
        }

        private void AlReaparecerJugador()
        {
            if (estado != Estado.Inactivo) ApagarYRearmar();
        }

        private void ApagarYRearmar()
        {
            ApagarInmediato();
            if (trigger != null) trigger.Rearmar();
        }

        private void Desvanecer()
        {
            if (estado != Estado.Persiguiendo) return;
            estado = Estado.Apagandose;
            col.enabled = false;
            rutinaFade = StartCoroutine(RutinaFade(0.5f));
        }

        private IEnumerator RutinaFade(float duracion)
        {
            float t = 0f;
            float volInicial = audioLoop != null ? audioLoop.volume : 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float a = 1f - t / duracion;
                foreach (var sr in llamas) sr.color = new Color(1f, 1f, 1f, a);
                if (audioLoop != null) audioLoop.volume = volInicial * a;
                yield return null;
            }
            ApagarInmediato();
            if (audioLoop != null) audioLoop.volume = volInicial;
            rutinaFade = null;
        }

        private void ApagarInmediato()
        {
            estado = Estado.Inactivo;
            if (llamas != null)
            {
                for (int i = 0; i < llamas.Length; i++)
                {
                    if (llamas[i] != null) { llamas[i].enabled = false; llamas[i].color = Color.white; }
                    if (animLlamas[i] != null) animLlamas[i].Stop();
                }
            }
            if (col != null) col.enabled = false;
            if (luz != null) luz.enabled = false;
            if (audioLoop != null) audioLoop.Stop();
        }

        public void ResetObject()
        {
            StopAllCoroutines();
            rutinaFade = null;
            ApagarInmediato();
        }
    }
}
