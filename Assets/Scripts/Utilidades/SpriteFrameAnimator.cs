using UnityEngine;
using UnityEngine.UI;

namespace ElMinero.Utilidades
{
    /// <summary>
    /// Animador ligero por cuadros para SpriteRenderer o Image.
    /// Se usa en elementos decorativos y efectos que solo necesitan una secuencia de sprites
    /// (llamas del incendio, rueda del castillete, campanas, palomas, habitantes, ondas, polvo...).
    /// </summary>
    public class SpriteFrameAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] frames;
        [SerializeField] private float fps = 10f;
        [SerializeField] private bool loop = true;
        [SerializeField] private bool playOnStart = true;
        [SerializeField] private bool inicioAleatorio = false;
        [SerializeField] private bool tiempoSinEscala = false;

        private SpriteRenderer sr;
        private Image img;
        private float tiempo;
        private bool reproduciendo;
        private bool terminado;

        public bool Reproduciendo => reproduciendo;
        public bool Terminado => terminado;
        public Sprite[] Frames => frames;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            img = GetComponent<Image>();
        }

        private void Start()
        {
            if (playOnStart && !reproduciendo) Play();
        }

        public void Configurar(Sprite[] nuevosFrames, float nuevosFps, bool enLoop, bool reproducirAlIniciar = true, bool aleatorio = false)
        {
            frames = nuevosFrames;
            fps = nuevosFps;
            loop = enLoop;
            playOnStart = reproducirAlIniciar;
            inicioAleatorio = aleatorio;
        }

        public void UsarTiempoSinEscala(bool valor)
        {
            tiempoSinEscala = valor;
        }

        public void Play()
        {
            Play(frames, fps, loop);
        }

        public void Play(Sprite[] nuevosFrames, float nuevosFps, bool enLoop)
        {
            if (sr == null) sr = GetComponent<SpriteRenderer>();
            if (img == null) img = GetComponent<Image>();

            frames = nuevosFrames;
            fps = Mathf.Max(0.01f, nuevosFps);
            loop = enLoop;
            terminado = false;
            reproduciendo = frames != null && frames.Length > 0;
            tiempo = (inicioAleatorio && frames != null && frames.Length > 0) ? Random.Range(0f, frames.Length) : 0f;
            Aplicar();
        }

        public void Stop(int cuadroFinal = -1)
        {
            reproduciendo = false;
            if (cuadroFinal >= 0) MostrarCuadro(cuadroFinal);
        }

        public void MostrarCuadro(int indice)
        {
            if (frames == null || frames.Length == 0) return;
            indice = Mathf.Clamp(indice, 0, frames.Length - 1);
            Asignar(frames[indice]);
        }

        private void Update()
        {
            if (!reproduciendo || frames == null || frames.Length == 0) return;

            tiempo += (tiempoSinEscala ? Time.unscaledDeltaTime : Time.deltaTime) * fps;
            if (!loop && tiempo >= frames.Length)
            {
                tiempo = frames.Length - 1;
                reproduciendo = false;
                terminado = true;
            }
            Aplicar();
        }

        private void Aplicar()
        {
            if (frames == null || frames.Length == 0) return;
            int indice = Mathf.FloorToInt(tiempo);
            indice = loop ? ((indice % frames.Length) + frames.Length) % frames.Length : Mathf.Clamp(indice, 0, frames.Length - 1);
            Asignar(frames[indice]);
        }

        private void Asignar(Sprite s)
        {
            if (sr != null) sr.sprite = s;
            else if (img != null) img.sprite = s;
        }
    }
}
