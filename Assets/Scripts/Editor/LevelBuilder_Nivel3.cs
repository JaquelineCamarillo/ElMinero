using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using ElMinero.Coleccionables;
using ElMinero.Jefe;
using ElMinero.Managers;
using ElMinero.Niveles;
using ElMinero.Obstaculos;
using ElMinero.Plataformas;
using ElMinero.UI;
using ElMinero.Utilidades;

namespace ElMinero.Editor
{
    /// <summary>
    /// Nivel 3 (La Valenciana) construido EXACTAMENTE desde Assets/Settings/Niveles/Mapa_Nivel3.txt.
    ///
    /// Formato: 216 columnas × 50 filas; 1 carácter = 1 tile = 1 unidad. La primera línea es y = 49.
    /// La celda (x, y) ocupa [x, x+1] × [y, y+1]. Cada elemento va en (x + 0.5, y) con pivot abajo,
    /// salvo que se indique otra cosa. La leyenda completa está en el prompt (sección B3.2).
    /// </summary>
    public static partial class LevelBuilder
    {
        private const string RutaMapaNivel3 = "Assets/Settings/Niveles/Mapa_Nivel3.txt";

        // ====================================================================
        // LECTURA DEL MAPA
        // ====================================================================
        public class MapaNivel
        {
            public int Ancho;
            public int Alto;
            public char[,] Celdas; // [x, y] con y = 0 abajo

            public char C(int x, int y)
            {
                if (x < 0 || y < 0 || x >= Ancho || y >= Alto) return '#';
                return Celdas[x, y];
            }

            public bool Roca(int x, int y) => C(x, y) == '#';
            public bool Aire(int x, int y) => C(x, y) != '#';

            /// <summary>Altura de la superficie del piso bajo la columna x, buscando desde yDesde hacia abajo.</summary>
            public int SueloBajo(int x, int yDesde)
            {
                for (int y = yDesde; y >= 0; y--)
                {
                    if (Roca(x, y)) return y + 1;
                }
                return 0;
            }

            /// <summary>Altura del techo (primera fila de roca) sobre la columna x, desde yDesde hacia arriba.</summary>
            public int TechoSobre(int x, int yDesde)
            {
                for (int y = yDesde; y < Alto; y++)
                {
                    if (Roca(x, y)) return y;
                }
                return Alto;
            }

            public List<Vector2Int> Buscar(char c)
            {
                var r = new List<Vector2Int>();
                for (int y = 0; y < Alto; y++)
                    for (int x = 0; x < Ancho; x++)
                        if (Celdas[x, y] == c) r.Add(new Vector2Int(x, y));
                return r;
            }

            /// <summary>Tramos horizontales continuos de un carácter: (x inicial, y, largo).</summary>
            public List<Vector3Int> Tramos(char c)
            {
                var r = new List<Vector3Int>();
                for (int y = 0; y < Alto; y++)
                {
                    int x = 0;
                    while (x < Ancho)
                    {
                        if (Celdas[x, y] != c) { x++; continue; }
                        int inicio = x;
                        while (x < Ancho && Celdas[x, y] == c) x++;
                        r.Add(new Vector3Int(inicio, y, x - inicio));
                    }
                }
                return r;
            }
        }

        public static MapaNivel LeerMapa(TextAsset texto)
        {
            var lineas = new List<string>();
            foreach (string l in texto.text.Replace("\r", "").Split('\n'))
            {
                if (l.Length > 0) lineas.Add(l);
            }

            int alto = lineas.Count;
            int ancho = 0;
            foreach (var l in lineas) ancho = Mathf.Max(ancho, l.Length);

            var mapa = new MapaNivel { Ancho = ancho, Alto = alto, Celdas = new char[ancho, alto] };
            for (int fila = 0; fila < alto; fila++)
            {
                int y = alto - 1 - fila; // la primera línea es la fila de arriba
                string l = lineas[fila];
                for (int x = 0; x < ancho; x++)
                {
                    mapa.Celdas[x, y] = x < l.Length ? l[x] : '#';
                }
            }
            return mapa;
        }

        // ====================================================================
        // DATOS DE CADA NIVEL CONSTRUIDO DESDE MAPA
        // ====================================================================
        public class DatosNivelMapa
        {
            public int Numero;
            public string RutaMapa;
            public string Escena;
            public string Tileset;            // "ElNopal", "LaValenciana"
            public string RutaConfig;
            public string MineralDistintivo;  // nombre del prefab de mineral (letra R)
            public Color ColorFondo;
            public Color ColorLuz;
            public float IntensidadGlobal;
            public Color TinteFondo;
            public bool SalidaSuperficie;
            public float IntervaloRocas;
        }

        public static readonly DatosNivelMapa DatosNivel2 = new DatosNivelMapa
        {
            Numero = 2,
            RutaMapa = "Assets/Settings/Niveles/Mapa_Nivel2.txt",
            Escena = "Assets/Scenes/Nivel2_ElNopal.unity",
            Tileset = "ElNopal",
            RutaConfig = "Assets/Settings/Niveles/LevelConfig_Nivel2.asset",
            MineralDistintivo = "Mineral_PepitaOro",
            ColorFondo = new Color(0.16f, 0.10f, 0.05f),   // #2A1A0E
            ColorLuz = new Color(1.0f, 0.82f, 0.48f),      // #FFD27A
            IntensidadGlobal = 0.7f,
            TinteFondo = new Color(0.80f, 0.76f, 0.70f),
            SalidaSuperficie = false,
            IntervaloRocas = 2.4f
        };

        public static readonly DatosNivelMapa DatosNivel3 = new DatosNivelMapa
        {
            Numero = 3,
            RutaMapa = RutaMapaNivel3,
            Escena = "Assets/Scenes/Nivel3_LaValenciana.unity",
            Tileset = "LaValenciana",
            RutaConfig = "Assets/Settings/Niveles/LevelConfig_Nivel3.asset",
            MineralDistintivo = "Mineral_Pirargirita",
            ColorFondo = new Color(0.16f, 0.04f, 0.04f),   // #2A0A0A
            ColorLuz = new Color(1.0f, 0.69f, 0.38f),      // #FFB060
            IntensidadGlobal = 0.65f,
            TinteFondo = new Color(0.78f, 0.74f, 0.74f),
            SalidaSuperficie = true,
            IntervaloRocas = 2.6f
        };

