using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using ElMinero.Camara;
using ElMinero.Coleccionables;
using ElMinero.Entorno;
using ElMinero.Jugador;
using ElMinero.Managers;
using ElMinero.Niveles;
using ElMinero.Obstaculos;
using ElMinero.Plataformas;
using ElMinero.UI;
using ElMinero.Utilidades;
using ElMinero.Enemigos;

namespace ElMinero.Editor
{
    /// <summary>
    /// Constructor procedural de escenas para "El Minero".
    /// Genera las 6 escenas completas:
    /// - MenuPrincipal.unity
    /// - Nivel1_MinaRayas.unity
    /// - Nivel2_ElNopal.unity
    /// - Nivel3_LaValenciana.unity
    /// - Victoria.unity
    /// - Derrota.unity
    /// </summary>
    public static class LevelBuilder
    {
        [MenuItem("Herramientas/El Minero/7. Construir escenas", priority = 70)]
        public static void ConstruirTodasLasEscenas()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog("El Minero", "Por favor sal del Modo Play (detÃ©n el juego) antes de construir las escenas.", "Entendido");
                return;
            }

            EditorUtility.DisplayProgressBar("El Minero", "Construyendo escenas del juego...", 0.1f);

            try
            {
                // Asegurar que las dependencias previas estÃ©n configuradas
                ElMineroBuilder.ConfigurarTagsYLayers();
                ElMineroBuilder.CrearMaterialesFisicosYConfigs();

                // 1. Escenas de Interfaz y Narrativa
                ConstruirSplash();
                ConstruirMenuPrincipal();
                ConstruirIntro();

                // 2. Tres Niveles de Ascenso (Mina de Rayas, El Nopal, La Valenciana)
                ConstruirNivel1_MinaRayas();
                ConstruirNivel2_ElNopal();
                ConstruirNivel3_LaValenciana();

                // 3. Escena Final (Superficie y Templo de San Cayetano)
                ConstruirFinal();

                // 4. Limpieza de escenas obsoletas
                EliminarEscenasObsoletas();

                // 5. Registrar las 7 escenas en Build Settings automÃ¡ticamente
                ElMineroBuilder.ConfigurarBuildSettingsYPlayer();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[El Minero] Â¡Las 7 escenas construidas y guardadas exitosamente en Assets/Scenes/!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ====================================================================
        // 1. MENÃš PRINCIPAL (Parallax 3 capas, Logo Grande, Panel 620x740)
        // ====================================================================
        public static void ConstruirMenuPrincipal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // CÃ¡mara
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.09f, 0.19f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // Fondo visual con efecto Parallax de 3 capas
            GameObject rootFondo = new GameObject("Fondo_Parallax_Menu");
            var parallaxComp = rootFondo.AddComponent<MenuParallax>();

            // Capa 0: Cielo nocturno con luna
            GameObject goCapa0 = new GameObject("Capa0_Cielo");
            goCapa0.transform.SetParent(rootFondo.transform);
            goCapa0.transform.position = new Vector3(0f, 0f, 0f);
            goCapa0.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            var sr0 = goCapa0.AddComponent<SpriteRenderer>();
            sr0.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/Menu/FondoMenu_Capa0_Cielo.png");
            sr0.sortingLayerName = Constantes.SortingLayers.Fondo;
            sr0.sortingOrder = -10;

            // Capa 1: CaÃ±ada de Guanajuato y BasÃ­lica
            GameObject goCapa1 = new GameObject("Capa1_Ciudad");
            goCapa1.transform.SetParent(rootFondo.transform);
            goCapa1.transform.position = new Vector3(0f, -0.2f, 0f);
            goCapa1.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            var sr1 = goCapa1.AddComponent<SpriteRenderer>();
            sr1.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/Menu/FondoMenu_Capa1_Ciudad.png");
            sr1.sortingLayerName = Constantes.SortingLayers.Fondo;
            sr1.sortingOrder = -5;

            // Capa 2: Frente (Bocamina, rieles, faroles, vagoneta)
            GameObject goCapa2 = new GameObject("Capa2_Frente");
            goCapa2.transform.SetParent(rootFondo.transform);
            goCapa2.transform.position = new Vector3(0f, -0.3f, 0f);
            goCapa2.transform.localScale = new Vector3(1.15f, 1.15f, 1f);
            var sr2 = goCapa2.AddComponent<SpriteRenderer>();
            sr2.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/Menu/FondoMenu_Capa2_Frente.png");
            sr2.sortingLayerName = Constantes.SortingLayers.Fondo;
            sr2.sortingOrder = 0;

            // Configurar referencias en MenuParallax
            SerializedObject soParallax = new SerializedObject(parallaxComp);
            soParallax.FindProperty("capaCielo").objectReferenceValue = goCapa0.transform;
            soParallax.FindProperty("capaCiudad").objectReferenceValue = goCapa1.transform;
            soParallax.FindProperty("capaFrente").objectReferenceValue = goCapa2.transform;
            soParallax.ApplyModifiedProperties();

            // Luz global
            GameObject luz = new GameObject("LuzGlobal");
            var l2d = luz.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.intensity = 1.0f;
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            // Personaje Minero en Idle sobre los rieles a la izquierda (~200px visual)
            GameObject mineroObj = new GameObject("Minero_Menu");
            mineroObj.transform.position = new Vector3(-4.5f, -2.5f, 0f);
            mineroObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            var srM = mineroObj.AddComponent<SpriteRenderer>();
            srM.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jugador/Minero_Idle.png");
            srM.sortingLayerName = Constantes.SortingLayers.Escenario;
            srM.sortingOrder = 5;
            var animM = mineroObj.AddComponent<Animator>();
            animM.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Jugador/Minero.controller");

            // Canvas UI
            GameObject canvasObj = CrearCanvasBase("Canvas_MenuPrincipal");
            var menuCtrl = canvasObj.AddComponent<MainMenuController>();

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            // Logo Grande centrado arriba
            GameObject logoObj = new GameObject("Logo_ElMinero");
            logoObj.transform.SetParent(canvasObj.transform, false);
            var rectLogo = logoObj.AddComponent<RectTransform>();
            rectLogo.anchorMin = new Vector2(0.5f, 1f);
            rectLogo.anchorMax = new Vector2(0.5f, 1f);
            rectLogo.pivot = new Vector2(0.5f, 1f);
            rectLogo.anchoredPosition = new Vector2(0f, -40f);
            rectLogo.sizeDelta = new Vector2(1160f, 256f);
            var imgLogo = logoObj.AddComponent<Image>();
            Sprite logoGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero_Grande.png");
            if (logoGrande == null) logoGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero.png");
            imgLogo.sprite = logoGrande;
            imgLogo.preserveAspect = true;

            // Panel de Botones a la derecha (620x740)
            GameObject panelMadera = new GameObject("Panel_Lateral");
            panelMadera.transform.SetParent(canvasObj.transform, false);
            var rectPanel = panelMadera.AddComponent<RectTransform>();
            rectPanel.anchorMin = new Vector2(1f, 0.5f);
            rectPanel.anchorMax = new Vector2(1f, 0.5f);
            rectPanel.pivot = new Vector2(1f, 0.5f);
            rectPanel.anchoredPosition = new Vector2(-120f, -40f);
            rectPanel.sizeDelta = new Vector2(620f, 740f);
            var imgPanel = panelMadera.AddComponent<Image>();
            imgPanel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPanel.type = Image.Type.Sliced;
            imgPanel.pixelsPerUnitMultiplier = 0.25f;

            // Etiqueta Nombre
            GameObject labelNombre = new GameObject("Label_Nombre");
            labelNombre.transform.SetParent(panelMadera.transform, false);
            var rectLn = labelNombre.AddComponent<RectTransform>();
            rectLn.anchoredPosition = new Vector2(0f, 295f);
            rectLn.sizeDelta = new Vector2(540f, 36f);
            var tmpLn = labelNombre.AddComponent<TextMeshProUGUI>();
            tmpLn.font = fontTexto;
            tmpLn.text = "Nombre del Minero:";
            tmpLn.fontSize = 30f;
            tmpLn.alignment = TextAlignmentOptions.Center;
            tmpLn.color = new Color(0.96f, 0.91f, 0.75f);

            // InputField de Nombre (540x76)
            GameObject inputObj = new GameObject("InputField_Nombre");
            inputObj.transform.SetParent(panelMadera.transform, false);
            var rectInput = inputObj.AddComponent<RectTransform>();
            rectInput.anchoredPosition = new Vector2(0f, 225f);
            rectInput.sizeDelta = new Vector2(540f, 76f);
            var imgInput = inputObj.AddComponent<Image>();
            imgInput.color = new Color(0.10f, 0.07f, 0.05f, 0.92f);

            // Placeholder
            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(inputObj.transform, false);
            var rectPh = phObj.AddComponent<RectTransform>();
            rectPh.anchorMin = Vector2.zero;
            rectPh.anchorMax = Vector2.one;
            rectPh.offsetMin = new Vector2(16f, 0f);
            rectPh.offsetMax = new Vector2(-16f, 0f);
            var tmpPh = phObj.AddComponent<TextMeshProUGUI>();
            tmpPh.font = fontTexto;
            tmpPh.fontSize = 28f;
            tmpPh.alignment = TextAlignmentOptions.Midline;
            tmpPh.color = new Color(0.6f, 0.55f, 0.5f, 0.7f);
            tmpPh.text = "Refugio";

            // Texto ingresado
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var rectTxt = textObj.AddComponent<RectTransform>();
            rectTxt.anchorMin = Vector2.zero;
            rectTxt.anchorMax = Vector2.one;
            rectTxt.offsetMin = new Vector2(16f, 0f);
            rectTxt.offsetMax = new Vector2(-16f, 0f);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.font = fontTexto;
            tmpText.fontSize = 34f;
            tmpText.alignment = TextAlignmentOptions.Midline;
            tmpText.color = new Color(1f, 0.88f, 0.35f);

            var inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textViewport = rectInput;
            inputField.textComponent = tmpText;
            inputField.placeholder = tmpPh;
            inputField.characterLimit = 12;

            // 6 Botones del menÃº (Continuar, Nuevo Juego, Niveles, Opciones, CrÃ©ditos, Salir)
            Button btnContinue = CrearBotonMadera(panelMadera.transform, "Boton_Continuar", "Continuar", new Vector2(0f, 135f), new Vector2(540f, 84f));
            Button btnPlay = CrearBotonMadera(panelMadera.transform, "Boton_Jugar", "Nuevo Juego", new Vector2(0f, 45f), new Vector2(540f, 84f));
            Button btnLevels = CrearBotonMadera(panelMadera.transform, "Boton_Niveles", "Niveles", new Vector2(0f, -45f), new Vector2(540f, 84f));
            Button btnOptions = CrearBotonMadera(panelMadera.transform, "Boton_Opciones", "Opciones", new Vector2(0f, -135f), new Vector2(540f, 84f));
            Button btnCredits = CrearBotonMadera(panelMadera.transform, "Boton_Creditos", "CrÃ©ditos", new Vector2(0f, -225f), new Vector2(540f, 84f));
            Button btnQuit = CrearBotonMadera(panelMadera.transform, "Boton_Salir", "Salir", new Vector2(0f, -305f), new Vector2(540f, 56f));

            // Texto VersiÃ³n en esquina inferior izquierda
            GameObject verObj = new GameObject("Texto_Version");
            verObj.transform.SetParent(canvasObj.transform, false);
            var rectVer = verObj.AddComponent<RectTransform>();
            rectVer.anchorMin = new Vector2(0f, 0f);
            rectVer.anchorMax = new Vector2(0f, 0f);
            rectVer.pivot = new Vector2(0f, 0f);
            rectVer.anchoredPosition = new Vector2(40f, 30f);
            rectVer.sizeDelta = new Vector2(300f, 40f);
            var tmpVer = verObj.AddComponent<TextMeshProUGUI>();
            tmpVer.font = fontTexto;
            tmpVer.text = "v1.0.0";
            tmpVer.fontSize = 28f;
            tmpVer.alignment = TextAlignmentOptions.BottomLeft;
            tmpVer.color = new Color(0.8f, 0.8f, 0.8f, 0.85f);

            // Instanciar Prefabs Modales (Niveles, Opciones, CrÃ©ditos)
            GameObject pLevelSelect = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LevelSelectPanel.prefab");
            GameObject pOptions = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/OptionsPanel.prefab");
            GameObject pCredits = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CreditsPanel.prefab");

            LevelSelectPanel levelSelectComp = null;
            if (pLevelSelect != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pLevelSelect, canvasObj.transform);
                levelSelectComp = inst.GetComponent<LevelSelectPanel>();
                inst.SetActive(false);
            }

