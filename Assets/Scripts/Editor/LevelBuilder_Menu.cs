using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ElMinero.Entorno;
using ElMinero.UI;
using ElMinero.Utilidades;

namespace ElMinero.Editor
{
    /// <summary>
    /// Menú Principal (diseño v2): paisaje nocturno de Guanajuato con el templo
    /// al fondo, el minero que sale de la bocamina y salta a las vías, logo con
    /// caída y rebote, letrero de madera plantado en el suelo con antorchas,
    /// botones con iconos y efectos, estrellas, luciérnagas, niebla, murciélagos
    /// y barra inferior con versión, controles y mejor puntaje.
    /// </summary>
    public static partial class LevelBuilder
    {
        private const string RutaMenuUI = "Assets/Sprites/UI/Menu/";

        // Conversión del fondo (496x270 px) a coordenadas locales de cada capa
        private static Vector2 FondoLocal(float px, float py) => new Vector2((px - 248f) / 16f, (135f - py) / 16f);

        public static void ConstruirMenuPrincipal()
        {
            SpriteImportSetup.ConfigurarSpritesMenu();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            string capaFondo = Constantes.SortingLayers.Fondo;

            // ------------------------------------------------------------
            // CÁMARA Y LUZ
            // ------------------------------------------------------------
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 8.4375f; // 270 px de alto a 16 PPU: el paisaje completo en 16:9
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.05f, 0.12f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            GameObject luz = new GameObject("LuzGlobal");
            var l2d = luz.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.intensity = 0.95f;
            l2d.color = new Color(0.92f, 0.9f, 1f);
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            // ------------------------------------------------------------
            // FONDO EN 3 CAPAS (cielo, ciudad con el templo, frente con la bocamina)
            // ------------------------------------------------------------
            GameObject rootFondo = new GameObject("Fondo_Parallax_Menu");
            var parallaxComp = rootFondo.AddComponent<MenuParallax>();
            Transform capa0 = CrearCapaMenu(rootFondo.transform, "Capa0_Cielo", "FondoMenu_Capa0_Cielo.png", -10);
            Transform capa1 = CrearCapaMenu(rootFondo.transform, "Capa1_Ciudad", "FondoMenu_Capa1_Ciudad.png", -5);
            Transform capa2 = CrearCapaMenu(rootFondo.transform, "Capa2_Frente", "FondoMenu_Capa2_Frente.png", 0);

            SerializedObject soParallax = new SerializedObject(parallaxComp);
            soParallax.FindProperty("capaCielo").objectReferenceValue = capa0;
            soParallax.FindProperty("capaCiudad").objectReferenceValue = capa1;
            soParallax.FindProperty("capaFrente").objectReferenceValue = capa2;
            soParallax.FindProperty("maxOffset").floatValue = 0.6f;
            soParallax.ApplyModifiedProperties();

            // Niebla entre la ciudad y el frente (dos copias que se deslizan)
            Sprite spNiebla = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMenuUI + "Menu_Niebla.png");
            SpriteRenderer[] nieblas = new SpriteRenderer[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject n = new GameObject("Niebla_" + i);
                n.transform.SetParent(capa1, false);
                n.transform.localScale = new Vector3(2.2f, 2.4f, 1f);
                n.transform.localPosition = new Vector3(-8.8f + i * 17.6f, -4.7f, 0f);
                var srN = n.AddComponent<SpriteRenderer>();
                srN.sprite = spNiebla;
                srN.color = new Color(1f, 1f, 1f, 0.38f);
                srN.sortingLayerName = capaFondo;
                srN.sortingOrder = -3;
                nieblas[i] = srN;
            }

            // Luces cálidas que parpadean: faroles y el farol dentro de la mina
            CrearLuzMenu(capa2, "Luz_Farol_1", FondoLocal(127f, 201f), new Color(1f, 0.72f, 0.38f), 0.55f, 3.2f);
            CrearLuzMenu(capa2, "Luz_Farol_2", FondoLocal(257f, 201f), new Color(1f, 0.72f, 0.38f), 0.55f, 3.2f);
            CrearLuzMenu(capa2, "Luz_Mina", FondoLocal(54f, 182f), new Color(1f, 0.55f, 0.22f), 0.7f, 2.6f);

