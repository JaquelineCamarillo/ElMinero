using UnityEngine;

namespace ElMinero.Utilidades
{
    /// <summary>
    /// Constantes globales del juego: Tags, Capas (Layers), Sorting Layers y claves de PlayerPrefs.
    /// </summary>
    public static class Constantes
    {
        // ====================================================================
        // TAGS
        // ====================================================================
        public static class Tags
        {
            public const string Player = "Player";
            public const string Hazard = "Hazard";
            public const string Collectible = "Collectible";
            public const string Checkpoint = "Checkpoint";
            public const string Exit = "Exit";
        }

        // ====================================================================
        // LAYERS (Nombres y Máscaras)
        // ====================================================================
        public static class Layers
        {
            public const string Default = "Default";
            public const string UI = "UI";
            public const string Suelo = "Suelo";
            public const string Jugador = "Jugador";
            public const string Peligro = "Peligro";
            public const string Coleccionable = "Coleccionable";
            public const string Enemigo = "Enemigo";

            public static int MaskSuelo => LayerMask.GetMask(Suelo);
            public static int MaskJugador => LayerMask.GetMask(Jugador);
            public static int MaskPeligro => LayerMask.GetMask(Peligro);
            public static int MaskColeccionable => LayerMask.GetMask(Coleccionable);
            public static int MaskEnemigo => LayerMask.GetMask(Enemigo);
        }

        // ====================================================================
        // SORTING LAYERS
        // ====================================================================
        public static class SortingLayers
        {
            public const string Fondo = "Fondo";
            public const string FondoMedio = "FondoMedio";
            public const string Escenario = "Escenario";
            public const string Objetos = "Objetos";
            public const string Jugador = "Jugador";
            public const string PrimerPlano = "Primer plano";
            public const string UI = "UI";
        }

        // ====================================================================
        // ESCENAS
        // ====================================================================
        public static class Escenas
        {
            public const string MenuPrincipal = "MenuPrincipal";
            public const string Nivel1 = "Nivel1_MinaRayas";
            public const string Nivel2 = "Nivel2_ElNopal";
            public const string Nivel3 = "Nivel3_LaValenciana";
            public const string Victoria = "Victoria";
            public const string Derrota = "Derrota";
            public const string Splash = "Splash";
            public const string Intro = "Intro";
            public const string Final = "Final";
            public const string Creditos = "Creditos";
        }

        // ====================================================================
        // PREFS Y CLAVES DE GUARDADO
        // ====================================================================
        public static class Prefs
        {
            public const string VolumenMaster = "VolumenMaster";
            public const string VolumenMusica = "VolumenMusica";
            public const string VolumenSFX = "VolumenSFX";
            public const string PantallaCompleta = "PantallaCompleta";
            public const string CalidadGrafica = "CalidadGrafica";
            public const string NombreJugador = "NombreJugador";
            public const string NombrePorDefecto = "Minero";
            public const string RecordPuntaje = "RecordPuntaje";
            public const string MejorPuntaje = "MejorPuntaje";
            public const string NivelAlcanzado = "NivelAlcanzado";
        }
    }
}
