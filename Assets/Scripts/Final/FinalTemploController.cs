using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using TMPro;
using ElMinero.Utilidades;
using ElMinero.Managers;
using ElMinero.Jugador;
using ElMinero.UI;

namespace ElMinero.Final
{
    /// <summary>
    /// Epílogo jugable "Final_Templo": el minero sale del Tiro de San Cayetano con las
    /// joyas del Coloso y las reparte entre la gente de La Valenciana.
    ///
    /// 1. Fundido desde negro: el minero sale de la oscuridad del tiro (zoom 6 → 8).
    /// 2. Aviso "¡ERES LIBRE!" y contador de joyas (0 / N).
    /// 3. Al acercarse a cada habitante que espera, le lanza una joya: chispas, corazón,
    ///    frase de agradecimiento y el contador sube.
    /// 4. Cuando todos tienen su joya: repican las campanas, cohetes sobre el templo,
    ///    confeti y todo el pueblo celebra. Una flecha marca la puerta del templo.
    /// 5. En la puerta: el minero celebra, zoom 8 → 6.5 y panel final.
    /// </summary>
    public class FinalTemploController : MonoBehaviour
    {
        [Header("Jugador y cámara")]
        [SerializeField] private Transform jugador;
        [SerializeField] private Camera camara;
        [SerializeField] private float orthoIntro = 6f;
        [SerializeField] private float orthoInicial = 8f;
        [SerializeField] private float orthoFinal = 6.5f;
        [SerializeField] private float limiteIzquierdo = -2f;
        [SerializeField] private float limiteDerecho = 62f;
        [SerializeField] private float bordeInferiorVista = -2f;

        [Header("Parallax (capas y factor 0 = fija al mundo, 1 = pegada a la cámara)")]
        [SerializeField] private Transform[] capasFondo;
        [SerializeField] private float[] seguimientoCapas = { 0.9f, 0.65f, 0.4f };

        [Header("Intro")]
        [SerializeField] private CanvasGroup fundidoNegro;
        [SerializeField] private float xSalidaTiro = 7f;

        [Header("Amanecer")]
        [SerializeField] private Light2D luzGlobal;
        [SerializeField] private float intensidadInicial = 0.6f;
        [SerializeField] private float intensidadFinal = 1.0f;
        [SerializeField] private float duracionAmanecer = 8f;

        [Header("Faroles (cuadro 0 = apagado/tenue, 1 = encendido)")]
        [SerializeField] private SpriteFrameAnimator[] faroles;
        [SerializeField] private Light2D[] lucesFaroles;
        [SerializeField] private float apagarFarolesA = 5f;

        [Header("Campanas")]
        [SerializeField] private SpriteFrameAnimator[] campanas;

        [Header("Reparto de joyas")]
        [SerializeField] private HabitanteJoya[] habitantes;
        [Tooltip("Hojas de joyas aplanadas: cada joya ocupa 'cuadrosPorJoya' cuadros seguidos.")]
        [SerializeField] private Sprite[] cuadrosJoyas;
        [SerializeField] private int cuadrosPorJoya = 3;
        [SerializeField] private float radioEntrega = 1.7f;
        [SerializeField] private TMP_FontAsset fuenteFrases;
        [SerializeField] private EfectosFiesta efectos;

        [Header("Contador (UI)")]
        [SerializeField] private CanvasGroup grupoContador;
        [SerializeField] private TextMeshProUGUI textoContador;
        [SerializeField] private RectTransform cajaContador;

        [Header("Puerta del templo")]
        [SerializeField] private float xPuerta = 46f;
        [SerializeField] private Transform flechaPuerta;
        [SerializeField] private PanelFinalTemplo panelFinal;

        [Header("Audio")]
        [SerializeField] private AudioSource ambienteAves;

