using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ElMinero.Camara;
using ElMinero.Final;
using ElMinero.Jugador;
using ElMinero.Utilidades;
using ElMinero.UI;

namespace ElMinero.Editor
{
    /// <summary>
    /// Escena final jugable "Final_Templo": el minero sale por el Tiro de San Cayetano con las
    /// joyas del Coloso, las reparte entre la gente de La Valenciana y entra al Templo de San Cayetano
    /// mientras el pueblo celebra con campanas, cohetes y confeti.
    /// Coordenadas en unidades; suelo en y = 0.
    /// </summary>
    public static partial class LevelBuilder
    {
        private const string RutaFinal = "Assets/Sprites/Final/";

        public static void ConstruirFinalTemplo()
        {
            // Importación de los sprites de la escena (pivotes abajo-centro, hojas de habitantes)
            SpriteImportSetup.ConfigurarSpritesJefeYFinal();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 8;

            // ---------------------------------------------------------------- Cámara (ortho 8)
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(14f, 6f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.99f, 0.80f, 0.60f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();
            camObj.AddComponent<CameraShake>();

            // ---------------------------------------------------------------- Luz del amanecer
            GameObject luzObj = new GameObject("Global Light 2D (Amanecer)");
            var luzGlobal = luzObj.AddComponent<Light2D>();
            luzGlobal.lightType = Light2D.LightType.Global;
            luzGlobal.color = new Color(1f, 0.886f, 0.69f); // #FFE2B0
            luzGlobal.intensity = 0.6f;
            ElMineroBuilder.AsignarCapasALuz2D(luzGlobal);

            // Rayos de sol detrás del templo (Light2D Freeform en abanico)
            GameObject rayos = new GameObject("Rayos_Sol");
            rayos.transform.position = new Vector3(46f, 15f, 0f);
            var luzRayos = rayos.AddComponent<Light2D>();
            luzRayos.lightType = Light2D.LightType.Freeform;
            luzRayos.SetShapePath(new Vector3[]
            {
                new Vector3(0f, 0f, 0f), new Vector3(-9f, -15f, 0f), new Vector3(-3f, -15f, 0f),
                new Vector3(0f, -2f, 0f), new Vector3(3f, -15f, 0f), new Vector3(9f, -15f, 0f)
            });
            luzRayos.color = new Color(1f, 0.9f, 0.55f);
            luzRayos.intensity = 0.35f;
            luzRayos.shapeLightFalloffSize = 2.5f;
            ElMineroBuilder.AsignarCapasALuz2D(luzRayos);

            // ---------------------------------------------------------------- Fondo (3 capas)
            string[] capas = { "FondoFinal_Capa0_Amanecer", "FondoFinal_Capa1_Pueblo", "FondoFinal_Capa2_Cerro" };
            var transformsCapas = new Transform[capas.Length];
            for (int i = 0; i < capas.Length; i++)
            {
                Sprite sp = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Fondos/Final/{capas[i]}.png");
                GameObject capa = new GameObject(capas[i]);
                float alto = sp != null ? sp.bounds.size.y : 16.875f;
                float ancho = sp != null ? sp.bounds.size.x : 31f;
                capa.transform.position = new Vector3(30f, -2f + alto * 0.5f, 0f);
                var sr = capa.AddComponent<SpriteRenderer>();
                sr.sprite = sp;
                sr.drawMode = SpriteDrawMode.Tiled;
                sr.size = new Vector2(ancho * 4f, alto);
                sr.sortingLayerName = Constantes.SortingLayers.Fondo;
                sr.sortingOrder = -30 + i * 10;
                transformsCapas[i] = capa.transform;
            }

            // ---------------------------------------------------------------- Suelo y barda
            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();
            Tilemap tmSuelo = CrearTilemap(gridObj, "Tilemap_Suelo", Constantes.SortingLayers.Escenario, layerSuelo, true);
            Tilemap tmDeco = CrearTilemap(gridObj, "Tilemap_Decoracion", Constantes.SortingLayers.Escenario, 0, false);
            tmDeco.GetComponent<TilemapRenderer>().sortingOrder = -2;

            var tiles = EditorSpriteUtil.TilesPorIndice("Assets/Tiles/Exterior");
            System.Func<int, Tile> T = i => tiles.TryGetValue(i, out Tile t) ? t : null;

            for (int x = -2; x <= 62; x++)
            {
                int sup;
                if (x <= 29) sup = x == -2 ? 0 : (x == 29 ? 2 : 1); // empedrado
                else sup = 3;                                        // losa de cantera del atrio
                tmSuelo.SetTile(new Vector3Int(x, -1, 0), T(sup));
                tmSuelo.SetTile(new Vector3Int(x, -2, 0), T(9));
                tmSuelo.SetTile(new Vector3Int(x, -3, 0), T(9));
            }
            for (int x = 11; x <= 28; x++)
            {
                tmDeco.SetTile(new Vector3Int(x, 0, 0), T(7)); // cuerpo de la barda
                tmDeco.SetTile(new Vector3Int(x, 1, 0), T(6)); // remate
            }
            FinalizarColisionesTilemap(tmSuelo);

            // Paredes invisibles
            foreach (float xp in new[] { -2.5f, 62.5f })
            {
                GameObject pared = new GameObject("Pared_Invisible");
                pared.layer = layerSuelo;
                pared.transform.position = new Vector3(xp, 10f, 0f);
                var col = pared.AddComponent<BoxCollider2D>();
                col.size = new Vector2(1f, 30f);
            }

            // ---------------------------------------------------------------- Tiro (salida de la mina) y templo
            GameObject tiro = CrearSpriteSuelo("Tiro_SanCayetano", RutaFinal + "Tiro_SanCayetano.png", 4f, Constantes.SortingLayers.Objetos, -1);
            GameObject luzTiro = new GameObject("Luz_Tiro");
            luzTiro.transform.SetParent(tiro.transform, false);
            luzTiro.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var lt = luzTiro.AddComponent<Light2D>();
            lt.lightType = Light2D.LightType.Point;
            lt.color = new Color(1f, 0.62f, 0.3f);
            lt.pointLightOuterRadius = 2.6f;
            lt.pointLightInnerRadius = 0.3f;
            lt.intensity = 0.9f;
            ElMineroBuilder.AsignarCapasALuz2D(lt);
            luzTiro.AddComponent<ElMinero.Entorno.FlickeringLight2D>();

            GameObject templo = CrearSpriteSuelo("Templo_SanCayetano", RutaFinal + "Templo_SanCayetano.png", 46f, Constantes.SortingLayers.Objetos, -4);
            Sprite[] cuadrosCampana = EditorSpriteUtil.Cuadros(RutaFinal + "Campana.png");
            var campanas = new List<SpriteFrameAnimator>();
            foreach (Vector2 pc in new[] { new Vector2(-4.0f, 10.375f), new Vector2(-3.0f, 10.375f), new Vector2(-3.5f, 12.125f) })
            {
                GameObject c = new GameObject("Campana");
                c.transform.SetParent(templo.transform, false);
                c.transform.localPosition = new Vector3(pc.x, pc.y, 0f);
                var sr = c.AddComponent<SpriteRenderer>();
                sr.sprite = cuadrosCampana.Length > 0 ? cuadrosCampana[0] : null;
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = -3;
                var an = c.AddComponent<SpriteFrameAnimator>();
                an.Configurar(cuadrosCampana, 6f, true, false);
                campanas.Add(an);
            }
            GameObject luzPuerta = new GameObject("Luz_Puerta");
            luzPuerta.transform.SetParent(templo.transform, false);
            luzPuerta.transform.localPosition = new Vector3(0.06f, 3.0f, 0f);
            var lp = luzPuerta.AddComponent<Light2D>();
            lp.lightType = Light2D.LightType.Point;
            lp.color = new Color(1f, 0.824f, 0.478f); // #FFD27A
            lp.pointLightOuterRadius = 3f;
            lp.pointLightInnerRadius = 0.5f;
            lp.intensity = 1.2f;
            ElMineroBuilder.AsignarCapasALuz2D(lp);

            // ---------------------------------------------------------------- Plaza: puestos, árbol, faroles, macetas
            CrearSpriteSuelo("Puesto_Rosa", RutaFinal + "Puesto_Rosa.png", 15f, Constantes.SortingLayers.Objetos, -3);
            CrearSpriteSuelo("Puesto_Verde", RutaFinal + "Puesto_Verde.png", 26.5f, Constantes.SortingLayers.Objetos, -3);
            CrearSpriteSuelo("Arbol_Pirul", RutaFinal + "Arbol_Pirul.png", 21f, Constantes.SortingLayers.Objetos, -3);
            CrearSpriteSuelo("Maguey", RutaFinal + "Maguey.png", 10f, Constantes.SortingLayers.Objetos, -2);
            CrearSpriteSuelo("Banca", RutaFinal + "Banca.png", 33.75f, Constantes.SortingLayers.Objetos, -1);
            CrearSpriteSuelo("Cruz_Atrial", RutaFinal + "Cruz_Atrial.png", 37.5f, Constantes.SortingLayers.Objetos, -2);
            CrearSpriteSuelo("Nopal", RutaFinal + "Nopal.png", 58f, Constantes.SortingLayers.Objetos, -2);
            foreach (float xm in new[] { 8.2f, 40.2f, 51.8f, 55f })
            {
                CrearSpriteSuelo("Maceta_Cempasuchil", RutaFinal + "Maceta_Cempasuchil.png", xm, Constantes.SortingLayers.Objetos, 0);
            }

            Sprite[] cuadrosFarol = EditorSpriteUtil.Cuadros(RutaFinal + "Farol_Plaza.png");
            var faroles = new List<SpriteFrameAnimator>();
            var lucesFaroles = new List<Light2D>();
            foreach (float xf in new[] { 18.5f, 30.5f })
            {
                GameObject farol = CrearSpriteSuelo("Farol_Plaza", RutaFinal + "Farol_Plaza.png", xf, Constantes.SortingLayers.Objetos, -1);
                var sr = farol.GetComponent<SpriteRenderer>();
                if (cuadrosFarol.Length > 1) sr.sprite = cuadrosFarol[1];
                var an = farol.AddComponent<SpriteFrameAnimator>();
                an.Configurar(cuadrosFarol, 1f, false, false);
                faroles.Add(an);

                var luz = farol.AddComponent<Light2D>();
                luz.lightType = Light2D.LightType.Point;
                luz.color = new Color(1f, 0.82f, 0.5f);
                luz.pointLightOuterRadius = 3f;
                luz.pointLightInnerRadius = 0.3f;
                luz.intensity = 1f;
                ElMineroBuilder.AsignarCapasALuz2D(luz);
                lucesFaroles.Add(luz);
            }

            // Papel picado de lado a lado de la plaza
            Sprite[] cuadrosPapel = EditorSpriteUtil.Cuadros(RutaFinal + "Papel_Picado.png");
            for (float xpp = 13f; xpp <= 37.1f; xpp += 4f)
            {
                GameObject papel = new GameObject("Papel_Picado");
                papel.transform.position = new Vector3(xpp, 6.6f, 0f);
                var sr = papel.AddComponent<SpriteRenderer>();
                sr.sprite = cuadrosPapel.Length > 0 ? cuadrosPapel[0] : null;
                sr.sortingLayerName = Constantes.SortingLayers.PrimerPlano;
                sr.sortingOrder = 0;
                var an = papel.AddComponent<SpriteFrameAnimator>();
                an.Configurar(cuadrosPapel, 3f, true, true, true);
            }

            // ---------------------------------------------------------------- Habitantes que esperan su joya
            Sprite spBurbuja = EditorSpriteUtil.Primero(RutaFinal + "Burbuja.png");
            string[] hojasJoyas = { "Gema_Amatista", "Pepita_Oro", "Pirargirita", "Aguilarita", "Veta_Plata" };
            var cuadrosJoyas = new List<Sprite>();
            foreach (string j in hojasJoyas)
            {
                Sprite[] c = EditorSpriteUtil.Cuadros($"Assets/Sprites/Coleccionables/{j}.png");
                for (int k = 0; k < 3; k++) cuadrosJoyas.Add(c.Length > 0 ? c[Mathf.Min(k, c.Length - 1)] : null);
            }

            (string archivo, string carpeta, float x, string frase)[] habitantes =
            {
                ("Senora_Rebozo",    RutaFinal + "Habitantes/", 12.5f, "¡Gracias, minero!"),
                ("Nino",             RutaFinal + "Habitantes/", 17f,   "¡Ahora sí hay pan en casa!"),
                ("Arriero",          RutaFinal + "Habitantes/", 23.5f, "¡Eres un héroe!"),
                ("Abuela",           RutaFinal + "Habitantes/", 28.5f, "¡Dios te bendiga, mijo!"),
                ("Nina",             RutaFinal + "Habitantes/", 32f,   "¡Qué bonita brilla!"),
                ("Minero_Companero", RutaFinal + "Habitantes/", 35.6f, "¡Viva La Valenciana!")
            };
            var listaHabitantes = new List<HabitanteJoya>();
            for (int i = 0; i < habitantes.Length; i++)
            {
                var hb = habitantes[i];
                Sprite[] hoja = EditorSpriteUtil.Cuadros(hb.carpeta + hb.archivo + ".png");
                GameObject go = new GameObject("Habitante_" + hb.archivo);
                go.transform.position = new Vector3(hb.x, 0f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = hoja.Length > 0 ? hoja[0] : null;
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = 2;

                // Burbuja con la joya que le toca
                GameObject burbuja = new GameObject("Burbuja");
                burbuja.transform.SetParent(go.transform, false);
                burbuja.transform.localPosition = new Vector3(0f, 2.05f, 0f);
                var srB = burbuja.AddComponent<SpriteRenderer>();
                srB.sprite = spBurbuja;
                srB.sortingLayerName = Constantes.SortingLayers.Objetos;
                srB.sortingOrder = 6;

                GameObject joya = new GameObject("Joya");
                joya.transform.SetParent(burbuja.transform, false);
                joya.transform.localPosition = new Vector3(0f, 0.6f, 0f);
                joya.transform.localScale = new Vector3(0.6f, 0.6f, 1f);
                var srJ = joya.AddComponent<SpriteRenderer>();
                int tipo = i % hojasJoyas.Length;
                Sprite[] cj = cuadrosJoyas.GetRange(tipo * 3, 3).ToArray();
                srJ.sprite = cj[0];
                srJ.sortingLayerName = Constantes.SortingLayers.Objetos;
                srJ.sortingOrder = 7;
                var anJ = joya.AddComponent<SpriteFrameAnimator>();
                anJ.Configurar(cj, 6f, true, true);

                var h = go.AddComponent<HabitanteJoya>();
                h.Configurar(hoja, hb.frase, burbuja.transform);
                listaHabitantes.Add(h);
            }

            Sprite[] hojaPaloma = EditorSpriteUtil.Cuadros(RutaFinal + "Paloma.png");
            foreach (float xpa in new[] { 39f, 43.2f, 49.5f, 53.5f })
            {
                GameObject go = new GameObject("Paloma");
                go.transform.position = new Vector3(xpa, 0f, 0f);
                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = hojaPaloma.Length > 4 ? hojaPaloma[4] : (hojaPaloma.Length > 0 ? hojaPaloma[0] : null);
                sr.sortingLayerName = Constantes.SortingLayers.Objetos;
                sr.sortingOrder = 3;
                go.AddComponent<SpriteFrameAnimator>();
                go.AddComponent<PalomaAsustadiza>().Configurar(hojaPaloma);
            }

            // Flecha sobre la puerta del templo (aparece en la fiesta)
            GameObject flecha = new GameObject("Flecha_Puerta");
            flecha.transform.position = new Vector3(46f, 5.6f, 0f);
            flecha.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            var srF = flecha.AddComponent<SpriteRenderer>();
            srF.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Iconos/Icono_FlechaAbajo.png");
            srF.color = new Color(1f, 0.85f, 0.35f);
            srF.sortingLayerName = Constantes.SortingLayers.PrimerPlano;
            srF.sortingOrder = 5;

            // Efectos de la fiesta (cohetes, chispas, corazones, confeti)
            GameObject efectosObj = new GameObject("Efectos_Fiesta");
            var efectos = efectosObj.AddComponent<EfectosFiesta>();
            var soEf = new SerializedObject(efectos);
            soEf.FindProperty("chispa").objectReferenceValue = EditorSpriteUtil.Primero(RutaFinal + "Chispa.png");
            soEf.FindProperty("destello").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Menu/Menu_Destello.png");
            soEf.FindProperty("confeti").objectReferenceValue = EditorSpriteUtil.Primero(RutaFinal + "Confeti.png");
            soEf.FindProperty("corazon").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Corazon_Lleno.png");
            soEf.FindProperty("capaCielo").stringValue = Constantes.SortingLayers.Fondo;
            soEf.FindProperty("capaFrente").stringValue = Constantes.SortingLayers.PrimerPlano;
            soEf.ApplyModifiedProperties();

            // ---------------------------------------------------------------- Jugador (empieza dentro del tiro, en la sombra)
            GameObject jugador = CrearJugador(new Vector3(4f, 0.05f, 0f), layerSuelo);
            var luzCasco = jugador.transform.Find("Luz_Casco");
            if (luzCasco != null) luzCasco.gameObject.SetActive(false); // ya es de día

            // ---------------------------------------------------------------- Audio ambiente
            GameObject ctrlObj = new GameObject("[FinalTemplo]");
            var aves = ctrlObj.AddComponent<AudioSource>();
            aves.loop = true;
            aves.playOnAwake = false;
            aves.clip = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audios/SFX/Aves_Amanecer_Loop.wav");

            // ---------------------------------------------------------------- UI: contador, avisos, fundido, panel final y créditos
            GameObject canvasObj = CrearCanvasBase("Canvas_Final");
            canvasObj.GetComponent<Canvas>().sortingOrder = 10;
            CrearControlesTactiles(canvasObj, jugador.GetComponent<PlayerInputHandler>());

            // Contador de joyas (arriba a la izquierda)
            GameObject caja = new GameObject("Contador_Joyas");
            caja.transform.SetParent(canvasObj.transform, false);
            var rtCaja = caja.AddComponent<RectTransform>();
            rtCaja.anchorMin = rtCaja.anchorMax = new Vector2(0f, 1f);
            rtCaja.pivot = new Vector2(0f, 1f);
            rtCaja.anchoredPosition = new Vector2(40f, -40f);
            rtCaja.sizeDelta = new Vector2(330f, 100f);
            var imgCaja = caja.AddComponent<Image>();
            imgCaja.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgCaja.type = Image.Type.Sliced;
            imgCaja.pixelsPerUnitMultiplier = 1.5625f;
            imgCaja.raycastTarget = false;
            var grupoCaja = caja.AddComponent<CanvasGroup>();
            grupoCaja.blocksRaycasts = false;

            GameObject icono = new GameObject("Icono_Joya");
            icono.transform.SetParent(caja.transform, false);
            var rtIc = icono.AddComponent<RectTransform>();
            rtIc.anchorMin = rtIc.anchorMax = new Vector2(0f, 0.5f);
            rtIc.pivot = new Vector2(0f, 0.5f);
            rtIc.anchoredPosition = new Vector2(24f, 0f);
            rtIc.sizeDelta = new Vector2(64f, 64f);
            var imgIc = icono.AddComponent<Image>();
            imgIc.sprite = cuadrosJoyas.Count > 0 ? cuadrosJoyas[0] : null;
            imgIc.preserveAspect = true;
            imgIc.raycastTarget = false;

            var textoContador = CrearTextoUI(caja.transform, "Texto_Contador", FontAssetBuilder.ObtenerPressStart2P(), 30f, TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(104f, 0f), new Vector2(-20f, 0f));
            textoContador.text = "0 / 6";
            textoContador.color = new Color(1f, 0.85f, 0.43f);
            textoContador.raycastTarget = false;

            CrearAvisoPantalla(canvasObj.transform);
            PanelFinalTemplo panel = CrearPanelFinalTemplo(canvasObj.transform);
            CanvasGroup fundido = CrearFundidoNegro(canvasObj.transform);
            fundido.transform.SetAsLastSibling();

            // ---------------------------------------------------------------- Controlador
            var ctrl = ctrlObj.AddComponent<FinalTemploController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("jugador").objectReferenceValue = jugador.transform;
            so.FindProperty("camara").objectReferenceValue = cam;
            so.FindProperty("fundidoNegro").objectReferenceValue = fundido;
            so.FindProperty("luzGlobal").objectReferenceValue = luzGlobal;
            so.FindProperty("panelFinal").objectReferenceValue = panel;
            so.FindProperty("ambienteAves").objectReferenceValue = aves;
            so.FindProperty("efectos").objectReferenceValue = efectos;
            so.FindProperty("fuenteFrases").objectReferenceValue = FontAssetBuilder.ObtenerPixelifySans();
            so.FindProperty("grupoContador").objectReferenceValue = grupoCaja;
            so.FindProperty("textoContador").objectReferenceValue = textoContador;
            so.FindProperty("cajaContador").objectReferenceValue = rtCaja;
            so.FindProperty("flechaPuerta").objectReferenceValue = flecha.transform;
            AsignarArreglo(so.FindProperty("capasFondo"), transformsCapas);
            AsignarArreglo(so.FindProperty("faroles"), faroles.ToArray());
            AsignarArreglo(so.FindProperty("lucesFaroles"), lucesFaroles.ToArray());
            AsignarArreglo(so.FindProperty("campanas"), campanas.ToArray());
            AsignarArreglo(so.FindProperty("habitantes"), listaHabitantes.ToArray());
            AsignarArreglo(so.FindProperty("cuadrosJoyas"), cuadrosJoyas.ToArray());
            so.ApplyModifiedProperties();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Final_Templo.unity");
            Debug.Log("[LevelBuilder] Escena Final_Templo construida.");
        }

        private static void AsignarArreglo(SerializedProperty prop, Object[] objetos)
        {
            if (prop == null) return;
            prop.arraySize = objetos.Length;
            for (int i = 0; i < objetos.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = objetos[i];
        }

        private static GameObject CrearSpriteSuelo(string nombre, string ruta, float x, string sortingLayer, int orden)
        {
            GameObject go = new GameObject(nombre);
            go.transform.position = new Vector3(x, 0f, 0f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = EditorSpriteUtil.Primero(ruta);
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = orden;
            return go;
        }

        private static CanvasGroup CrearFundidoNegro(Transform canvas)
        {
            GameObject go = new GameObject("Fundido_Negro");
            go.transform.SetParent(canvas, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            var img = go.AddComponent<Image>();
            img.color = Color.black;
            var cg = go.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            return cg;
        }

        private static TextMeshProUGUI CrearTextoUI(Transform padre, string nombre, TMP_FontAsset font, float tam, TextAlignmentOptions alin,
                                                    Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = tam;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = Mathf.Min(24f, tam);
            tmp.fontSizeMax = tam;
            tmp.alignment = alin;
            tmp.color = Color.white;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            return tmp;
        }

        private static PanelFinalTemplo CrearPanelFinalTemplo(Transform canvas)
        {
            TMP_FontAsset fTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fTexto = FontAssetBuilder.ObtenerPixelifySans();
            Sprite spPanel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBtn = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera.png");
            Sprite spBtnRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Resaltado.png");
            Sprite spBtnPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Presionado.png");

            // ---- Panel (1300 × 860)
            GameObject raiz = new GameObject("Panel_Final_Templo");
            raiz.transform.SetParent(canvas, false);
            var rt = raiz.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(1300f, 860f);
            var img = raiz.AddComponent<Image>();
            img.sprite = spPanel;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1.5625f;
            var grupo = raiz.AddComponent<CanvasGroup>();

            var titulo = CrearTextoUI(raiz.transform, "Titulo", fTitulo, 60f, TextAlignmentOptions.Center,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(72f, -156f), new Vector2(-72f, -56f));
            titulo.textWrappingMode = TextWrappingModes.NoWrap;
            titulo.color = new Color(1f, 0.85f, 0.4f);

            var cuerpo = CrearTextoUI(raiz.transform, "Texto_Cierre", fTexto, 34f, TextAlignmentOptions.Top,
                new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(80f, -380f), new Vector2(-80f, -176f));

            var puntaje = CrearTextoUI(raiz.transform, "Texto_Puntaje", fTexto, 32f, TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(110f, -450f), new Vector2(0f, -400f));
            var estrellas = CrearTextoUI(raiz.transform, "Texto_Estrellas", fTexto, 32f, TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(110f, -510f), new Vector2(0f, -460f));
            var coloso = CrearTextoUI(raiz.transform, "Texto_Coloso", fTexto, 32f, TextAlignmentOptions.MidlineLeft,
                new Vector2(0f, 1f), new Vector2(0.5f, 1f), new Vector2(110f, -570f), new Vector2(0f, -520f));
            var minerales = CrearTextoUI(raiz.transform, "Texto_Minerales", fTexto, 32f, TextAlignmentOptions.MidlineLeft,
                new Vector2(0.5f, 1f), new Vector2(1f, 1f), new Vector2(20f, -450f), new Vector2(-90f, -400f));

            // Iconos de los 3 minerales distintivos
            string[] iconos = { "Aguilarita", "Pepita_Oro", "Pirargirita" };
            var imagenes = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject ic = new GameObject("Icono_" + iconos[i]);
                ic.transform.SetParent(raiz.transform, false);
                var rti = ic.AddComponent<RectTransform>();
                rti.anchorMin = new Vector2(0.5f, 1f);
                rti.anchorMax = new Vector2(0.5f, 1f);
                rti.pivot = new Vector2(0f, 1f);
                rti.sizeDelta = new Vector2(80f, 80f);
                rti.anchoredPosition = new Vector2(40f + i * 110f, -470f);
                imagenes[i] = ic.AddComponent<Image>();
                imagenes[i].sprite = EditorSpriteUtil.Primero($"Assets/Sprites/Coleccionables/{iconos[i]}.png");
                imagenes[i].preserveAspect = true;
            }

            // Botones Créditos y Menú (abajo-centro, dentro del marco)
            GameObject fila = new GameObject("Fila_Botones");
            fila.transform.SetParent(raiz.transform, false);
            var rtf = fila.AddComponent<RectTransform>();
            rtf.anchorMin = new Vector2(0.5f, 0f);
            rtf.anchorMax = new Vector2(0.5f, 0f);
            rtf.pivot = new Vector2(0.5f, 0f);
            rtf.sizeDelta = new Vector2(800f, 84f);
            rtf.anchoredPosition = new Vector2(0f, 56f);
            var hlg = fila.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 40f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
            Button btnCreditos = UIPrefabBuilder.CrearBotonMadera(fila.transform, "Boton_Creditos", "Créditos", spBtn, spBtnRes, spBtnPres, fTexto, 36f, 84f);
            Button btnMenu = UIPrefabBuilder.CrearBotonMadera(fila.transform, "Boton_Menu", "Menú", spBtn, spBtnRes, spBtnPres, fTexto, 36f, 84f);

            // ---- Créditos con desplazamiento sobre FondoFinal_Completo
            GameObject creditos = new GameObject("Panel_Creditos");
            creditos.transform.SetParent(canvas, false);
            var rtc = creditos.AddComponent<RectTransform>();
            rtc.anchorMin = Vector2.zero;
            rtc.anchorMax = Vector2.one;
            rtc.offsetMin = Vector2.zero;
            rtc.offsetMax = Vector2.zero;
            var fondo = creditos.AddComponent<Image>();
            fondo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/Final/FondoFinal_Completo.png");
            fondo.color = Color.white;

            GameObject velo = new GameObject("Velo");
            velo.transform.SetParent(creditos.transform, false);
            var rtv = velo.AddComponent<RectTransform>();
            rtv.anchorMin = new Vector2(0.5f, 0f);
            rtv.anchorMax = new Vector2(0.5f, 1f);
            rtv.sizeDelta = new Vector2(1200f, 0f);
            var imgVelo = velo.AddComponent<Image>();
            imgVelo.color = new Color(0.08f, 0.04f, 0.02f, 0.65f);
            velo.AddComponent<RectMask2D>();

            GameObject contenido = new GameObject("Contenido_Creditos");
            contenido.transform.SetParent(velo.transform, false);
            var rtCont = contenido.AddComponent<RectTransform>();
            rtCont.anchorMin = new Vector2(0.5f, 0f);
            rtCont.anchorMax = new Vector2(0.5f, 0f);
            rtCont.pivot = new Vector2(0.5f, 1f);
            rtCont.sizeDelta = new Vector2(1100f, 1500f);
            rtCont.anchoredPosition = new Vector2(0f, -40f);
            var tmpCred = contenido.AddComponent<TextMeshProUGUI>();
            tmpCred.font = fTexto;
            tmpCred.fontSize = 34f;
            tmpCred.alignment = TextAlignmentOptions.Top;
            tmpCred.textWrappingMode = TextWrappingModes.Normal;
            tmpCred.color = Color.white;
            tmpCred.text =
                "<size=56>EL MINERO</size>\n" +
                "Un videojuego de plataformas 2D en las minas de Guanajuato\n\n" +
                "<b>DISEÑO Y DESARROLLO</b>\n" +
                "Juana Jaqueline Camarillo Oláez\n\n" +
                "<b>INSTITUCIÓN</b>\n" +
                "Universidad Tecnológica del Norte de Guanajuato (UTNG)\n" +
                "GIDS6102-E · Creación de Videojuegos\n" +
                "Profesor: Gabriel Barrón Rodríguez\n\n" +
                "<b>MINAS</b>\n" +
                "Mina de Rayas · El Nopal · La Valenciana\n" +
                "Guanajuato, México\n\n" +
                "<b>VERSIÓN</b>\n" +
                "v1.0.0\n\n" +
                "<i>¡Gracias por jugar!</i>";

            GameObject filaC = new GameObject("Fila_Menu_Creditos");
            filaC.transform.SetParent(creditos.transform, false);
            var rtfc = filaC.AddComponent<RectTransform>();
            rtfc.anchorMin = new Vector2(1f, 0f);
            rtfc.anchorMax = new Vector2(1f, 0f);
            rtfc.pivot = new Vector2(1f, 0f);
            rtfc.sizeDelta = new Vector2(360f, 84f);
            rtfc.anchoredPosition = new Vector2(-48f, 48f);
            var hlgC = filaC.AddComponent<HorizontalLayoutGroup>();
            hlgC.childControlWidth = false;
            hlgC.childControlHeight = false;
            hlgC.childAlignment = TextAnchor.MiddleCenter;
            Button btnMenuCreditos = UIPrefabBuilder.CrearBotonMadera(filaC.transform, "Boton_Menu_Creditos", "Menú", spBtn, spBtnRes, spBtnPres, fTexto, 36f, 84f);
            creditos.SetActive(false);

            var panel = raiz.AddComponent<PanelFinalTemplo>();
            var so = new SerializedObject(panel);
            so.FindProperty("grupo").objectReferenceValue = grupo;
            so.FindProperty("textoTitulo").objectReferenceValue = titulo;
            so.FindProperty("textoCuerpo").objectReferenceValue = cuerpo;
            so.FindProperty("textoPuntaje").objectReferenceValue = puntaje;
            so.FindProperty("textoEstrellas").objectReferenceValue = estrellas;
            so.FindProperty("textoMinerales").objectReferenceValue = minerales;
            so.FindProperty("textoColoso").objectReferenceValue = coloso;
            so.FindProperty("iconoAguilarita").objectReferenceValue = imagenes[0];
            so.FindProperty("iconoOro").objectReferenceValue = imagenes[1];
            so.FindProperty("iconoPirargirita").objectReferenceValue = imagenes[2];
            so.FindProperty("botonCreditos").objectReferenceValue = btnCreditos;
            so.FindProperty("botonMenu").objectReferenceValue = btnMenu;
            so.FindProperty("panelCreditos").objectReferenceValue = creditos;
            so.FindProperty("contenidoCreditos").objectReferenceValue = rtCont;
            so.FindProperty("botonMenuCreditos").objectReferenceValue = btnMenuCreditos;
            so.ApplyModifiedProperties();

            return panel;
        }
    }
}
