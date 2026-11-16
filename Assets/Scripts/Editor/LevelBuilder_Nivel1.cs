using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ElMinero.Entorno;
using ElMinero.Managers;
using ElMinero.Niveles;
using ElMinero.Utilidades;

namespace ElMinero.Editor
{
    /// <summary>
    /// Nivel 1 — Mina de Rayas (diseño v2).
    /// La jugabilidad es la misma (130 u, 6 vetas, 2 gemas, 4 huecos, 7 lanzadores de rocas,
    /// 2 derrumbes, 3 checkpoints); lo que cambia es la ambientación:
    /// - Ademes de madera (postes y vigas) a lo largo del túnel, rieles en el piso y vagonetas.
    /// - Plataformas colgadas del techo con cuerdas, lámparas de mina colgantes con luz.
    /// - Estalactitas variadas, escombro en el piso, vetas de plata en la roca con destellos.
    /// - Huecos con profundidad (degradado), siluetas de roca en primer plano, polvo y goteras.
    /// - Letreros "PELIGRO" y "SALIDA". Checkpoints y derrumbes bien apoyados en el piso.
    /// Coordenadas: la superficie del piso está en y = -1 y el techo empieza en y = 6.
    /// </summary>
    public static partial class LevelBuilder
    {
        private const string RutaMina = "Assets/Sprites/Entorno/Mina/";

        // Huecos (x inicial y final, inclusive) y plataformas fijas (x, y)
        private static readonly Vector2Int[] HuecosN1 = { new Vector2Int(18, 22), new Vector2Int(66, 70), new Vector2Int(84, 88), new Vector2Int(106, 110) };
        private static readonly Vector2[] PlataformasN1 =
        {
            new Vector2(5f, 0.3f), new Vector2(20f, 0.2f), new Vector2(35f, 0.3f), new Vector2(39f, 1.4f), new Vector2(43f, 0.4f),
            new Vector2(54f, 0.3f), new Vector2(58f, 1.4f), new Vector2(68f, 0.2f), new Vector2(77f, 0.4f), new Vector2(86f, 0.3f),
            new Vector2(98f, 0.4f), new Vector2(108f, 0.2f), new Vector2(115f, 0.5f)
        };

        private static bool EsHuecoN1(int x)
        {
            foreach (var h in HuecosN1) if (x >= h.x && x <= h.y) return true;
            return false;
        }

        // Pseudoaleatorio determinista por celda (el nivel sale igual en cada construcción)
        private static float Azar(int x, int y, int semilla)
        {
            unchecked
            {
                int h = x * 73856093 ^ y * 19349663 ^ semilla * 83492791;
                h = (h << 13) ^ h;
                int v = (h * (h * h * 15731 + 789221) + 1376312589) & 0x7fffffff;
                return v / 2147483647f;
            }
        }

