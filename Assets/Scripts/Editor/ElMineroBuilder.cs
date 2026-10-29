using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Unity.Cinemachine;
using ElMinero.Utilidades;
using ElMinero.Jugador;
using ElMinero.Camara;
using ElMinero.Managers;
using ElMinero.Coleccionables;
using ElMinero.Niveles;
using ElMinero.Obstaculos;
using ElMinero.Plataformas;
using ElMinero.UI;

namespace ElMinero.Editor
{
    /// <summary>
    /// Herramienta de automatizaciÃ³n y configuraciÃ³n completa para el proyecto "El Minero".
    /// Accesible desde el menÃº superior: Herramientas > El Minero.
    /// </summary>
    public static class ElMineroBuilder
    {
        private const string MenuRoot = "Herramientas/El Minero/";

        // ====================================================================
        // 1. CREAR CARPETAS
        // ====================================================================
        [MenuItem(MenuRoot + "1. Crear carpetas", priority = 10)]
        public static void CrearCarpetas()
        {
            string[] carpetas = new string[]
            {
                "Assets/Animaciones/Jugador",
                "Assets/Animaciones/Coleccionables",
                "Assets/Animaciones/Obstaculos",
                "Assets/Animaciones/Entorno",
                "Assets/Animaciones/UI",
                "Assets/Audios/Musica",
                "Assets/Audios/SFX",
                "Assets/Controladores/Jugador",
                "Assets/Controladores/Coleccionables",
                "Assets/Controladores/Obstaculos",
                "Assets/Controladores/Entorno",
                "Assets/Controladores/UI",
                "Assets/Fuentes",
                "Assets/Materiales/Fisica",
                "Assets/Materiales/Sprites",
                "Assets/Paleta/Paleta_MinaRayas",
                "Assets/Paleta/Paleta_ElNopal",
                "Assets/Paleta/Paleta_LaValenciana",
                "Assets/Prefabs/Jugador",
                "Assets/Prefabs/Plataformas",
                "Assets/Prefabs/Obstaculos",
                "Assets/Prefabs/Coleccionables",
                "Assets/Prefabs/Niveles",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Sistemas",
                "Assets/Scenes",
                "Assets/Scripts/Jugador",
                "Assets/Scripts/Managers",
                "Assets/Scripts/Niveles",
                "Assets/Scripts/Plataformas",
                "Assets/Scripts/Obstaculos",
                "Assets/Scripts/Coleccionables",
                "Assets/Scripts/UI",
                "Assets/Scripts/Camara",
                "Assets/Scripts/Utilidades",
                "Assets/Scripts/Editor",
                "Assets/Settings/Niveles"
            };

            int creadas = 0;
            foreach (var ruta in carpetas)
            {
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                    creadas++;
                }
            }

            AssetDatabase.Refresh();
            Debug.Log($"[El Minero] Carpetas verificadas/creadas con Ã©xito ({creadas} nuevas).");
        }

        // ====================================================================
        // 2. CONFIGURAR TAGS, LAYERS Y SORTING LAYERS
        // ====================================================================
        [MenuItem(MenuRoot + "2. Configurar Tags, Layers y Sorting Layers", priority = 20)]
        public static void ConfigurarTagsYLayers()
        {
            var tagManagerAsset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if (tagManagerAsset == null || tagManagerAsset.Length == 0)
            {
                Debug.LogError("[El Minero] No se pudo cargar TagManager.asset.");
                return;
            }

            SerializedObject tagManager = new SerializedObject(tagManagerAsset[0]);

            // --- A) TAGS ---
            SerializedProperty tagsProp = tagManager.FindProperty("tags");
            string[] tagsDeseados = new string[]
            {
                Constantes.Tags.Hazard,
                Constantes.Tags.Collectible,
                Constantes.Tags.Checkpoint,
                Constantes.Tags.Exit
            };

            foreach (var tag in tagsDeseados)
            {
                bool existe = false;
                for (int i = 0; i < tagsProp.arraySize; i++)
                {
                    if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag)
                    {
                        existe = true;
                        break;
                    }
                }
                if (!existe)
                {
                    tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
                    tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
                    Debug.Log($"[El Minero] Tag agregado: {tag}");
                }
            }

            // --- B) LAYERS ---
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            AsignarLayer(layersProp, 8, Constantes.Layers.Suelo);
            AsignarLayer(layersProp, 9, Constantes.Layers.Jugador);
            AsignarLayer(layersProp, 10, Constantes.Layers.Peligro);
            AsignarLayer(layersProp, 11, Constantes.Layers.Coleccionable);
            AsignarLayer(layersProp, 12, Constantes.Layers.Enemigo);

            // --- C) SORTING LAYERS ---
            SerializedProperty sortingLayersProp = tagManager.FindProperty("m_SortingLayers");
            string[] sortingLayersDeseadas = new string[]
            {
                Constantes.SortingLayers.Fondo,
                Constantes.SortingLayers.FondoMedio,
                Constantes.SortingLayers.Escenario,
                Constantes.SortingLayers.Objetos,
                Constantes.SortingLayers.Jugador,
                Constantes.SortingLayers.PrimerPlano,
                Constantes.SortingLayers.UI
            };

            foreach (var slName in sortingLayersDeseadas)
            {
                bool existe = false;
                for (int i = 0; i < sortingLayersProp.arraySize; i++)
                {
                    var sl = sortingLayersProp.GetArrayElementAtIndex(i);
                    var nameProp = sl.FindPropertyRelative("name");
                    if (nameProp != null && nameProp.stringValue == slName)
                    {
                        existe = true;
                        break;
                    }
                }
                if (!existe)
                {
                    int index = sortingLayersProp.arraySize;
                    sortingLayersProp.InsertArrayElementAtIndex(index);
                    var newSl = sortingLayersProp.GetArrayElementAtIndex(index);
                    var nameProp = newSl.FindPropertyRelative("name");
                    var uniqueIdProp = newSl.FindPropertyRelative("uniqueID");
                    if (nameProp != null) nameProp.stringValue = slName;
                    if (uniqueIdProp != null) uniqueIdProp.intValue = UnityEngine.Random.Range(100000, 999999);
                    Debug.Log($"[El Minero] Sorting Layer agregada: {slName}");
                }
            }

            tagManager.ApplyModifiedProperties();

                        // --- D) MATRIZ DE COLISIÃ“N 2D ---
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            int layerJugador = LayerMask.NameToLayer(Constantes.Layers.Jugador);
            int layerColeccionable = LayerMask.NameToLayer(Constantes.Layers.Coleccionable);
            int layerEnemigo = LayerMask.NameToLayer(Constantes.Layers.Enemigo);

            if (layerEnemigo != -1 && layerJugador != -1 && layerSuelo != -1)
            {
                // Enemigo colisiona con Jugador y Suelo, ignora Coleccionable
                if (layerColeccionable != -1)
                {
                    Physics2D.IgnoreLayerCollision(layerEnemigo, layerColeccionable, true);
                }
            }

            if (layerColeccionable != -1 && layerJugador != -1)
            {
                for (int i = 0; i < 32; i++)
                {
                    if (i != layerJugador)
                    {
                        Physics2D.IgnoreLayerCollision(layerColeccionable, i, true);
                    }
                    else
                    {
                        Physics2D.IgnoreLayerCollision(layerColeccionable, layerJugador, false);
                    }
                }
            }