        private PlayerController control;
        private PlayerAnimator animJugador;
        private SpriteRenderer srJugador;
        private Vector3 camInicio;
        private Vector3[] capasInicio;
        private Vector3 velocidadCam;
        private bool introTerminada;
        private bool entregando;
        private bool fiestaIniciada;
        private bool finalIniciado;
        private int entregadas;
        private float proximoAvisoPuerta;
        private Vector3 posFlecha;

        private int Total => habitantes != null ? habitantes.Length : 0;

        private void Start()
        {
            Time.timeScale = 1f;

            if (jugador == null)
            {
                GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
                if (p != null) jugador = p.transform;
            }
            if (camara == null) camara = Camera.main;
            if (jugador != null)
            {
                control = jugador.GetComponent<PlayerController>();
                animJugador = jugador.GetComponent<PlayerAnimator>();
                srJugador = jugador.GetComponentInChildren<SpriteRenderer>();
            }

            if (camara != null)
            {
                camara.orthographicSize = orthoIntro;
                camara.transform.position = PosicionCamaraObjetivo();
                // Referencia del parallax: la cámara ya abierta (ortho normal), así el fondo
                // queda en su lugar después del zoom de la intro
                camInicio = new Vector3(camara.transform.position.x, bordeInferiorVista + orthoInicial, -10f);
            }

            if (capasFondo != null)
            {
                capasInicio = new Vector3[capasFondo.Length];
                for (int i = 0; i < capasFondo.Length; i++) capasInicio[i] = capasFondo[i] != null ? capasFondo[i].position : Vector3.zero;
            }

            if (luzGlobal != null) luzGlobal.intensity = intensidadInicial;
            if (grupoContador != null) grupoContador.alpha = 0f;
            if (flechaPuerta != null)
            {
                posFlecha = flechaPuerta.position;
                flechaPuerta.gameObject.SetActive(false);
            }
            ActualizarContador(false);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMusicNombre("Musica_Final_Amanecer", true, 1f);
                if (ambienteAves != null)
                {
                    ambienteAves.clip = AudioManager.Instance.ObtenerClip("Aves_Amanecer_Loop");
                    AudioManager.Instance.RegistrarFuenteEfectos(ambienteAves);
                    ambienteAves.volume = AudioManager.Instance.VolumenEfectos * 0.6f;
                    if (ambienteAves.clip != null) ambienteAves.Play();
                }
            }

            if (faroles != null) foreach (var f in faroles) if (f != null) f.MostrarCuadro(1);

            StartCoroutine(Intro());
            StartCoroutine(Amanecer());
        }

        private void OnDestroy()
        {
            if (AudioManager.Instance != null && ambienteAves != null)
            {
                AudioManager.Instance.DesregistrarFuenteEfectos(ambienteAves);
            }
        }

        // ================================================================== INTRO