        public static void ConstruirNivel1_MinaRayas()
        {
            SpriteImportSetup.ConfigurarSpritesMina();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Color bgVioleta = new Color(0.12f, 0.09f, 0.19f); // #1E1630
            Color luzAmbar = new Color(0.96f, 0.83f, 0.54f);

            // 1. Iluminación: un poco más oscura para que luzcan antorchas y lámparas
            CrearIluminacionNivel(bgVioleta, 0.62f);

            // 2. Fondo de cueva en 3 capas (el original)
            CrearParallaxNivel("MinaRayas", 130f);

            // 3. Grid y Tilemaps
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 8;
            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();

            Tilemap tmEstructura = CrearTilemap(gridObj, "Tilemap_Ademes", Constantes.SortingLayers.FondoMedio, 0, false);
            var rendEstructura = tmEstructura.GetComponent<TilemapRenderer>();
            rendEstructura.sortingOrder = 5;
            tmEstructura.color = new Color(0.72f, 0.66f, 0.74f);

            Tilemap tmSuelo = CrearTilemap(gridObj, "Tilemap_Suelo", Constantes.SortingLayers.Escenario, layerSuelo, true);
            Tilemap tmDeco = CrearTilemap(gridObj, "Tilemap_Decoracion", Constantes.SortingLayers.Objetos, 0, false);
            tmDeco.GetComponent<TilemapRenderer>().sortingOrder = -3;

            var vetas = new List<Vector2>();
            var goteras = new List<float>();
            ConstruirTerrenoNivel1(tmSuelo, tmDeco, tmEstructura, vetas, goteras);
            FinalizarColisionesTilemap(tmSuelo);

            // 4. Huecos letales (+ degradado de profundidad)
            CrearHuecoLetal(new Vector3(20f, -4f, 0f), new Vector2(5f, 2f));
            CrearHuecoLetal(new Vector3(68f, -4f, 0f), new Vector2(5f, 2f));
            CrearHuecoLetal(new Vector3(86f, -4f, 0f), new Vector2(5f, 2f));
            CrearHuecoLetal(new Vector3(108f, -4f, 0f), new Vector2(5f, 2f));
            Sprite spAbismo = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMina + "Abismo.png");
            foreach (var h in HuecosN1)
            {
                GameObject ab = new GameObject("Abismo");
                ab.transform.position = new Vector3((h.x + h.y + 1) * 0.5f, -3f, 0f); // ocupa de y = -1 a y = -5
                var sr = ab.AddComponent<SpriteRenderer>();
                sr.sprite = spAbismo;
                sr.drawMode = SpriteDrawMode.Sliced;
                sr.size = new Vector2(h.y - h.x + 1, 4f);
                sr.sortingLayerName = Constantes.SortingLayers.Escenario;
                sr.sortingOrder = 2;
            }

            // 5. Plataformas fijas colgadas del techo con cuerdas
            Sprite spCable = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Cable.png");
            foreach (Vector2 p in PlataformasN1)
            {
                CrearPlataformaFija(new Vector3(p.x, p.y, 0f), layerSuelo);
                foreach (float dx in new[] { -1.3f, 1.3f })
                {
                    float y0 = p.y + 0.2f;
                    float largo = 6f - y0;
                    CrearCuerda(spCable, new Vector3(p.x + dx, y0 + largo * 0.5f, 0f), largo);
                }
            }