        // ====================================================================
        // NIVEL 2 — EL NOPAL y NIVEL 3 — LA VALENCIANA
        // ====================================================================
        public static void ConstruirNivel2_ElNopal()
        {
            ConstruirNivelDesdeArchivo(DatosNivel2);
        }

        public static void ConstruirNivel3_LaValenciana()
        {
            ConstruirNivelDesdeArchivo(DatosNivel3);
        }

        private static void ConstruirNivelDesdeArchivo(DatosNivelMapa d)
        {
            TextAsset mapa = AssetDatabase.LoadAssetAtPath<TextAsset>(d.RutaMapa);
            if (mapa == null)
            {
                Debug.LogError("[LevelBuilder] No se encontró " + d.RutaMapa + ". El nivel " + d.Numero + " no se construyó.");
                return;
            }
            ConstruirNivelDesdeMapa(mapa, d);
        }

        /// <summary>Compatibilidad: construye el nivel 3 desde un TextAsset.</summary>
        public static void ConstruirNivel3DesdeMapa(TextAsset mapaTxt)
        {
            ConstruirNivelDesdeMapa(mapaTxt, DatosNivel3);
        }

        public static void ConstruirNivelDesdeMapa(TextAsset mapaTxt, DatosNivelMapa d)
        {
            MapaNivel m = LeerMapa(mapaTxt);
            Debug.Log($"[LevelBuilder] Mapa del nivel {d.Numero}: {m.Ancho} × {m.Alto}");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            CrearIluminacionNivel(d.ColorFondo, d.IntensidadGlobal);

            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 8;

            // ---------------------------------------------------------------- Tilemaps
            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();
            Tilemap tmSuelo = CrearTilemap(gridObj, "Tilemap_Suelo", Constantes.SortingLayers.Escenario, layerSuelo, true);
            Tilemap tmFondo = CrearTilemap(gridObj, "Tilemap_Fondo", Constantes.SortingLayers.FondoMedio, 0, false);
            tmFondo.GetComponent<TilemapRenderer>().sortingOrder = 10;
            tmFondo.color = d.TinteFondo;

            PintarTerrenoMapa(m, tmSuelo, tmFondo, d.Tileset);
            FinalizarColisionesTilemap(tmSuelo);

            // ---------------------------------------------------------------- Plataformas (cada 3 caracteres = 1)
            foreach (var t in m.Tramos('='))
                for (int i = 0; i + 3 <= t.z; i += 3)
                    CrearPlataformaFija(new Vector3(t.x + i + 1.5f, t.y + 0.75f, 0f), layerSuelo);
            foreach (var t in m.Tramos('~'))
                for (int i = 0; i + 3 <= t.z; i += 3)
                    CrearPlataformaQuebradiza(new Vector3(t.x + i + 1.5f, t.y + 0.75f, 0f), layerSuelo);
            // M: plataforma móvil que recorre su tramo de un extremo al otro
            foreach (var t in m.Tramos('M'))
            {
                float y = t.y + 0.75f;
                CrearPlataformaMovil(new Vector3(t.x + 1.5f, y, 0f), new Vector3(t.x + t.z - 1.5f, y, 0f), 2.2f, layerSuelo);
            }

            // ---------------------------------------------------------------- Zonas letales
            foreach (var t in m.Tramos('x'))
                CrearHuecoLetal(new Vector3(t.x + t.z * 0.5f, t.y + 0.6f, 0f), new Vector2(t.z, 1.2f));
            CrearHuecoLetal(new Vector3(m.Ancho * 0.5f, -1.5f, 0f), new Vector2(m.Ancho + 20f, 2f));

            // ---------------------------------------------------------------- Coleccionables
            int totalVetas = 0, totalGemas = 0;
            foreach (var p in m.Buscar('v')) { CrearVetaPlata(new Vector3(p.x + 0.5f, p.y + 1f, 0f)); totalVetas++; }
            foreach (var p in m.Buscar('g')) { CrearGemaAmatista(new Vector3(p.x + 0.5f, p.y + 1f, 0f)); totalGemas++; }
            foreach (var p in m.Buscar('R')) CrearMineralDistintivo(d.MineralDistintivo, new Vector3(p.x + 0.5f, p.y + 1f, 0f));

            // ---------------------------------------------------------------- Enemigos
            foreach (var p in m.Buscar('a')) CrearAlacran(new Vector3(p.x + 0.5f, p.y, 0f), 4f);
            foreach (var p in m.Buscar('b')) CrearMurcielagoRojo(new Vector3(p.x + 0.5f, p.y + 0.5f, 0f));
            foreach (var p in m.Buscar('u')) CrearMurcielago(new Vector3(p.x + 0.5f, p.y + 0.5f, 0f), 5f);
            foreach (var p in m.Buscar('s')) CrearArana(new Vector3(p.x + 0.5f, p.y + 0.5f, 0f));

            // ---------------------------------------------------------------- Checkpoints, antorchas, derrumbes
            foreach (var p in m.Buscar('C')) CrearCheckpoint(new Vector3(p.x + 0.5f, p.y, 0f));
            foreach (var p in m.Buscar('c')) CrearCheckpointCapilla(new Vector3(p.x + 0.5f, p.y, 0f));
            foreach (var p in m.Buscar('t')) CrearAntorchaConLuz(new Vector3(p.x + 0.5f, p.y + 0.5f, 0f), d.ColorLuz);
            foreach (var p in m.Buscar('D')) CrearDerrumbeDesdeMapa(m, p);

            // ---------------------------------------------------------------- Rieles y vagonetas
            // Cada 'k' es el inicio de una vagoneta: los rieles siguen a la derecha mientras haya piso firme
            // debajo, y el trigger 'K' más cercano en la misma fila la dispara.
            foreach (var k in m.Buscar('k'))
            {
                int finRieles = k.x;
                while (finRieles + 1 < m.Ancho && m.Aire(finRieles + 1, k.y) && m.Roca(finRieles + 1, k.y - 1))
                {
                    finRieles++;
                }
                Vector2Int kTrig = new Vector2Int(k.x + 10, k.y);
                foreach (var kt in m.Buscar('K'))
                {
                    if (kt.y == k.y && kt.x > k.x && kt.x <= finRieles) { kTrig = kt; break; }
                }
                for (int x = k.x; x <= finRieles; x++) CrearRiel(new Vector3(x + 0.5f, k.y + 0.25f, 0f));
                CrearVagonetaMapa(new Vector3(k.x + 0.5f, k.y + 1f, 0f), new Vector3(finRieles + 0.5f, k.y + 1f, 0f),
                                  new Vector3(kTrig.x + 0.5f, kTrig.y + 3f, 0f));
            }

            // ---------------------------------------------------------------- Marcas del nivel 3 (si existen)
            bool hayElevador = m.Buscar('E').Count > 0 && m.Buscar('e').Count > 0;
            bool hayArena = m.Buscar('G').Count > 0 && m.Buscar('Q').Count > 0 && m.Buscar('B').Count > 0;
            Vector2Int eAbajo = hayElevador ? m.Buscar('E')[0] : new Vector2Int(-100, -100);
            Vector2Int eArriba = hayElevador ? m.Buscar('e')[0] : new Vector2Int(-100, -100);
            Vector2Int pArenaG = hayArena ? m.Buscar('G')[0] : new Vector2Int(-100, -100);
            Vector2Int pArenaQ = hayArena ? m.Buscar('Q')[0] : new Vector2Int(-100, -100);

            RockSpawner rocasTiro = null;
            var rocasArena = new List<RockSpawner>();
            foreach (var p in m.Buscar('o'))
            {
                Vector3 pos = new Vector3(p.x + 0.5f, p.y + 0.5f, 0f);
                if (hayElevador && Mathf.Abs(p.x - eAbajo.x) <= 2 && p.y > eArriba.y)
                {
                    rocasTiro = CrearRockSpawner(pos, 3.5f);
                    rocasTiro.name = "RockSpawner_Tiro";
                }
                else if (hayArena && p.x > pArenaG.x && p.x < pArenaQ.x)
                {
                    var r = CrearRockSpawner(pos, 3f);
                    r.name = "RockSpawner_Arena";
                    r.Configurar(true, null);
                    rocasArena.Add(r);
                }
                else
                {
                    CrearRockSpawner(pos, d.IntervaloRocas);
                }
            }

            float retrasoFuego = 0.3f;
            foreach (var p in m.Buscar('f'))
            {
                CrearGeneradorBolasFuego(new Vector3(p.x + 0.5f, p.y + 0.5f, 0f), 3.0f + (p.x % 3) * 0.3f, retrasoFuego);
                retrasoFuego += 0.45f;
            }

            if (hayElevador)
            {
                Vector2Int h = m.Buscar('h').Count > 0 ? m.Buscar('h')[0] : new Vector2Int(eArriba.x, eArriba.y + 11);
                GameObject elevador = CrearElevadorTiro(
                    new Vector3(eAbajo.x + 0.5f, eAbajo.y, 0f),
                    new Vector3(eArriba.x + 0.5f, eArriba.y, 0f),
                    new Vector3(h.x + 0.5f, eArriba.y, 0f),
                    rocasTiro);
                if (rocasTiro != null) rocasTiro.Configurar(true, elevador.transform);
            }

            // ---------------------------------------------------------------- Decoración
            if (d.Numero == 3) DecorarNivel3(m);
            DecorarVetasFondo(m, d.Numero == 3 ? "Deco_VetaPlataRoja" : "Deco_VetaOro", 1780 + d.Numero);

            // ---------------------------------------------------------------- Salida
            Vector2Int sal = m.Buscar('S').Count > 0 ? m.Buscar('S')[0] : new Vector2Int(m.Ancho - 4, 4);
            CrearSalida(new Vector3(sal.x + 0.5f, sal.y, 0f), d.SalidaSuperficie);

            // ---------------------------------------------------------------- Jugador, cámara y HUD
            Vector2Int ini = m.Buscar('P').Count > 0 ? m.Buscar('P')[0] : new Vector2Int(4, 4);
            GameObject jugador = CrearJugador(new Vector3(ini.x + 0.5f, ini.y, 0f), layerSuelo);
            ConfigurarCamaraNivel(jugador, d.ColorFondo, new Vector2(0f, 0f), new Vector2(m.Ancho, m.Alto));

            Collider2D limitesNivel = CrearLimitesCamara("Limites_Camara_Nivel", new Rect(0f, 0f, m.Ancho, m.Alto));
            Collider2D limitesArena = hayArena
                ? CrearLimitesCamara("Limites_Camara_Arena", new Rect(pArenaG.x, pArenaG.y, pArenaQ.x + 1 - pArenaG.x, 12f))
                : null;
            CinemachineConfiner2D confiner = null;
            var vcam = Object.FindAnyObjectByType<CinemachineCamera>();
            if (vcam != null)
            {
                confiner = vcam.gameObject.AddComponent<CinemachineConfiner2D>();
                confiner.BoundingShape2D = limitesNivel;
            }

            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>(d.RutaConfig);
            ConfigurarManagersYHUD(jugador, config, totalVetas, totalGemas);

            GameObject canvasHud = GameObject.Find("Canvas_HUD");
            if (canvasHud != null)
            {
                // Se colocan justo después de la barra superior del HUD para que los paneles
                // (pausa, resultados, game over) siempre queden ENCIMA de ellos.
                GameObject aviso = CrearAvisoPantalla(canvasHud.transform);
                aviso.transform.SetSiblingIndex(1);
                if (hayArena)
                {
                    GameObject barra = CrearBarraJefe(canvasHud.transform);
                    barra.transform.SetSiblingIndex(1);
                }
            }

            // Temblor periódico
            CrearMineTremor();
            var tremor = Object.FindAnyObjectByType<MineTremor>();
            if (tremor != null)
            {
                var soT = new SerializedObject(tremor);
                var pMin = soT.FindProperty("minInterval");
                var pMax = soT.FindProperty("maxInterval");
                if (pMin != null) pMin.floatValue = d.Numero == 3 ? 12f : 14f;
                if (pMax != null) pMax.floatValue = d.Numero == 3 ? 16f : 18f;
                soT.ApplyModifiedProperties();
            }

            // ---------------------------------------------------------------- Arena del jefe
            if (hayArena)
            {
                CrearArenaColoso(m, confiner, limitesNivel, limitesArena, rocasArena.ToArray(), layerSuelo);
            }

            // ---------------------------------------------------------------- Incendio final
            if (m.Buscar('X').Count > 0)
            {
                Vector2Int x4 = m.Buscar('X')[0];
                Vector2Int c4 = new Vector2Int(x4.x - 2, x4.y);
                foreach (var c in m.Buscar('C')) if (c.x < x4.x && c.x > c4.x - 20) c4 = c;
                CrearIncendioFinal(new Vector3(x4.x + 0.5f, x4.y + 3f, 0f), new Vector3(c4.x + 0.5f, c4.y, 0f), sal.x - 3f);
            }

            EditorSceneManager.SaveScene(scene, d.Escena);
            Debug.Log($"[LevelBuilder] Nivel {d.Numero} construido desde el mapa: {totalVetas} vetas, {totalGemas} gemas.");
        }