            Debug.Log("[El Minero] Tags, Layers, Sorting Layers y Matriz de Colisiones configurados.");
        }

        private static void AsignarLayer(SerializedProperty layersProp, int index, string layerName)
        {
            if (index < layersProp.arraySize)
            {
                var elem = layersProp.GetArrayElementAtIndex(index);
                if (string.IsNullOrEmpty(elem.stringValue) || elem.stringValue != layerName)
                {
                    elem.stringValue = layerName;
                    Debug.Log($"[El Minero] Layer {index} asignada a '{layerName}'");
                }
            }
        }

        // ====================================================================
        // 5. CREAR MATERIALES FÃSICOS Y LEVELCONFIGS
        // ====================================================================
        [MenuItem(MenuRoot + "5. Crear materiales fÃ­sicos y LevelConfigs", priority = 50)]
        public static void CrearMaterialesFisicosYConfigs()
        {
            string dirFisica = "Assets/Materiales/Fisica";
            if (!Directory.Exists(dirFisica)) Directory.CreateDirectory(dirFisica);

            // Material SinFriccion (Friction = 0, Bounciness = 0)
            string pathSinFriccion = Path.Combine(dirFisica, "SinFriccion.physicsMaterial2D");
            PhysicsMaterial2D matSinFriccion = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(pathSinFriccion);
            if (matSinFriccion == null)
            {
                matSinFriccion = new PhysicsMaterial2D("SinFriccion")
                {
                    friction = 0f,
                    bounciness = 0f
                };
                AssetDatabase.CreateAsset(matSinFriccion, pathSinFriccion);
            }

            // Material Plataforma
            string pathPlataforma = Path.Combine(dirFisica, "Plataforma.physicsMaterial2D");
            PhysicsMaterial2D matPlataforma = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(pathPlataforma);
            if (matPlataforma == null)
            {
                matPlataforma = new PhysicsMaterial2D("Plataforma")
                {
                    friction = 0.4f,
                    bounciness = 0f
                };
                AssetDatabase.CreateAsset(matPlataforma, pathPlataforma);
            }

            // Crear los 3 LevelConfig ScriptableObjects
            string dirConfigs = "Assets/Settings/Niveles";
            if (!Directory.Exists(dirConfigs)) Directory.CreateDirectory(dirConfigs);

            CrearOActualizarLevelConfig(Path.Combine(dirConfigs, "LevelConfig_Nivel1.asset"), "Nivel 1 â€” Mina Rayas", 1, 100f, 6, 2, "Aguilarita", "Aguilarita (Ag4SeS): mineral de plata y selenio descubierto en Guanajuato en 1891.", 4.0f, new Color(0.12f, 0.09f, 0.19f));
            CrearOActualizarLevelConfig(Path.Combine(dirConfigs, "LevelConfig_Nivel2.asset"), "Nivel 2 â€” El Nopal", 2, 120f, 8, 3, "Pepita de oro", "Pepita de oro nativo extraÃ­da de las profundidades de Guanajuato.", 3.0f, new Color(0.16f, 0.10f, 0.05f));
            CrearOActualizarLevelConfig(Path.Combine(dirConfigs, "LevelConfig_Nivel3.asset"), "Nivel 3 â€” La Valenciana", 3, 140f, 10, 4, "Pirargirita", "Pirargirita (Ag3SbS3, plata roja): mineral emblemÃ¡tico que enriqueciÃ³ a Guanajuato.", 2.2f, new Color(0.16f, 0.04f, 0.04f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[El Minero] Materiales fÃ­sicos y LevelConfigs creados correctamente.");
        }

        private static void CrearOActualizarLevelConfig(string path, string nombre, int index, float tiempo, int vetas, int gemas, string minNombre, string minDesc, float rocasIntervalo, Color bg, Sprite mini = null, string dato = "")
        {
            LevelConfig config = AssetDatabase.LoadAssetAtPath<LevelConfig>(path);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<LevelConfig>();
                AssetDatabase.CreateAsset(config, path);
            }

            SerializedObject so = new SerializedObject(config);
            so.FindProperty("levelName").stringValue = nombre;
            so.FindProperty("levelIndex").intValue = index;
            so.FindProperty("timeLimit").floatValue = tiempo;
            so.FindProperty("silverVeinCount").intValue = vetas;
            so.FindProperty("amethystGemCount").intValue = gemas;
            so.FindProperty("silverVeinPoints").intValue = 10;
            so.FindProperty("amethystGemPoints").intValue = 25;
            so.FindProperty("allSilverBonus").intValue = 50;
            so.FindProperty("distinctiveMineralName").stringValue = minNombre;
            so.FindProperty("distinctiveMineralDescription").stringValue = minDesc;
            so.FindProperty("distinctiveMineralPoints").intValue = 50;
            so.FindProperty("rockSpawnInterval").floatValue = rocasIntervalo;
            so.FindProperty("cameraBackgroundColor").colorValue = bg;
            if (mini != null) so.FindProperty("miniatura").objectReferenceValue = mini;
            if (!string.IsNullOrEmpty(dato)) so.FindProperty("datoHistorico").stringValue = dato;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(config);
        }

        // ====================================================================
        // ESCENA DE PRUEBA COMPLETA (FASE 1 Y FASE 2)
        // ====================================================================
        [MenuItem(MenuRoot + "Crear Escena de Prueba (Fase 1 y 2)", priority = 100)]
        public static void CrearEscenaPruebaFase1()
        {
            ConfigurarTagsYLayers();
            CrearMaterialesFisicosYConfigs();
            SpriteImportSetup.ConfigurarTodosLosSprites();

            Scene nuevaEscena = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Luz Global 2D (URP)
            GameObject luzGlobalObj = new GameObject("Global Light 2D");
            var light2D = luzGlobalObj.AddComponent<Light2D>();
            light2D.lightType = Light2D.LightType.Global;
            light2D.intensity = 0.75f;
            light2D.color = new Color(0.85f, 0.8f, 0.95f);
            AsignarCapasALuz2D(light2D);

            // 2. Crear Grid y los 3 Tilemaps (Fondo, Suelo con Collider, DecoraciÃ³n)
            GameObject gridObj = new GameObject("Grid");
            var grid = gridObj.AddComponent<Grid>();
            grid.cellSize = new Vector3(1f, 1f, 0f);

            // Tilemap Fondo
            GameObject tmFondoObj = new GameObject("Tilemap_Fondo");
            tmFondoObj.transform.SetParent(gridObj.transform);
            var tmFondo = tmFondoObj.AddComponent<Tilemap>();
            var tmFondoRend = tmFondoObj.AddComponent<TilemapRenderer>();
            tmFondoRend.sortingLayerName = Constantes.SortingLayers.Fondo;

            // Tilemap Suelo (con fÃ­sica)
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 0;

            GameObject tmSueloObj = new GameObject("Tilemap_Suelo");
            tmSueloObj.layer = layerSuelo;
            tmSueloObj.transform.SetParent(gridObj.transform);
            var tmSuelo = tmSueloObj.AddComponent<Tilemap>();
            var tmSueloRend = tmSueloObj.AddComponent<TilemapRenderer>();
            tmSueloRend.sortingLayerName = Constantes.SortingLayers.Escenario;

            var rbTilemap = tmSueloObj.AddComponent<Rigidbody2D>();
            rbTilemap.bodyType = RigidbodyType2D.Static;

            var tmCol = tmSueloObj.AddComponent<TilemapCollider2D>();
            tmCol.compositeOperation = Collider2D.CompositeOperation.Merge;

            var compCol = tmSueloObj.AddComponent<CompositeCollider2D>();
            compCol.geometryType = CompositeCollider2D.GeometryType.Polygons;

            // Tilemap DecoraciÃ³n
            GameObject tmDecoObj = new GameObject("Tilemap_Decoracion");
            tmDecoObj.transform.SetParent(gridObj.transform);
            var tmDeco = tmDecoObj.AddComponent<Tilemap>();
            var tmDecoRend = tmDecoObj.AddComponent<TilemapRenderer>();
            tmDecoRend.sortingLayerName = Constantes.SortingLayers.Objetos;

            // 3. Pintar escenario con tiles de Mina Rayas (mockup GDD)
            ConstruirTerrenoMinaRayas(tmSuelo, tmDeco);
            FinalizarColisionesTilemap(tmSuelo);

            // 4. Hueco Letal (DeathPit) en el precipicio central
            CrearHuecoLetal(new Vector3(0.5f, -3.5f, 0f), new Vector2(4f, 1.5f));

            // 5. Plataforma flotante sobre el hueco (Plataforma_Fija)
            CrearPlataformaFlotante(new Vector3(0.5f, 0.5f, 0f), layerSuelo);

            // 6. Elementos de ambientaciÃ³n (Marco con antorcha, vagoneta, poste con farol)
            CrearElementosEntorno();

            // 7. Checkpoint funcional (a la mitad del recorrido)
            CrearCheckpoint(new Vector3(4f, -0.5f, 0f));

            // 8. Salida del nivel (LevelExit al final)
            CrearSalidaNivel(new Vector3(16f, -1f, 0f));

            // 9. Coleccionables del nivel (4 vetas de plata y 1 gema de amatista)
            CrearColeccionablesCompletos();

            // 10. ObstÃ¡culos y Eventos de Fase 3 (Rocas, Plataforma mÃ³vil, quebradiza, derrumbe y temblor)
            CrearObstaculosYEventosFase3(layerSuelo);

            // 11. Jugador (Minero a escala real 32x32 PPU=16, 2 unidades de alto, con vidas y etiqueta)
            GameObject jugadorObj = CrearJugadorCompleto(layerSuelo);

            // 12. Managers del Nivel y GameManager
            ConfigurarManagers(jugadorObj);

            // 13. HUD Completo en Canvas
            ConfigurarHUDCanvas();

            // 14. CÃ¡mara Cinemachine 3 y CÃ¡mara Principal
            ConfigurarCamara(jugadorObj);

            // Guardar la escena
            string pathEscena = "Assets/Scenes/EscenaPrueba_Fase1.unity";
            EditorSceneManager.SaveScene(nuevaEscena, pathEscena);
            AssetDatabase.Refresh();

            Debug.Log($"[El Minero] Escena completa generada en: {pathEscena}");
            EditorUtility.DisplayDialog("El Minero â€” Sistemas y Terreno Listos",
                "Â¡Escena construida con Ã©xito!\n\n" +
                "âœ“ Terreno construido con Tilemap oficial de Mina Rayas.\n" +
                "âœ“ Minero a escala correcta (PPU=16, 2 unidades de alto).\n" +
                "âœ“ FÃ­sicas exactas (5 u/s, salto 2.5 u).\n" +
                "âœ“ 3 Corazones de vida y reapariciÃ³n en Checkpoint.\n" +
                "âœ“ Reloj con cuenta regresiva y parpadeo rojo en Ãºltimos 10s.\n" +
                "âœ“ 4 Vetas de plata (+10) y 1 Gema (+25).\n" +
                "âœ“ Racha de recolecciÃ³n: 3 coleccionables activan x2 por 10s.\n" +
                "âœ“ Salida del nivel con bono de +50 si juntas toda la plata.\n" +
                "âœ“ Reinicio automÃ¡tico al perder vidas o tiempo.\n\n" +
                "Presiona Play para probarla.", "Â¡A jugar!");
        }

        private static void ConstruirTerrenoMinaRayas(Tilemap tmSuelo, Tilemap tmDeco)
        {
            string nivel = "MinaRayas";
            Tile t0 = CargarTile(nivel, 0);   // Sup-Izq
            Tile t1 = CargarTile(nivel, 1);   // Superficie centro
            Tile t2 = CargarTile(nivel, 2);   // Sup-Der
            Tile t3 = CargarTile(nivel, 3);   // Repisa Izq
            Tile t4 = CargarTile(nivel, 4);   // Repisa Centro
            Tile t5 = CargarTile(nivel, 5);   // Repisa Der
            Tile t7 = CargarTile(nivel, 7);   // Estalactita
            Tile t8 = CargarTile(nivel, 8);   // Pared Izq
            Tile t9 = CargarTile(nivel, 9);   // Relleno piedra
            Tile t10 = CargarTile(nivel, 10); // Pared Der
            Tile t16 = CargarTile(nivel, 16); // Techo Izq
            Tile t17 = CargarTile(nivel, 17); // Techo Centro
            Tile t18 = CargarTile(nivel, 18); // Techo Der

            // A) Suelo izquierdo (X de -14 a -2, Y de -2 a -4)
            for (int x = -14; x <= -2; x++)
            {
                Tile sup = (x == -14) ? (t0 != null ? t0 : t1) : (x == -2 ? (t2 != null ? t2 : t1) : t1);
                tmSuelo.SetTile(new Vector3Int(x, -2, 0), sup);

                Tile sub = (x == -14) ? (t8 != null ? t8 : t9) : (x == -2 ? (t10 != null ? t10 : t9) : t9);
                tmSuelo.SetTile(new Vector3Int(x, -3, 0), sub);
                tmSuelo.SetTile(new Vector3Int(x, -4, 0), t9);
            }

            // B) Hueco letal: X entre -1 y +2 queda vacÃ­o

            // C) Suelo derecho (X de +3 a +18, Y de -2 a -4)
            for (int x = 3; x <= 18; x++)
            {
                Tile sup = (x == 3) ? (t0 != null ? t0 : t1) : (x == 18 ? (t2 != null ? t2 : t1) : t1);
                tmSuelo.SetTile(new Vector3Int(x, -2, 0), sup);

                Tile sub = (x == 3) ? (t8 != null ? t8 : t9) : (x == 18 ? (t10 != null ? t10 : t9) : t9);
                tmSuelo.SetTile(new Vector3Int(x, -3, 0), sub);
                tmSuelo.SetTile(new Vector3Int(x, -4, 0), t9);
            }

            // D) Repisa elevada derecha (X de 8 a 12, Y = 1)
            tmSuelo.SetTile(new Vector3Int(8, 1, 0), t3 != null ? t3 : t1);
            tmSuelo.SetTile(new Vector3Int(9, 1, 0), t4 != null ? t4 : t1);
            tmSuelo.SetTile(new Vector3Int(10, 1, 0), t4 != null ? t4 : t1);
            tmSuelo.SetTile(new Vector3Int(11, 1, 0), t4 != null ? t4 : t1);
            tmSuelo.SetTile(new Vector3Int(12, 1, 0), t5 != null ? t5 : t1);

            // E) Techo de la cueva (X de -14 a 18, Y = 5)
            for (int x = -14; x <= 18; x++)
            {
                tmSuelo.SetTile(new Vector3Int(x, 5, 0), t17 != null ? t17 : t9);
                tmSuelo.SetTile(new Vector3Int(x, 6, 0), t9);
            }

            // F) Decoraciones: Estalactitas
            if (t7 != null)
            {
                tmDeco.SetTile(new Vector3Int(-11, 4, 0), t7);
                tmDeco.SetTile(new Vector3Int(-5, 4, 0), t7);
                tmDeco.SetTile(new Vector3Int(1, 4, 0), t7);
                tmDeco.SetTile(new Vector3Int(6, 4, 0), t7);
                tmDeco.SetTile(new Vector3Int(14, 4, 0), t7);
            }

            // Paredes laterales para contener al minero
            for (int y = -1; y <= 4; y++)
            {
                tmSuelo.SetTile(new Vector3Int(-14, y, 0), t9);
                tmSuelo.SetTile(new Vector3Int(18, y, 0), t9);
            }
        }

        private static void FinalizarColisionesTilemap(Tilemap tm)
        {
            if (tm == null) return;

            var tmCol = tm.GetComponent<TilemapCollider2D>();
            var compCol = tm.GetComponent<CompositeCollider2D>();

            if (tmCol != null)
            {
                tmCol.ProcessTilemapChanges();
            }

            if (compCol != null)
            {
                compCol.GenerateGeometry();
                if (compCol.pathCount == 0)
                {
                    Debug.LogWarning($"[ElMineroBuilder] CompositeCollider2D en '{tm.name}' tiene 0 caminos. Usando colisiÃ³n directa en TilemapCollider2D.");
                    tmCol.compositeOperation = Collider2D.CompositeOperation.None;
                    UnityEngine.Object.DestroyImmediate(compCol);
                }
                else
                {
                    Debug.Log($"[ElMineroBuilder] CompositeCollider2D en '{tm.name}' generado exitosamente con {compCol.pathCount} caminos poligonales.");
                }
            }
        }

        private static Tile CargarTile(string nivel, int index)
        {
            return AssetDatabase.LoadAssetAtPath<Tile>($"Assets/Tiles/{nivel}/Tile_{nivel}_{index}.asset");
        }

        private static void CrearHuecoLetal(Vector3 pos, Vector2 size)
        {
            GameObject pit = new GameObject("HuecoLetal_Precipicio");
            pit.transform.position = pos;
            var col = pit.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;
            pit.AddComponent<DeathPit>();
        }

        private static void CrearPlataformaFlotante(Vector3 pos, int layer)
        {
            GameObject plat = new GameObject("Plataforma_Fija_Flotante");
            plat.layer = layer;
            plat.transform.position = pos;

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Fija.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
        }

        private static void CrearElementosEntorno()
        {
            // Marco de soporte a la izquierda
            GameObject marco = new GameObject("Marco_Soporte");
            marco.transform.position = new Vector3(-10f, 0f, 0f);
            var srMarco = marco.AddComponent<SpriteRenderer>();
            srMarco.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Marco_Soporte.png");
            srMarco.sortingLayerName = Constantes.SortingLayers.Escenario;

            // Antorcha con luz
            GameObject antorcha = new GameObject("Antorcha");
            antorcha.transform.position = new Vector3(-10f, 0.7f, 0f);
            var srAntorcha = antorcha.AddComponent<SpriteRenderer>();
            srAntorcha.sprite = CargarSubSprite("Assets/Sprites/Entorno/Antorcha.png", "Antorcha_0");
            srAntorcha.sortingLayerName = Constantes.SortingLayers.Objetos;

            var luzAntorcha = antorcha.AddComponent<Light2D>();
            luzAntorcha.lightType = Light2D.LightType.Point;
            luzAntorcha.pointLightInnerRadius = 0.5f;
            luzAntorcha.pointLightOuterRadius = 3.5f;
            luzAntorcha.intensity = 1.3f;
            luzAntorcha.color = new Color(1f, 0.82f, 0.48f);
            AsignarCapasALuz2D(luzAntorcha);

            // Poste con farol
            GameObject poste = new GameObject("Poste_Farol");
            poste.transform.position = new Vector3(6f, 0f, 0f);
            var srPoste = poste.AddComponent<SpriteRenderer>();
            srPoste.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Poste_Vertical.png");
            srPoste.sortingLayerName = Constantes.SortingLayers.Objetos;

            var luzPoste = poste.AddComponent<Light2D>();
            luzPoste.lightType = Light2D.LightType.Point;
            luzPoste.pointLightInnerRadius = 0.5f;
            luzPoste.pointLightOuterRadius = 3.0f;
            luzPoste.intensity = 1.1f;
            luzPoste.color = new Color(1f, 0.9f, 0.6f);
            AsignarCapasALuz2D(luzPoste);

            // Vagoneta
            GameObject vagoneta = new GameObject("Vagoneta_Deco");
            vagoneta.transform.position = new Vector3(14f, -0.65f, 0f);
            var srVagoneta = vagoneta.AddComponent<SpriteRenderer>();
            srVagoneta.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Vagoneta_Deco.png");
            srVagoneta.sortingLayerName = Constantes.SortingLayers.Objetos;
        }

        private static void CrearCheckpoint(Vector3 pos)
        {
            GameObject cp = new GameObject("Checkpoint");
            cp.transform.position = pos;

            var col = cp.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);
            col.offset = new Vector2(0f, 1f);

            var sr = cp.AddComponent<SpriteRenderer>();
            Sprite sprOff = CargarSubSprite("Assets/Sprites/Coleccionables/Checkpoint.png", "Checkpoint_0");
            Sprite sprOn = CargarSubSprite("Assets/Sprites/Coleccionables/Checkpoint.png", "Checkpoint_1");
            sr.sprite = sprOff;
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var luzCp = cp.AddComponent<Light2D>();
            luzCp.lightType = Light2D.LightType.Point;
            luzCp.pointLightInnerRadius = 0.5f;
            luzCp.pointLightOuterRadius = 3f;
            luzCp.intensity = 1.2f;
            luzCp.color = new Color(0.4f, 0.9f, 1f); // Azul mÃ­stico al encender
            luzCp.enabled = false;
            AsignarCapasALuz2D(luzCp);

            var scriptCp = cp.AddComponent<Checkpoint>();
            SerializedObject so = new SerializedObject(scriptCp);
            so.FindProperty("spriteOff").objectReferenceValue = sprOff;
            so.FindProperty("spriteOn").objectReferenceValue = sprOn;
            so.FindProperty("checkpointLight").objectReferenceValue = luzCp;
            so.ApplyModifiedProperties();
        }

        private static void CrearSalidaNivel(Vector3 pos)
        {
            GameObject exit = new GameObject("Salida_Tunel");
            exit.transform.position = pos;

            var col = exit.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 3f);
            col.offset = new Vector2(0f, 1.5f);

            var sr = exit.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Salida_Tunel.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            exit.AddComponent<LevelExit>();
        }

        private static void CrearColeccionablesCompletos()
        {
            // 4 Vetas de Plata
            CrearVetaPlata("Veta_Plata_1", new Vector3(-6f, -0.3f, 0f));
            CrearVetaPlata("Veta_Plata_2", new Vector3(-3.5f, -0.3f, 0f));
            CrearVetaPlata("Veta_Plata_3", new Vector3(9f, 2.2f, 0f));
            CrearVetaPlata("Veta_Plata_4", new Vector3(11f, 2.2f, 0f));

            // 1 Gema de Amatista (sobre la plataforma del hueco)
            CrearGemaAmatista("Gema_Amatista_1", new Vector3(0.5f, 1.5f, 0f));
        }

        private static void CrearVetaPlata(string nombre, Vector3 pos)
        {
            GameObject veta = new GameObject(nombre);
            veta.transform.position = pos;

            var col = veta.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var sr = veta.AddComponent<SpriteRenderer>();
            sr.sprite = CargarSubSprite("Assets/Sprites/Coleccionables/Veta_Plata.png", "Veta_Plata_0");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            veta.AddComponent<SilverVein>();
        }

        private static void CrearGemaAmatista(string nombre, Vector3 pos)
        {
            GameObject gema = new GameObject(nombre);
            gema.transform.position = pos;

            var col = gema.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var sr = gema.AddComponent<SpriteRenderer>();
            sr.sprite = CargarSubSprite("Assets/Sprites/Coleccionables/Gema_Amatista.png", "Gema_Amatista_0");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            gema.AddComponent<AmethystGem>();
        }

        private static GameObject CrearJugadorCompleto(int layerSuelo)
        {
            PhysicsMaterial2D matSinFriccion = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materiales/Fisica/SinFriccion.physicsMaterial2D");

            GameObject jugadorObj = new GameObject("Minero");
            jugadorObj.tag = Constantes.Tags.Player;
            int layerJugador = LayerMask.NameToLayer(Constantes.Layers.Jugador);
            jugadorObj.layer = layerJugador != -1 ? layerJugador : 0;
            jugadorObj.transform.position = new Vector3(-7f, 0.5f, 0f);

            var srJugador = jugadorObj.AddComponent<SpriteRenderer>();
            srJugador.sprite = CargarSubSprite("Assets/Sprites/Jugador/Minero_Idle.png", "Minero_Idle_0");
            srJugador.sortingLayerName = Constantes.SortingLayers.Jugador;

            var rbJugador = jugadorObj.AddComponent<Rigidbody2D>();
            rbJugador.bodyType = RigidbodyType2D.Dynamic;
            rbJugador.interpolation = RigidbodyInterpolation2D.Interpolate;
            rbJugador.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rbJugador.freezeRotation = true;
            rbJugador.gravityScale = 3f;

            var colJugador = jugadorObj.AddComponent<CapsuleCollider2D>();
            colJugador.direction = CapsuleDirection2D.Vertical;
            colJugador.size = new Vector2(0.75f, 1.4f);
            colJugador.offset = new Vector2(0f, 0.7f);
            if (matSinFriccion != null)
            {
                colJugador.sharedMaterial = matSinFriccion;
            }

            var inputHandler = jugadorObj.AddComponent<PlayerInputHandler>();
            var playerCtrl = jugadorObj.AddComponent<PlayerController>();
            var health = jugadorObj.AddComponent<PlayerHealth>();

            SerializedObject soCtrl = new SerializedObject(playerCtrl);
            var propGroundLayer = soCtrl.FindProperty("groundLayer");
            if (propGroundLayer != null)
            {
                propGroundLayer.intValue = 1 << layerSuelo;
            }
            var propSr = soCtrl.FindProperty("spriteRenderer");
            if (propSr != null)
            {
                propSr.objectReferenceValue = srJugador;
            }
            soCtrl.ApplyModifiedProperties();

            // Etiqueta del nombre (World Space)
            GameObject labelObj = new GameObject("EtiquetaNombre");
            labelObj.transform.SetParent(jugadorObj.transform);
            labelObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            var tmp = labelObj.AddComponent<TextMeshPro>();
            tmp.text = "Refugio";
            tmp.fontSize = 3.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingLayerID = SortingLayer.NameToID(Constantes.SortingLayers.UI);
            tmp.color = Color.yellow;
            var nameLabel = labelObj.AddComponent<NameLabel>();

            // Luz en el casco del minero
            GameObject luzCascoObj = new GameObject("Luz_Casco");
            luzCascoObj.transform.SetParent(jugadorObj.transform);
            luzCascoObj.transform.localPosition = new Vector3(0.2f, 1.2f, 0f);
            var luzCasco = luzCascoObj.AddComponent<Light2D>();
            luzCasco.lightType = Light2D.LightType.Point;
            luzCasco.pointLightInnerRadius = 0.5f;
            luzCasco.pointLightOuterRadius = 4.0f;
            luzCasco.intensity = 1.0f;
            luzCasco.color = new Color(1f, 0.95f, 0.8f);
            AsignarCapasALuz2D(luzCasco);

            return jugadorObj;
        }

        private static void ConfigurarManagers(GameObject jugadorObj)
        {
            GameObject managersObj = new GameObject("[Managers]");
            var lm = managersObj.AddComponent<LevelManager>();
            var timer = managersObj.AddComponent<LevelTimer>();
            var cm = managersObj.AddComponent<CollectibleManager>();
            var streak = managersObj.AddComponent<StreakSystem>();

            var config = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Settings/Niveles/LevelConfig_Nivel1.asset");

            SerializedObject soLm = new SerializedObject(lm);
            soLm.FindProperty("levelConfig").objectReferenceValue = config;
            soLm.FindProperty("timer").objectReferenceValue = timer;
            soLm.FindProperty("collectibleManager").objectReferenceValue = cm;
            soLm.FindProperty("streakSystem").objectReferenceValue = streak;
            soLm.FindProperty("playerHealth").objectReferenceValue = jugadorObj.GetComponent<PlayerHealth>();
            soLm.FindProperty("playerController").objectReferenceValue = jugadorObj.GetComponent<PlayerController>();
            soLm.ApplyModifiedProperties();
        }

        private static void ConfigurarHUDCanvas()
        {
            // Canvas UI
            GameObject canvasObj = new GameObject("Canvas_HUD");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            // EventSystem si no existe
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }

            var hud = canvasObj.AddComponent<HUDController>();

            // 1. Panel Superior / Barra Superior
            GameObject panelSuperior = new GameObject("Panel_Superior");
            panelSuperior.transform.SetParent(canvasObj.transform, false);
            var rectSuperior = panelSuperior.AddComponent<RectTransform>();
            rectSuperior.anchorMin = new Vector2(0f, 1f);
            rectSuperior.anchorMax = new Vector2(1f, 1f);
            rectSuperior.pivot = new Vector2(0.5f, 1f);
            rectSuperior.sizeDelta = new Vector2(0f, 100f);
            rectSuperior.anchoredPosition = Vector2.zero;

            // Vidas (3 Corazones en esquina superior izquierda)
            Sprite sprCorazonLleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Corazon_Lleno.png");
            Sprite sprCorazonVacio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Corazon_Vacio.png");
            Image[] hearts = new Image[3];

            for (int i = 0; i < 3; i++)
            {
                GameObject hObj = new GameObject($"Corazon_{i + 1}");
                hObj.transform.SetParent(panelSuperior.transform, false);
                var hRect = hObj.AddComponent<RectTransform>();
                hRect.anchorMin = new Vector2(0f, 0.5f);
                hRect.anchorMax = new Vector2(0f, 0.5f);
                hRect.pivot = new Vector2(0f, 0.5f);
                hRect.sizeDelta = new Vector2(48f, 48f);
                hRect.anchoredPosition = new Vector2(40f + (i * 56f), 0f);

                var img = hObj.AddComponent<Image>();
                img.sprite = sprCorazonLleno;
                hearts[i] = img;
            }

            // Reloj en el centro superior
            GameObject relojObj = new GameObject("Reloj_Contenedor");
            relojObj.transform.SetParent(panelSuperior.transform, false);
            var rRect = relojObj.AddComponent<RectTransform>();
            rRect.anchorMin = new Vector2(0.5f, 0.5f);
            rRect.anchorMax = new Vector2(0.5f, 0.5f);
            rRect.pivot = new Vector2(0.5f, 0.5f);
            rRect.sizeDelta = new Vector2(250f, 60f);
            rRect.anchoredPosition = Vector2.zero;

            // Icono Reloj
            GameObject iconoReloj = new GameObject("Icono_Reloj");
            iconoReloj.transform.SetParent(relojObj.transform, false);
            var irRect = iconoReloj.AddComponent<RectTransform>();
            irRect.anchorMin = new Vector2(0f, 0.5f);
            irRect.anchorMax = new Vector2(0f, 0.5f);
            irRect.pivot = new Vector2(0f, 0.5f);
            irRect.sizeDelta = new Vector2(48f, 48f);
            irRect.anchoredPosition = new Vector2(0f, 0f);
            var imgReloj = iconoReloj.AddComponent<Image>();
            imgReloj.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Reloj.png");

            // Texto Reloj
            GameObject textoReloj = new GameObject("Texto_Reloj");
            textoReloj.transform.SetParent(relojObj.transform, false);
            var trRect = textoReloj.AddComponent<RectTransform>();
            trRect.anchorMin = new Vector2(0f, 0f);
            trRect.anchorMax = new Vector2(1f, 1f);
            trRect.offsetMin = new Vector2(56f, 0f);
            trRect.offsetMax = Vector2.zero;
            var tmpReloj = textoReloj.AddComponent<TextMeshProUGUI>();
            tmpReloj.text = "01:30";
            tmpReloj.fontSize = 42f;
            tmpReloj.fontStyle = FontStyles.Bold;
            tmpReloj.alignment = TextAlignmentOptions.MidlineLeft;

            // Contador de Plata (esquina superior derecha)
            GameObject plataObj = new GameObject("Plata_Contenedor");
            plataObj.transform.SetParent(panelSuperior.transform, false);
            var pRect = plataObj.AddComponent<RectTransform>();
            pRect.anchorMin = new Vector2(1f, 0.5f);
            pRect.anchorMax = new Vector2(1f, 0.5f);
            pRect.pivot = new Vector2(1f, 0.5f);
            pRect.sizeDelta = new Vector2(300f, 60f);
            pRect.anchoredPosition = new Vector2(-40f, 0f);

            GameObject iconoPlata = new GameObject("Icono_Plata");
            iconoPlata.transform.SetParent(plataObj.transform, false);
            var ipRect = iconoPlata.AddComponent<RectTransform>();
            ipRect.anchorMin = new Vector2(0f, 0.5f);
            ipRect.anchorMax = new Vector2(0f, 0.5f);
            ipRect.pivot = new Vector2(0f, 0.5f);
            ipRect.sizeDelta = new Vector2(48f, 48f);
            ipRect.anchoredPosition = new Vector2(0f, 0f);
            var imgPlata = iconoPlata.AddComponent<Image>();
            imgPlata.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Plata.png");

            GameObject textoPlata = new GameObject("Texto_Plata");
            textoPlata.transform.SetParent(plataObj.transform, false);
            var tpRect = textoPlata.AddComponent<RectTransform>();
            tpRect.anchorMin = new Vector2(0f, 0f);
            tpRect.anchorMax = new Vector2(0.5f, 1f);
            tpRect.offsetMin = new Vector2(56f, 0f);
            tpRect.offsetMax = Vector2.zero;
            var tmpPlata = textoPlata.AddComponent<TextMeshProUGUI>();
            tmpPlata.text = "0 / 4";
            tmpPlata.fontSize = 36f;
            tmpPlata.alignment = TextAlignmentOptions.MidlineLeft;

            // Puntaje
            GameObject textoScore = new GameObject("Texto_Score");
            textoScore.transform.SetParent(plataObj.transform, false);
            var tsRect = textoScore.AddComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0.5f, 0f);
            tsRect.anchorMax = new Vector2(1f, 1f);
            tsRect.offsetMin = Vector2.zero;
            tsRect.offsetMax = Vector2.zero;
            var tmpScore = textoScore.AddComponent<TextMeshProUGUI>();
            tmpScore.text = "0 pts";
            tmpScore.fontSize = 36f;
            tmpScore.fontStyle = FontStyles.Bold;
            tmpScore.alignment = TextAlignmentOptions.MidlineRight;

            // Insignia x2
            GameObject badgeObj = new GameObject("Insignia_x2_Badge");
            badgeObj.transform.SetParent(canvasObj.transform, false);
            var bRect = badgeObj.AddComponent<RectTransform>();
            bRect.anchorMin = new Vector2(1f, 1f);
            bRect.anchorMax = new Vector2(1f, 1f);
            bRect.pivot = new Vector2(1f, 1f);
            bRect.sizeDelta = new Vector2(120f, 50f);
            bRect.anchoredPosition = new Vector2(-40f, -110f);

            var imgBadge = badgeObj.AddComponent<Image>();
            imgBadge.sprite = CargarSubSprite("Assets/Sprites/UI/Insignia_x2.png", "Insignia_x2_0");

            GameObject badgeTextObj = new GameObject("Texto_x2_Timer");
            badgeTextObj.transform.SetParent(badgeObj.transform, false);
            var btRect = badgeTextObj.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;
            var tmpBadge = badgeTextObj.AddComponent<TextMeshProUGUI>();
            tmpBadge.text = "10.0s";
            tmpBadge.fontSize = 24f;
            tmpBadge.alignment = TextAlignmentOptions.Center;
            tmpBadge.color = Color.white;

            badgeObj.SetActive(false); // Inactiva por defecto hasta lograr 3 de racha

            // Asignar referencias en HUDController
            SerializedObject soHud = new SerializedObject(hud);
            var propHearts = soHud.FindProperty("heartImages");
            propHearts.arraySize = 3;
            for (int i = 0; i < 3; i++) propHearts.GetArrayElementAtIndex(i).objectReferenceValue = hearts[i];
            soHud.FindProperty("heartFullSprite").objectReferenceValue = sprCorazonLleno;
            soHud.FindProperty("heartEmptySprite").objectReferenceValue = sprCorazonVacio;
            soHud.FindProperty("timerText").objectReferenceValue = tmpReloj;
            soHud.FindProperty("silverCountText").objectReferenceValue = tmpPlata;
            soHud.FindProperty("scoreText").objectReferenceValue = tmpScore;
            soHud.FindProperty("streakBadgeObject").objectReferenceValue = badgeObj;
            soHud.FindProperty("streakTimerText").objectReferenceValue = tmpBadge;
            soHud.ApplyModifiedProperties();
        }

        private static void ConfigurarCamara(GameObject jugadorObj)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(-7f, 1.5f, -10f); // PosiciÃ³n Z = -10 fundamental para 2D
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.cullingMask = ~0; // Renderizar todas las capas
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.09f, 0.19f); // #1E1630 (Violeta Mina Rayas)
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // Cinemachine Brain, Listener de Impulsos y CameraShake
            camObj.AddComponent<CinemachineBrain>();
            camObj.AddComponent<CinemachineImpulseListener>();
            camObj.AddComponent<CameraShake>();

            // Cinemachine Camera (Cinemachine 3)
            GameObject vcamObj = new GameObject("CinemachineCamera");
            vcamObj.transform.position = new Vector3(-7f, 1.5f, -10f);
            var vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcam.Follow = jugadorObj.transform;
            vcam.Lens.OrthographicSize = 5.5f;
            vcam.Lens.NearClipPlane = 0.1f;
            vcam.Lens.FarClipPlane = 100f;

            // CinemachineFollow mantiene la cÃ¡mara exactamente en Z = -10 para vista 2D sin recortar sprites
            var follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 1f, -10f);
            follow.TrackerSettings.PositionDamping = new Vector3(0.3f, 0.3f, 0f);

            var camFollow = camObj.AddComponent<CameraFollowSetup>();
            camFollow.SetTarget(jugadorObj.transform);
            camFollow.SetBounds(new Vector2(-15f, -5f), new Vector2(30f, 10f));
        }

        private static void CrearObstaculosYEventosFase3(int layerSuelo)
        {
            int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (layerPeligro == -1) layerPeligro = 0;

            // 1. Prefab de Roca que cae y Advertencia
            GameObject rockTemplate = new GameObject("Roca_Template");
            rockTemplate.tag = Constantes.Tags.Hazard;
            rockTemplate.layer = layerPeligro;
            var srRock = rockTemplate.AddComponent<SpriteRenderer>();
            srRock.sprite = CargarSubSprite("Assets/Sprites/Obstaculos/Roca.png", "Roca_0");
            srRock.sortingLayerName = Constantes.SortingLayers.Objetos;
            var colRock = rockTemplate.AddComponent<CircleCollider2D>();
            colRock.radius = 0.45f;
            var rbRock = rockTemplate.AddComponent<Rigidbody2D>();
            rbRock.bodyType = RigidbodyType2D.Dynamic;
            rockTemplate.AddComponent<FallingRock>();

            // Advertencia sombra
            GameObject warnTemplate = new GameObject("Advertencia_Template");
            var srWarn = warnTemplate.AddComponent<SpriteRenderer>();
            srWarn.sprite = CargarSubSprite("Assets/Sprites/Obstaculos/Advertencia_Sombra.png", "Advertencia_Sombra_0");
            srWarn.sortingLayerName = Constantes.SortingLayers.Objetos;
            srWarn.color = new Color(1f, 1f, 1f, 0.7f);

            // Generador de rocas (RockSpawner) en el techo sobre el cruce
            GameObject spawnerObj = new GameObject("GeneradorRocas_Techo");
            spawnerObj.transform.position = new Vector3(-0.5f, 4.5f, 0f);
            var spawner = spawnerObj.AddComponent<RockSpawner>();

            SerializedObject soSpawner = new SerializedObject(spawner);
            soSpawner.FindProperty("rockPrefab").objectReferenceValue = rockTemplate;
            soSpawner.FindProperty("warningIndicatorPrefab").objectReferenceValue = warnTemplate;
            soSpawner.FindProperty("spawnInterval").floatValue = 4.0f;
            soSpawner.FindProperty("warningDuration").floatValue = 0.6f;
            soSpawner.ApplyModifiedProperties();

            // Ocultar templates originales
            rockTemplate.SetActive(false);
            warnTemplate.SetActive(false);

            // 2. Plataforma MÃ³vil horizontal (MovingPlatform)
            GameObject platMovil = new GameObject("Plataforma_Movil");
            platMovil.layer = layerSuelo;
            platMovil.transform.position = new Vector3(0.5f, -0.5f, 0f);

            var colPm = platMovil.AddComponent<BoxCollider2D>();
            colPm.size = new Vector2(3f, 0.5f);

            var srPm = platMovil.AddComponent<SpriteRenderer>();
            srPm.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Movil.png");
            srPm.sortingLayerName = Constantes.SortingLayers.Escenario;

            var mp = platMovil.AddComponent<MovingPlatform>();
            SerializedObject soMp = new SerializedObject(mp);
            soMp.FindProperty("moveDistance").floatValue = 2.5f;
            soMp.FindProperty("speed").floatValue = 1.8f;
            soMp.FindProperty("waitTimeAtEnds").floatValue = 0.5f;
            soMp.ApplyModifiedProperties();

            // 3. Plataforma Quebradiza (BreakablePlatform) que colapsa a los 2s
            GameObject platQuebradiza = new GameObject("Plataforma_Quebradiza");
            platQuebradiza.layer = layerSuelo;
            platQuebradiza.transform.position = new Vector3(9.5f, 1f, 0f);

            var colPq = platQuebradiza.AddComponent<BoxCollider2D>();
            colPq.size = new Vector2(3f, 0.5f);

            var srPq = platQuebradiza.AddComponent<SpriteRenderer>();
            Sprite sprPq0 = CargarSubSprite("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png", "Plataforma_Quebradiza_0");
            Sprite sprPq1 = CargarSubSprite("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png", "Plataforma_Quebradiza_1");
            Sprite sprPq2 = CargarSubSprite("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png", "Plataforma_Quebradiza_2");
            srPq.sprite = sprPq0;
            srPq.sortingLayerName = Constantes.SortingLayers.Escenario;

            var bp = platQuebradiza.AddComponent<BreakablePlatform>();
            SerializedObject soBp = new SerializedObject(bp);
            soBp.FindProperty("spriteNormal").objectReferenceValue = sprPq0;
            soBp.FindProperty("spriteCracked").objectReferenceValue = sprPq1;
            soBp.FindProperty("spriteBreaking").objectReferenceValue = sprPq2;
            soBp.FindProperty("collapseDelay").floatValue = 2.0f;
            soBp.FindProperty("restoreDelay").floatValue = 4.0f;
            soBp.ApplyModifiedProperties();

            // 4. Bloque de Derrumbe y Zona de Derrumbe Sorpresa
            GameObject bloqueDerrumbeObj = new GameObject("Bloque_Derrumbe");
            bloqueDerrumbeObj.layer = layerSuelo;
            bloqueDerrumbeObj.transform.position = new Vector3(7f, 4.5f, 0f); // En el techo inicialmente

            var colBd = bloqueDerrumbeObj.AddComponent<BoxCollider2D>();
            colBd.size = new Vector2(2f, 3f);
            colBd.offset = new Vector2(0f, 1.5f);

            var srBd = bloqueDerrumbeObj.AddComponent<SpriteRenderer>();
            srBd.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png");
            srBd.sortingLayerName = Constantes.SortingLayers.Escenario;

            var bloqueComp = bloqueDerrumbeObj.AddComponent<BloqueDerrumbe>();

            // Zona trigger oculta de derrumbe sorpresa (X = 13, Y = 0)
            GameObject zonaDerrumbeObj = new GameObject("ZonaDerrumbe_Sorpresa");
            zonaDerrumbeObj.transform.position = new Vector3(13f, 0f, 0f);

            var colZd = zonaDerrumbeObj.AddComponent<BoxCollider2D>();
            colZd.isTrigger = true;
            colZd.size = new Vector2(2.5f, 4f);

            var impulseSource = zonaDerrumbeObj.AddComponent<CinemachineImpulseSource>();
            var caveIn = zonaDerrumbeObj.AddComponent<CaveInZone>();

            SerializedObject soCi = new SerializedObject(caveIn);
            soCi.FindProperty("blockObject").objectReferenceValue = bloqueComp;
            soCi.FindProperty("blockDropPosition").vector3Value = new Vector3(7f, -0.5f, 0f); // Bloquea el camino detrÃ¡s
            soCi.FindProperty("impulseSource").objectReferenceValue = impulseSource;
            soCi.ApplyModifiedProperties();

            // 5. Temblor periÃ³dico de mina (MineTremor) cada 20-25s
            GameObject tremorObj = new GameObject("[MineTremor]");
            tremorObj.AddComponent<CinemachineImpulseSource>();
            tremorObj.AddComponent<MineTremor>();
        }

        public static void AsignarCapasALuz2D(Light2D light)
        {
            if (light == null) return;
            int[] allSortingLayerIDs = new int[SortingLayer.layers.Length];
            for (int i = 0; i < SortingLayer.layers.Length; i++)
            {
                allSortingLayerIDs[i] = SortingLayer.layers[i].id;
            }

            SerializedObject soLight = new SerializedObject(light);
            var propLayers = soLight.FindProperty("m_ApplyToSortingLayers");
            if (propLayers != null)
            {
                propLayers.arraySize = allSortingLayerIDs.Length;
                for (int i = 0; i < allSortingLayerIDs.Length; i++)
                {
                    propLayers.GetArrayElementAtIndex(i).intValue = allSortingLayerIDs[i];
                }
                soLight.ApplyModifiedProperties();
            }
        }

        private static Sprite CargarSubSprite(string texturePath, string spriteName)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            foreach (var a in assets)
            {
                if (a is Sprite s && s.name == spriteName)
                {
                    return s;
                }
            }
            return AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
        }

        // ====================================================================
        // 8. CONFIGURAR BUILD SETTINGS Y PLAYER SETTINGS
        // ====================================================================
        [MenuItem(MenuRoot + "8. Configurar Build Settings", priority = 80)]
        public static void ConfigurarBuildSettingsYPlayer()
        {
            string[] rutasEscenas = new string[]
            {
                "Assets/Scenes/Splash.unity",
                "Assets/Scenes/MenuPrincipal.unity",
                "Assets/Scenes/Intro.unity",
                "Assets/Scenes/Nivel1_MinaRayas.unity",
                "Assets/Scenes/Nivel2_ElNopal.unity",
                "Assets/Scenes/Nivel3_LaValenciana.unity",
                "Assets/Scenes/Final.unity"
            };

            var buildScenes = new EditorBuildSettingsScene[rutasEscenas.Length];
            for (int i = 0; i < rutasEscenas.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(rutasEscenas[i], true);
            }
            EditorBuildSettings.scenes = buildScenes;

            PlayerSettings.companyName = "UTNG - Camarillo Olaez";
            PlayerSettings.productName = "El Minero";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

            // Icono del juego
            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/Jugador/Minero_Retrato.png");
            if (icon != null)
            {
                PlayerSettings.SetIcons(UnityEditor.Build.NamedBuildTarget.Unknown, new Texture2D[] { icon }, IconKind.Application);
            }

            // Android
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;

            // WebGL
            PlayerSettings.WebGL.decompressionFallback = true;

            AssetDatabase.SaveAssets();
            Debug.Log("[El Minero] Build Settings y Player Settings configurados correctamente.");
        }

        // ====================================================================
        // â˜… CONSTRUIR TODO (OPCIÃ“N DEFINITIVA)
        // ====================================================================
        [MenuItem(MenuRoot + "â˜… Construir TODO", priority = 0)]
        public static void ConstruirTodoElProyecto()
        {
            EditorUtility.DisplayProgressBar("El Minero", "Construyendo todo el proyecto...", 0.05f);

            try
            {
                Debug.Log("[El Minero] === INICIANDO CONSTRUCCIÃ“N COMPLETA ===");

                // 1. Crear carpetas
                CrearCarpetas();
                EditorUtility.DisplayProgressBar("El Minero", "1/10 Carpetas...", 0.10f);

                // 2. Configurar Tags, Layers y Matriz
                ConfigurarTagsYLayers();
                EditorUtility.DisplayProgressBar("El Minero", "2/10 Tags y Layers...", 0.20f);

                // 3. Crear AudioMixer
                AudioMixerBuilder.CrearMixer();
                AudioLibraryBuilder.GenerarAudioLibrary();
                EditorUtility.DisplayProgressBar("El Minero", "3/10 AudioMixer y AudioLibrary...", 0.30f);

                // 4. Configurar importaciÃ³n de sprites y tilesets
                SpriteImportSetup.ConfigurarTodosLosSprites();
                EditorUtility.DisplayProgressBar("El Minero", "4/10 Sprites y Tilesets...", 0.40f);

                // 5. Crear animaciones y Animator Controllers
                AnimationGenerator.GenerarTodasLasAnimaciones();
                EditorUtility.DisplayProgressBar("El Minero", "5/10 Animaciones...", 0.50f);

                // 6. Crear Fuentes y Prefabs de UI
                FontAssetBuilder.GenerarFontAssets();
                UIPrefabBuilder.CrearTodosLosPrefabsUI();
                EditorUtility.DisplayProgressBar("El Minero", "6/10 UI y Fuentes...", 0.60f);

                // 7. Crear materiales fÃ­sicos y LevelConfigs
                CrearMaterialesFisicosYConfigs();
                EditorUtility.DisplayProgressBar("El Minero", "7/10 FÃ­sicas y Configs...", 0.70f);

                // 8. Crear Prefabs de juego
                PrefabGenerator.CrearTodosLosPrefabs();
                EditorUtility.DisplayProgressBar("El Minero", "8/10 Prefabs...", 0.80f);

                // 9. Construir las 7 Escenas
                LevelBuilder.ConstruirTodasLasEscenas();
                EditorUtility.DisplayProgressBar("El Minero", "9/10 Escenas...", 0.90f);

                // 10. Configurar Build Settings y Player Settings
                BuildSettingsSetup.ConfigurarBuildSettings();
                EditorUtility.DisplayProgressBar("El Minero", "10/10 Build Settings...", 1.0f);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[El Minero] === ¡PROYECTO COMPLETO CONSTRUIDO CON ÉXITO! ===");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("El Minero — Construcción Completa",
                        "¡El videojuego 'El Minero' ha sido construido completamente!\n\n" +
                        "✓ Carpetas, Tags, Layers y Sorting Layers.\n" +
                        "✓ AudioMixer con buses Música y Efectos.\n" +
                        "✓ Sprites importados con PPU=16 y Point Filter.\n" +
                        "✓ Animaciones y Animator Controllers generados.\n" +
                        "✓ TextMeshPro Font Assets (PressStart2P y PixelifySans).\n" +
                        "✓ Prefabs de UI modales y HUD completo.\n" +
                        "✓ Físicas (SinFriccion) y LevelConfigs (N1: 100s, N2: 120s, N3: 140s).\n" +
                        "✓ Prefabs completos de jugador, enemigos, obstáculos y coleccionables.\n" +
                        "✓ Las 7 escenas construidas y enlazadas en Build Settings:\n" +
                        "   • Splash.unity\n" +
                        "   • MenuPrincipal.unity\n" +
                        "   • Intro.unity\n" +
                        "   • Nivel1_MinaRayas.unity\n" +
                        "   • Nivel2_ElNopal.unity\n" +
                        "   • Nivel3_LaValenciana.unity\n" +
                        "   • Final.unity\n\n" +
                        "Para jugar:\n" +
                        "1. Abre 'Assets/Scenes/Splash.unity' o 'Assets/Scenes/MenuPrincipal.unity'\n" +
                        "2. Pulsa Play y disfruta de la aventura.",
                        "¡Entendido!");
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
