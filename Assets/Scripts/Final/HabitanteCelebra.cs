using UnityEngine;
using ElMinero.Utilidades;
using ElMinero.Managers;

namespace ElMinero.Final
{
    /// <summary>
    /// Habitante de La Valenciana en el atrio del templo. Está en idle (cuadros 0–1) y,
    /// cuando el minero se acerca a menos de 6 u, saluda o brinca en loop (cuadros 2–3).
    /// El primero que lo ve dispara los aplausos del pueblo (una sola vez).
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HabitanteCelebra : MonoBehaviour
    {
        [SerializeField] private Sprite[] cuadros;
        [SerializeField] private float distancia = 6f;

        private static bool aplausosSonaron;
        private SpriteFrameAnimator anim;
        private Transform jugador;
        private bool celebrando;

        public void Configurar(Sprite[] hoja)
        {
            cuadros = hoja;
        }

        private void Awake()
        {
            aplausosSonaron = false;
            anim = GetComponent<SpriteFrameAnimator>();
            if (anim == null) anim = gameObject.AddComponent<SpriteFrameAnimator>();
        }

        private void Start()
        {
            GameObject p = GameObject.FindGameObjectWithTag(Constantes.Tags.Player);
            if (p != null) jugador = p.transform;
            anim.Play(Sub(0, 2), 3f, true);
        }

        private void Update()
        {
            if (celebrando || jugador == null) return;
            if (Mathf.Abs(jugador.position.x - transform.position.x) < distancia)
            {
                celebrando = true;
                anim.Play(Sub(2, 2), 5f, true);
                if (!aplausosSonaron)
                {
                    aplausosSonaron = true;
                    if (AudioManager.Instance != null) AudioManager.Instance.PlaySFXNombre("Aplausos_Pueblo");
                }
            }
        }

        private Sprite[] Sub(int inicio, int cantidad)
        {
            if (cuadros == null || cuadros.Length == 0) return new Sprite[0];
            var r = new Sprite[cantidad];
            for (int i = 0; i < cantidad; i++) r[i] = cuadros[Mathf.Min(inicio + i, cuadros.Length - 1)];
            return r;
        }
    }
}