        // ====================================================================
        // TERRENO (autotile por vecinos) Y FONDO
        // ====================================================================
        private static void PintarTerrenoMapa(MapaNivel m, Tilemap tmSuelo, Tilemap tmFondo, string tileset)
        {
            var tiles = EditorSpriteUtil.TilesPorIndice("Assets/Tiles/" + tileset);
            System.Func<int, Tile> T = i => tiles.TryGetValue(i, out Tile t) ? t : CargarTile(tileset, i);

            var rng = new System.Random(1817);
            for (int y = 0; y < m.Alto; y++)
            {
                for (int x = 0; x < m.Ancho; x++)
                {
                    var celda = new Vector3Int(x, y, 0);
                    if (m.Roca(x, y))
                    {
                        bool arriba = m.Aire(x, y + 1);
                        bool abajo = m.Aire(x, y - 1);
                        bool izq = m.Aire(x - 1, y);
                        bool der = m.Aire(x + 1, y);
                        int indice = IndiceAutotile(arriba, abajo, izq, der, rng);
                        tmSuelo.SetTile(celda, T(indice));
                    }
                    else
                    {
                        int fondo = rng.NextDouble() < 0.08 ? 15 : (rng.NextDouble() < 0.5 ? 19 : 20);
                        tmFondo.SetTile(celda, T(fondo));
                    }
                }
            }
        }