        private IEnumerator Intro()
        {
            if (control != null) control.BloquearControl(true, 0f);
            if (srJugador != null) srJugador.color = Color.black;

            // El malacate del tiro suena mientras se aclara la pantalla
            AudioSource malacate = gameObject.AddComponent<AudioSource>();
            if (AudioManager.Instance != null)
            {
                malacate.clip = AudioManager.Instance.GetAudioClipMalacateLoop();
                malacate.volume = AudioManager.Instance.VolumenEfectos;
            }
            if (malacate.clip != null) malacate.Play();

            float t = 0f;
            while (t < 1.2f)
            {
                t += Time.deltaTime;
                if (fundidoNegro != null) fundidoNegro.alpha = 1f - t / 1.2f;
                yield return null;
            }
            if (fundidoNegro != null) { fundidoNegro.alpha = 0f; fundidoNegro.blocksRaycasts = false; }
            malacate.Stop();
            yield return new WaitForSeconds(0.3f);

            // Sale caminando de la oscuridad del tiro mientras la cámara se abre
            float xInicio = jugador != null ? jugador.position.x : 0f;
            if (control != null) control.BloquearControl(true, 1f);
            float limite = 5f;
            t = 0f;
            while (jugador != null && jugador.position.x < xSalidaTiro && limite > 0f)
            {
                limite -= Time.deltaTime;
                t += Time.deltaTime;
                float k = Mathf.InverseLerp(xInicio, xSalidaTiro, jugador.position.x);
                if (srJugador != null) srJugador.color = Color.Lerp(Color.black, Color.white, Mathf.Clamp01(k * 1.6f));
                if (camara != null) camara.orthographicSize = Mathf.Lerp(orthoIntro, orthoInicial, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            if (srJugador != null) srJugador.color = Color.white;
            if (camara != null) camara.orthographicSize = orthoInicial;
            if (control != null) control.BloquearControl(true, 0f);

            // Celebra un instante al ver la luz del día
            if (animJugador != null) animJugador.PlayWinAnimation();
            AvisoPantalla.Mostrar("¡ERES LIBRE!", "Reparte las joyas del Coloso entre la gente del pueblo", 3.5f, 60f, 34f, new Color(1f, 0.85f, 0.4f));
            yield return new WaitForSeconds(1.2f);

            if (control != null) control.BloquearControl(false);
            introTerminada = true;

            t = 0f;
            while (t < 0.5f)
            {
                t += Time.deltaTime;
                if (grupoContador != null) grupoContador.alpha = t / 0.5f;
                yield return null;
            }
            if (grupoContador != null) grupoContador.alpha = 1f;
        }

        private IEnumerator Amanecer()
        {
            float t = 0f;
            bool farolesApagados = false;
            while (t < Mathf.Max(duracionAmanecer, apagarFarolesA))
            {
                t += Time.deltaTime;
                if (luzGlobal != null) luzGlobal.intensity = Mathf.Lerp(intensidadInicial, intensidadFinal, Mathf.Clamp01(t / duracionAmanecer));

                if (!farolesApagados && t >= apagarFarolesA)
                {
                    farolesApagados = true;
                    if (faroles != null) foreach (var f in faroles) if (f != null) f.MostrarCuadro(0);
                    if (lucesFaroles != null) foreach (var l in lucesFaroles) if (l != null) l.intensity *= 0.3f;
                }
                yield return null;
            }
        }

        // ================================================================== JUEGO

        private void Update()
        {
            if (jugador == null) return;

            // Entregar joya al habitante más cercano que la espera
            if (introTerminada && !entregando && !finalIniciado && habitantes != null)
            {
                HabitanteJoya cercano = null;
                float mejor = radioEntrega;
                foreach (var h in habitantes)
                {
                    if (h == null || !h.Esperando) continue;
                    float d = Mathf.Abs(h.transform.position.x - jugador.position.x);
                    if (d < mejor) { mejor = d; cercano = h; }
                }
                if (cercano != null) StartCoroutine(Entregar(cercano));
            }

            // Puerta del templo
            if (introTerminada && !finalIniciado && jugador.position.x >= xPuerta - 1f)
            {
                if (fiestaIniciada)
                {
                    finalIniciado = true;
                    StartCoroutine(LlegadaAlTemplo());
                }
                else if (Time.time >= proximoAvisoPuerta)
                {
                    proximoAvisoPuerta = Time.time + 6f;
                    int faltan = Total - entregadas;
                    AvisoPantalla.Mostrar("AUN NO", faltan == 1 ? "Falta 1 persona por recibir su joya" : $"Faltan {faltan} personas por recibir su joya", 2.5f, 48f, 32f, new Color(1f, 0.85f, 0.4f));
                }
            }

            // Flecha sobre la puerta
            if (flechaPuerta != null && flechaPuerta.gameObject.activeSelf)
            {
                flechaPuerta.position = posFlecha + new Vector3(0f, Mathf.Abs(Mathf.Sin(Time.time * 4f)) * 0.35f, 0f);
            }
        }

        private IEnumerator Entregar(HabitanteJoya h)
        {
            entregando = true;
            if (control != null) control.BloquearControl(true, 0f);

            // La joya sale del minero y vuela en arco hasta el habitante
            int tipo = entregadas;
            Sprite[] cuadros = CuadrosJoya(tipo);
            GameObject joya = new GameObject("Joya_Entregada");
            var sr = joya.AddComponent<SpriteRenderer>();
            sr.sprite = cuadros.Length > 0 ? cuadros[0] : null;
            sr.sortingLayerName = Constantes.SortingLayers.PrimerPlano;
            sr.sortingOrder = 6;

            Vector3 desde = jugador.position + new Vector3(0f, 1.6f, 0f);
            Vector3 hasta = h.PuntoJoya;
            float dur = 0.55f;
            float t = 0f;
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySalto();
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / dur);
                Vector3 p = Vector3.Lerp(desde, hasta, k);
                p.y += Mathf.Sin(k * Mathf.PI) * 1.6f;
                joya.transform.position = p;
                joya.transform.localScale = Vector3.one * Mathf.Lerp(0.8f, 1.3f, k);
                if (cuadros.Length > 0) sr.sprite = cuadros[Mathf.FloorToInt(t * 12f) % cuadros.Length];
                yield return null;
            }

            // ¡La recibe!
            h.Recibir();
            entregadas++;
            ActualizarContador(true);
            if (AudioManager.Instance != null) AudioManager.Instance.PlayRecolectarGema();
            if (efectos != null)
            {
                efectos.ChispasEntrega(hasta);
                efectos.Corazon(hasta + new Vector3(0f, 0.2f, 0f));
            }
            CrearFrase(h.Frase, hasta + new Vector3(0f, 0.9f, 0f));

            // La joya brilla un momento sobre su cabeza y desaparece
            t = 0f;
            while (t < 0.45f)
            {
                t += Time.deltaTime;
                float k = t / 0.45f;
                joya.transform.localScale = Vector3.one * Mathf.Lerp(1.3f, 0f, k * k);
                joya.transform.position = hasta + new Vector3(0f, -k * 0.8f, 0f);
                if (cuadros.Length > 0) sr.sprite = cuadros[Mathf.FloorToInt(t * 12f) % cuadros.Length];
                yield return null;
            }
            Destroy(joya);

            if (control != null) control.BloquearControl(false);
            entregando = false;

            if (entregadas >= Total && !fiestaIniciada) StartCoroutine(Fiesta());
        }