            // Letrero "MINA" sobre la bocamina
            GameObject letreroMina = new GameObject("Texto_Letrero_Mina");
            letreroMina.transform.SetParent(capa2, false);
            Vector2 posLetrero = FondoLocal(54f, 125.5f);
            letreroMina.transform.localPosition = new Vector3(posLetrero.x, posLetrero.y, 0f);
            var tmpMina = letreroMina.AddComponent<TextMeshPro>();
            tmpMina.rectTransform.sizeDelta = new Vector2(1.7f, 0.45f);
            tmpMina.font = FontAssetBuilder.ObtenerPressStart2P();
            tmpMina.text = "MINA";
            tmpMina.enableAutoSizing = true;
            tmpMina.fontSizeMin = 0.5f;
            tmpMina.fontSizeMax = 3.4f;
            tmpMina.alignment = TextAlignmentOptions.Center;
            tmpMina.color = new Color(0.27f, 0.15f, 0.07f);
            tmpMina.sortingLayerID = SortingLayer.NameToID(capaFondo);
            tmpMina.sortingOrder = 1;

            // Vetas de mineral en el suelo (con destellos)
            Vector2[] vetas =
            {
                FondoLocal(24f, 228f), FondoLocal(60f, 250f), FondoLocal(175f, 251f),
                FondoLocal(318f, 252f), FondoLocal(440f, 249f)
            };
            string[] tiposVeta = { "Deco_VetaPlataRoja", "Deco_VetaOro", "Deco_VetaPlataRoja", "Deco_VetaOro", "Deco_VetaOro" };
            for (int i = 0; i < vetas.Length; i++)
            {
                GameObject v = new GameObject("Veta_Suelo_" + i);
                v.transform.SetParent(capa2, false);
                v.transform.localPosition = vetas[i];
                var srV = v.AddComponent<SpriteRenderer>();
                srV.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/Sprites/Entorno/{tiposVeta[i]}.png");
                srV.flipX = i % 2 == 1;
                srV.sortingLayerName = capaFondo;
                srV.sortingOrder = 1;
            }

            // Minero que sale de la bocamina
            GameObject minero = new GameObject("Minero_Menu");
            minero.transform.SetParent(capa2, false);
            var srMinero = minero.AddComponent<SpriteRenderer>();
            srMinero.sortingLayerName = capaFondo;
            srMinero.sortingOrder = 2;
            srMinero.sprite = EditorSpriteUtil.Primero("Assets/Sprites/Jugador/Minero_Idle.png");
            var intro = minero.AddComponent<MenuMineroIntro>();
            SerializedObject soMinero = new SerializedObject(intro);
            AsignarCuadros(soMinero, "cuadrosCaminar", EditorSpriteUtil.Cuadros("Assets/Sprites/Jugador/Minero_Caminar.png"));
            AsignarCuadros(soMinero, "cuadrosIdle", EditorSpriteUtil.Cuadros("Assets/Sprites/Jugador/Minero_Idle.png"));
            AsignarCuadros(soMinero, "cuadrosSaltar", EditorSpriteUtil.Cuadros("Assets/Sprites/Jugador/Minero_Saltar.png"));
            AsignarCuadros(soMinero, "cuadrosCelebrar", EditorSpriteUtil.Cuadros("Assets/Sprites/Jugador/Minero_Celebrar.png"));
            AsignarCuadros(soMinero, "cuadrosPolvo", EditorSpriteUtil.Cuadros("Assets/Sprites/Jefe/Polvo_Grande.png"));
            soMinero.FindProperty("puntoInicio").vector2Value = FondoLocal(50f, 207f);
            soMinero.FindProperty("puntoBorde").vector2Value = FondoLocal(90f, 207f);
            soMinero.FindProperty("puntoAterrizaje").vector2Value = FondoLocal(102f, 237f);
            soMinero.FindProperty("puntoEspera").vector2Value = FondoLocal(108f, 237f);
            soMinero.FindProperty("xFinSombra").floatValue = FondoLocal(82f, 0f).x;
            soMinero.ApplyModifiedProperties();
            minero.transform.localPosition = FondoLocal(50f, 207f);