        private static int IndiceAutotile(bool arriba, bool abajo, bool izq, bool der, System.Random rng)
        {
            if (arriba && abajo && izq && der) return 6;
            if (arriba && abajo && izq) return 3;
            if (arriba && abajo && der) return 5;
            if (arriba && abajo) return 4;
            if (arriba && izq && der) return 6;
            if (arriba && izq) return 0;
            if (arriba && der) return 2;
            if (arriba) return 1;
            if (abajo && izq && der) return 17;
            if (abajo && izq) return 16;
            if (abajo && der) return 18;
            if (abajo) return 17;
            if (izq && der) return 9;
            if (izq) return 8;
            if (der) return 10;
            return rng.NextDouble() < 0.1 ? 11 : 9;
        }

        // ====================================================================
        // ELEMENTOS DEL NIVEL 3
        // ====================================================================
        private static void CrearRiel(Vector3 pos)
        {
            GameObject riel = new GameObject("Riel");
            riel.transform.position = pos;
            var sr = riel.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Riel.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
            sr.sortingOrder = 3;
        }

        private static void CrearVagonetaMapa(Vector3 inicio, Vector3 fin, Vector3 posTrigger)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstaculos/Vagoneta_Desbocada.prefab");
            RunawayCart cart = null;
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = inicio;
                cart = inst.GetComponent<RunawayCart>();
                if (cart != null)
                {
                    var so = new SerializedObject(cart);
                    so.FindProperty("startPoint").vector3Value = inicio;
                    so.FindProperty("endPoint").vector3Value = fin;
                    so.ApplyModifiedProperties();
                }
            }
            else
            {
                Debug.LogWarning("[LevelBuilder] No se encontró el prefab Vagoneta_Desbocada.");
            }