            OptionsPanel optionsComp = null;
            if (pOptions != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pOptions, canvasObj.transform);
                optionsComp = inst.GetComponent<OptionsPanel>();
                inst.SetActive(false);
            }

            CreditsPanel creditsComp = null;
            if (pCredits != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pCredits, canvasObj.transform);
                creditsComp = inst.GetComponent<CreditsPanel>();
                inst.SetActive(false);
            }

            // Asignar referencias en MainMenuController
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

            // Managers persistentes
            CrearSistemasPersistentes();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MenuPrincipal.unity");
            Debug.Log("[LevelBuilder] Escena MenuPrincipal.unity construida exitosamente.");
        }

        // ====================================================================
        // 2. PANTALLA SPLASH (SecciÃ³n 3.1)
        // ====================================================================
        public static void ConstruirSplash()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // CÃ¡mara
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // Canvas
            GameObject canvasObj = CrearCanvasBase("Canvas_Splash");
            var splashCtrl = canvasObj.AddComponent<SplashController>();

            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            // Grupo 1: Logo Autora + Texto UTNG
            GameObject goGrupoAutora = new GameObject("Grupo_Autora");
            goGrupoAutora.transform.SetParent(canvasObj.transform, false);
            var rectGa = goGrupoAutora.AddComponent<RectTransform>();
            rectGa.anchorMin = Vector2.zero;
            rectGa.anchorMax = Vector2.one;
            rectGa.offsetMin = Vector2.zero;
            rectGa.offsetMax = Vector2.zero;
            var cgAutora = goGrupoAutora.AddComponent<CanvasGroup>();

            GameObject goLogoAutora = new GameObject("Logo_Autora");
            goLogoAutora.transform.SetParent(goGrupoAutora.transform, false);
            var rectLa = goLogoAutora.AddComponent<RectTransform>();
            rectLa.anchorMin = new Vector2(0.5f, 0.55f);
            rectLa.anchorMax = new Vector2(0.5f, 0.55f);
            rectLa.sizeDelta = new Vector2(280f, 280f);
            var imgLa = goLogoAutora.AddComponent<Image>();
            imgLa.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_Autora.png");
            imgLa.preserveAspect = true;

            GameObject goTxtUtng = new GameObject("Texto_UTNG");
            goTxtUtng.transform.SetParent(goGrupoAutora.transform, false);
            var rectUtng = goTxtUtng.AddComponent<RectTransform>();
            rectUtng.anchorMin = new Vector2(0.5f, 0.35f);
            rectUtng.anchorMax = new Vector2(0.5f, 0.35f);
            rectUtng.sizeDelta = new Vector2(600f, 40f);
            var tmpUtng = goTxtUtng.AddComponent<TextMeshProUGUI>();
            tmpUtng.font = fontTexto;
            tmpUtng.text = "UTNG Â· CreaciÃ³n de Videojuegos";
            tmpUtng.fontSize = 22f;
            tmpUtng.alignment = TextAlignmentOptions.Center;
            tmpUtng.color = new Color(0.9f, 0.85f, 0.75f);

            // Grupo 2: Logo El Minero
            GameObject goGrupoJuego = new GameObject("Grupo_LogoJuego");
            goGrupoJuego.transform.SetParent(canvasObj.transform, false);
            var rectGj = goGrupoJuego.AddComponent<RectTransform>();
            rectGj.anchorMin = Vector2.zero;
            rectGj.anchorMax = Vector2.one;
            rectGj.offsetMin = Vector2.zero;
            rectGj.offsetMax = Vector2.zero;
            var cgJuego = goGrupoJuego.AddComponent<CanvasGroup>();

            GameObject goLogoJuego = new GameObject("Logo_ElMinero");
            goLogoJuego.transform.SetParent(goGrupoJuego.transform, false);
            var rectLj = goLogoJuego.AddComponent<RectTransform>();
            rectLj.anchorMin = new Vector2(0.5f, 0.5f);
            rectLj.anchorMax = new Vector2(0.5f, 0.5f);
            rectLj.sizeDelta = new Vector2(520f, 200f);
            var imgLj = goLogoJuego.AddComponent<Image>();
            imgLj.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero.png");
            imgLj.preserveAspect = true;

            SerializedObject so = new SerializedObject(splashCtrl);
            so.FindProperty("grupoAutora").objectReferenceValue = cgAutora;
            so.FindProperty("grupoLogoJuego").objectReferenceValue = cgJuego;
            so.ApplyModifiedProperties();

            CrearSistemasPersistentes();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Splash.unity");
            Debug.Log("[LevelBuilder] Escena Splash.unity guardada.");
        }

        // ====================================================================
        // 3. PANTALLA INTRO NARRATIVA (SecciÃ³n 3.6)
        // ====================================================================
        public static void ConstruirIntro()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // CÃ¡mara
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.03f, 0.06f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // Fondo visual atenuado
            GameObject fondo = new GameObject("Fondo_Intro");
            var srFondo = fondo.AddComponent<SpriteRenderer>();
            srFondo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/MinaRayas/Fondo_MinaRayas_Capa0_Lejana.png");
            srFondo.sortingLayerName = Constantes.SortingLayers.Fondo;
            srFondo.color = new Color(0.25f, 0.25f, 0.3f, 1f);
            fondo.transform.position = new Vector3(0f, 0f, 0f);
            fondo.transform.localScale = new Vector3(1.3f, 1.3f, 1f);

            // Canvas UI
            GameObject canvasObj = CrearCanvasBase("Canvas_Intro");
            var introCtrl = canvasObj.AddComponent<IntroController>();

            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            // Panel oscuro semitransparente (1500x520 para Canvas 1920x1080)
            GameObject panelOscuro = new GameObject("Panel_Oscuro");
            panelOscuro.transform.SetParent(canvasObj.transform, false);
            var rectPo = panelOscuro.AddComponent<RectTransform>();
            rectPo.sizeDelta = new Vector2(1500f, 520f);
            rectPo.anchorMin = new Vector2(0.5f, 0.5f);
            rectPo.anchorMax = new Vector2(0.5f, 0.5f);
            var imgPo = panelOscuro.AddComponent<Image>();
            imgPo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPo.type = Image.Type.Sliced;
            imgPo.pixelsPerUnitMultiplier = 0.25f;
            imgPo.color = new Color(0.12f, 0.1f, 0.12f, 0.95f);

            // Texto narrativo tipo mÃ¡quina de escribir (46 pt)
            GameObject goTxt = new GameObject("Texto_Narrativo");
            goTxt.transform.SetParent(panelOscuro.transform, false);
            var rectTxt = goTxt.AddComponent<RectTransform>();
            rectTxt.anchorMin = Vector2.zero;
            rectTxt.anchorMax = Vector2.one;
            rectTxt.offsetMin = new Vector2(60f, 50f);
            rectTxt.offsetMax = new Vector2(-60f, -50f);
            var tmpTxt = goTxt.AddComponent<TextMeshProUGUI>();
            tmpTxt.font = fontTexto;
            tmpTxt.fontSize = 46f;
            tmpTxt.alignment = TextAlignmentOptions.Center;
            tmpTxt.color = new Color(0.96f, 0.91f, 0.76f);
            tmpTxt.textWrappingMode = TextWrappingModes.Normal;

            // Indicador de avance (28 pt, regla >= 26 pt)
            GameObject goInd = new GameObject("Texto_Indicador");
            goInd.transform.SetParent(canvasObj.transform, false);
            var rectInd = goInd.AddComponent<RectTransform>();
            rectInd.anchorMin = new Vector2(0.5f, 0.12f);
            rectInd.anchorMax = new Vector2(0.5f, 0.12f);
            rectInd.sizeDelta = new Vector2(900f, 44f);
            var tmpInd = goInd.AddComponent<TextMeshProUGUI>();
            tmpInd.font = fontTexto;
            tmpInd.fontSize = 28f;
            tmpInd.fontStyle = FontStyles.Italic;
            tmpInd.alignment = TextAlignmentOptions.Center;
            tmpInd.color = new Color(0.85f, 0.85f, 0.85f, 0.85f);
            tmpInd.text = "Presiona [Espacio] o Toca para continuar";

            // BotÃ³n Saltar (220x64, texto 30 pt)
            Button btnSaltar = CrearBotonMadera(canvasObj.transform, "Boton_Saltar", "Saltar >>", new Vector2(-100f, -60f), new Vector2(220f, 64f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            var txtBtnSaltar = btnSaltar.GetComponentInChildren<TextMeshProUGUI>();
            if (txtBtnSaltar != null) txtBtnSaltar.fontSize = 30f;
            SerializedObject so = new SerializedObject(introCtrl);
            so.FindProperty("textoNarrativo").objectReferenceValue = tmpTxt;
            so.FindProperty("textoIndicador").objectReferenceValue = tmpInd;
            so.FindProperty("botonSaltar").objectReferenceValue = btnSaltar;
            so.ApplyModifiedProperties();

            CrearSistemasPersistentes();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Intro.unity");
            Debug.Log("[LevelBuilder] Escena Intro.unity guardada.");
        }

        // ====================================================================
        // 4. PANTALLA FINAL (SecciÃ³n 3.7)
        // ====================================================================
        public static void ConstruirFinal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // CÃ¡mara
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.35f, 0.5f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // Fondo: Salida_Superficie en grande (Templo San Cayetano)
            GameObject templo = new GameObject("Templo_SanCayetano");
            var srTemplo = templo.AddComponent<SpriteRenderer>();
            srTemplo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Salida_Superficie.png");
            srTemplo.sortingLayerName = Constantes.SortingLayers.Escenario;
            templo.transform.position = new Vector3(0f, -0.5f, 0f);
            templo.transform.localScale = new Vector3(2.2f, 2.2f, 1f);

            // Luz cÃ¡lida 2D simulando el amanecer
            GameObject luzAmanecer = new GameObject("Luz_Amanecer");
            var l2d = luzAmanecer.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.color = new Color(1.0f, 0.88f, 0.65f);
            l2d.intensity = 1.35f;
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            // Minero en celebraciÃ³n frente al templo
            GameObject minero = new GameObject("Minero_Celebrando");
            var srMinero = minero.AddComponent<SpriteRenderer>();
            srMinero.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jugador/Minero_Celebrar.png");
            srMinero.sortingLayerName = Constantes.SortingLayers.Jugador;
            minero.transform.position = new Vector3(0f, -2.8f, 0f);
            minero.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

            // Canvas UI Final
            GameObject canvasObj = CrearCanvasBase("Canvas_Final");
            var finalCtrl = canvasObj.AddComponent<FinalController>();

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            // Panel Superior Narrativo
            GameObject panelNarrativo = new GameObject("Panel_Narrativo");
            panelNarrativo.transform.SetParent(canvasObj.transform, false);
            var rectPn = panelNarrativo.AddComponent<RectTransform>();
            rectPn.anchorMin = new Vector2(0.5f, 1f);
            rectPn.anchorMax = new Vector2(0.5f, 1f);
            rectPn.anchoredPosition = new Vector2(0f, -80f);
            rectPn.sizeDelta = new Vector2(850f, 110f);
            var imgPn = panelNarrativo.AddComponent<Image>();
            imgPn.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPn.type = Image.Type.Sliced;
            imgPn.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);

            GameObject goTxtNarrativo = new GameObject("Texto_Narrativo");
            goTxtNarrativo.transform.SetParent(panelNarrativo.transform, false);
            var rectTn = goTxtNarrativo.AddComponent<RectTransform>();
            rectTn.anchorMin = Vector2.zero;
            rectTn.anchorMax = Vector2.one;
            rectTn.offsetMin = new Vector2(25f, 15f);
            rectTn.offsetMax = new Vector2(-25f, -15f);
            var tmpTn = goTxtNarrativo.AddComponent<TextMeshProUGUI>();
            tmpTn.font = fontTexto;
            tmpTn.fontSize = 17f;
            tmpTn.alignment = TextAlignmentOptions.Center;
            tmpTn.color = new Color(1f, 0.95f, 0.85f);
            tmpTn.text = "Refugio sale a la luz junto al Templo de San Cayetano. DetrÃ¡s, la mina se cierra para siempreâ€¦ pero la plata que llevas cuenta otra historia.";

            // Panel Izquierdo: Resumen Acumulado
            GameObject panelStats = new GameObject("Panel_Resumen");
            panelStats.transform.SetParent(canvasObj.transform, false);
            var rectPs = panelStats.AddComponent<RectTransform>();
            rectPs.anchorMin = new Vector2(0f, 0.5f);
            rectPs.anchorMax = new Vector2(0f, 0.5f);
            rectPs.anchoredPosition = new Vector2(230f, 0f);
            rectPs.sizeDelta = new Vector2(360f, 320f);
            var imgPs = panelStats.AddComponent<Image>();
            imgPs.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPs.type = Image.Type.Sliced;

            // TÃ­tulo Resumen
            GameObject goTitRes = new GameObject("Titulo_Resumen");
            goTitRes.transform.SetParent(panelStats.transform, false);
            var rectTr = goTitRes.AddComponent<RectTransform>();
            rectTr.anchorMin = new Vector2(0.5f, 1f);
            rectTr.anchorMax = new Vector2(0.5f, 1f);
            rectTr.anchoredPosition = new Vector2(0f, -30f);
            rectTr.sizeDelta = new Vector2(320f, 30f);
            var tmpTr = goTitRes.AddComponent<TextMeshProUGUI>();
            tmpTr.font = fontTitulo;
            tmpTr.fontSize = 16f;
            tmpTr.alignment = TextAlignmentOptions.Center;
            tmpTr.text = "LOGROS TOTALES";
            tmpTr.color = new Color(1f, 0.85f, 0.4f);

            // Stats textos
            TextMeshProUGUI tScore = CrearTextoStat(panelStats.transform, "Texto_PuntajeTotal", "PUNTAJE TOTAL: 0 pts", fontTexto);
            tScore.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 50f);
            tScore.fontSize = 18f;

            TextMeshProUGUI tEstrellas = CrearTextoStat(panelStats.transform, "Texto_EstrellasTotales", "ESTRELLAS: 0 / 9 â˜…", fontTexto);
            tEstrellas.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 15f);
            tEstrellas.fontSize = 17f;

            TextMeshProUGUI tMinerales = CrearTextoStat(panelStats.transform, "Texto_MineralesTotales", "MINERALES HISTÃ“RICOS: 0 / 3", fontTexto);
            tMinerales.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, -20f);
            tMinerales.fontSize = 15f;

            // Fila de 3 Iconos de Minerales
            GameObject goFilaMin = new GameObject("Fila_IconosMinerales");
            goFilaMin.transform.SetParent(panelStats.transform, false);
            var rectFm = goFilaMin.AddComponent<RectTransform>();
            rectFm.anchorMin = new Vector2(0.5f, 0f);
            rectFm.anchorMax = new Vector2(0.5f, 0f);
            rectFm.anchoredPosition = new Vector2(0f, 65f);
            rectFm.sizeDelta = new Vector2(220f, 48f);

            var hlgMin = goFilaMin.AddComponent<HorizontalLayoutGroup>();
            hlgMin.spacing = 20f;
            hlgMin.childAlignment = TextAnchor.MiddleCenter;

            Image icAg = CrearIcono(goFilaMin.transform, "Icono_Aguilarita", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Aguilarita.png"), new Vector2(40f, 40f));
            Image icPep = CrearIcono(goFilaMin.transform, "Icono_PepitaOro", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Pepita_Oro.png"), new Vector2(40f, 40f));
            Image icPir = CrearIcono(goFilaMin.transform, "Icono_Pirargirita", AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Pirargirita.png"), new Vector2(40f, 40f));

            // Panel Derecho: CrÃ©ditos en Scroll
            GameObject panelCreditos = new GameObject("Panel_Creditos_Mascara");
            panelCreditos.transform.SetParent(canvasObj.transform, false);
            var rectPc = panelCreditos.AddComponent<RectTransform>();
            rectPc.anchorMin = new Vector2(1f, 0.5f);
            rectPc.anchorMax = new Vector2(1f, 0.5f);
            rectPc.anchoredPosition = new Vector2(-230f, 0f);
            rectPc.sizeDelta = new Vector2(360f, 320f);
            var imgPc = panelCreditos.AddComponent<Image>();
            imgPc.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPc.type = Image.Type.Sliced;
            panelCreditos.AddComponent<RectMask2D>();

            GameObject goContCred = new GameObject("Contenedor_Creditos");
            goContCred.transform.SetParent(panelCreditos.transform, false);
            var rectCc = goContCred.AddComponent<RectTransform>();
            rectCc.anchorMin = new Vector2(0.5f, 0f);
            rectCc.anchorMax = new Vector2(0.5f, 0f);
            rectCc.pivot = new Vector2(0.5f, 0f);
            rectCc.anchoredPosition = new Vector2(0f, -280f);
            rectCc.sizeDelta = new Vector2(320f, 650f);

            var tmpCred = goContCred.AddComponent<TextMeshProUGUI>();
            tmpCred.font = fontTexto;
            tmpCred.fontSize = 15f;
            tmpCred.alignment = TextAlignmentOptions.Center;
            tmpCred.text = "<b><size=18>EL MINERO</size></b>\n" +
                           "Plataformas 2D\n\n" +
                           "<b>DESARROLLO:</b>\n" +
                           "Juana Jaqueline Camarillo Olaez\n" +
                           "GIDS6102-E\n\n" +
                           "<b>UNIVERSIDAD:</b>\n" +
                           "UTNG\n\n" +
                           "<b>DOCENTE:</b>\n" +
                           "Gabriel BarrÃ³n RodrÃ­guez\n" +
                           "CreaciÃ³n de Videojuegos\n\n" +
                           "<b>MÃšSICA Y ARTE:</b>\n" +
                           "Herramientas de IA Asistida\n\n" +
                           "<b>FUENTES:</b>\n" +
                           "Press Start 2P & Pixelify Sans\n(SIL OFL 1.1)\n\n" +
                           "<i>Â¡Felicidades por escapar!</i>";

            // BotÃ³n Volver al MenÃº Principal
            Button btnMenu = CrearBotonMadera(canvasObj.transform, "Boton_MenuPrincipal", "MenÃº Principal", new Vector2(0f, 40f), new Vector2(220f, 45f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));

            SerializedObject so = new SerializedObject(finalCtrl);
            so.FindProperty("textoNarrativo").objectReferenceValue = tmpTn;
            so.FindProperty("textoPuntajeTotal").objectReferenceValue = tScore;
            so.FindProperty("textoEstrellasTotales").objectReferenceValue = tEstrellas;
            so.FindProperty("textoMineralesTotales").objectReferenceValue = tMinerales;
            so.FindProperty("iconoAguilarita").objectReferenceValue = icAg;
            so.FindProperty("iconoPepitaOro").objectReferenceValue = icPep;
            so.FindProperty("iconoPirargirita").objectReferenceValue = icPir;
            so.FindProperty("contenedorCreditos").objectReferenceValue = rectCc;
            so.FindProperty("botonMenuPrincipal").objectReferenceValue = btnMenu;
            so.ApplyModifiedProperties();

            CrearSistemasPersistentes();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Final.unity");
            Debug.Log("[LevelBuilder] Escena Final.unity guardada.");
        }

        private static TextMeshProUGUI CrearTextoStat(Transform parent, string name, string text, TMP_FontAsset font)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(320f, 30f);
            var tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = text;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        private static Image CrearIcono(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            var rect = obj.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            var img = obj.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return img;
        }

        public static void EliminarEscenasObsoletas()
        {
            string[] obsoletas = new string[]
            {
                "Assets/Scenes/Victoria.unity",
                "Assets/Scenes/Derrota.unity",
                "Assets/Scenes/SampleScene.unity"
            };

            foreach (var path in obsoletas)
            {
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                    Debug.Log($"[LevelBuilder] Escena obsoleta eliminada: {path}");
                }
            }
        }

        // ====================================================================
        // 4. NIVEL 1 â€” MINA RAYAS (130u, 110s, 6 vetas, 2 gemas, 4 huecos, 7 spawners, 2 derrumbes)
        // ====================================================================
        public static void ConstruirNivel1_MinaRayas()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Color bgVioleta = new Color(0.12f, 0.09f, 0.19f); // #1E1630
            Color luzAmbar = new Color(0.96f, 0.83f, 0.54f);

            // 1. IluminaciÃ³n URP 2D
            CrearIluminacionNivel(bgVioleta, 0.75f);

            // 2. Parallax 3 Capas Mina Rayas (extendido a 130u)
            CrearParallaxNivel("MinaRayas", 130f);

            // 3. Grid y Tilemaps
            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 8;
            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();

            Tilemap tmSuelo = CrearTilemap(gridObj, "Tilemap_Suelo", Constantes.SortingLayers.Escenario, layerSuelo, true);
            Tilemap tmDeco = CrearTilemap(gridObj, "Tilemap_Decoracion", Constantes.SortingLayers.Objetos, 0, false);

            ConstruirTerrenoNivel1(tmSuelo, tmDeco);
            FinalizarColisionesTilemap(tmSuelo);

            // 4. Huecos Letales (4 huecos)
            CrearHuecoLetal(new Vector3(20f, -4f, 0f), new Vector2(5f, 2f));
            CrearHuecoLetal(new Vector3(68f, -4f, 0f), new Vector2(5f, 2f));
            CrearHuecoLetal(new Vector3(86f, -4f, 0f), new Vector2(5f, 2f));
            CrearHuecoLetal(new Vector3(108f, -4f, 0f), new Vector2(5f, 2f));

            // 5. Plataformas fijas con escalones alcanzables (Î”Y <= 1.3u)
            CrearPlataformaFija(new Vector3(5f, 0.3f, 0f), layerSuelo);     // EscalÃ³n bajo
            CrearPlataformaFija(new Vector3(20f, 0.2f, 0f), layerSuelo);    // Sobre Hueco 1
            CrearPlataformaFija(new Vector3(35f, 0.3f, 0f), layerSuelo);    // Escalera 1A
            CrearPlataformaFija(new Vector3(39f, 1.4f, 0f), layerSuelo);    // Escalera 1B
            CrearPlataformaFija(new Vector3(43f, 0.4f, 0f), layerSuelo);    // Escalera 1C
            CrearPlataformaFija(new Vector3(54f, 0.3f, 0f), layerSuelo);    // Escalera 2A
            CrearPlataformaFija(new Vector3(58f, 1.4f, 0f), layerSuelo);    // Escalera 2B
            CrearPlataformaFija(new Vector3(68f, 0.2f, 0f), layerSuelo);    // Sobre Hueco 2
            CrearPlataformaFija(new Vector3(77f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(86f, 0.3f, 0f), layerSuelo);    // Sobre Hueco 3
            CrearPlataformaFija(new Vector3(98f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(108f, 0.2f, 0f), layerSuelo);   // Sobre Hueco 4
            CrearPlataformaFija(new Vector3(115f, 0.5f, 0f), layerSuelo);

            // 6. Coleccionables (6 vetas de plata, 2 gemas)
            CrearVetaPlata(new Vector3(5f, 1.3f, 0f));
            CrearVetaPlata(new Vector3(39f, 2.4f, 0f));
            CrearVetaPlata(new Vector3(58f, 2.4f, 0f));
            CrearVetaPlata(new Vector3(77f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(98f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(115f, 1.5f, 0f));

            CrearGemaAmatista(new Vector3(20f, 1.4f, 0f)); // Sobre hueco 1
            CrearGemaAmatista(new Vector3(86f, 1.5f, 0f)); // Sobre hueco 3

            // Mineral distintivo Nivel 1: Aguilarita
            CrearMineralDistintivo("Mineral_Aguilarita", new Vector3(58f, 3.5f, 0f));

            // Enemigos Nivel 1: 2 MurciÃ©lagos estÃ¡ndar
            CrearMurcielago(new Vector3(25f, 2.5f, 0f), 5f);
            CrearMurcielago(new Vector3(75f, 2.5f, 0f), 5f);

            // Elevador tipo malacate
            CrearElevador(new Vector3(50f, -1f, 0f), new Vector3(50f, 3f, 0f), 2f);

            // 7. ObstÃ¡culos: 7 Spawners de Rocas rÃ¡pidos (intervalo 2.0s - 2.2s)
            CrearRockSpawner(new Vector3(12f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(28f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(48f, 7f, 0f), 2.0f);
            CrearRockSpawner(new Vector3(64f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(80f, 7f, 0f), 2.0f);
            CrearRockSpawner(new Vector3(95f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(112f, 7f, 0f), 1.8f);

            // 8. Checkpoints funcionales (con Capilla en el centro)
            CrearCheckpoint(new Vector3(30f, -0.5f, 0f));
            CrearCheckpointCapilla(new Vector3(73f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(102f, -0.5f, 0f));

            // 9. Zonas de derrumbe sorpresa (2 derrumbes que colapsan tras pasar)
            CrearZonaDerrumbe(new Vector3(46f, 0f, 0f), new Vector3(41f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(92f, 0f, 0f), new Vector3(87f, -0.5f, 0f));

            // 10. Temblor periÃ³dico
            CrearMineTremor();

            // 11. Salida del nivel (TÃºnel al final X = 124)
            CrearSalida(new Vector3(124f, -1f, 0f), false);

            // 12. AmbientaciÃ³n (Antorchas)
            CrearAntorchaConLuz(new Vector3(-5f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(14f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(32f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(50f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(70f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(90f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(108f, 0.8f, 0f), luzAmbar);
            CrearAntorchaConLuz(new Vector3(122f, 0.8f, 0f), luzAmbar);

            // 13. Jugador (Minero)
            GameObject jugador = CrearJugador(new Vector3(-7f, 0.5f, 0f), layerSuelo);

            // 14. CÃ¡mara Cinemachine y Brain
            ConfigurarCamaraNivel(jugador, bgVioleta, new Vector2(-15f, -6f), new Vector2(135f, 12f));

            // 15. Managers y HUD
            var configN1 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Settings/Niveles/LevelConfig_Nivel1.asset");
            ConfigurarManagersYHUD(jugador, configN1, 6, 2);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Nivel1_MinaRayas.unity");
        }

        // ====================================================================
        // 5. NIVEL 2 â€” EL NOPAL (170u, 120s, 8 vetas, 3 gemas, 4 plat mÃ³viles, 6 huecos, 10 spawners, 3 derrumbes)
        // ====================================================================
        public static void ConstruirNivel2_ElNopal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Color bgAmbar = new Color(0.16f, 0.10f, 0.05f); // #2A1A0E
            Color luzAmbarCalido = new Color(1.0f, 0.82f, 0.48f); // #FFD27A

            CrearIluminacionNivel(bgAmbar, 0.7f);
            CrearParallaxNivel("ElNopal", 170f);

            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 8;
            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();

            Tilemap tmSuelo = CrearTilemap(gridObj, "Tilemap_Suelo", Constantes.SortingLayers.Escenario, layerSuelo, true);
            Tilemap tmDeco = CrearTilemap(gridObj, "Tilemap_Decoracion", Constantes.SortingLayers.Objetos, 0, false);

            ConstruirTerrenoNivel2(tmSuelo, tmDeco);
            FinalizarColisionesTilemap(tmSuelo);

            // 6 Huecos Letales
            CrearHuecoLetal(new Vector3(14f, -4f, 0f), new Vector2(6f, 2f));
            CrearHuecoLetal(new Vector3(44f, -4f, 0f), new Vector2(12f, 2f));
            CrearHuecoLetal(new Vector3(78f, -4f, 0f), new Vector2(8f, 2f));
            CrearHuecoLetal(new Vector3(98f, -4f, 0f), new Vector2(8f, 2f));
            CrearHuecoLetal(new Vector3(126f, -4f, 0f), new Vector2(8f, 2f));
            CrearHuecoLetal(new Vector3(152f, -4f, 0f), new Vector2(8f, 2f));

            // Plataformas MÃ³viles sobre los huecos
            CrearPlataformaMovil(new Vector3(11f, 0.2f, 0f), new Vector3(17f, 0.2f, 0f), 2.2f, layerSuelo);
            CrearPlataformaMovil(new Vector3(37f, 0.2f, 0f), new Vector3(43f, 0.2f, 0f), 2.0f, layerSuelo);
            CrearPlataformaMovil(new Vector3(44f, 0.5f, 0f), new Vector3(49f, 0.5f, 0f), 2.2f, layerSuelo);
            CrearPlataformaMovil(new Vector3(94f, 0.3f, 0f), new Vector3(101f, 0.3f, 0f), 2.5f, layerSuelo);
            CrearPlataformaMovil(new Vector3(122f, 0.4f, 0f), new Vector3(129f, 0.4f, 0f), 3.0f, layerSuelo);

            // Plataformas fijas con escalones alcanzables (Î”Y <= 1.3u)
            CrearPlataformaFija(new Vector3(24f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(58f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(62f, 1.4f, 0f), layerSuelo); // EscalÃ³n alto accesible
            CrearPlataformaFija(new Vector3(76f, 0.2f, 0f), layerSuelo); // Sobre hueco 3A
            CrearPlataformaFija(new Vector3(80f, 0.5f, 0f), layerSuelo); // Sobre hueco 3B
            CrearPlataformaFija(new Vector3(86f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(110f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(114f, 1.5f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(138f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(152f, 0.3f, 0f), layerSuelo); // Sobre hueco 6

            // 8 Vetas de plata
            CrearVetaPlata(new Vector3(4f, 0.5f, 0f));
            CrearVetaPlata(new Vector3(24f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(45f, 1.5f, 0f));
            CrearVetaPlata(new Vector3(62f, 2.4f, 0f));
            CrearVetaPlata(new Vector3(86f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(114f, 2.5f, 0f));
            CrearVetaPlata(new Vector3(138f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(158f, 0.5f, 0f));

            // 3 Gemas de amatista
            CrearGemaAmatista(new Vector3(42f, 1.6f, 0f));
            CrearGemaAmatista(new Vector3(97f, 1.4f, 0f));
            CrearGemaAmatista(new Vector3(126f, 1.5f, 0f));

            // Mineral distintivo Nivel 2: Pepita de Oro
            CrearMineralDistintivo("Mineral_PepitaOro", new Vector3(97f, 2.8f, 0f));

            // Enemigos Nivel 2: 3 AraÃ±as que bajan y 2 MurciÃ©lagos
            CrearArana(new Vector3(32f, 5.5f, 0f));
            CrearArana(new Vector3(88f, 5.5f, 0f));
            CrearArana(new Vector3(142f, 5.5f, 0f));
            CrearMurcielago(new Vector3(20f, 2.5f, 0f), 6f);
            CrearMurcielago(new Vector3(110f, 2.5f, 0f), 6f);

            // ObstÃ¡culos Nivel 2: 2 Vagonetas desbocadas con triggers
            CrearVagoneta(new Vector3(45f, 0f, 0f), new Vector3(65f, -1.2f, 0f), Vector2.left, 22f);
            CrearVagoneta(new Vector3(120f, 0f, 0f), new Vector3(140f, -1.2f, 0f), Vector2.left, 22f);

            // 10 Rock spawners rÃ¡pidos (intervalo 1.8s)
            CrearRockSpawner(new Vector3(8f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(20f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(32f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(52f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(68f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(88f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(105f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(126f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(142f, 7f, 0f), 1.8f);
            CrearRockSpawner(new Vector3(156f, 7f, 0f), 1.8f);

            // 4 Checkpoints (con Capillas intermedias)
            CrearCheckpoint(new Vector3(28f, -0.5f, 0f));
            CrearCheckpointCapilla(new Vector3(66f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(108f, -0.5f, 0f));
            CrearCheckpointCapilla(new Vector3(142f, -0.5f, 0f));

            // 3 Zonas de derrumbe sorpresa
            CrearZonaDerrumbe(new Vector3(54f, 0f, 0f), new Vector3(49f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(104f, 0f, 0f), new Vector3(99f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(158f, 0f, 0f), new Vector3(153f, -0.5f, 0f));

            CrearMineTremor();
            CrearSalida(new Vector3(164f, -1f, 0f), false);

            CrearAntorchaConLuz(new Vector3(-5f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(18f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(38f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(56f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(76f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(96f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(116f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(136f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(154f, 0.8f, 0f), luzAmbarCalido);
            CrearAntorchaConLuz(new Vector3(162f, 0.8f, 0f), luzAmbarCalido);

            GameObject jugador = CrearJugador(new Vector3(-7f, 0.5f, 0f), layerSuelo);
            ConfigurarCamaraNivel(jugador, bgAmbar, new Vector2(-15f, -6f), new Vector2(175f, 14f));

            var configN2 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Settings/Niveles/LevelConfig_Nivel2.asset");
            ConfigurarManagersYHUD(jugador, configN2, 8, 3);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Nivel2_ElNopal.unity");
        }

        // ====================================================================
        // 6. NIVEL 3 â€” LA VALENCIANA (210u, 130s, 10 vetas, 4 gemas, 9 quebradizas, 2 mÃ³viles, 9 huecos, 14 spawners, 4 derrumbes)
        // ====================================================================
        public static void ConstruirNivel3_LaValenciana()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Color bgRojo = new Color(0.16f, 0.04f, 0.04f); // #2A0A0A
            Color luzFuego = new Color(1.0f, 0.69f, 0.38f); // #FFB060

            CrearIluminacionNivel(bgRojo, 0.65f);
            CrearParallaxNivel("LaValenciana", 210f);

            int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
            if (layerSuelo == -1) layerSuelo = 8;
            GameObject gridObj = new GameObject("Grid");
            gridObj.AddComponent<Grid>();

            Tilemap tmSuelo = CrearTilemap(gridObj, "Tilemap_Suelo", Constantes.SortingLayers.Escenario, layerSuelo, true);
            Tilemap tmDeco = CrearTilemap(gridObj, "Tilemap_Decoracion", Constantes.SortingLayers.Objetos, 0, false);

            ConstruirTerrenoNivel3(tmSuelo, tmDeco);
            FinalizarColisionesTilemap(tmSuelo);

            // 9 Huecos Letales
            CrearHuecoLetal(new Vector3(11f, -4f, 0f), new Vector2(6f, 2f));
            CrearHuecoLetal(new Vector3(25f, -4f, 0f), new Vector2(6f, 2f));
            CrearHuecoLetal(new Vector3(47f, -4f, 0f), new Vector2(14f, 2f));
            CrearHuecoLetal(new Vector3(83f, -4f, 0f), new Vector2(14f, 2f));
            CrearHuecoLetal(new Vector3(104f, -4f, 0f), new Vector2(12f, 2f));
            CrearHuecoLetal(new Vector3(132f, -4f, 0f), new Vector2(12f, 2f));
            CrearHuecoLetal(new Vector3(152f, -4f, 0f), new Vector2(8f, 2f));
            CrearHuecoLetal(new Vector3(172f, -4f, 0f), new Vector2(12f, 2f));
            CrearHuecoLetal(new Vector3(191f, -4f, 0f), new Vector2(10f, 2f));

            // Plataformas Quebradizas
            CrearPlataformaQuebradiza(new Vector3(11f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(25f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(43f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(51f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(79f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(101f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(106f, 0.4f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(129f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(152f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(175f, 0.3f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(189f, 0.2f, 0f), layerSuelo);
            CrearPlataformaQuebradiza(new Vector3(193f, 0.4f, 0f), layerSuelo);

            // Plataformas MÃ³viles
            CrearPlataformaMovil(new Vector3(83f, 0.4f, 0f), new Vector3(88f, 0.4f, 0f), 2.5f, layerSuelo);
            CrearPlataformaMovil(new Vector3(166f, 0.3f, 0f), new Vector3(173f, 0.3f, 0f), 2.8f, layerSuelo);

            // Plataformas fijas con escalones alcanzables (Î”Y <= 1.3u)
            CrearPlataformaFija(new Vector3(18f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(35f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(47f, 0.5f, 0f), layerSuelo);  // Sobre hueco 3
            CrearPlataformaFija(new Vector3(64f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(68f, 1.4f, 0f), layerSuelo);  // EscalÃ³n alto
            CrearPlataformaFija(new Vector3(94f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(118f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(122f, 1.5f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(134f, 0.5f, 0f), layerSuelo); // Sobre hueco 6
            CrearPlataformaFija(new Vector3(144f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(162f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(182f, 0.4f, 0f), layerSuelo);

            // 10 Vetas de plata
            CrearVetaPlata(new Vector3(4f, 0.5f, 0f));
            CrearVetaPlata(new Vector3(18f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(47f, 1.5f, 0f));
            CrearVetaPlata(new Vector3(68f, 2.4f, 0f));
            CrearVetaPlata(new Vector3(86f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(106f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(122f, 2.5f, 0f));
            CrearVetaPlata(new Vector3(144f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(162f, 1.4f, 0f));
            CrearVetaPlata(new Vector3(182f, 1.4f, 0f));

            // 4 Gemas de amatista
            CrearGemaAmatista(new Vector3(25f, 1.4f, 0f));
            CrearGemaAmatista(new Vector3(51f, 1.4f, 0f));
            CrearGemaAmatista(new Vector3(134f, 1.5f, 0f));
            CrearGemaAmatista(new Vector3(193f, 1.5f, 0f));

            // Mineral distintivo Nivel 3: Pirargirita
            CrearMineralDistintivo("Mineral_Pirargirita", new Vector3(144f, 2.8f, 0f));

            // Enemigos Nivel 3: 3 Alacranes, 3 MurciÃ©lagos Rojos en picada, 2 AraÃ±as
            CrearAlacran(new Vector3(35f, -1.5f, 0f), 4f);
            CrearAlacran(new Vector3(94f, -1.5f, 0f), 5f);
            CrearAlacran(new Vector3(162f, -1.5f, 0f), 4f);

            CrearMurcielagoRojo(new Vector3(20f, 4.0f, 0f));
            CrearMurcielagoRojo(new Vector3(80f, 4.0f, 0f));
            CrearMurcielagoRojo(new Vector3(150f, 4.0f, 0f));

            CrearArana(new Vector3(60f, 6.0f, 0f));
            CrearArana(new Vector3(130f, 6.0f, 0f));

            // ObstÃ¡culos Nivel 3: 2 Vagonetas, 2 Llamas de gas, 1 Elevador malacate
            CrearVagoneta(new Vector3(50f, 0f, 0f), new Vector3(72f, -1.2f, 0f), Vector2.left, 22f);
            CrearVagoneta(new Vector3(154f, 0f, 0f), new Vector3(175f, -1.2f, 0f), Vector2.left, 22f);

            CrearFuego(new Vector3(104f, -1.6f, 0f));
            CrearFuego(new Vector3(172f, -1.6f, 0f));

            CrearElevador(new Vector3(118f, -1.5f, 0f), new Vector3(118f, 2.8f, 0f), 2.2f);

            // 14 Spawners de Rocas implacables (intervalo urgente 1.3s)
            CrearRockSpawner(new Vector3(6f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(16f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(26f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(44f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(56f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(74f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(88f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(104f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(120f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(136f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(150f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(168f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(184f, 8f, 0f), 1.3f);
            CrearRockSpawner(new Vector3(198f, 8f, 0f), 1.3f);

            // 5 Checkpoints (con Capillas intermedias)
            CrearCheckpoint(new Vector3(32f, -0.5f, 0f));
            CrearCheckpointCapilla(new Vector3(72f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(114f, -0.5f, 0f));
            CrearCheckpointCapilla(new Vector3(158f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(180f, -0.5f, 0f));

            // 4 Zonas de derrumbe sorpresa
            CrearZonaDerrumbe(new Vector3(58f, 0f, 0f), new Vector3(53f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(112f, 0f, 0f), new Vector3(107f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(160f, 0f, 0f), new Vector3(155f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(198f, 0f, 0f), new Vector3(194f, -0.5f, 0f));

            CrearMineTremor();

            // Salida a la Superficie (Final)
            CrearSalida(new Vector3(204f, -1f, 0f), true);

            CrearAntorchaConLuz(new Vector3(-5f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(18f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(36f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(54f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(72f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(90f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(108f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(126f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(144f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(162f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(180f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(198f, 0.8f, 0f), luzFuego);
            CrearAntorchaConLuz(new Vector3(202f, 0.8f, 0f), luzFuego);

            GameObject jugador = CrearJugador(new Vector3(-7f, 0.5f, 0f), layerSuelo);
            ConfigurarCamaraNivel(jugador, bgRojo, new Vector2(-15f, -6f), new Vector2(215f, 15f));

            var configN3 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Settings/Niveles/LevelConfig_Nivel3.asset");
            ConfigurarManagersYHUD(jugador, configN3, 10, 4);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Nivel3_LaValenciana.unity");
        }

        // ====================================================================
        // CONSTRUCCIÃ“N PROCEDURAL DE TERRENOS POR NIVEL
        // ====================================================================
        private static void ConstruirTerrenoNivel1(Tilemap tmSuelo, Tilemap tmDeco)
        {
            string nivel = "MinaRayas";
            Tile t0 = CargarTile(nivel, 0); // Sup-Izq
            Tile t1 = CargarTile(nivel, 1); // Superficie
            Tile t2 = CargarTile(nivel, 2); // Sup-Der
            Tile t9 = CargarTile(nivel, 9); // Relleno
            Tile t17 = CargarTile(nivel, 17); // Techo
            Tile t7 = CargarTile(nivel, 7); // Estalactita

            // Suelo extendido de X = -12 a X = 128 con 4 huecos
            for (int x = -12; x <= 128; x++)
            {
                bool esHueco = (x >= 18 && x <= 22) || (x >= 66 && x <= 70) || (x >= 84 && x <= 88) || (x >= 106 && x <= 110);

                if (!esHueco)
                {
                    Tile sup = (x == -12) ? t0 : (x == 128 ? t2 : t1);
                    if (x == 17 || x == 65 || x == 83 || x == 105) sup = t2;
                    else if (x == 23 || x == 71 || x == 89 || x == 111) sup = t0;

                    tmSuelo.SetTile(new Vector3Int(x, -2, 0), sup);
                    tmSuelo.SetTile(new Vector3Int(x, -3, 0), t9);
                    tmSuelo.SetTile(new Vector3Int(x, -4, 0), t9);
                }

                // Techo
                tmSuelo.SetTile(new Vector3Int(x, 6, 0), t17);
                tmSuelo.SetTile(new Vector3Int(x, 7, 0), t9);

                // Estalactitas periÃ³dicas
                if (x % 7 == 0 && t7 != null)
                {
                    tmDeco.SetTile(new Vector3Int(x, 5, 0), t7);
                }
            }

            // Paredes laterales de la mina para contener al jugador
            for (int y = -1; y <= 5; y++)
            {
                tmSuelo.SetTile(new Vector3Int(-12, y, 0), t9);
                tmSuelo.SetTile(new Vector3Int(128, y, 0), t9);
            }
        }

        private static void ConstruirTerrenoNivel2(Tilemap tmSuelo, Tilemap tmDeco)
        {
            string nivel = "ElNopal";
            Tile t0 = CargarTile(nivel, 0);
            Tile t1 = CargarTile(nivel, 1);
            Tile t2 = CargarTile(nivel, 2);
            Tile t9 = CargarTile(nivel, 9);
            Tile t17 = CargarTile(nivel, 17);
            Tile t7 = CargarTile(nivel, 7);

            // Suelo extendido de X = -12 a X = 168 con 6 huecos
            for (int x = -12; x <= 168; x++)
            {
                bool esHueco = (x >= 11 && x <= 17) || (x >= 38 && x <= 50) || (x >= 74 && x <= 82) || 
                               (x >= 94 && x <= 102) || (x >= 122 && x <= 130) || (x >= 148 && x <= 156);

                if (!esHueco)
                {
                    Tile sup = t1;
                    if (x == 10 || x == 37 || x == 73 || x == 93 || x == 121 || x == 147) sup = t2;
                    else if (x == 18 || x == 51 || x == 83 || x == 103 || x == 131 || x == 157) sup = t0;

                    tmSuelo.SetTile(new Vector3Int(x, -2, 0), sup);
                    tmSuelo.SetTile(new Vector3Int(x, -3, 0), t9);
                    tmSuelo.SetTile(new Vector3Int(x, -4, 0), t9);
                }

                // Techo
                tmSuelo.SetTile(new Vector3Int(x, 6, 0), t17);
                tmSuelo.SetTile(new Vector3Int(x, 7, 0), t9);

                if (x % 8 == 0 && t7 != null)
                {
                    tmDeco.SetTile(new Vector3Int(x, 5, 0), t7);
                }
            }

            // Paredes laterales
            for (int y = -1; y <= 5; y++)
            {
                tmSuelo.SetTile(new Vector3Int(-12, y, 0), t9);
                tmSuelo.SetTile(new Vector3Int(168, y, 0), t9);
            }
        }

        private static void ConstruirTerrenoNivel3(Tilemap tmSuelo, Tilemap tmDeco)
        {
            string nivel = "LaValenciana";
            Tile t0 = CargarTile(nivel, 0);
            Tile t1 = CargarTile(nivel, 1);
            Tile t2 = CargarTile(nivel, 2);
            Tile t9 = CargarTile(nivel, 9);
            Tile t17 = CargarTile(nivel, 17);
            Tile t7 = CargarTile(nivel, 7);

            // Suelo extendido de X = -12 a X = 208 con 9 huecos desafiantes
            for (int x = -12; x <= 208; x++)
            {
                bool esHueco = (x >= 8 && x <= 14) || (x >= 22 && x <= 28) || (x >= 40 && x <= 54) ||
                               (x >= 76 && x <= 90) || (x >= 98 && x <= 110) || (x >= 126 && x <= 138) ||
                               (x >= 148 && x <= 156) || (x >= 166 && x <= 178) || (x >= 186 && x <= 196);

                if (!esHueco)
                {
                    Tile sup = t1;
                    if (x == 7 || x == 21 || x == 39 || x == 75 || x == 97 || x == 125 || x == 147 || x == 165 || x == 185) sup = t2;
                    else if (x == 15 || x == 29 || x == 55 || x == 91 || x == 111 || x == 139 || x == 157 || x == 179 || x == 197) sup = t0;

                    tmSuelo.SetTile(new Vector3Int(x, -2, 0), sup);
                    tmSuelo.SetTile(new Vector3Int(x, -3, 0), t9);
                    tmSuelo.SetTile(new Vector3Int(x, -4, 0), t9);
                }

                // Techo
                tmSuelo.SetTile(new Vector3Int(x, 7, 0), t17);
                tmSuelo.SetTile(new Vector3Int(x, 8, 0), t9);

                if (x % 6 == 0 && t7 != null)
                {
                    tmDeco.SetTile(new Vector3Int(x, 6, 0), t7);
                }
            }

            // Paredes laterales
            for (int y = -1; y <= 6; y++)
            {
                tmSuelo.SetTile(new Vector3Int(-12, y, 0), t9);
                tmSuelo.SetTile(new Vector3Int(208, y, 0), t9);
            }
        }

        private static Tile CargarTile(string nivel, int index)
        {
            return AssetDatabase.LoadAssetAtPath<Tile>($"Assets/Tiles/{nivel}/Tile_{nivel}_{index}.asset");
        }

        // ====================================================================
        // HELPERS DE INSTANCIACIÃ“N DE ELEMENTOS
        // ====================================================================
        private static void CrearIluminacionNivel(Color bgColor, float globalIntensity)
        {
            GameObject luzGlobal = new GameObject("Global Light 2D");
            var l2d = luzGlobal.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.intensity = globalIntensity;
            l2d.color = new Color(0.9f, 0.85f, 0.95f);
            ElMineroBuilder.AsignarCapasALuz2D(l2d);
        }

        private static void CrearParallaxNivel(string nivel, float levelLength)
        {
            GameObject parallaxRoot = new GameObject("[Parallax_Fondos]");

            string path0 = $"Assets/Sprites/Fondos/{nivel}/Fondo_{nivel}_Capa0_Lejana.png";
            string path1 = $"Assets/Sprites/Fondos/{nivel}/Fondo_{nivel}_Capa1_Media.png";
            string path2 = $"Assets/Sprites/Fondos/{nivel}/Fondo_{nivel}_Capa2_Cercana.png";

            CrearCapaParallax(parallaxRoot, "Capa0_Lejana", path0, 0.8f, Constantes.SortingLayers.Fondo, levelLength);
            CrearCapaParallax(parallaxRoot, "Capa1_Media", path1, 0.5f, Constantes.SortingLayers.FondoMedio, levelLength);
            CrearCapaParallax(parallaxRoot, "Capa2_Cercana", path2, 0.2f, Constantes.SortingLayers.FondoMedio, levelLength);
        }

        private static void CrearCapaParallax(GameObject parent, string name, string spritePath, float effect, string sortingLayer, float levelLength)
        {
            GameObject capa = new GameObject(name);
            capa.transform.SetParent(parent.transform);
            capa.transform.position = new Vector3(levelLength * 0.4f, 2f, 0f);

            var sr = capa.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingLayerName = sortingLayer;
            sr.drawMode = SpriteDrawMode.Tiled;
            sr.size = new Vector2(levelLength * 2.5f, 15f);

            var parallax = capa.AddComponent<ParallaxBackground>();
            SerializedObject so = new SerializedObject(parallax);
            so.FindProperty("parallaxEffectX").floatValue = effect;
            so.FindProperty("parallaxEffectY").floatValue = 0.02f;
            so.ApplyModifiedProperties();
        }

        private static Tilemap CrearTilemap(GameObject grid, string name, string sortingLayer, int layer, bool conCollider)
        {
            GameObject tmObj = new GameObject(name);
            tmObj.transform.SetParent(grid.transform);
            tmObj.layer = layer;

            var tm = tmObj.AddComponent<Tilemap>();
            var rend = tmObj.AddComponent<TilemapRenderer>();
            rend.sortingLayerName = sortingLayer;

            if (conCollider)
            {
                var rb = tmObj.AddComponent<Rigidbody2D>();
                rb.bodyType = RigidbodyType2D.Static;

                var tmCol = tmObj.AddComponent<TilemapCollider2D>();
                tmCol.compositeOperation = Collider2D.CompositeOperation.Merge;

                var compCol = tmObj.AddComponent<CompositeCollider2D>();
                compCol.geometryType = CompositeCollider2D.GeometryType.Polygons;
            }

            return tm;
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
                    Debug.LogWarning($"[LevelBuilder] CompositeCollider2D en '{tm.name}' tiene 0 caminos. Usando colisiÃ³n directa en TilemapCollider2D.");
                    tmCol.compositeOperation = Collider2D.CompositeOperation.None;
                    UnityEngine.Object.DestroyImmediate(compCol);
                }
                else
                {
                    Debug.Log($"[LevelBuilder] CompositeCollider2D en '{tm.name}' generado exitosamente con {compCol.pathCount} caminos poligonales.");
                }
            }
        }

        private static void CrearPlataformaFija(Vector3 pos, int layerSuelo)
        {
            GameObject plat = new GameObject("Plataforma_Fija");
            plat.transform.position = pos;
            plat.layer = layerSuelo;

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Fija.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);
        }

        private static void CrearPlataformaMovil(Vector3 posA, Vector3 posB, float velocidad, int layerSuelo)
        {
            GameObject plat = new GameObject("Plataforma_Movil");
            plat.transform.position = posA;
            plat.layer = layerSuelo;

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Movil.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var rb = plat.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            GameObject pA = new GameObject("PuntoA");
            pA.transform.SetParent(plat.transform);
            pA.transform.position = posA;

            GameObject pB = new GameObject("PuntoB");
            pB.transform.SetParent(plat.transform);
            pB.transform.position = posB;

            var mov = plat.AddComponent<MovingPlatform>();
            SerializedObject so = new SerializedObject(mov);
            so.FindProperty("pointA").objectReferenceValue = pA.transform;
            so.FindProperty("pointB").objectReferenceValue = pB.transform;
            so.FindProperty("speed").floatValue = velocidad;
            so.ApplyModifiedProperties();
        }

        private static void CrearPlataformaQuebradiza(Vector3 pos, int layerSuelo)
        {
            GameObject plat = new GameObject("Plataforma_Quebradiza");
            plat.transform.position = pos;
            plat.layer = layerSuelo;

            var sr = plat.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = plat.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            var bp = plat.AddComponent<BreakablePlatform>();
            var anim = plat.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/PlataformaQuebradiza.controller");
        }

        private static void CrearHuecoLetal(Vector3 pos, Vector2 size)
        {
            GameObject pit = new GameObject("HuecoLetal");
            pit.tag = Constantes.Tags.Hazard;
            pit.transform.position = pos;

            var col = pit.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = size;

            pit.AddComponent<DeathPit>();
        }

        private static void CrearVetaPlata(Vector3 pos)
        {
            GameObject veta = new GameObject("Veta_Plata");
            veta.tag = Constantes.Tags.Collectible;
            int layerColeccionable = LayerMask.NameToLayer(Constantes.Layers.Coleccionable);
            veta.layer = layerColeccionable != -1 ? layerColeccionable : 0;
            veta.transform.position = pos;

            var sr = veta.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Veta_Plata.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = veta.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            veta.AddComponent<SilverVein>();

            var anim = veta.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Coleccionables/Veta.controller");
        }

        private static void CrearGemaAmatista(Vector3 pos)
        {
            GameObject gema = new GameObject("Gema_Amatista");
            gema.tag = Constantes.Tags.Collectible;
            int layerColeccionable = LayerMask.NameToLayer(Constantes.Layers.Coleccionable);
            gema.layer = layerColeccionable != -1 ? layerColeccionable : 0;
            gema.transform.position = pos;

            var sr = gema.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Gema_Amatista.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = gema.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            gema.AddComponent<AmethystGem>();

            var anim = gema.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Coleccionables/Gema.controller");
        }

        private static void CrearRockSpawner(Vector3 pos, float intervalo)
        {
            int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
            if (layerPeligro == -1) layerPeligro = 0;

            // Template Roca
            GameObject rockTemplate = new GameObject("Roca_Template");
            rockTemplate.tag = Constantes.Tags.Hazard;
            rockTemplate.layer = layerPeligro;
            var srRock = rockTemplate.AddComponent<SpriteRenderer>();
            srRock.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Roca.png");
            srRock.sortingLayerName = Constantes.SortingLayers.Objetos;
            var colRock = rockTemplate.AddComponent<CircleCollider2D>();
            colRock.radius = 0.45f;
            var rbRock = rockTemplate.AddComponent<Rigidbody2D>();
            rbRock.bodyType = RigidbodyType2D.Dynamic;
            rockTemplate.AddComponent<FallingRock>();
            var animRock = rockTemplate.AddComponent<Animator>();
            animRock.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/Roca.controller");

            // Template Advertencia
            GameObject warnTemplate = new GameObject("Advertencia_Template");
            var srWarn = warnTemplate.AddComponent<SpriteRenderer>();
            srWarn.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Advertencia_Sombra.png");
            srWarn.sortingLayerName = Constantes.SortingLayers.Objetos;
            srWarn.color = new Color(1f, 1f, 1f, 0.7f);

            // Spawner
            GameObject spawnerObj = new GameObject("RockSpawner");
            spawnerObj.transform.position = pos;
            var spawner = spawnerObj.AddComponent<RockSpawner>();

            SerializedObject so = new SerializedObject(spawner);
            so.FindProperty("rockPrefab").objectReferenceValue = rockTemplate;
            so.FindProperty("warningIndicatorPrefab").objectReferenceValue = warnTemplate;
            so.FindProperty("spawnInterval").floatValue = intervalo;
            so.FindProperty("warningDuration").floatValue = 0.6f;
            so.ApplyModifiedProperties();

            rockTemplate.transform.SetParent(spawnerObj.transform);
            warnTemplate.transform.SetParent(spawnerObj.transform);
            rockTemplate.SetActive(false);
            warnTemplate.SetActive(false);
        }

        private static void CrearCheckpoint(Vector3 pos)
        {
            GameObject cp = new GameObject("Checkpoint");
            cp.tag = Constantes.Tags.Checkpoint;
            cp.transform.position = pos;

            var col = cp.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);
            col.offset = new Vector2(0f, 1f);

            var sr = cp.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Checkpoint.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var luzCp = cp.AddComponent<Light2D>();
            luzCp.lightType = Light2D.LightType.Point;
            luzCp.pointLightInnerRadius = 0.5f;
            luzCp.pointLightOuterRadius = 3f;
            luzCp.intensity = 1.2f;
            luzCp.color = new Color(0.4f, 0.9f, 1f);
            luzCp.enabled = false;
            ElMineroBuilder.AsignarCapasALuz2D(luzCp);

            var scriptCp = cp.AddComponent<Checkpoint>();
            SerializedObject so = new SerializedObject(scriptCp);
            so.FindProperty("checkpointLight").objectReferenceValue = luzCp;
            so.ApplyModifiedProperties();
        }

        private static void CrearZonaDerrumbe(Vector3 triggerPos, Vector3 dropPos)
        {
            GameObject bloqueObj = new GameObject("BloqueDerrumbe");
            bloqueObj.tag = Constantes.Tags.Hazard;
            bloqueObj.transform.position = new Vector3(dropPos.x, dropPos.y + 12f, 0f);

            var colB = bloqueObj.AddComponent<BoxCollider2D>();
            colB.size = new Vector2(2f, 3f);
            colB.offset = new Vector2(0f, 1.5f);

            var srB = bloqueObj.AddComponent<SpriteRenderer>();
            srB.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png");
            srB.sortingLayerName = Constantes.SortingLayers.Escenario;

            var compBloque = bloqueObj.AddComponent<BloqueDerrumbe>();

            GameObject zonaObj = new GameObject("ZonaDerrumbe_Sorpresa");
            zonaObj.transform.position = triggerPos;

            var colZ = zonaObj.AddComponent<BoxCollider2D>();
            colZ.isTrigger = true;
            colZ.size = new Vector2(2.5f, 4f);

            var impSource = zonaObj.AddComponent<CinemachineImpulseSource>();
            var caveIn = zonaObj.AddComponent<CaveInZone>();

            SerializedObject soCi = new SerializedObject(caveIn);
            soCi.FindProperty("blockObject").objectReferenceValue = compBloque;
            soCi.FindProperty("blockDropPosition").vector3Value = dropPos;
            soCi.FindProperty("impulseSource").objectReferenceValue = impSource;
            soCi.ApplyModifiedProperties();
        }

        private static void CrearMineTremor()
        {
            GameObject tremorObj = new GameObject("[MineTremor]");
            tremorObj.AddComponent<CinemachineImpulseSource>();
            tremorObj.AddComponent<MineTremor>();
        }

        private static void CrearSalida(Vector3 pos, bool esSuperficie)
        {
            GameObject exit = new GameObject("Salida_Nivel");
            exit.transform.position = pos;

            var col = exit.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 3f);
            col.offset = new Vector2(0f, 1.5f);

            var sr = exit.AddComponent<SpriteRenderer>();
            string sprPath = esSuperficie ? "Assets/Sprites/Entorno/Salida_Superficie.png" : "Assets/Sprites/Entorno/Salida_Tunel.png";
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(sprPath);
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            exit.AddComponent<LevelExit>();
        }

        private static void CrearAntorchaConLuz(Vector3 pos, Color colorLuz)
        {
            GameObject antorcha = new GameObject("Antorcha");
            antorcha.transform.position = pos;

            var sr = antorcha.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Antorcha.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = antorcha.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Entorno/Antorcha.controller");

            var luz = antorcha.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightInnerRadius = 0.5f;
            luz.pointLightOuterRadius = 3.5f;
            luz.intensity = 1.3f;
            luz.color = colorLuz;
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            antorcha.AddComponent<FlickeringLight2D>();
        }

        private static void CrearMineralDistintivo(string nombrePrefab, Vector3 pos)
        {
            string cleanName = nombrePrefab.Replace("Mineral_", "");
            if (cleanName == "PepitaOro") cleanName = "Pepita_Oro";

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Coleccionables/{cleanName}.prefab");
            if (prefab == null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Prefabs/Coleccionables/{nombrePrefab}.prefab");
            }

            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
            }
            else
            {
                Debug.LogWarning($"[LevelBuilder] No se encontrÃ³ el prefab {nombrePrefab} ni {cleanName}");
            }
        }

        private static void CrearMurcielago(Vector3 pos, float rangoPatrulla = 4f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemigos/Murcielago.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
                var bat = inst.GetComponent<BatPatrol>();
                if (bat != null)
                {
                    SerializedObject so = new SerializedObject(bat);
                    var pDist = so.FindProperty("patrolDistance");
                    if (pDist != null) pDist.floatValue = rangoPatrulla;
                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void CrearMurcielagoRojo(Vector3 pos)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemigos/Murcielago_Rojo.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
            }
        }

        private static void CrearArana(Vector3 pos)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemigos/Arana.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
            }
        }

        private static void CrearAlacran(Vector3 pos, float rangoPatrulla = 4f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Enemigos/Alacran.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
                var scorp = inst.GetComponent<ScorpionAI>();
                if (scorp != null)
                {
                    SerializedObject so = new SerializedObject(scorp);
                    var pDist = so.FindProperty("patrolDistance");
                    if (pDist != null) pDist.floatValue = rangoPatrulla;
                    so.ApplyModifiedProperties();
                }
            }
        }

        private static void CrearVagoneta(Vector3 triggerPos, Vector3 cartPos, Vector2 direccion, float distancia = 20f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstaculos/Vagoneta_Desbocada.prefab");
            RunawayCart cartComp = null;
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = cartPos;
                cartComp = inst.GetComponent<RunawayCart>();
                if (cartComp != null)
                {
                    SerializedObject so = new SerializedObject(cartComp);
                    so.FindProperty("startPoint").vector3Value = cartPos;
                    so.FindProperty("endPoint").vector3Value = cartPos + (Vector3)(direccion * distancia);
                    so.ApplyModifiedProperties();
                }
            }

            GameObject trigObj = new GameObject("Trigger_Vagoneta");
            trigObj.transform.position = triggerPos;
            var col = trigObj.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 4f);
            var triggerComp = trigObj.AddComponent<RunawayCartTrigger>();
            if (cartComp != null)
            {
                triggerComp.SetTargetCart(cartComp);
            }
        }

        private static void CrearFuego(Vector3 pos)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Obstaculos/Fuego.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
            }
        }

        private static void CrearElevador(Vector3 posA, Vector3 posB, float velocidad = 2.2f)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Plataformas/Elevador_Malacate.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = posA;
                var elev = inst.GetComponent<MineElevator>();
                if (elev != null)
                {
                    Transform wheel = inst.transform.Find("RuedaMalacate");
                    elev.ConfigurarPuntos(posA, posB, wheel);
                }
            }
        }

        private static void CrearCheckpointCapilla(Vector3 pos)
        {
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Niveles/Checkpoint_Capilla.prefab");
            if (prefab != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                inst.transform.position = pos;
            }
            else
            {
                CrearCheckpoint(pos);
            }
        }

        private static GameObject CrearJugador(Vector3 spawnPos, int layerSuelo)
        {
            PhysicsMaterial2D matSinFriccion = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materiales/Fisica/SinFriccion.physicsMaterial2D");

            GameObject jugadorObj = new GameObject("Minero");
            jugadorObj.tag = Constantes.Tags.Player;
            int layerJugador = LayerMask.NameToLayer(Constantes.Layers.Jugador);
            jugadorObj.layer = layerJugador != -1 ? layerJugador : 0;
            jugadorObj.transform.position = spawnPos;

            var srJugador = jugadorObj.AddComponent<SpriteRenderer>();
            srJugador.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jugador/Minero_Idle.png");
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
            if (matSinFriccion != null) colJugador.sharedMaterial = matSinFriccion;

            var inputHandler = jugadorObj.AddComponent<PlayerInputHandler>();
            var playerCtrl = jugadorObj.AddComponent<PlayerController>();
            var health = jugadorObj.AddComponent<PlayerHealth>();

            // Animator con controlador Minero
            var anim = jugadorObj.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Jugador/Minero.controller");
            jugadorObj.AddComponent<PlayerAnimator>();

            SerializedObject soCtrl = new SerializedObject(playerCtrl);
            var propGroundLayer = soCtrl.FindProperty("groundLayer");
            if (propGroundLayer != null) propGroundLayer.intValue = 1 << layerSuelo;
            var propSr = soCtrl.FindProperty("spriteRenderer");
            if (propSr != null) propSr.objectReferenceValue = srJugador;
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
            labelObj.AddComponent<NameLabel>();

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
            ElMineroBuilder.AsignarCapasALuz2D(luzCasco);

            return jugadorObj;
        }

        private static void ConfigurarCamaraNivel(GameObject jugador, Color bgColor, Vector2 minBounds, Vector2 maxBounds)
        {
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(jugador.transform.position.x, jugador.transform.position.y + 1f, -10f);

            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 100f;
            cam.cullingMask = ~0;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = bgColor;
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            camObj.AddComponent<CinemachineBrain>();
            camObj.AddComponent<CinemachineImpulseListener>();
            camObj.AddComponent<CameraShake>();

            GameObject vcamObj = new GameObject("CinemachineCamera");
            vcamObj.transform.position = new Vector3(jugador.transform.position.x, jugador.transform.position.y + 1f, -10f);
            var vcam = vcamObj.AddComponent<CinemachineCamera>();
            vcam.Follow = jugador.transform;
            vcam.Lens.OrthographicSize = 5.5f;
            vcam.Lens.NearClipPlane = 0.1f;
            vcam.Lens.FarClipPlane = 100f;

            // CinemachineFollow garantiza la distancia Z = -10
            var follow = vcamObj.AddComponent<CinemachineFollow>();
            follow.FollowOffset = new Vector3(0f, 1f, -10f);
            follow.TrackerSettings.PositionDamping = new Vector3(0.3f, 0.3f, 0f);

            var camFollow = camObj.AddComponent<CameraFollowSetup>();
            camFollow.SetTarget(jugador.transform);
            camFollow.SetBounds(minBounds, maxBounds);
        }

        private static void ConfigurarManagersYHUD(GameObject jugador, LevelConfig config, int totalVetas, int totalGemas)
        {
            // Managers
            GameObject managersObj = new GameObject("[Managers]");
            var lm = managersObj.AddComponent<LevelManager>();
            var timer = managersObj.AddComponent<LevelTimer>();
            var cm = managersObj.AddComponent<CollectibleManager>();
            var streak = managersObj.AddComponent<StreakSystem>();

            SerializedObject soLm = new SerializedObject(lm);
            soLm.FindProperty("levelConfig").objectReferenceValue = config;
            soLm.FindProperty("timer").objectReferenceValue = timer;
            soLm.FindProperty("collectibleManager").objectReferenceValue = cm;
            soLm.FindProperty("streakSystem").objectReferenceValue = streak;
            soLm.FindProperty("playerHealth").objectReferenceValue = jugador.GetComponent<PlayerHealth>();
            soLm.FindProperty("playerController").objectReferenceValue = jugador.GetComponent<PlayerController>();
            soLm.ApplyModifiedProperties();

            // HUD Canvas
            GameObject canvasObj = CrearCanvasBase("Canvas_HUD");
            var hud = canvasObj.AddComponent<HUDController>();

            // Panel Superior
            GameObject panelSup = new GameObject("Panel_Superior");
            panelSup.transform.SetParent(canvasObj.transform, false);
            var rectSup = panelSup.AddComponent<RectTransform>();
            rectSup.anchorMin = new Vector2(0f, 1f);
            rectSup.anchorMax = new Vector2(1f, 1f);
            rectSup.pivot = new Vector2(0.5f, 1f);
            rectSup.sizeDelta = new Vector2(0f, 120f);

            // 3 Corazones (64x64, separación 12 px)
            Sprite sprCorazonLleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Corazon_Lleno.png");
            Sprite sprCorazonVacio = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Corazon_Vacio.png");
            Image[] hearts = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject hObj = new GameObject($"Corazon_{i + 1}");
                hObj.transform.SetParent(panelSup.transform, false);
                var rectH = hObj.AddComponent<RectTransform>();
                rectH.anchorMin = new Vector2(0f, 0.5f);
                rectH.anchorMax = new Vector2(0f, 0.5f);
                rectH.pivot = new Vector2(0f, 0.5f);
                rectH.sizeDelta = new Vector2(64f, 64f);
                rectH.anchoredPosition = new Vector2(40f + (i * 76f), 0f);
                hearts[i] = hObj.AddComponent<Image>();
                hearts[i].sprite = sprCorazonLleno;
            }

            // Reloj en el centro
            GameObject relojObj = new GameObject("Elemento_Reloj");
            relojObj.transform.SetParent(panelSup.transform, false);
            var rectReloj = relojObj.AddComponent<RectTransform>();
            rectReloj.anchorMin = new Vector2(0.5f, 0.5f);
            rectReloj.anchorMax = new Vector2(0.5f, 0.5f);
            rectReloj.sizeDelta = new Vector2(240f, 60f);

            GameObject iconoReloj = new GameObject("Icono_Reloj");
            iconoReloj.transform.SetParent(relojObj.transform, false);
            var rectIr = iconoReloj.AddComponent<RectTransform>();
            rectIr.anchorMin = new Vector2(0f, 0.5f);
            rectIr.anchorMax = new Vector2(0f, 0.5f);
            rectIr.pivot = new Vector2(0f, 0.5f);
            rectIr.sizeDelta = new Vector2(56f, 56f);
            rectIr.anchoredPosition = new Vector2(0f, 0f);
            var imgReloj = iconoReloj.AddComponent<Image>();
            imgReloj.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Reloj.png");

            GameObject textoReloj = new GameObject("Texto_Reloj");
            textoReloj.transform.SetParent(relojObj.transform, false);
            var rectTr = textoReloj.AddComponent<RectTransform>();
            rectTr.anchorMin = new Vector2(0f, 0f);
            rectTr.anchorMax = new Vector2(1f, 1f);
            rectTr.offsetMin = new Vector2(64f, 0f);
            rectTr.offsetMax = Vector2.zero;
            var tmpReloj = textoReloj.AddComponent<TextMeshProUGUI>();
            tmpReloj.text = "01:30";
            tmpReloj.fontSize = 42f;
            tmpReloj.alignment = TextAlignmentOptions.MidlineLeft;
            tmpReloj.fontStyle = FontStyles.Bold;

            // Plata y Puntaje a la derecha
            GameObject plataObj = new GameObject("Elemento_Plata");
            plataObj.transform.SetParent(panelSup.transform, false);
            var rectPlata = plataObj.AddComponent<RectTransform>();
            rectPlata.anchorMin = new Vector2(1f, 0.5f);
            rectPlata.anchorMax = new Vector2(1f, 0.5f);
            rectPlata.pivot = new Vector2(1f, 0.5f);
            rectPlata.sizeDelta = new Vector2(440f, 60f);
            rectPlata.anchoredPosition = new Vector2(-160f, 0f);

            GameObject iconoPlata = new GameObject("Icono_Plata");
            iconoPlata.transform.SetParent(plataObj.transform, false);
            var ipRect = iconoPlata.AddComponent<RectTransform>();
            ipRect.anchorMin = new Vector2(0f, 0.5f);
            ipRect.anchorMax = new Vector2(0f, 0.5f);
            ipRect.pivot = new Vector2(0f, 0.5f);
            ipRect.sizeDelta = new Vector2(56f, 56f);
            var imgPlata = iconoPlata.AddComponent<Image>();
            imgPlata.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Plata.png");

            GameObject textoPlata = new GameObject("Texto_Plata");
            textoPlata.transform.SetParent(plataObj.transform, false);
            var tpRect = textoPlata.AddComponent<RectTransform>();
            tpRect.anchorMin = new Vector2(0f, 0f);
            tpRect.anchorMax = new Vector2(0.5f, 1f);
            tpRect.offsetMin = new Vector2(64f, 0f);
            tpRect.offsetMax = Vector2.zero;
            var tmpPlata = textoPlata.AddComponent<TextMeshProUGUI>();
            tmpPlata.text = $"0 / {totalVetas}";
            tmpPlata.fontSize = 42f;
            tmpPlata.alignment = TextAlignmentOptions.MidlineLeft;

            GameObject textoScore = new GameObject("Texto_Score");
            textoScore.transform.SetParent(plataObj.transform, false);
            var tsRect = textoScore.AddComponent<RectTransform>();
            tsRect.anchorMin = new Vector2(0.5f, 0f);
            tsRect.anchorMax = new Vector2(1f, 1f);
            tsRect.offsetMin = Vector2.zero;
            tsRect.offsetMax = Vector2.zero;
            var tmpScore = textoScore.AddComponent<TextMeshProUGUI>();
            tmpScore.text = "0 pts";
            tmpScore.fontSize = 42f;
            tmpScore.fontStyle = FontStyles.Bold;
            tmpScore.alignment = TextAlignmentOptions.MidlineRight;

            // Botón Pausa (104x104, esquina sup derecha a -40, 0)
            GameObject btnPausaObj = new GameObject("Boton_Pausa");
            btnPausaObj.transform.SetParent(panelSup.transform, false);
            var bpRect = btnPausaObj.AddComponent<RectTransform>();
            bpRect.anchorMin = new Vector2(1f, 0.5f);
            bpRect.anchorMax = new Vector2(1f, 0.5f);
            bpRect.pivot = new Vector2(1f, 0.5f);
            bpRect.sizeDelta = new Vector2(104f, 104f);
            bpRect.anchoredPosition = new Vector2(-40f, 0f);
            var imgPausa = btnPausaObj.AddComponent<Image>();
            imgPausa.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Pausa.png");
            var btnPausa = btnPausaObj.AddComponent<Button>();

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
            imgBadge.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Insignia_x2.png");
            var animBadge = badgeObj.AddComponent<Animator>();
            animBadge.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/UI/InsigniaX2.controller");

            GameObject badgeTextObj = new GameObject("Texto_x2_Timer");
            badgeTextObj.transform.SetParent(badgeObj.transform, false);
            var btRect = badgeTextObj.AddComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;
            var tmpBadge = badgeTextObj.AddComponent<TextMeshProUGUI>();
            tmpBadge.text = "10.0s";
            tmpBadge.fontSize = 22f;
            tmpBadge.alignment = TextAlignmentOptions.Center;
            badgeObj.SetActive(false);

            // Asignar en HUDController
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

            // Instanciar Paneles Modales de UI
            GameObject pStartCard = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/LevelStartCard.prefab");
            GameObject pResults = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ResultsPanel.prefab");
            GameObject pGameOver = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/GameOverPanel.prefab");
            GameObject pOptions = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/OptionsPanel.prefab");

            LevelStartCard startCardComp = null;
            if (pStartCard != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pStartCard, canvasObj.transform);
                startCardComp = inst.GetComponent<LevelStartCard>();
                inst.SetActive(false);
            }

            ResultsPanel resultsComp = null;
            if (pResults != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pResults, canvasObj.transform);
                resultsComp = inst.GetComponent<ResultsPanel>();
                inst.SetActive(false);
            }

            GameOverPanel gameOverComp = null;
            if (pGameOver != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pGameOver, canvasObj.transform);
                gameOverComp = inst.GetComponent<GameOverPanel>();
                inst.SetActive(false);
            }

            OptionsPanel optionsComp = null;
            if (pOptions != null)
            {
                var inst = (GameObject)PrefabUtility.InstantiatePrefab(pOptions, canvasObj.transform);
                optionsComp = inst.GetComponent<OptionsPanel>();
                inst.SetActive(false);
            }

            SerializedObject soLm2 = new SerializedObject(lm);
            soLm2.FindProperty("hudController").objectReferenceValue = hud;
            soLm2.FindProperty("resultsPanel").objectReferenceValue = resultsComp;
            soLm2.FindProperty("gameOverPanel").objectReferenceValue = gameOverComp;
            soLm2.FindProperty("levelStartCard").objectReferenceValue = startCardComp;
            soLm2.FindProperty("initialSpawnPoint").vector3Value = jugador.transform.position;
            soLm2.ApplyModifiedProperties();

            // MenÃº de Pausa
            CrearMenuPausaEnCanvas(canvasObj, btnPausa, optionsComp);

            // Controles TÃ¡ctiles para MÃ³vil
            CrearControlesTactiles(canvasObj, jugador.GetComponent<PlayerInputHandler>());

            // Sistemas Persistentes
            CrearSistemasPersistentes();
        }

        private static void CrearMenuPausaEnCanvas(GameObject canvasObj, Button btnPausaIcono, OptionsPanel optionsPanelComp)
        {
            var pauseComp = canvasObj.AddComponent<PauseMenu>();

            GameObject panelPausa = new GameObject("Panel_Pausa");
            panelPausa.transform.SetParent(canvasObj.transform, false);
            var rectP = panelPausa.AddComponent<RectTransform>();
            rectP.sizeDelta = new Vector2(800f, 650f);
            var imgP = panelPausa.AddComponent<Image>();
            imgP.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgP.type = Image.Type.Sliced;
            imgP.pixelsPerUnitMultiplier = 0.25f;

            // Título Pausa
            GameObject tObj = new GameObject("Texto_Pausa");
            tObj.transform.SetParent(panelPausa.transform, false);
            var rectT = tObj.AddComponent<RectTransform>();
            rectT.anchoredPosition = new Vector2(0f, 220f);
            rectT.sizeDelta = new Vector2(700f, 65f);
            var tmpT = tObj.AddComponent<TextMeshProUGUI>();
            tmpT.text = "PAUSA";
            tmpT.fontSize = 56f;
            tmpT.alignment = TextAlignmentOptions.Center;
            tmpT.fontStyle = FontStyles.Bold;
            tmpT.color = new Color(1f, 0.9f, 0.4f);

            Button btnResume = CrearBotonMadera(panelPausa.transform, "Boton_Continuar", "Continuar", new Vector2(0f, 100f), new Vector2(420f, 80f));
            Button btnRestart = CrearBotonMadera(panelPausa.transform, "Boton_Reiniciar", "Reiniciar Nivel", new Vector2(0f, 0f), new Vector2(420f, 80f));
            Button btnOptions = CrearBotonMadera(panelPausa.transform, "Boton_Opciones", "Opciones", new Vector2(0f, -100f), new Vector2(420f, 80f));
            Button btnMenu = CrearBotonMadera(panelPausa.transform, "Boton_MenuPrincipal", "Menú Principal", new Vector2(0f, -200f), new Vector2(420f, 80f));

            SerializedObject so = new SerializedObject(pauseComp);
            so.FindProperty("pausePanel").objectReferenceValue = panelPausa;
            so.FindProperty("resumeButton").objectReferenceValue = btnResume;
            so.FindProperty("restartButton").objectReferenceValue = btnRestart;
            so.FindProperty("optionsButton").objectReferenceValue = btnOptions;
            so.FindProperty("mainMenuButton").objectReferenceValue = btnMenu;
            so.FindProperty("pauseIconButton").objectReferenceValue = btnPausaIcono;
            so.FindProperty("optionsPanel").objectReferenceValue = optionsPanelComp;
            so.ApplyModifiedProperties();

            panelPausa.SetActive(false);
        }

        private static void CrearControlesTactiles(GameObject canvasObj, PlayerInputHandler inputHandler)
        {
            GameObject touchRoot = new GameObject("ControlesTactiles");
            touchRoot.transform.SetParent(canvasObj.transform, false);
            var rectTr = touchRoot.AddComponent<RectTransform>();
            rectTr.anchorMin = Vector2.zero;
            rectTr.anchorMax = Vector2.one;
            rectTr.offsetMin = Vector2.zero;
            rectTr.offsetMax = Vector2.zero;

            var touch = touchRoot.AddComponent<TouchControls>();
            touch.SetInputHandler(inputHandler);

            // BotÃ³n Izquierda (esquina inferior izquierda)
            GameObject btnLeft = CrearBotonTactil(touchRoot.transform, "Boton_Izquierda", "Assets/Sprites/UI/Boton_Izquierda.png",
                new Vector2(140f, 140f), new Vector2(180f, 180f), new Vector2(0f, 0f));

            // BotÃ³n Derecha (al lado del botÃ³n izquierda)
            GameObject btnRight = CrearBotonTactil(touchRoot.transform, "Boton_Derecha", "Assets/Sprites/UI/Boton_Derecha.png",
                new Vector2(340f, 140f), new Vector2(180f, 180f), new Vector2(0f, 0f));

            // BotÃ³n Saltar (esquina inferior derecha)
            GameObject btnJump = CrearBotonTactil(touchRoot.transform, "Boton_Saltar", "Assets/Sprites/UI/Boton_Saltar.png",
                new Vector2(-160f, 160f), new Vector2(200f, 200f), new Vector2(1f, 0f));

            // EventTriggers para PointerDown / PointerUp
            AgregarEventTrigger(btnLeft, () => touch.OnLeftDown(), () => touch.OnLeftUp());
            AgregarEventTrigger(btnRight, () => touch.OnRightDown(), () => touch.OnRightUp());
            AgregarEventTrigger(btnJump, () => touch.OnJumpDown(), () => touch.OnJumpUp());

            touchRoot.SetActive(false); // TouchControls lo activarÃ¡ automÃ¡ticamente en mÃ³vil
        }

        private static GameObject CrearBotonTactil(Transform parent, string name, string spritePath, Vector2 pos, Vector2 size, Vector2 anchor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            img.color = new Color(1f, 1f, 1f, 0.85f);
            btnObj.AddComponent<Button>();

            return btnObj;
        }

        private static void AgregarEventTrigger(GameObject obj, System.Action onDown, System.Action onUp)
        {
            var trigger = obj.AddComponent<EventTrigger>();

            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener((data) => onDown());
            trigger.triggers.Add(entryDown);

            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener((data) => onUp());
            trigger.triggers.Add(entryUp);
        }

        private static GameObject CrearCanvasBase(string name)
        {
            GameObject canvasObj = new GameObject(name);
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObj.AddComponent<GraphicRaycaster>();

            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<InputSystemUIInputModule>();
            }

            return canvasObj;
        }

        private static Button CrearBotonMadera(Transform parent, string name, string texto, Vector2 pos, Vector2 size, Vector2 anchorMin = default, Vector2 anchorMax = default)
        {
            if (anchorMin == default) anchorMin = new Vector2(0.5f, 0.5f);
            if (anchorMax == default) anchorMax = new Vector2(0.5f, 0.5f);

            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            var rect = btnObj.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;

            var img = btnObj.AddComponent<Image>();
            img.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera.png");
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 0.25f;

            var btn = btnObj.AddComponent<Button>();

            // Texto del botón
            GameObject txtObj = new GameObject("Texto");
            txtObj.transform.SetParent(btnObj.transform, false);
            var rectT = txtObj.AddComponent<RectTransform>();
            rectT.anchorMin = Vector2.zero;
            rectT.anchorMax = Vector2.one;
            rectT.offsetMin = Vector2.zero;
            rectT.offsetMax = Vector2.zero;
            var tmp = txtObj.AddComponent<TextMeshProUGUI>();
            tmp.text = texto;
            tmp.fontSize = 34f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.9f, 0.8f);

            return btn;
        }

        private static void CrearSistemasPersistentes()
        {
            // GameManager
            if (UnityEngine.Object.FindAnyObjectByType<GameManager>() == null)
            {
                GameObject gmObj = new GameObject("[GameManager]");
                gmObj.AddComponent<GameManager>();
            }

            // AudioManager
            if (UnityEngine.Object.FindAnyObjectByType<AudioManager>() == null)
            {
                GameObject amObj = new GameObject("[AudioManager]");
                amObj.AddComponent<AudioManager>();
            }

            // SceneLoader
            if (UnityEngine.Object.FindAnyObjectByType<SceneLoader>() == null)
            {
                GameObject slObj = new GameObject("[SceneLoader]");
                slObj.AddComponent<SceneLoader>();
            }
        }
    }
}