        private Sprite[] CuadrosJoya(int indice)
        {
            if (cuadrosJoyas == null || cuadrosJoyas.Length == 0) return new Sprite[0];
            int porJoya = Mathf.Max(1, cuadrosPorJoya);
            int tipos = Mathf.Max(1, cuadrosJoyas.Length / porJoya);
            int inicio = (indice % tipos) * porJoya;
            var r = new Sprite[Mathf.Min(porJoya, cuadrosJoyas.Length - inicio)];
            for (int i = 0; i < r.Length; i++) r[i] = cuadrosJoyas[inicio + i];
            return r;
        }

        private void CrearFrase(string frase, Vector3 pos)
        {
            if (string.IsNullOrEmpty(frase)) return;
            GameObject go = new GameObject("Frase");
            go.transform.position = pos;
            var tmp = go.AddComponent<TextMeshPro>();
            if (fuenteFrases != null) tmp.font = fuenteFrases;
            tmp.text = frase;
            tmp.fontSize = 4.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.color = new Color(1f, 0.95f, 0.8f);
            tmp.outlineWidth = 0.25f;
            tmp.outlineColor = new Color32(40, 20, 10, 255);
            tmp.sortingLayerID = SortingLayer.NameToID(Constantes.SortingLayers.PrimerPlano);
            tmp.sortingOrder = 10;
            tmp.rectTransform.sizeDelta = new Vector2(8f, 1.2f);
            go.AddComponent<TextoFlotante>().Configurar(2f, 0.9f); // toma el color en Awake (ya asignado), sube y se destruye sola
        }

