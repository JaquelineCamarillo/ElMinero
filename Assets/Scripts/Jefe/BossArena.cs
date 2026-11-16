using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;
using ElMinero.Obstaculos;
using ElMinero.UI;
using ElMinero.Camara;

namespace ElMinero.Jefe
{
    /// <summary>
    /// Arena del Coloso (x 153–183, piso y = 34, techo y = 46). Va en el trigger "T" (x = 156).
    ///
    /// Al cruzar T: se cierra la compuerta de entrada (3 bloques caen), la cámara se limita a la arena,
    /// suena la música del jefe y hay una intro de 2.5 s (reloj y control en pausa) con el cartel
    /// "EL COLOSO DE PLATA ROJA". Después aparece la barra de vida y empieza la pelea.
    ///
    /// Si el jugador pierde una vida: reaparece en C3, el jefe vuelve a 3 de vida, la compuerta se abre
    /// y al volver a cruzar T la intro dura 0.8 s sin cartel.
    ///
    /// Al vencerlo: jingle, "¡VENCISTE AL COLOSO!", +300 puntos, +1 vida, se abre la compuerta de salida,
    /// la música vuelve a la del nivel con intensidad alta y se guarda JefeVencido_N3 = 1.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BossArena : MonoBehaviour, IResettable
    {
        private enum EstadoArena { Esperando, Intro, Pelea, Vencido }

        [Header("Jefe")]
        [SerializeField] private BossColoso jefe;
        [SerializeField] private string nombreJefe = "El Coloso de Plata Roja";

        [Header("Compuertas")]
        [SerializeField] private Transform[] bloquesEntrada;
        [SerializeField] private Transform[] bloquesSalida;

        [Header("Rocas del techo de la arena")]
        [SerializeField] private RockSpawner[] rocasTecho;
        [Tooltip("Si está apagado, durante la pelea NO caen rocas del techo (solo las que lanza el Coloso).")]
        [SerializeField] private bool rocasDelTecho = false;

        [Header("Cámara")]
        [SerializeField] private CinemachineConfiner2D confiner;
        [SerializeField] private Collider2D limitesNivel;
        [SerializeField] private Collider2D limitesArena;

        [Header("Efectos")]
        [SerializeField] private Sprite[] cuadrosPolvoGrande;

        private EstadoArena estado = EstadoArena.Esperando;
        private bool introLargaMostrada;
        private Vector3[] posFinalEntrada;
        private Vector3[] posFinalSalida;
        private int faseActual = 1;
        private Coroutine rutinaLluvia;
        private Transform jugador;
        private PlayerController controlJugador;
        private PlayerHealth saludJugador;

        public void Configurar(BossColoso coloso, Transform[] entrada, Transform[] salida, RockSpawner[] rocas,
                               CinemachineConfiner2D conf, Collider2D boundsNivel, Collider2D boundsArena, Sprite[] polvoGrande)
        {
            jefe = coloso;
            bloquesEntrada = entrada;
            bloquesSalida = salida;
            rocasTecho = rocas;
            confiner = conf;
            limitesNivel = boundsNivel;
            limitesArena = boundsArena;
            cuadrosPolvoGrande = polvoGrande;
        }

        private void Awake()
        {
            GetComponent<Collider2D>().isTrigger = true;

            posFinalEntrada = GuardarPosiciones(bloquesEntrada);
            posFinalSalida = GuardarPosiciones(bloquesSalida);

            MostrarBloques(bloquesEntrada, false);
            MostrarBloques(bloquesSalida, true);
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

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnJugadorReaparecio += AlReaparecerJugador;
            }
            if (saludJugador != null)
            {
                saludJugador.OnDeath += AlMorirJugador;
            }
        }