            // Trigger vertical de 1 × 6: el jugador está 11 u adelante y la vagoneta sale fuera de cámara
            GameObject trig = new GameObject("Trigger_Vagoneta");
            trig.transform.position = posTrigger;
            var col = trig.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 6f);
            var tc = trig.AddComponent<RunawayCartTrigger>();
            if (cart != null) tc.SetTargetCart(cart);
        }

        private static void CrearDerrumbeDesdeMapa(MapaNivel m, Vector2Int p)
        {
            // El bloque cae 4 u atrás del trigger y se apoya en el piso real
            int xCaida = p.x - 4;
            int piso = m.SueloBajo(xCaida, p.y + 1);
            int techo = m.TechoSobre(xCaida, piso);
            Vector3 destino = new Vector3(xCaida + 0.5f, piso, 0f);

            GameObject bloque = new GameObject("BloqueDerrumbe");
            bloque.tag = Constantes.Tags.Hazard;
            bloque.transform.position = new Vector3(destino.x, techo - 1.6f, 0f);
            var sr = bloque.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
            sr.sortingOrder = 4;
            var colB = bloque.AddComponent<BoxCollider2D>();
            colB.size = new Vector2(1.9f, 3f);
            var compBloque = bloque.AddComponent<BloqueDerrumbe>();

            GameObject zona = new GameObject("ZonaDerrumbe_Sorpresa");
            zona.transform.position = new Vector3(p.x + 0.5f, p.y + 2f, 0f);
            var colZ = zona.AddComponent<BoxCollider2D>();
            colZ.isTrigger = true;
            colZ.size = new Vector2(1.5f, 4f);
            var imp = zona.AddComponent<CinemachineImpulseSource>();
            var cave = zona.AddComponent<CaveInZone>();
            var so = new SerializedObject(cave);
            so.FindProperty("blockObject").objectReferenceValue = compBloque;
            so.FindProperty("blockDropPosition").vector3Value = destino;
            so.FindProperty("impulseSource").objectReferenceValue = imp;
            so.ApplyModifiedProperties();
        }

        private static void CrearGeneradorBolasFuego(Vector3 pos, float intervalo, float retraso)
        {
            GameObject spawnerObj = new GameObject("BolaFuego_Spawner");
            spawnerObj.transform.position = pos;

            // Humo de aviso en el techo
            GameObject humo = new GameObject("Aviso_Humo");
            humo.transform.SetParent(spawnerObj.transform, false);
            humo.transform.localPosition = new Vector3(0f, 0.1f, 0f);
            var srHumo = humo.AddComponent<SpriteRenderer>();
            Sprite[] cuadrosHumo = EditorSpriteUtil.Cuadros("Assets/Sprites/Obstaculos/Humo.png");
            srHumo.sprite = cuadrosHumo.Length > 0 ? cuadrosHumo[0] : null;
            srHumo.sortingLayerName = Constantes.SortingLayers.Objetos;
            srHumo.sortingOrder = 4;
            var animHumo = humo.AddComponent<SpriteFrameAnimator>();
            animHumo.Configurar(cuadrosHumo, 8f, true, true);

            // Plantilla de la bola (desactivada; el generador arma su pool en Awake)
            int capaPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            GameObject bola = new GameObject("Bola_Fuego_Plantilla");
            bola.transform.SetParent(spawnerObj.transform, false);
            bola.tag = Constantes.Tags.Hazard;
            if (capaPeligro != -1) bola.layer = capaPeligro;
            Sprite[] cuadrosBola = EditorSpriteUtil.Cuadros("Assets/Sprites/Obstaculos/Bola_Fuego.png");
            var srBola = bola.AddComponent<SpriteRenderer>();
            srBola.sprite = cuadrosBola.Length > 0 ? cuadrosBola[0] : null;
            srBola.sortingLayerName = Constantes.SortingLayers.Objetos;
            srBola.sortingOrder = 6;
            var animBola = bola.AddComponent<SpriteFrameAnimator>();
            animBola.Configurar(cuadrosBola, 12f, true, true);
            var rb = bola.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;
            var col = bola.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.4f;
            var fb = bola.AddComponent<Fireball>();
            fb.Configurar(EditorSpriteUtil.Cuadros("Assets/Sprites/Obstaculos/Polvo.png"));
            var luz = bola.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightOuterRadius = 2.2f;
            luz.pointLightInnerRadius = 0.2f;
            luz.intensity = 1.2f;
            luz.color = new Color(1f, 0.55f, 0.2f);
            ElMineroBuilder.AsignarCapasALuz2D(luz);
            bola.SetActive(false);

            var spawner = spawnerObj.AddComponent<FireballSpawner>();
            spawner.Configurar(intervalo, retraso, fb, humo);
        }

        private static GameObject CrearElevadorTiro(Vector3 abajo, Vector3 arriba, Vector3 posCastillete, RockSpawner rocas)
        {
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);

            // Raíz: la posición es la del piso de la jaula
            GameObject root = new GameObject("Elevador_Tiro");
            root.transform.position = abajo;
            var rb = root.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;
            root.AddComponent<AudioSource>();

            // Dibujo de la jaula (48×24, pivot abajo): sin colliders en las barras
            GameObject jaula = new GameObject("Jaula");
            jaula.transform.SetParent(root.transform, false);
            jaula.transform.localPosition = new Vector3(0f, -0.25f, 0f);
            var srJaula = jaula.AddComponent<SpriteRenderer>();
            srJaula.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Elevador_Jaula.png");
            srJaula.sortingLayerName = Constantes.SortingLayers.Objetos;
            srJaula.sortingOrder = 3;

            // Piso delgado one-way (PlatformEffector2D)
            GameObject piso = new GameObject("Piso_Jaula");
            piso.transform.SetParent(root.transform, false);
            piso.layer = layerSuelo != -1 ? layerSuelo : 0;
            var colPiso = piso.AddComponent<BoxCollider2D>();
            colPiso.size = new Vector2(2.9f, 0.25f);
            colPiso.offset = new Vector2(0f, -0.125f);
            colPiso.usedByEffector = true;
            var eff = piso.AddComponent<PlatformEffector2D>();
            eff.useOneWay = true;
            eff.surfaceArc = 170f;

            // Castillete simétrico apoyado en los dos bordes del tiro (sin collider)
            GameObject castillete = new GameObject("Castillete_Tiro");
            castillete.transform.position = posCastillete;
            Sprite[] cuadrosCast = EditorSpriteUtil.Cuadros("Assets/Sprites/Entorno/Castillete_Tiro.png");
            var srCast = castillete.AddComponent<SpriteRenderer>();
            srCast.sprite = cuadrosCast.Length > 0 ? cuadrosCast[0] : null;
            srCast.sortingLayerName = Constantes.SortingLayers.Escenario;
            srCast.sortingOrder = 5;
            var animCast = castillete.AddComponent<SpriteFrameAnimator>();
            animCast.Configurar(cuadrosCast, 10f, true, false);

            GameObject centroRueda = new GameObject("Centro_Rueda");
            centroRueda.transform.SetParent(castillete.transform, false);
            centroRueda.transform.localPosition = new Vector3(0f, 5.1f, 0f);

            // Cable (sprite Tiled) de la rueda al techo de la jaula
            GameObject cable = new GameObject("Cable");
            cable.transform.position = new Vector3(abajo.x, (abajo.y + posCastillete.y + 5.1f) * 0.5f, 0f);
            var srCable = cable.AddComponent<SpriteRenderer>();
            srCable.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Cable.png");
            srCable.drawMode = SpriteDrawMode.Tiled;
            srCable.size = new Vector2(0.125f, 10f);
            srCable.sortingLayerName = Constantes.SortingLayers.Objetos;
            srCable.sortingOrder = 2;

            var elev = root.AddComponent<MineElevator>();
            elev.Configurar(abajo, arriba, 3f, animCast, centroRueda.transform, srCable, rocas);

            return root;
        }

        private static void DecorarNivel3(MapaNivel m)
        {
            // Zona de maquinaria quemada (1817): vigas con brasas en el techo y restos de malacate en el piso
            Sprite[] cuadrosViga = EditorSpriteUtil.Cuadros("Assets/Sprites/Entorno/Viga_Quemada.png");
            foreach (float xv in new[] { 59.5f, 68.5f })
            {
                int techo = m.TechoSobre(Mathf.FloorToInt(xv), 9);
                GameObject viga = new GameObject("Viga_Quemada");
                viga.transform.position = new Vector3(xv, techo, 0f);
                var sr = viga.AddComponent<SpriteRenderer>();
                sr.sprite = cuadrosViga.Length > 0 ? cuadrosViga[0] : null;
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = -2;
                var anim = viga.AddComponent<SpriteFrameAnimator>();
                anim.Configurar(cuadrosViga, 3f, true, true, true);
            }

            Sprite maquinaria = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Maquinaria_Quemada.png");
            foreach (float xm in new[] { 59f, 69f })
            {
                int piso = m.SueloBajo(Mathf.FloorToInt(xm), 12);
                GameObject maq = new GameObject("Maquinaria_Quemada");
                maq.transform.position = new Vector3(xm, piso, 0f);
                var sr = maq.AddComponent<SpriteRenderer>();
                sr.sprite = maquinaria;
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = -5;
            }

        }

        private static void DecorarVetasFondo(MapaNivel m, string sprite, int semilla)
        {
            Sprite veta = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/" + sprite + ".png");
            if (veta == null) return;
            var rng = new System.Random(semilla);
            GameObject raiz = new GameObject("[Deco_Vetas]");
            int puestas = 0;
            for (int y = 1; y < m.Alto - 1 && puestas < 40; y++)
            {
                for (int x = 1; x < m.Ancho - 1 && puestas < 40; x++)
                {
                    if (m.C(x, y) != '.' || rng.NextDouble() > 0.02) continue;
                    if (m.Roca(x, y - 1)) continue; // no justo sobre el piso
                    GameObject dd = new GameObject(sprite);
                    dd.transform.SetParent(raiz.transform);
                    dd.transform.position = new Vector3(x + 0.5f, y + 0.5f, 0f);
                    var sr = dd.AddComponent<SpriteRenderer>();
                    sr.sprite = veta;
                    sr.sortingLayerName = Constantes.SortingLayers.FondoMedio;
                    sr.sortingOrder = 12;
                    sr.color = new Color(1f, 1f, 1f, 0.85f);
                    puestas++;
                }
            }
        }

        private static Collider2D CrearLimitesCamara(string nombre, Rect r)
        {
            GameObject go = new GameObject(nombre);
            go.layer = 2; // Ignore Raycast
            var poly = go.AddComponent<PolygonCollider2D>();
            poly.isTrigger = true;
            poly.points = new[]
            {
                new Vector2(r.xMin, r.yMin), new Vector2(r.xMax, r.yMin),
                new Vector2(r.xMax, r.yMax), new Vector2(r.xMin, r.yMax)
            };
            return poly;
        }

        // ====================================================================
        // ARENA DEL COLOSO
        // ====================================================================
        private static void CrearArenaColoso(MapaNivel m, CinemachineConfiner2D confiner, Collider2D limitesNivel,
                                             Collider2D limitesArena, RockSpawner[] rocasArena, int layerSuelo)
        {
            Vector2Int g = m.Buscar('G').Count > 0 ? m.Buscar('G')[0] : new Vector2Int(153, 34);
            Vector2Int q = m.Buscar('Q').Count > 0 ? m.Buscar('Q')[0] : new Vector2Int(183, 34);
            Vector2Int t = m.Buscar('T').Count > 0 ? m.Buscar('T')[0] : new Vector2Int(156, 34);
            Vector2Int b = m.Buscar('B').Count > 0 ? m.Buscar('B')[0] : new Vector2Int(174, 34);
            float piso = g.y;

            Sprite spBloque = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png");
            System.Func<string, float, Transform[]> crearCompuerta = (nombre, xc) =>
            {
                var bloques = new Transform[3];
                GameObject raiz = new GameObject(nombre);
                for (int i = 0; i < 3; i++)
                {
                    GameObject bl = new GameObject($"{nombre}_Bloque_{i}");
                    bl.transform.SetParent(raiz.transform);
                    bl.transform.position = new Vector3(xc, piso + 1.5f + i * 3f, 0f);
                    bl.layer = layerSuelo;
                    var sr = bl.AddComponent<SpriteRenderer>();
                    sr.sprite = spBloque;
                    sr.sortingLayerName = Constantes.SortingLayers.Escenario;
                    sr.sortingOrder = 4;
                    var col = bl.AddComponent<BoxCollider2D>();
                    col.size = new Vector2(1.9f, 3f);
                    bloques[i] = bl.transform;
                }
                return bloques;
            };

            Transform[] entrada = crearCompuerta("Compuerta_Entrada", g.x + 0.5f);
            Transform[] salida = crearCompuerta("Compuerta_Salida", q.x + 0.5f);

            // Paredes efectivas de la arena (borde interior de las compuertas)
            float paredIzq = g.x + 0.5f + 0.95f;
            float paredDer = q.x + 0.5f - 0.95f;

            // ---------------------------------------------------------------- El Coloso
            GameObject coloso = new GameObject("Coloso_PlataRoja");
            coloso.transform.position = new Vector3(b.x + 0.5f, piso, 0f);
            int capaEnemigo = LayerMask.NameToLayer(Constantes.Layers.Enemigo);
            if (capaEnemigo != -1) coloso.layer = capaEnemigo;

            var sr0 = coloso.AddComponent<SpriteRenderer>();
            sr0.sprite = EditorSpriteUtil.Primero("Assets/Sprites/Jefe/Coloso_Idle.png");
            sr0.sortingLayerName = Constantes.SortingLayers.Objetos;
            sr0.sortingOrder = 8;

            var anim = coloso.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Jefe/Coloso.controller");

            var rb = coloso.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.useFullKinematicContacts = true;

            var cuerpo = coloso.AddComponent<BoxCollider2D>();
            cuerpo.size = new Vector2(2.6f, 3.2f);
            cuerpo.offset = new Vector2(0f, 1.6f);

            var audioLoop = coloso.AddComponent<AudioSource>();
            audioLoop.loop = true;
            audioLoop.playOnAwake = false;

            GameObject debil = new GameObject("Punto_Debil");
            debil.transform.SetParent(coloso.transform, false);
            debil.transform.localPosition = new Vector3(0f, 3.4f, 0f);
            if (capaEnemigo != -1) debil.layer = capaEnemigo;
            var colDebil = debil.AddComponent<BoxCollider2D>();
            colDebil.isTrigger = true;
            colDebil.size = new Vector2(2.4f, 0.6f);
            var pd = debil.AddComponent<PuntoDebilColoso>();

            GameObject estrellas = new GameObject("Estrellas_Aturdido");
            estrellas.transform.SetParent(coloso.transform, false);
            estrellas.transform.localPosition = new Vector3(0f, 4.3f, 0f);
            Sprite[] cuadrosEstrellas = EditorSpriteUtil.Cuadros("Assets/Sprites/Jefe/Estrellas_Aturdido.png");
            var srE = estrellas.AddComponent<SpriteRenderer>();
            srE.sprite = cuadrosEstrellas.Length > 0 ? cuadrosEstrellas[0] : null;
            srE.sortingLayerName = Constantes.SortingLayers.Objetos;
            srE.sortingOrder = 9;
            var animE = estrellas.AddComponent<SpriteFrameAnimator>();
            animE.Configurar(cuadrosEstrellas, 10f, true, true);
            estrellas.SetActive(false);

            var jefe = coloso.AddComponent<BossColoso>();
            jefe.ConfigurarComponentes(anim, sr0, colDebil, estrellas, audioLoop);
            jefe.ConfigurarEfectos(
                EditorSpriteUtil.Cuadros("Assets/Sprites/Jefe/Onda_Choque.png"),
                EditorSpriteUtil.Cuadros("Assets/Sprites/Jefe/Roca_Lanzada.png"),
                EditorSpriteUtil.Cuadros("Assets/Sprites/Jefe/Polvo_Grande.png"),
                EditorSpriteUtil.Cuadros("Assets/Sprites/Obstaculos/Polvo.png"));
            pd.Configurar(jefe);
            jefe.ConfigurarFuente(FontAssetBuilder.ObtenerPixelifySans());

            // ---------------------------------------------------------------- Trigger T y arena
            GameObject trigArena = new GameObject("Trigger_Arena_Coloso");
            trigArena.transform.position = new Vector3(t.x + 0.5f, piso + 3f, 0f);
            var colT = trigArena.AddComponent<BoxCollider2D>();
            colT.isTrigger = true;
            colT.size = new Vector2(1f, 6f);
            var arena = trigArena.AddComponent<BossArena>();
            arena.Configurar(jefe, entrada, salida, rocasArena, confiner, limitesNivel, limitesArena,
                             EditorSpriteUtil.Cuadros("Assets/Sprites/Jefe/Polvo_Grande.png"));

            jefe.Configurar(arena, piso, paredIzq + 1.4f, paredDer - 1.4f, paredIzq + 0.8f, paredDer - 0.8f);
        }

        // ====================================================================
        // INCENDIO FINAL
        // ====================================================================
        private static void CrearIncendioFinal(Vector3 posTrigger, Vector3 posC4, float finX)
        {
            GameObject muro = new GameObject("Muro_Incendio");
            muro.transform.position = new Vector3(posTrigger.x - 9f, posTrigger.y, 0f);
            var src = muro.AddComponent<AudioSource>();
            src.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/SFX/Fuego_Loop.wav");
            src.loop = true;
            src.playOnAwake = false;
            muro.AddComponent<BoxCollider2D>();

            var luz = muro.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightOuterRadius = 7f;
            luz.pointLightInnerRadius = 1f;
            luz.intensity = 1.4f;
            luz.color = new Color(1f, 0.5f, 0.2f);
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            GameObject trig = new GameObject("Trigger_Incendio_X");
            trig.transform.position = posTrigger;
            var colT = trig.AddComponent<BoxCollider2D>();
            colT.isTrigger = true;
            colT.size = new Vector2(1f, 6f);
            var fct = trig.AddComponent<FireChaseTrigger>();

            var fw = muro.AddComponent<FireWallChase>();
            fw.Configurar(EditorSpriteUtil.Cuadros("Assets/Sprites/Obstaculos/Llamas_Incendio.png"), fct, posC4, finX);
            fct.Configurar(fw);
        }

        // ====================================================================
        // UI EXTRA DEL HUD: aviso grande y barra de vida del jefe
        // ====================================================================
        private static GameObject CrearAvisoPantalla(Transform canvas)
        {
            TMP_FontAsset fTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject raiz = new GameObject("Aviso_Pantalla");
            raiz.transform.SetParent(canvas, false);
            var rt = raiz.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1700f, 320f);
            rt.anchoredPosition = new Vector2(0f, 160f);
            var grupo = raiz.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.interactable = false;
            grupo.blocksRaycasts = false;

            GameObject goT = new GameObject("Titulo");
            goT.transform.SetParent(raiz.transform, false);
            var rtT = goT.AddComponent<RectTransform>();
            rtT.anchorMin = new Vector2(0f, 0.45f);
            rtT.anchorMax = new Vector2(1f, 1f);
            rtT.offsetMin = Vector2.zero;
            rtT.offsetMax = Vector2.zero;
            var tmpT = goT.AddComponent<TextMeshProUGUI>();
            tmpT.font = fTitulo;
            tmpT.fontSize = 56f;
            tmpT.enableAutoSizing = true;
            tmpT.fontSizeMin = 32f;
            tmpT.fontSizeMax = 60f;
            tmpT.alignment = TextAlignmentOptions.Center;
            tmpT.textWrappingMode = TextWrappingModes.NoWrap;
            tmpT.outlineWidth = 0.2f;
            tmpT.outlineColor = new Color32(30, 10, 5, 255);

            GameObject goS = new GameObject("Subtitulo");
            goS.transform.SetParent(raiz.transform, false);
            var rtS = goS.AddComponent<RectTransform>();
            rtS.anchorMin = new Vector2(0f, 0f);
            rtS.anchorMax = new Vector2(1f, 0.45f);
            rtS.offsetMin = Vector2.zero;
            rtS.offsetMax = Vector2.zero;
            var tmpS = goS.AddComponent<TextMeshProUGUI>();
            tmpS.font = fTexto;
            tmpS.fontSize = 34f;
            tmpS.enableAutoSizing = true;
            tmpS.fontSizeMin = 24f;
            tmpS.fontSizeMax = 34f;
            tmpS.alignment = TextAlignmentOptions.Top;
            tmpS.color = new Color(0.96f, 0.91f, 0.75f);

            var aviso = raiz.AddComponent<AvisoPantalla>();
            var so = new SerializedObject(aviso);
            so.FindProperty("textoTitulo").objectReferenceValue = tmpT;
            so.FindProperty("textoSubtitulo").objectReferenceValue = tmpS;
            so.FindProperty("grupo").objectReferenceValue = grupo;
            so.ApplyModifiedProperties();
            return raiz;
        }

        private static GameObject CrearBarraJefe(Transform canvas)
        {
            TMP_FontAsset fTexto = FontAssetBuilder.ObtenerPixelifySans();
            Sprite spMarco = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Jefe/Barra_Jefe_Marco.png");
            Sprite spRelleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Jefe/Barra_Jefe_Relleno.png");
            Sprite spRetrato = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Jefe/Retrato_Coloso.png");

            GameObject raiz = new GameObject("Barra_Jefe");
            raiz.transform.SetParent(canvas, false);
            var rt = raiz.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(1020f, 110f);
            rt.anchoredPosition = new Vector2(0f, -128f);
            var grupo = raiz.AddComponent<CanvasGroup>();
            grupo.alpha = 0f;
            grupo.interactable = false;
            grupo.blocksRaycasts = false;

            GameObject ret = new GameObject("Retrato_Coloso");
            ret.transform.SetParent(raiz.transform, false);
            var rtR = ret.AddComponent<RectTransform>();
            rtR.anchorMin = new Vector2(0f, 0.5f);
            rtR.anchorMax = new Vector2(0f, 0.5f);
            rtR.pivot = new Vector2(0f, 0.5f);
            rtR.sizeDelta = new Vector2(96f, 96f);
            rtR.anchoredPosition = Vector2.zero;
            var imgR = ret.AddComponent<Image>();
            imgR.sprite = spRetrato;
            imgR.preserveAspect = true;

            GameObject nom = new GameObject("Nombre_Jefe");
            nom.transform.SetParent(raiz.transform, false);
            var rtN = nom.AddComponent<RectTransform>();
            rtN.anchorMin = new Vector2(0f, 1f);
            rtN.anchorMax = new Vector2(0f, 1f);
            rtN.pivot = new Vector2(0f, 1f);
            rtN.sizeDelta = new Vector2(900f, 44f);
            rtN.anchoredPosition = new Vector2(112f, 0f);
            var tmpN = nom.AddComponent<TextMeshProUGUI>();
            tmpN.font = fTexto;
            tmpN.fontSize = 36f;
            tmpN.enableAutoSizing = true;
            tmpN.fontSizeMin = 26f;
            tmpN.fontSizeMax = 36f;
            tmpN.alignment = TextAlignmentOptions.MidlineLeft;
            tmpN.color = new Color(1f, 0.8f, 0.7f);
            tmpN.text = "El Coloso de Plata Roja";

            GameObject marco = new GameObject("Marco_Vida");
            marco.transform.SetParent(raiz.transform, false);
            var rtM = marco.AddComponent<RectTransform>();
            rtM.anchorMin = new Vector2(0f, 0f);
            rtM.anchorMax = new Vector2(0f, 0f);
            rtM.pivot = new Vector2(0f, 0f);
            rtM.sizeDelta = new Vector2(900f, 48f);
            rtM.anchoredPosition = new Vector2(112f, 8f);
            var imgM = marco.AddComponent<Image>();
            imgM.sprite = spMarco;
            imgM.type = Image.Type.Sliced;
            imgM.pixelsPerUnitMultiplier = 3.125f;
            var hlg = marco.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(14, 14, 10, 10);
            hlg.spacing = 8f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = true;
            hlg.childForceExpandHeight = true;

            var segmentos = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject seg = new GameObject($"Segmento_{i + 1}");
                seg.transform.SetParent(marco.transform, false);
                seg.AddComponent<RectTransform>();
                var img = seg.AddComponent<Image>();
                img.sprite = spRelleno;
                img.type = Image.Type.Sliced;
                img.pixelsPerUnitMultiplier = 1.5625f;
                segmentos[i] = img;
            }

            // Instrucción permanente debajo de la barra
            GameObject instr = new GameObject("Instruccion_Jefe");
            instr.transform.SetParent(raiz.transform, false);
            var rtI = instr.AddComponent<RectTransform>();
            rtI.anchorMin = new Vector2(0f, 0f);
            rtI.anchorMax = new Vector2(1f, 0f);
            rtI.pivot = new Vector2(0.5f, 1f);
            rtI.offsetMin = new Vector2(112f, -40f);
            rtI.offsetMax = new Vector2(-8f, 0f);
            var tmpI = instr.AddComponent<TextMeshProUGUI>();
            tmpI.font = fTexto;
            tmpI.fontSize = 26f;
            tmpI.enableAutoSizing = true;
            tmpI.fontSizeMin = 20f;
            tmpI.fontSizeMax = 26f;
            tmpI.alignment = TextAlignmentOptions.MidlineLeft;
            tmpI.color = new Color(1f, 0.92f, 0.7f);
            tmpI.outlineWidth = 0.2f;
            tmpI.outlineColor = new Color32(20, 8, 4, 255);
            tmpI.text = "Cuando esté aturdido (estrellas), ¡salta sobre su cabeza!";

            var barra = raiz.AddComponent<BossHealthBarUI>();
            var so = new SerializedObject(barra);
            so.FindProperty("grupo").objectReferenceValue = grupo;
            so.FindProperty("textoNombre").objectReferenceValue = tmpN;
            var pSeg = so.FindProperty("segmentos");
            pSeg.arraySize = 3;
            for (int i = 0; i < 3; i++) pSeg.GetArrayElementAtIndex(i).objectReferenceValue = segmentos[i];
            so.FindProperty("textoInstruccion").objectReferenceValue = tmpI;
            so.ApplyModifiedProperties();
            return raiz;
        }
    }
}