            // Ambiente: estrellas, estrellas fugaces, luciérnagas, murciélagos, niebla, destellos
            GameObject ambienteObj = new GameObject("Ambiente_Menu");
            var ambiente = ambienteObj.AddComponent<MenuAmbiente>();
            SerializedObject soAmb = new SerializedObject(ambiente);
            soAmb.FindProperty("capaCielo").objectReferenceValue = capa0;
            soAmb.FindProperty("capaFrente").objectReferenceValue = capa2;
            soAmb.FindProperty("sortingLayer").stringValue = capaFondo;
            soAmb.FindProperty("spriteDestello").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMenuUI + "Menu_Destello.png");
            soAmb.FindProperty("spriteEstrellaFugaz").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMenuUI + "Menu_EstrellaFugaz.png");
            soAmb.FindProperty("spriteLuciernaga").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMenuUI + "Menu_Luciernaga.png");
            AsignarCuadros(soAmb, "cuadrosMurcielago", EditorSpriteUtil.Cuadros("Assets/Sprites/Enemigos/Murcielago.png"));
            Vector2[] focos = { FondoLocal(127f, 208f), FondoLocal(257f, 208f), FondoLocal(70f, 190f) };
            var propFocos = soAmb.FindProperty("focosLuciernagas");
            propFocos.arraySize = focos.Length;
            for (int i = 0; i < focos.Length; i++) propFocos.GetArrayElementAtIndex(i).vector2Value = focos[i];
            soAmb.FindProperty("luciernagasPorFoco").intValue = 4;
            var propVetas = soAmb.FindProperty("puntosVetas");
            propVetas.arraySize = vetas.Length;
            for (int i = 0; i < vetas.Length; i++) propVetas.GetArrayElementAtIndex(i).vector2Value = vetas[i];
            var propNiebla = soAmb.FindProperty("nieblas");
            propNiebla.arraySize = nieblas.Length;
            for (int i = 0; i < nieblas.Length; i++) propNiebla.GetArrayElementAtIndex(i).objectReferenceValue = nieblas[i];
            soAmb.ApplyModifiedProperties();

            // ------------------------------------------------------------
            // INTERFAZ (1920x1080)
            // ------------------------------------------------------------
            GameObject canvasObj = CrearCanvasBase("Canvas_MenuPrincipal");
            var menuCtrl = canvasObj.AddComponent<MainMenuController>();
            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            // Viñeta (oscurece las orillas y centra la mirada)
            Image vineta = CrearImagenUI(canvasObj.transform, "Vineta", RutaMenuUI + "Menu_Vineta.png", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            Estirar(vineta.rectTransform);

            // Resplandor dorado detrás del logo
            Image brilloLogo = CrearImagenUI(canvasObj.transform, "Brillo_Logo", RutaMenuUI + "Menu_Resplandor.png", new Vector2(-480f, 400f), new Vector2(1100f, 420f), new Vector2(0.5f, 0.5f));
            brilloLogo.color = new Color(1f, 0.78f, 0.35f, 0f);

            // Logo (291x56 px a escala 3)
            Sprite logoGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero_Grande.png");
            if (logoGrande == null) logoGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero.png");
            Image logo = CrearImagenUI(canvasObj.transform, "Logo_ElMinero", null, new Vector2(-480f, 484f), new Vector2(873f, 168f), new Vector2(0.5f, 1f));
            logo.sprite = logoGrande;
            logo.preserveAspect = true;

            // ---------------- Letrero de madera plantado en el suelo ----------------
            const float anchoLetrero = 500f;
            const float altoLetrero = 650f;
            GameObject letrero = new GameObject("Letrero_Opciones");
            letrero.transform.SetParent(canvasObj.transform, false);
            var rectLetrero = letrero.AddComponent<RectTransform>();
            rectLetrero.anchorMin = rectLetrero.anchorMax = new Vector2(0.5f, 0.5f);
            rectLetrero.pivot = new Vector2(0.5f, 1f);
            rectLetrero.anchoredPosition = new Vector2(310f, 304f); // borde superior a 236 px del techo
            rectLetrero.sizeDelta = new Vector2(anchoLetrero, altoLetrero);

            // Patas (detrás del tablero)
            Sprite spPoste = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Poste_Vertical.png");
            foreach (float px in new[] { 90f, anchoLetrero - 90f })
            {
                Image pata = CrearImagenUI(letrero.transform, "Pata", null, new Vector2(px, 60f), new Vector2(32f, 192f), new Vector2(0.5f, 1f), new Vector2(0f, 0f));
                pata.sprite = spPoste;
            }

            // Resplandor de las antorchas
            Image[] resplandores = new Image[2];
            Image[] antorchas = new Image[2];
            Sprite[] cuadrosAntorcha = EditorSpriteUtil.Cuadros("Assets/Sprites/Entorno/Antorcha.png");
            for (int i = 0; i < 2; i++)
            {
                Vector2 anclaEsq = i == 0 ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
                float x = i == 0 ? 10f : -10f;
                resplandores[i] = CrearImagenUI(letrero.transform, "Resplandor_Antorcha_" + i, RutaMenuUI + "Menu_Resplandor.png", new Vector2(x, 36f), new Vector2(230f, 230f), new Vector2(0.5f, 0.5f), anclaEsq);
                resplandores[i].color = new Color(1f, 0.66f, 0.25f, 0.3f);
            }

            // Tablero
            GameObject panelMadera = new GameObject("Panel_Lateral");
            panelMadera.transform.SetParent(letrero.transform, false);
            var rectPanel = panelMadera.AddComponent<RectTransform>();
            Estirar(rectPanel);
            var imgPanel = panelMadera.AddComponent<Image>();
            imgPanel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPanel.type = Image.Type.Sliced;
            imgPanel.pixelsPerUnitMultiplier = 1.5625f;

            var vlg = panelMadera.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(30, 30, 30, 24);
            vlg.spacing = 12f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // Antorchas sobre las esquinas (encima del tablero)
            for (int i = 0; i < 2; i++)
            {
                Vector2 anclaEsq = i == 0 ? new Vector2(0f, 1f) : new Vector2(1f, 1f);
                float x = i == 0 ? 10f : -10f;
                antorchas[i] = CrearImagenUI(letrero.transform, "Antorcha_" + i, null, new Vector2(x, -16f), new Vector2(64f, 64f), new Vector2(0.5f, 0f), anclaEsq);
                antorchas[i].sprite = cuadrosAntorcha.Length > 0 ? cuadrosAntorcha[0] : null;
            }

            // 1. Etiqueta del nombre
            GameObject labelNombre = new GameObject("Label_Nombre");
            labelNombre.transform.SetParent(panelMadera.transform, false);
            labelNombre.AddComponent<RectTransform>().sizeDelta = new Vector2(440f, 28f);
            var tmpLn = labelNombre.AddComponent<TextMeshProUGUI>();
            tmpLn.font = fontTitulo;
            tmpLn.text = "NOMBRE DEL MINERO";
            tmpLn.fontSize = 22f;
            tmpLn.alignment = TextAlignmentOptions.Center;
            tmpLn.color = new Color(1f, 0.84f, 0.43f);
            tmpLn.raycastTarget = false;

            // 2. Campo de nombre
            GameObject inputObj = new GameObject("InputField_Nombre");
            inputObj.transform.SetParent(panelMadera.transform, false);
            var rectInput = inputObj.AddComponent<RectTransform>();
            rectInput.sizeDelta = new Vector2(440f, 64f);
            var imgInput = inputObj.AddComponent<Image>();
            imgInput.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Campo_Texto.png");
            imgInput.type = Image.Type.Sliced;
            imgInput.pixelsPerUnitMultiplier = 1.5625f;

            TextMeshProUGUI tmpPh = CrearTextoCampo(inputObj.transform, "Placeholder", fontTexto, new Color(0.6f, 0.55f, 0.5f, 0.7f));
            tmpPh.text = "Refugio";
            TextMeshProUGUI tmpText = CrearTextoCampo(inputObj.transform, "Text", fontTexto, new Color(1f, 0.88f, 0.35f));

            var inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textViewport = rectInput;
            inputField.textComponent = tmpText;
            inputField.placeholder = tmpPh;
            inputField.characterLimit = 12;

            // 3. Separador dorado
            Image sep = CrearImagenUI(panelMadera.transform, "Separador", RutaMenuUI + "Menu_Separador.png", Vector2.zero, new Vector2(384f, 20f), new Vector2(0.5f, 0.5f));

            // 4. Botones con icono
            Button btnContinue = CrearBotonMenu(panelMadera.transform, "Boton_Continuar", "Continuar", "Icono_Siguiente");
            Button btnPlay = CrearBotonMenu(panelMadera.transform, "Boton_Jugar", "Nuevo juego", "Icono_Jugar");
            Button btnLevels = CrearBotonMenu(panelMadera.transform, "Boton_Niveles", "Niveles", "Icono_Niveles");
            Button btnOptions = CrearBotonMenu(panelMadera.transform, "Boton_Opciones", "Opciones", "Icono_Opciones");
            Button btnCredits = CrearBotonMenu(panelMadera.transform, "Boton_Creditos", "Créditos", "Icono_Creditos");
            Button btnQuit = CrearBotonMenu(panelMadera.transform, "Boton_Salir", "Salir", "Icono_Salir");

            // Cada elemento del tablero aparece uno tras otro
            GameObject[] elementos = { labelNombre, inputObj, sep.gameObject, btnContinue.gameObject, btnPlay.gameObject, btnLevels.gameObject, btnOptions.gameObject, btnCredits.gameObject, btnQuit.gameObject };
            CanvasGroup[] grupos = elementos.Select(e => e.AddComponent<CanvasGroup>()).ToArray();

            // ---------------- Barra inferior ----------------
            TextMeshProUGUI tmpVer = CrearTextoBarra(canvasObj.transform, "Texto_Version", fontTexto, "v1.0.0", new Vector2(0f, 0f), new Vector2(40f, 26f), TextAlignmentOptions.BottomLeft, new Color(0.78f, 0.74f, 0.86f, 0.9f));
            CrearTextoBarra(canvasObj.transform, "Texto_Controles", fontTexto, "Flechas o Mouse: elegir     Enter: aceptar     Esc: volver", new Vector2(0f, 0f), new Vector2(170f, 26f), TextAlignmentOptions.BottomLeft, new Color(0.78f, 0.74f, 0.86f, 0.75f));
            TextMeshProUGUI tmpRecord = CrearTextoBarra(canvasObj.transform, "Texto_Record", fontTexto, "EL TESORO TE ESPERA", new Vector2(1f, 0f), new Vector2(-40f, 26f), TextAlignmentOptions.BottomRight, new Color(1f, 0.84f, 0.43f));

            // ---------------- Paneles modales (Niveles, Opciones, Créditos) ----------------
            LevelSelectPanel levelSelectComp = InstanciarModal<LevelSelectPanel>(canvasObj.transform, "Assets/Prefabs/UI/LevelSelectPanel.prefab");
            OptionsPanel optionsComp = InstanciarModal<OptionsPanel>(canvasObj.transform, "Assets/Prefabs/UI/OptionsPanel.prefab");
            CreditsPanel creditsComp = InstanciarModal<CreditsPanel>(canvasObj.transform, "Assets/Prefabs/UI/CreditsPanel.prefab");

            // ---------------- Fundido desde negro (siempre encima) ----------------
            Image negro = CrearImagenUI(canvasObj.transform, "Fundido_Negro", null, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            Estirar(negro.rectTransform);
            negro.color = Color.black;
            var cgNegro = negro.gameObject.AddComponent<CanvasGroup>();
            cgNegro.blocksRaycasts = false;
            cgNegro.interactable = false;
            negro.transform.SetAsLastSibling();

            // ---------------- Referencias ----------------
            SerializedObject soMenu = new SerializedObject(menuCtrl);
            soMenu.FindProperty("nameInputField").objectReferenceValue = inputField;
            soMenu.FindProperty("playButton").objectReferenceValue = btnPlay;
            soMenu.FindProperty("continueButton").objectReferenceValue = btnContinue;
            soMenu.FindProperty("levelsButton").objectReferenceValue = btnLevels;
            soMenu.FindProperty("optionsButton").objectReferenceValue = btnOptions;
            soMenu.FindProperty("creditsButton").objectReferenceValue = btnCredits;
            soMenu.FindProperty("quitButton").objectReferenceValue = btnQuit;
            soMenu.FindProperty("levelSelectPanel").objectReferenceValue = levelSelectComp;
            soMenu.FindProperty("optionsPanel").objectReferenceValue = optionsComp;
            soMenu.FindProperty("creditsPanel").objectReferenceValue = creditsComp;
            soMenu.FindProperty("versionText").objectReferenceValue = tmpVer;
            soMenu.ApplyModifiedProperties();

            var animador = canvasObj.AddComponent<MenuUIAnimador>();
            SerializedObject soAnim = new SerializedObject(animador);
            soAnim.FindProperty("fundidoNegro").objectReferenceValue = cgNegro;
            soAnim.FindProperty("logo").objectReferenceValue = logo.rectTransform;
            soAnim.FindProperty("brilloLogo").objectReferenceValue = brilloLogo;
            soAnim.FindProperty("letrero").objectReferenceValue = rectLetrero;
            var propGrupos = soAnim.FindProperty("elementosLetrero");
            propGrupos.arraySize = grupos.Length;
            for (int i = 0; i < grupos.Length; i++) propGrupos.GetArrayElementAtIndex(i).objectReferenceValue = grupos[i];
            var propAnt = soAnim.FindProperty("antorchas");
            propAnt.arraySize = antorchas.Length;
            for (int i = 0; i < antorchas.Length; i++) propAnt.GetArrayElementAtIndex(i).objectReferenceValue = antorchas[i];
            AsignarCuadros(soAnim, "cuadrosAntorcha", cuadrosAntorcha);
            var propRes = soAnim.FindProperty("resplandores");
            propRes.arraySize = resplandores.Length;
            for (int i = 0; i < resplandores.Length; i++) propRes.GetArrayElementAtIndex(i).objectReferenceValue = resplandores[i];
            soAnim.FindProperty("textoRecord").objectReferenceValue = tmpRecord;
            soAnim.FindProperty("primerSeleccionado").objectReferenceValue = btnPlay.gameObject;
            soAnim.ApplyModifiedProperties();

            // Managers persistentes
            CrearSistemasPersistentes();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MenuPrincipal.unity");
            Debug.Log("[LevelBuilder] Escena MenuPrincipal.unity (diseño v2) construida exitosamente.");
        }

        // ====================================================================
        // HELPERS DEL MENÚ
        // ====================================================================

        private static Transform CrearCapaMenu(Transform padre, string nombre, string archivo, int orden)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = Vector3.zero;
            // 1.06: margen para que el parallax nunca deje ver las orillas
            go.transform.localScale = new Vector3(1.06f, 1.06f, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/Menu/" + archivo);
            sr.sortingLayerName = Constantes.SortingLayers.Fondo;
            sr.sortingOrder = orden;
            return go.transform;
        }

        private static void CrearLuzMenu(Transform padre, string nombre, Vector2 posLocal, Color color, float intensidad, float radio)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            go.transform.localPosition = new Vector3(posLocal.x, posLocal.y, 0f);
            var luz = go.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.color = color;
            luz.intensity = intensidad;
            luz.pointLightInnerRadius = 0.2f;
            luz.pointLightOuterRadius = radio;
            luz.falloffIntensity = 0.7f;
            ElMineroBuilder.AsignarCapasALuz2D(luz);
            go.AddComponent<FlickeringLight2D>();
        }

        private static void AsignarCuadros(SerializedObject so, string propiedad, Sprite[] cuadros)
        {
            var prop = so.FindProperty(propiedad);
            if (prop == null) return;
            prop.arraySize = cuadros != null ? cuadros.Length : 0;
            for (int i = 0; i < prop.arraySize; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = cuadros[i];
        }

        private static Image CrearImagenUI(Transform padre, string nombre, string rutaSprite, Vector2 pos, Vector2 tam, Vector2 pivot, Vector2? ancla = null)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rect = go.AddComponent<RectTransform>();
            Vector2 a = ancla ?? new Vector2(0.5f, 0.5f);
            rect.anchorMin = a;
            rect.anchorMax = a;
            rect.pivot = pivot;
            rect.anchoredPosition = pos;
            rect.sizeDelta = tam;
            var img = go.AddComponent<Image>();
            if (!string.IsNullOrEmpty(rutaSprite)) img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(rutaSprite);
            img.raycastTarget = false;
            return img;
        }

        private static void Estirar(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static TextMeshProUGUI CrearTextoCampo(Transform padre, string nombre, TMP_FontAsset font, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(16f, 0f);
            rect.offsetMax = new Vector2(-16f, 0f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.fontSize = 34f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = color;
            return tmp;
        }

        private static TextMeshProUGUI CrearTextoBarra(Transform padre, string nombre, TMP_FontAsset font, string texto, Vector2 ancla, Vector2 pos, TextAlignmentOptions alineacion, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = ancla;
            rect.anchorMax = ancla;
            rect.pivot = ancla;
            rect.anchoredPosition = pos;
            rect.sizeDelta = new Vector2(900f, 36f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = texto;
            tmp.fontSize = 24f;
            tmp.alignment = alineacion;
            tmp.color = color;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static Button CrearBotonMenu(Transform padre, string nombre, string texto, string icono)
        {
            Button btn = CrearBotonMadera(padre, nombre, texto, Vector2.zero, new Vector2(440f, 64f), default, default, 32f);

            // Texto alineado a la izquierda, dejando espacio para el icono
            var tmp = btn.GetComponentInChildren<TextMeshProUGUI>();
            var rectT = tmp.rectTransform;
            rectT.offsetMin = new Vector2(84f, 0f);
            rectT.offsetMax = new Vector2(-16f, 0f);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.raycastTarget = false;

            // Icono (16 px a escala 3)
            Image img = CrearImagenUI(btn.transform, "Icono", $"Assets/Sprites/UI/Iconos/{icono}.png", new Vector2(18f, 0f), new Vector2(48f, 48f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            img.preserveAspect = true;

            var anim = btn.gameObject.AddComponent<BotonMenuAnimado>();
            SerializedObject so = new SerializedObject(anim);
            so.FindProperty("texto").objectReferenceValue = tmp;
            so.FindProperty("icono").objectReferenceValue = img.rectTransform;
            so.ApplyModifiedProperties();
            return btn;
        }

        private static T InstanciarModal<T>(Transform canvas, string rutaPrefab) where T : Component
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(rutaPrefab);
            if (prefab == null) return null;
            var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab, canvas);
            T comp = inst.GetComponent<T>();
            inst.SetActive(false);
            return comp;
        }
    }
}