        /// <summary>Game Over durante la pelea: se esconden la barra y los avisos para no taparle el panel.</summary>
        private void AlMorirJugador()
        {
            if (estado != EstadoArena.Intro && estado != EstadoArena.Pelea) return;
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.Ocultar();
            AvisoPantalla.Ocultar();
            ApagarRocas();
            if (jefe != null) jefe.Detener();
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnJugadorReaparecio -= AlReaparecerJugador;
            }
            if (saludJugador != null)
            {
                saludJugador.OnDeath -= AlMorirJugador;
            }
        }

        private static Vector3[] GuardarPosiciones(Transform[] bloques)
        {
            if (bloques == null) return new Vector3[0];
            var arr = new Vector3[bloques.Length];
            for (int i = 0; i < bloques.Length; i++) arr[i] = bloques[i] != null ? bloques[i].position : Vector3.zero;
            return arr;
        }

        private static void MostrarBloques(Transform[] bloques, bool visibles)
        {
            if (bloques == null) return;
            foreach (var b in bloques)
            {
                if (b == null) continue;
                var sr = b.GetComponent<SpriteRenderer>();
                if (sr != null) { sr.enabled = visibles; sr.color = Color.white; }
                var col = b.GetComponent<Collider2D>();
                if (col != null) col.enabled = visibles;
            }
        }

        // ====================================================================
        // INICIO DE LA PELEA
        // ====================================================================

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (estado != EstadoArena.Esperando) return;
            if (!other.CompareTag(Constantes.Tags.Player)) return;
            if (jefe == null || jefe.Derrotado) return;

            StartCoroutine(IniciarPelea());
        }

        private IEnumerator IniciarPelea()
        {
            estado = EstadoArena.Intro;

            // 1. Se cierra la compuerta de entrada
            StartCoroutine(CerrarEntrada());

            // 2. La cámara se limita a la arena
            CambiarLimitesCamara(limitesArena);

            // 3. Música del jefe
            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicNombre("Musica_Jefe_Coloso", true, 0.6f);

            // 4. Intro con reloj y control en pausa
            var timer = LevelManager.Instance != null ? LevelManager.Instance.Timer : null;
            if (timer != null) timer.Pausar(true);
            if (controlJugador != null) controlJugador.BloquearControl(true);

            jefe.Levantarse();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre("Jefe_Aparece");

            if (!introLargaMostrada)
            {
                introLargaMostrada = true;
                if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre("Coloso_Rugido", 0.9f);
                AvisoPantalla.Mostrar("EL COLOSO DE PLATA ROJA", "Guardián de la Veta Madre", 2.5f, 60f, 34f, new Color(1f, 0.55f, 0.45f));
                yield return new WaitForSeconds(2.5f);

                // Instrucciones de la pelea (solo la primera vez, con el juego todavía en pausa)
                AvisoPantalla.Mostrar("PARA VENCERLO",
                    "Esquiva sus embestidas, sus ondas de choque y sus rocas.\nCuando choque contra la pared y le salgan estrellas, ¡salta sobre su cabeza!\nNecesitas 3 golpes.",
                    4.5f, 44f, 30f, new Color(1f, 0.85f, 0.4f));
                yield return new WaitForSeconds(4.5f);
            }
            else
            {
                yield return new WaitForSeconds(0.8f);
            }

            // 5. Barra de vida
            if (BossHealthBarUI.Instance != null)
            {
                BossHealthBarUI.Instance.Mostrar(nombreJefe, jefe.Vida);
                BossHealthBarUI.Instance.SetInstruccion("Cuando esté aturdido (estrellas), ¡salta sobre su cabeza!");
            }

            if (timer != null) timer.Pausar(false);
            if (controlJugador != null) controlJugador.BloquearControl(false);

            estado = EstadoArena.Pelea;
            AlCambiarFase(jefe.Fase);
            jefe.IniciarCombate();
        }

        private IEnumerator CerrarEntrada()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre("Compuerta_Cierra");
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.6f, 0.25f);

            for (int i = 0; i < bloquesEntrada.Length; i++)
            {
                var b = bloquesEntrada[i];
                if (b == null) continue;
                var sr = b.GetComponent<SpriteRenderer>();
                var col = b.GetComponent<Collider2D>();
                Vector3 destino = posFinalEntrada[i];
                Vector3 inicio = destino + Vector3.up * 8f;
                b.position = inicio;
                if (sr != null) sr.enabled = true;
                if (col != null) col.enabled = true;
                StartCoroutine(CaerBloque(b, inicio, destino, 0.3f));
                yield return new WaitForSeconds(0.12f);
            }

            yield return new WaitForSeconds(0.3f);
            if (bloquesEntrada.Length > 0 && bloquesEntrada[0] != null)
            {
                EfectoTemporal.Crear(posFinalEntrada[0] + new Vector3(0.8f, -0.6f, 0f), cuadrosPolvoGrande, 12f, Color.white, 1.6f);
            }
        }

        private IEnumerator CaerBloque(Transform b, Vector3 inicio, Vector3 destino, float duracion)
        {
            float t = 0f;
            while (t < duracion)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / duracion);
                b.position = Vector3.Lerp(inicio, destino, k * k);
                yield return null;
            }
            b.position = destino;
        }

        // ====================================================================
        // EVENTOS DEL JEFE
        // ====================================================================

        public void SoltarRocas(int cantidad)
        {
            if (!rocasDelTecho) return; // pelea sin rocas del techo
            if (rocasTecho == null || rocasTecho.Length == 0) return;
            var indices = new List<int>();
            for (int i = 0; i < rocasTecho.Length; i++) indices.Add(i);
            for (int n = 0; n < cantidad && indices.Count > 0; n++)
            {
                int k = Random.Range(0, indices.Count);
                var r = rocasTecho[indices[k]];
                indices.RemoveAt(k);
                if (r != null) r.SoltarAhora(n * 0.2f);
            }
        }

        /// <summary>El jefe chocó con la pared y quedó aturdido: momento de atacar.</summary>
        public void AlAturdirse()
        {
            AvisoPantalla.Mostrar("¡AHORA!", "Salta sobre su cabeza", 1.6f, 48f, 32f, new Color(1f, 0.95f, 0.5f));
        }

        public void AlRecibirGolpe(int vidaRestante)
        {
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.SetVida(vidaRestante);
        }

        public void AlCambiarFase(int fase)
        {
            faseActual = fase;
            if (rocasDelTecho && rutinaLluvia == null && fase >= 2 && estado == EstadoArena.Pelea)
            {
                rutinaLluvia = StartCoroutine(LluviaDeRocas());
            }
        }

        /// <summary>Fase 2: una roca del techo cada 3 s. Fase 3: cada 2 s.</summary>
        private IEnumerator LluviaDeRocas()
        {
            while (estado == EstadoArena.Pelea && faseActual >= 2)
            {
                yield return new WaitForSeconds(faseActual == 2 ? 3f : 2f);
                if (estado == EstadoArena.Pelea) SoltarRocas(1);
            }
            rutinaLluvia = null;
        }

        /// <summary>El jefe empieza a derrumbarse: se detienen los ataques y el reloj.</summary>
        public void AlIniciarDerrota()
        {
            var timer = LevelManager.Instance != null ? LevelManager.Instance.Timer : null;
            if (timer != null) timer.Pausar(true);
            ApagarRocas();
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.SetVida(0);
        }

        public void AlDerrotarJefe()
        {
            StartCoroutine(Victoria());
        }

        private IEnumerator Victoria()
        {
            estado = EstadoArena.Vencido;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicNombre("Musica_Jefe_Derrotado", false, 0.3f);
            AvisoPantalla.Mostrar("¡VENCISTE AL COLOSO!", "", 2f, 60f, 34f, new Color(1f, 0.85f, 0.35f));
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.Ocultar();

            // Recompensa: +300 puntos y +1 vida (máximo 3)
            if (LevelManager.Instance != null) LevelManager.Instance.AgregarPuntosBonus(300, "¡Coloso vencido: +300 pts y +1 vida!");
            if (saludJugador != null) saludJugador.AgregarVida(1);

            PlayerPrefs.SetInt("JefeVencido_N3", 1);
            PlayerPrefs.Save();

            // La compuerta de salida se desmorona
            yield return AbrirSalida();

            yield return new WaitForSeconds(1.2f);

            CambiarLimitesCamara(limitesNivel);
            var timer = LevelManager.Instance != null ? LevelManager.Instance.Timer : null;
            if (timer != null) timer.Pausar(false);

            // Al terminar el jingle (5 s) regresa la música del nivel con intensidad alta
            yield return new WaitForSeconds(3f);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicNivel(3);
                AudioManager.Instance.SetMusicUrgency(true);
            }
        }

        private IEnumerator AbrirSalida()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayDerrumbe();
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.5f, 0.2f);

            foreach (var b in bloquesSalida)
            {
                if (b == null) continue;
                var col = b.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
                EfectoTemporal.Crear(b.position, cuadrosPolvoGrande, 12f, Color.white, 1.6f);
            }

            float t = 0f;
            while (t < 0.8f)
            {
                t += Time.deltaTime;
                float a = 1f - t / 0.8f;
                foreach (var b in bloquesSalida)
                {
                    if (b == null) continue;
                    var sr = b.GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = new Color(1f, 1f, 1f, a);
                    b.position += Vector3.down * Time.deltaTime * 1.5f;
                }
                yield return null;
            }
            MostrarBloques(bloquesSalida, false);
        }

        // ====================================================================
        // REINICIO
        // ====================================================================

        private void AlReaparecerJugador()
        {
            if (estado != EstadoArena.Intro && estado != EstadoArena.Pelea) return;
            ReiniciarPelea();
        }

        private void ReiniciarPelea()
        {
            StopAllCoroutines();
            rutinaLluvia = null;
            estado = EstadoArena.Esperando;

            if (jefe != null) jefe.Reiniciar();
            ApagarRocas();

            // La compuerta de entrada se vuelve a abrir
            for (int i = 0; i < bloquesEntrada.Length; i++)
            {
                if (bloquesEntrada[i] != null) bloquesEntrada[i].position = posFinalEntrada[i];
            }
            MostrarBloques(bloquesEntrada, false);

            CambiarLimitesCamara(limitesNivel);
            AvisoPantalla.Ocultar();
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.Ocultar();

            var timer = LevelManager.Instance != null ? LevelManager.Instance.Timer : null;
            if (timer != null && !(LevelManager.Instance.Health != null && LevelManager.Instance.Health.IsDead)) timer.Pausar(false);
            if (controlJugador != null) controlJugador.BloquearControl(false);

            if (AudioManager.Instance != null) AudioManager.Instance.PlayMusicNivel(3);
        }

        private void ApagarRocas()
        {
            if (rutinaLluvia != null) { StopCoroutine(rutinaLluvia); rutinaLluvia = null; }
            faseActual = 1;
            if (rocasTecho == null) return;
            foreach (var r in rocasTecho) if (r != null) r.SetActivo(false);
        }

        private void CambiarLimitesCamara(Collider2D limites)
        {
            if (confiner == null || limites == null) return;
            confiner.BoundingShape2D = limites;
            confiner.InvalidateBoundingShapeCache();
        }

        public void ResetObject()
        {
            StopAllCoroutines();
            rutinaLluvia = null;
            estado = EstadoArena.Esperando;
            introLargaMostrada = false;
            if (jefe != null) jefe.Reiniciar();
            ApagarRocas();

            for (int i = 0; i < bloquesEntrada.Length; i++)
            {
                if (bloquesEntrada[i] != null) bloquesEntrada[i].position = posFinalEntrada[i];
            }
            for (int i = 0; i < bloquesSalida.Length; i++)
            {
                if (bloquesSalida[i] != null) bloquesSalida[i].position = posFinalSalida[i];
            }
            MostrarBloques(bloquesEntrada, false);
            MostrarBloques(bloquesSalida, true);
            CambiarLimitesCamara(limitesNivel);
            if (BossHealthBarUI.Instance != null) BossHealthBarUI.Instance.Ocultar();
        }
    }
}