            // 6. Coleccionables (6 vetas de plata, 2 gemas) y mineral distintivo
            CrearVetaPlata(new Vector3(5f, 1.3f, 0f));
            CrearVetaPlata(new Vector3(39f, 2.4f, 0f));
            CrearVetaPlata(new Vector3(58f, 2.4f, 0f));
            CrearVetaPlata(new Vector3(77f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(98f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(115f, 1.5f, 0f));
            CrearGemaAmatista(new Vector3(20f, 1.4f, 0f));
            CrearGemaAmatista(new Vector3(86f, 1.5f, 0f));
            CrearMineralDistintivo("Mineral_Aguilarita", new Vector3(58f, 3.5f, 0f));

            // Enemigos: 2 murciélagos
            CrearMurcielago(new Vector3(25f, 2.5f, 0f), 5f);
            CrearMurcielago(new Vector3(75f, 2.5f, 0f), 5f);

            // 7. Lanzadores de rocas
            CrearRockSpawner(new Vector3(12f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(28f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(48f, 7f, 0f), 2.0f);
            CrearRockSpawner(new Vector3(64f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(80f, 7f, 0f), 2.0f);
            CrearRockSpawner(new Vector3(95f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(112f, 7f, 0f), 1.8f);

            // 8. Checkpoints sobre el piso (y = -1)
            CrearCheckpoint(new Vector3(30f, -1f, 0f));
            CrearCheckpointCapilla(new Vector3(73f, -1f, 0f));
            CrearCheckpoint(new Vector3(102f, -1f, 0f));

            // 9. Derrumbes: el bloque cae DETRÁS del jugador y siempre sobre piso firme
            //    (antes el segundo caía sobre el borde del hueco 3 y se quedaba flotando)
            CrearZonaDerrumbe(new Vector3(50f, 0f, 0f), new Vector3(46.5f, -1f, 0f));
            CrearZonaDerrumbe(new Vector3(94f, 0f, 0f), new Vector3(90.5f, -1f, 0f));

            // 10. Temblor periódico
            CrearMineTremor();

            // 11. Salida
            CrearSalida(new Vector3(124f, -1f, 0f), false);

            // 12. Ambientación: antorchas, lámparas colgantes, rieles, vagonetas, letreros
            foreach (float xa in new[] { -5f, 14f, 32f, 50f, 70f, 90f, 108f, 122f })
            {
                CrearAntorchaConLuz(new Vector3(xa, 0.8f, 0f), luzAmbar);
            }
            Sprite spLampara = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMina + "Lampara_Mina.png");
            foreach (float xl in new[] { 3f, 23f, 41f, 61f, 80f, 100f, 118f })
            {
                CrearCuerda(spCable, new Vector3(xl, 5.3f, 0f), 1.4f);
                CrearLamparaColgante(spLampara, new Vector3(xl, 4.6f, 0f));
            }

            Sprite spRiel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Riel.png");
            GameObject raizRieles = new GameObject("[Rieles]");
            for (int x = -11; x <= 127; x++)
            {
                if (EsHuecoN1(x)) continue;
                GameObject r = new GameObject("Riel");
                r.transform.SetParent(raizRieles.transform);
                r.transform.position = new Vector3(x + 0.5f, -0.75f, 0f);
                var sr = r.AddComponent<SpriteRenderer>();
                sr.sprite = spRiel;
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = -2;
            }
            Sprite spVagoneta = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Vagoneta_Deco.png");
            foreach (float xv in new[] { 12.5f, 81f })
            {
                GameObject v = new GameObject("Vagoneta_Deco");
                v.transform.position = new Vector3(xv, -0.25f, 0f);
                var sr = v.AddComponent<SpriteRenderer>();
                sr.sprite = spVagoneta;
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = -1;
            }
            CrearLetreroMina(new Vector3(14.6f, -1f, 0f), "PELIGRO");
            CrearLetreroMina(new Vector3(119.5f, -1f, 0f), "SALIDA");

            // Siluetas de roca en primer plano (arriba y abajo de la vista)
            string[] abajo = { "Roca_Frente_Abajo_A", "Roca_Frente_Abajo_B" };
            string[] arriba = { "Roca_Frente_Arriba_A", "Roca_Frente_Arriba_B" };
            GameObject raizFrente = new GameObject("[Primer_Plano]");
            int k = 0;
            for (float xf = -12f; xf <= 130f; xf += 9f, k++)
            {
                CrearSilueta(raizFrente.transform, RutaMina + abajo[k % 2] + ".png", new Vector3(xf + (k % 3), -2.9f, 0f));
                CrearSilueta(raizFrente.transform, RutaMina + arriba[(k + 1) % 2] + ".png", new Vector3(xf + 4f + (k % 2), 8.7f, 0f));
            }

            // Polvo, goteras y destellos de las vetas
            GameObject ambienteObj = new GameObject("[Ambiente_Cueva]");
            var ambiente = ambienteObj.AddComponent<AmbienteCueva>();
            var soAmb = new SerializedObject(ambiente);
            soAmb.FindProperty("spritePolvo").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Menu/Menu_Luciernaga.png");
            soAmb.FindProperty("spriteGota").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMina + "Gota.png");
            soAmb.FindProperty("spriteDestello").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Menu/Menu_Destello.png");
            soAmb.FindProperty("yTecho").floatValue = 5.3f;
            soAmb.FindProperty("yPiso").floatValue = -1f;
            var propGot = soAmb.FindProperty("xGoteras");
            propGot.arraySize = goteras.Count;
            for (int i = 0; i < goteras.Count; i++) propGot.GetArrayElementAtIndex(i).floatValue = goteras[i];
            var propVet = soAmb.FindProperty("puntosVetas");
            propVet.arraySize = vetas.Count;
            for (int i = 0; i < vetas.Count; i++) propVet.GetArrayElementAtIndex(i).vector2Value = vetas[i];
            soAmb.ApplyModifiedProperties();

            // 13. Jugador
            GameObject jugador = CrearJugador(new Vector3(-7f, 0.5f, 0f), layerSuelo);

            // 14. Cámara
            ConfigurarCamaraNivel(jugador, bgVioleta, new Vector2(-15f, -6f), new Vector2(135f, 12f));

            // 15. Managers y HUD
            var configN1 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Settings/Niveles/LevelConfig_Nivel1.asset");
            ConfigurarManagersYHUD(jugador, configN1, 6, 2);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Nivel1_MinaRayas.unity");
        }

        // ====================================================================
        // TERRENO
        // ====================================================================
        private static void ConstruirTerrenoNivel1(Tilemap tmSuelo, Tilemap tmDeco, Tilemap tmEstructura, List<Vector2> vetas, List<float> goteras)
        {
            string nivel = "MinaRayas";
            Tile t0 = CargarTile(nivel, 0);   // superficie, orilla izquierda
            Tile t1 = CargarTile(nivel, 1);   // superficie
            Tile t2 = CargarTile(nivel, 2);   // superficie, orilla derecha
            Tile t9 = CargarTile(nivel, 9);   // relleno
            Tile t12 = CargarTile(nivel, 12); // relleno con veta de plata
            Tile t17 = CargarTile(nivel, 17); // techo
            Tile t7 = CargarTile(nivel, 7);   // estalactita
            Tile t13 = CargarTile(nivel, 13); // estalagmita (fondo)
            Tile t14 = CargarTile(nivel, 14); // escombro
            Tile t22 = CargarTile(nivel, 22); // viga de madera
            Tile t23 = CargarTile(nivel, 23); // poste de madera

            System.Func<int, int, Tile> Relleno = (x, y) =>
            {
                if (t12 != null && Azar(x, y, 4) < 0.07f)
                {
                    vetas.Add(new Vector2(x + 0.5f, y + 0.5f));
                    return t12;
                }
                return t9;
            };

            for (int x = -12; x <= 128; x++)
            {
                bool esHueco = EsHuecoN1(x);
                if (!esHueco)
                {
                    Tile sup = (x == -12) ? t0 : (x == 128 ? t2 : t1);
                    if (EsHuecoN1(x + 1)) sup = t2;
                    else if (EsHuecoN1(x - 1)) sup = t0;

                    tmSuelo.SetTile(new Vector3Int(x, -2, 0), sup);
                    tmSuelo.SetTile(new Vector3Int(x, -3, 0), Relleno(x, -3));
                    tmSuelo.SetTile(new Vector3Int(x, -4, 0), Relleno(x, -4));
                }

                // Techo
                tmSuelo.SetTile(new Vector3Int(x, 6, 0), t17);
                tmSuelo.SetTile(new Vector3Int(x, 7, 0), Relleno(x, 7));

                // Estalactitas irregulares (y goteras en algunas)
                if (t7 != null && Azar(x, 5, 1) < 0.3f)
                {
                    tmDeco.SetTile(new Vector3Int(x, 5, 0), t7);
                    if (Azar(x, 5, 2) < 0.45f) goteras.Add(x + 0.5f);
                }

                // Escombro y estalagmitas en el piso (lejos de los huecos)
                if (!esHueco && !EsHuecoN1(x - 1) && !EsHuecoN1(x + 1) && x > -11 && x < 127)
                {
                    float r = Azar(x, -1, 3);
                    if (r < 0.12f && t14 != null) tmDeco.SetTile(new Vector3Int(x, -1, 0), t14);
                    else if (r < 0.2f && t13 != null) tmEstructura.SetTile(new Vector3Int(x, -1, 0), t13);
                }
            }

            // Paredes laterales de la mina
            for (int y = -1; y <= 5; y++)
            {
                tmSuelo.SetTile(new Vector3Int(-12, y, 0), t9);
                tmSuelo.SetTile(new Vector3Int(128, y, 0), t9);
            }

            // Ademes de madera: dos postes y una viga cada tramo (fuera de los huecos)
            foreach (int xa in new[] { -9, 7, 25, 47, 60, 76, 95, 116 })
            {
                for (int y = -1; y <= 4; y++)
                {
                    tmEstructura.SetTile(new Vector3Int(xa, y, 0), t23);
                    tmEstructura.SetTile(new Vector3Int(xa + 4, y, 0), t23);
                }
                for (int x = xa; x <= xa + 4; x++)
                {
                    tmEstructura.SetTile(new Vector3Int(x, 5, 0), t22);
                    tmDeco.SetTile(new Vector3Int(x, 5, 0), null); // sin estalactitas sobre la viga
                }
                goteras.RemoveAll(g => g >= xa && g <= xa + 5);
            }
        }

        // ====================================================================
        // HELPERS DE AMBIENTACIÓN
        // ====================================================================
        private static void CrearCuerda(Sprite cable, Vector3 centro, float largo)
        {
            GameObject c = new GameObject("Cuerda");
            c.transform.position = centro;
            var sr = c.AddComponent<SpriteRenderer>();
            sr.sprite = cable;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(0.125f, Mathf.Max(0.1f, largo));
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
            sr.sortingOrder = -1;
            sr.color = new Color(0.8f, 0.72f, 0.6f);
        }

        private static void CrearLamparaColgante(Sprite lampara, Vector3 pos)
        {
            GameObject l = new GameObject("Lampara_Mina");
            l.transform.position = pos;
            var sr = l.AddComponent<SpriteRenderer>();
            sr.sprite = lampara;
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;
            sr.sortingOrder = 1;

            GameObject luzObj = new GameObject("Luz");
            luzObj.transform.SetParent(l.transform, false);
            luzObj.transform.localPosition = new Vector3(0f, -0.45f, 0f);
            var luz = luzObj.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.color = new Color(1f, 0.78f, 0.45f);
            luz.intensity = 0.9f;
            luz.pointLightInnerRadius = 0.3f;
            luz.pointLightOuterRadius = 3f;
            ElMineroBuilder.AsignarCapasALuz2D(luz);
            luzObj.AddComponent<FlickeringLight2D>();
        }

        private static void CrearLetreroMina(Vector3 pos, string texto)
        {
            // Raíz en el piso; el dibujo se acomoda para que la base del poste quede en "pos"
            // sin importar el pivote con que se haya importado el sprite.
            GameObject l = new GameObject("Letrero_" + texto);
            l.transform.position = pos;
            l.transform.localScale = new Vector3(1.25f, 1.25f, 1f);

            Sprite spLetrero = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Letrero.png");
            GameObject dibujo = new GameObject("Dibujo");
            dibujo.transform.SetParent(l.transform, false);
            var sr = dibujo.AddComponent<SpriteRenderer>();
            sr.sprite = spLetrero;
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;
            sr.sortingOrder = -1;

            float alto = 1.5f; // 24 px
            if (spLetrero != null)
            {
                dibujo.transform.localPosition = new Vector3(0f, -spLetrero.bounds.min.y, 0f);
                alto = spLetrero.bounds.size.y;
            }

            // La tabla ocupa los 17 px de arriba: su centro está 8.5 px (0.53 u) bajo el borde superior
            GameObject t = new GameObject("Texto");
            t.transform.SetParent(l.transform, false);
            t.transform.localPosition = new Vector3(0f, alto - 0.53f, 0f);
            var tmp = t.AddComponent<TextMeshPro>();
            tmp.rectTransform.sizeDelta = new Vector2(1.7f, 0.6f);
            tmp.font = FontAssetBuilder.ObtenerPressStart2P();
            tmp.text = texto;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 0.5f;
            tmp.fontSizeMax = 3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            tmp.color = new Color(0.27f, 0.13f, 0.05f);
            tmp.sortingLayerID = SortingLayer.NameToID(Constantes.SortingLayers.Objetos);
            tmp.sortingOrder = 0;
        }

        private static void CrearSilueta(Transform padre, string ruta, Vector3 pos)
        {
            GameObject s = new GameObject("Silueta_Roca");
            s.transform.SetParent(padre);
            s.transform.position = pos;
            var sr = s.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
            sr.sortingLayerName = Constantes.SortingLayers.PrimerPlano;
            sr.sortingOrder = 10;
        }
    }
}