        private void ActualizarContador(bool animar)
        {
            if (textoContador != null) textoContador.text = $"{entregadas} / {Total}";
            if (animar && cajaContador != null) StartCoroutine(PulsoContador());
        }

        private IEnumerator PulsoContador()
        {
            float t = 0f;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                float k = t / 0.35f;
                float e = 1f + Mathf.Sin(k * Mathf.PI) * 0.18f;
                cajaContador.localScale = new Vector3(e, e, 1f);
                yield return null;
            }
            cajaContador.localScale = Vector3.one;
        }

        // ================================================================== FIESTA Y FINAL

        private IEnumerator Fiesta()
        {
            fiestaIniciada = true;
            yield return new WaitForSeconds(0.6f);

            if (habitantes != null) foreach (var h in habitantes) if (h != null) h.Fiesta();
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre("Aplausos_Pueblo");
            StartCoroutine(RepicarCampanas());
            if (efectos != null)
            {
                efectos.IniciarCohetes();
                efectos.IniciarConfeti(camara);
            }
            AvisoPantalla.Mostrar("¡VIVA LA VALENCIANA!", "Todos tienen su parte del tesoro. Ve al Templo de San Cayetano", 4f, 52f, 32f, new Color(1f, 0.85f, 0.4f));
            if (flechaPuerta != null) flechaPuerta.gameObject.SetActive(true);
        }

        private IEnumerator RepicarCampanas()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre("Campanas_Templo");
            if (campanas != null) foreach (var c in campanas) if (c != null) c.Play();
            yield return new WaitForSeconds(5f);
            if (campanas != null) foreach (var c in campanas) if (c != null) c.Stop(0);
        }

        private IEnumerator LlegadaAlTemplo()
        {
            if (control != null) control.BloquearControl(true, 0f);
            if (animJugador != null) animJugador.PlayWinAnimation();
            if (flechaPuerta != null) flechaPuerta.gameObject.SetActive(false);
            StartCoroutine(RepicarCampanas());

            float t = 0f;
            float orthoDesde = camara != null ? camara.orthographicSize : orthoInicial;
            while (t < 1.5f)
            {
                t += Time.deltaTime;
                if (camara != null) camara.orthographicSize = Mathf.Lerp(orthoDesde, orthoFinal, Mathf.SmoothStep(0f, 1f, t / 1.5f));
                yield return null;
            }

            yield return new WaitForSeconds(0.6f);
            if (grupoContador != null) grupoContador.alpha = 0f;
            if (panelFinal != null) panelFinal.Mostrar();
        }

        // ================================================================== CÁMARA

        private void LateUpdate()
        {
            if (camara == null || jugador == null) return;

            Vector3 objetivo = PosicionCamaraObjetivo();
            camara.transform.position = Vector3.SmoothDamp(camara.transform.position, objetivo, ref velocidadCam, 0.25f);

            if (capasFondo != null && capasInicio != null)
            {
                Vector3 delta = camara.transform.position - camInicio;
                for (int i = 0; i < capasFondo.Length; i++)
                {
                    if (capasFondo[i] == null) continue;
                    float f = (seguimientoCapas != null && i < seguimientoCapas.Length) ? seguimientoCapas[i] : 0.5f;
                    capasFondo[i].position = capasInicio[i] + new Vector3(delta.x * f, delta.y * f, 0f);
                }
            }
        }

        private Vector3 PosicionCamaraObjetivo()
        {
            float ortho = camara != null ? camara.orthographicSize : orthoInicial;
            float aspecto = camara != null ? camara.aspect : 16f / 9f;
            float medioAncho = ortho * aspecto;
            float x = jugador != null ? jugador.position.x + 1f : 0f;
            x = Mathf.Clamp(x, limiteIzquierdo + medioAncho, Mathf.Max(limiteIzquierdo + medioAncho, limiteDerecho - medioAncho));
            float y = bordeInferiorVista + ortho;
            return new Vector3(x, y, -10f);
        }
    }
}
