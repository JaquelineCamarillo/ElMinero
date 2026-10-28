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
                EditorUtility.DisplayDialog("El Minero", "Por favor sal del Modo Play (detén el juego) antes de construir las escenas.", "Entendido");
                return;
            }

            EditorUtility.DisplayProgressBar("El Minero", "Construyendo escenas del juego...", 0.1f);

            try
            {
                // Asegurar que las dependencias previas estén configuradas
                ElMineroBuilder.ConfigurarTagsYLayers();
                ElMineroBuilder.CrearMaterialesFisicosYConfigs();

                // 1. Escenas de Interfaz
                ConstruirMenuPrincipal();
                ConstruirVictoria();
                ConstruirDerrota();

                // 2. Tres Niveles de Ascenso
                ConstruirNivel1_MinaRayas();
                ConstruirNivel2_ElNopal();
                ConstruirNivel3_LaValenciana();

                // 3. Registrar las 6 escenas en Build Settings automáticamente
                ElMineroBuilder.ConfigurarBuildSettingsYPlayer();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                Debug.Log("[El Minero] ¡Las 6 escenas construidas y guardadas exitosamente en Assets/Scenes/!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ====================================================================
        // 1. MENÚ PRINCIPAL
        // ====================================================================
        public static void ConstruirMenuPrincipal()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
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

            // Fondo visual del menú
            GameObject fondo = new GameObject("Fondo_Menu");
            var srFondo = fondo.AddComponent<SpriteRenderer>();
            srFondo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/Fondo_Menu.png");
            srFondo.sortingLayerName = Constantes.SortingLayers.Fondo;
            fondo.transform.position = new Vector3(0f, 0f, 0f);
            fondo.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

            // Luz tenue
            GameObject luz = new GameObject("LuzGlobal");
            var l2d = luz.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.intensity = 1.0f;
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            // Canvas UI
            GameObject canvasObj = CrearCanvasBase("Canvas_MenuPrincipal");
            var menuCtrl = canvasObj.AddComponent<MainMenuController>();

            // Logo
            GameObject logoObj = new GameObject("Logo_ElMinero");
            logoObj.transform.SetParent(canvasObj.transform, false);
            var rectLogo = logoObj.AddComponent<RectTransform>();
            rectLogo.anchorMin = new Vector2(0.5f, 0.75f);
            rectLogo.anchorMax = new Vector2(0.5f, 0.75f);
            rectLogo.sizeDelta = new Vector2(480f, 160f);
            var imgLogo = logoObj.AddComponent<Image>();
            imgLogo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero.png");
            imgLogo.preserveAspect = true;

            // Subtítulo
            GameObject subObj = new GameObject("Subtitulo");
            subObj.transform.SetParent(canvasObj.transform, false);
            var rectSub = subObj.AddComponent<RectTransform>();
            rectSub.anchorMin = new Vector2(0.5f, 0.62f);
            rectSub.anchorMax = new Vector2(0.5f, 0.62f);
            rectSub.sizeDelta = new Vector2(500f, 40f);
            var tmpSub = subObj.AddComponent<TextMeshProUGUI>();
            tmpSub.text = "Minas de Guanajuato, Siglo XIX";
            tmpSub.fontSize = 24f;
            tmpSub.alignment = TextAlignmentOptions.Center;
            tmpSub.color = new Color(0.9f, 0.8f, 0.6f);

            // Panel de Entrada de Nombre y Botones
            GameObject panelMadera = new GameObject("Panel_Central");
            panelMadera.transform.SetParent(canvasObj.transform, false);
            var rectPanel = panelMadera.AddComponent<RectTransform>();
            rectPanel.anchorMin = new Vector2(0.5f, 0.35f);
            rectPanel.anchorMax = new Vector2(0.5f, 0.35f);
            rectPanel.sizeDelta = new Vector2(450f, 260f);
            var imgPanel = panelMadera.AddComponent<Image>();
            imgPanel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgPanel.type = Image.Type.Sliced;

            // Etiqueta Nombre
            GameObject labelNombre = new GameObject("Label_Nombre");
            labelNombre.transform.SetParent(panelMadera.transform, false);
            var rectLn = labelNombre.AddComponent<RectTransform>();
            rectLn.anchoredPosition = new Vector2(0f, 85f);
            rectLn.sizeDelta = new Vector2(400f, 30f);
            var tmpLn = labelNombre.AddComponent<TextMeshProUGUI>();
            tmpLn.text = "Nombre del Minero:";
            tmpLn.fontSize = 20f;
            tmpLn.alignment = TextAlignmentOptions.Center;
            tmpLn.color = Color.white;

            // InputField
            GameObject inputObj = new GameObject("InputField_Nombre");
            inputObj.transform.SetParent(panelMadera.transform, false);
            var rectInput = inputObj.AddComponent<RectTransform>();
            rectInput.anchoredPosition = new Vector2(0f, 45f);
            rectInput.sizeDelta = new Vector2(320f, 45f);
            var imgInput = inputObj.AddComponent<Image>();
            imgInput.color = new Color(0.15f, 0.1f, 0.08f, 0.9f);

            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(inputObj.transform, false);
            var rectTxt = textObj.AddComponent<RectTransform>();
            rectTxt.anchorMin = Vector2.zero;
            rectTxt.anchorMax = Vector2.one;
            rectTxt.offsetMin = new Vector2(10f, 0f);
            rectTxt.offsetMax = new Vector2(-10f, 0f);
            var tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.fontSize = 22f;
            tmpText.alignment = TextAlignmentOptions.Midline;
            tmpText.color = Color.yellow;

            var inputField = inputObj.AddComponent<TMP_InputField>();
            inputField.textComponent = tmpText;
            inputField.characterLimit = 12;

            // Botón Jugar
            Button btnPlay = CrearBotonMadera(panelMadera.transform, "Boton_Jugar", "¡Iniciar Escape!", new Vector2(0f, -15f), new Vector2(280f, 50f));

            // Botón Cómo Jugar
            Button btnHow = CrearBotonMadera(panelMadera.transform, "Boton_ComoJugar", "Cómo Jugar", new Vector2(0f, -70f), new Vector2(280f, 45f));

            // Botón Salir
            Button btnQuit = CrearBotonMadera(canvasObj.transform, "Boton_Salir", "Salir", new Vector2(0f, 40f), new Vector2(160f, 40f), new Vector2(0.5f, 0f), new Vector2(0.5f, 0f));

            // Panel Modal Cómo Jugar
            GameObject howPanel = new GameObject("Panel_ComoJugar_Modal");
            howPanel.transform.SetParent(canvasObj.transform, false);
            var rectHp = howPanel.AddComponent<RectTransform>();
            rectHp.sizeDelta = new Vector2(600f, 400f);
            var imgHp = howPanel.AddComponent<Image>();
            imgHp.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgHp.type = Image.Type.Sliced;

            GameObject hpTextObj = new GameObject("Texto_Instrucciones");
            hpTextObj.transform.SetParent(howPanel.transform, false);
            var rectHpt = hpTextObj.AddComponent<RectTransform>();
            rectHpt.anchorMin = Vector2.zero;
            rectHpt.anchorMax = Vector2.one;
            rectHpt.offsetMin = new Vector2(30f, 70f);
            rectHpt.offsetMax = new Vector2(-30f, -30f);
            var tmpHpt = hpTextObj.AddComponent<TextMeshProUGUI>();
            tmpHpt.fontSize = 20f;
            tmpHpt.text = "<b>OBJETIVO:</b>\n" +
                          "¡La mina se está derrumbando! Escapa subiendo nivel por nivel hasta la superficie.\n\n" +
                          "<b>CONTROLES DE TECLADO:</b>\n" +
                          "• <b>A / D</b> o <b>Flechas</b> : Moverse a los lados\n" +
                          "• <b>Espacio</b> : Saltar (mantén para mayor altura)\n" +
                          "• <b>Escape</b> : Menú de Pausa\n\n" +
                          "<b>EN DISPOSITIVOS MÓVILES:</b>\n" +
                          "Usa los botones táctiles en pantalla.\n\n" +
                          "★ <i>¡Recolecta 3 minerales sin recibir daño para activar Racha x2!</i>";
            tmpHpt.color = Color.white;

            Button btnCloseHow = CrearBotonMadera(howPanel.transform, "Boton_Cerrar", "Entendido", new Vector2(0f, -150f), new Vector2(200f, 45f));

            // Asignar referencias en MainMenuController
            SerializedObject soMenu = new SerializedObject(menuCtrl);
            soMenu.FindProperty("nameInputField").objectReferenceValue = inputField;
            soMenu.FindProperty("playButton").objectReferenceValue = btnPlay;
            soMenu.FindProperty("howToPlayButton").objectReferenceValue = btnHow;
            soMenu.FindProperty("quitButton").objectReferenceValue = btnQuit;
            soMenu.FindProperty("howToPlayPanel").objectReferenceValue = howPanel;
            soMenu.FindProperty("closeHowToPlayButton").objectReferenceValue = btnCloseHow;
            soMenu.ApplyModifiedProperties();

            howPanel.SetActive(false);

            // Managers persistentes
            CrearSistemasPersistentes();

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MenuPrincipal.unity");
        }

        // ====================================================================
        // 2. PANTALLA DE VICTORIA
        // ====================================================================
        public static void ConstruirVictoria()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.2f, 0.45f, 0.7f); // Cielo azul exterior
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            // Fondo exterior
            GameObject fondo = new GameObject("Fondo_Victoria");
            var srFondo = fondo.AddComponent<SpriteRenderer>();
            srFondo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Fondos/LaValenciana/Fondo_LaValenciana_Capa0_Lejana.png");
            srFondo.sortingLayerName = Constantes.SortingLayers.Fondo;

            GameObject luz = new GameObject("LuzGlobal");
            var l2d = luz.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.intensity = 1.2f;
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            GameObject canvasObj = CrearCanvasBase("Canvas_Victoria");
            var vicComp = canvasObj.AddComponent<VictoryScreen>();

            // Panel de madera
            GameObject panel = new GameObject("Panel_Victoria");
            panel.transform.SetParent(canvasObj.transform, false);
            var rectP = panel.AddComponent<RectTransform>();
            rectP.sizeDelta = new Vector2(650f, 450f);
            var imgP = panel.AddComponent<Image>();
            imgP.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgP.type = Image.Type.Sliced;

            // Título
            GameObject titObj = new GameObject("Texto_Titulo");
            titObj.transform.SetParent(panel.transform, false);
            var rectT = titObj.AddComponent<RectTransform>();
            rectT.anchoredPosition = new Vector2(0f, 150f);
            rectT.sizeDelta = new Vector2(600f, 60f);
            var tmpT = titObj.AddComponent<TextMeshProUGUI>();
            tmpT.text = "¡Lo lograste, Refugio!";
            tmpT.fontSize = 36f;
            tmpT.alignment = TextAlignmentOptions.Center;
            tmpT.fontStyle = FontStyles.Bold;
            tmpT.color = new Color(1f, 0.9f, 0.4f);

            // Subtítulo
            GameObject subObj = new GameObject("Texto_Subtitulo");
            subObj.transform.SetParent(panel.transform, false);
            var rectS = subObj.AddComponent<RectTransform>();
            rectS.anchoredPosition = new Vector2(0f, 100f);
            rectS.sizeDelta = new Vector2(600f, 40f);
            var tmpS = subObj.AddComponent<TextMeshProUGUI>();
            tmpS.text = "Has escapado del colapso de las minas de Guanajuato.";
            tmpS.fontSize = 20f;
            tmpS.alignment = TextAlignmentOptions.Center;
            tmpS.color = Color.white;

            // Puntaje
            GameObject scoreObj = new GameObject("Texto_Score");
            scoreObj.transform.SetParent(panel.transform, false);
            var rectSc = scoreObj.AddComponent<RectTransform>();
            rectSc.anchoredPosition = new Vector2(0f, 40f);
            rectSc.sizeDelta = new Vector2(500f, 45f);
            var tmpSc = scoreObj.AddComponent<TextMeshProUGUI>();
            tmpSc.text = "Puntaje Final: 0 pts";
            tmpSc.fontSize = 28f;
            tmpSc.alignment = TextAlignmentOptions.Center;
            tmpSc.color = Color.yellow;

            // Récord
            GameObject highObj = new GameObject("Texto_HighScore");
            highObj.transform.SetParent(panel.transform, false);
            var rectHi = highObj.AddComponent<RectTransform>();
            rectHi.anchoredPosition = new Vector2(0f, -5f);
            rectHi.sizeDelta = new Vector2(500f, 35f);
            var tmpHi = highObj.AddComponent<TextMeshProUGUI>();
            tmpHi.text = "Mejor Récord: 0 pts";
            tmpHi.fontSize = 22f;
            tmpHi.alignment = TextAlignmentOptions.Center;
            tmpHi.color = new Color(0.8f, 0.8f, 0.8f);

            // Plata
            GameObject silverObj = new GameObject("Texto_Plata");
            silverObj.transform.SetParent(panel.transform, false);
            var rectSi = silverObj.AddComponent<RectTransform>();
            rectSi.anchoredPosition = new Vector2(0f, -45f);
            rectSi.sizeDelta = new Vector2(500f, 35f);
            var tmpSi = silverObj.AddComponent<TextMeshProUGUI>();
            tmpSi.text = "Plata Total Recolectada: 0";
            tmpSi.fontSize = 22f;
            tmpSi.alignment = TextAlignmentOptions.Center;
            tmpSi.color = new Color(0.9f, 0.9f, 1f);

            // Botones
            Button btnAgain = CrearBotonMadera(panel.transform, "Boton_JugarDeNuevo", "Jugar de Nuevo", new Vector2(-130f, -140f), new Vector2(220f, 50f));
            Button btnMenu = CrearBotonMadera(panel.transform, "Boton_MenuPrincipal", "Menú Principal", new Vector2(130f, -140f), new Vector2(220f, 50f));

            SerializedObject soVic = new SerializedObject(vicComp);
            soVic.FindProperty("titleText").objectReferenceValue = tmpT;
            soVic.FindProperty("scoreText").objectReferenceValue = tmpSc;
            soVic.FindProperty("highScoreText").objectReferenceValue = tmpHi;
            soVic.FindProperty("silverText").objectReferenceValue = tmpSi;
            soVic.FindProperty("playAgainButton").objectReferenceValue = btnAgain;
            soVic.FindProperty("mainMenuButton").objectReferenceValue = btnMenu;
            soVic.ApplyModifiedProperties();

            CrearSistemasPersistentes();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Victoria.unity");
        }

        // ====================================================================
        // 3. PANTALLA DE DERROTA
        // ====================================================================
        public static void ConstruirDerrota()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Cámara
            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.05f, 0.05f); // Rojo oscuro tétrico
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            GameObject luz = new GameObject("LuzGlobal");
            var l2d = luz.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Global;
            l2d.intensity = 0.5f;
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            GameObject canvasObj = CrearCanvasBase("Canvas_Derrota");
            var defComp = canvasObj.AddComponent<DefeatScreen>();

            // Panel de madera
            GameObject panel = new GameObject("Panel_Derrota");
            panel.transform.SetParent(canvasObj.transform, false);
            var rectP = panel.AddComponent<RectTransform>();
            rectP.sizeDelta = new Vector2(650f, 400f);
            var imgP = panel.AddComponent<Image>();
            imgP.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgP.type = Image.Type.Sliced;
            imgP.color = new Color(0.85f, 0.7f, 0.7f);

            // Título
            GameObject titObj = new GameObject("Texto_Titulo");
            titObj.transform.SetParent(panel.transform, false);
            var rectT = titObj.AddComponent<RectTransform>();
            rectT.anchoredPosition = new Vector2(0f, 120f);
            rectT.sizeDelta = new Vector2(600f, 60f);
            var tmpT = titObj.AddComponent<TextMeshProUGUI>();
            tmpT.text = "El derrumbe alcanzó a Refugio...";
            tmpT.fontSize = 32f;
            tmpT.alignment = TextAlignmentOptions.Center;
            tmpT.fontStyle = FontStyles.Bold;
            tmpT.color = new Color(1f, 0.35f, 0.35f);

            // Motivo
            GameObject reasonObj = new GameObject("Texto_Motivo");
            reasonObj.transform.SetParent(panel.transform, false);
            var rectR = reasonObj.AddComponent<RectTransform>();
            rectR.anchoredPosition = new Vector2(0f, 40f);
            rectR.sizeDelta = new Vector2(550f, 60f);
            var tmpR = reasonObj.AddComponent<TextMeshProUGUI>();
            tmpR.text = "Te has quedado sin vidas bajo la mina.";
            tmpR.fontSize = 22f;
            tmpR.alignment = TextAlignmentOptions.Center;
            tmpR.color = Color.white;

            // Botones
            Button btnRetry = CrearBotonMadera(panel.transform, "Boton_Reintentar", "Reintentar Nivel", new Vector2(-130f, -90f), new Vector2(220f, 50f));
            Button btnMenu = CrearBotonMadera(panel.transform, "Boton_MenuPrincipal", "Menú Principal", new Vector2(130f, -90f), new Vector2(220f, 50f));

            SerializedObject soDef = new SerializedObject(defComp);
            soDef.FindProperty("titleText").objectReferenceValue = tmpT;
            soDef.FindProperty("reasonText").objectReferenceValue = tmpR;
            soDef.FindProperty("retryButton").objectReferenceValue = btnRetry;
            soDef.FindProperty("mainMenuButton").objectReferenceValue = btnMenu;
            soDef.ApplyModifiedProperties();

            CrearSistemasPersistentes();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Derrota.unity");
        }

        // ====================================================================
        // 4. NIVEL 1 — MINA RAYAS (130u, 110s, 6 vetas, 2 gemas, 4 huecos, 7 spawners, 2 derrumbes)
        // ====================================================================
        public static void ConstruirNivel1_MinaRayas()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            Color bgVioleta = new Color(0.12f, 0.09f, 0.19f); // #1E1630
            Color luzAmbar = new Color(0.96f, 0.83f, 0.54f);

            // 1. Iluminación URP 2D
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

            // 5. Plataformas fijas con escalones alcanzables (ΔY <= 1.3u)
            CrearPlataformaFija(new Vector3(5f, 0.3f, 0f), layerSuelo);     // Escalón bajo
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

            // 7. Obstáculos: 7 Spawners de Rocas rápidos (intervalo 2.0s - 2.2s)
            CrearRockSpawner(new Vector3(12f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(28f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(48f, 7f, 0f), 2.0f);
            CrearRockSpawner(new Vector3(64f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(80f, 7f, 0f), 2.0f);
            CrearRockSpawner(new Vector3(95f, 7f, 0f), 2.2f);
            CrearRockSpawner(new Vector3(112f, 7f, 0f), 1.8f);

            // 8. Checkpoints funcionales (3 checkpoints)
            CrearCheckpoint(new Vector3(30f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(73f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(102f, -0.5f, 0f));

            // 9. Zonas de derrumbe sorpresa (2 derrumbes que colapsan tras pasar)
            CrearZonaDerrumbe(new Vector3(46f, 0f, 0f), new Vector3(41f, -0.5f, 0f));
            CrearZonaDerrumbe(new Vector3(92f, 0f, 0f), new Vector3(87f, -0.5f, 0f));

            // 10. Temblor periódico
            CrearMineTremor();

            // 11. Salida del nivel (Túnel al final X = 124)
            CrearSalida(new Vector3(124f, -1f, 0f), false);

            // 12. Ambientación (Antorchas)
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

            // 14. Cámara Cinemachine y Brain
            ConfigurarCamaraNivel(jugador, bgVioleta, new Vector2(-15f, -6f), new Vector2(135f, 12f));

            // 15. Managers y HUD
            var configN1 = AssetDatabase.LoadAssetAtPath<LevelConfig>("Assets/Settings/Niveles/LevelConfig_Nivel1.asset");
            ConfigurarManagersYHUD(jugador, configN1, 6, 2);

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Nivel1_MinaRayas.unity");
        }

        // ====================================================================
        // 5. NIVEL 2 — EL NOPAL (170u, 120s, 8 vetas, 3 gemas, 4 plat móviles, 6 huecos, 10 spawners, 3 derrumbes)
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

            // Plataformas Móviles sobre los huecos
            CrearPlataformaMovil(new Vector3(11f, 0.2f, 0f), new Vector3(17f, 0.2f, 0f), 2.2f, layerSuelo);
            CrearPlataformaMovil(new Vector3(37f, 0.2f, 0f), new Vector3(43f, 0.2f, 0f), 2.0f, layerSuelo);
            CrearPlataformaMovil(new Vector3(44f, 0.5f, 0f), new Vector3(49f, 0.5f, 0f), 2.2f, layerSuelo);
            CrearPlataformaMovil(new Vector3(94f, 0.3f, 0f), new Vector3(101f, 0.3f, 0f), 2.5f, layerSuelo);
            CrearPlataformaMovil(new Vector3(122f, 0.4f, 0f), new Vector3(129f, 0.4f, 0f), 3.0f, layerSuelo);

            // Plataformas fijas con escalones alcanzables (ΔY <= 1.3u)
            CrearPlataformaFija(new Vector3(24f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(58f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(62f, 1.4f, 0f), layerSuelo); // Escalón alto accesible
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

            // 10 Rock spawners rápidos (intervalo 1.8s)
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

            // 4 Checkpoints
            CrearCheckpoint(new Vector3(28f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(66f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(108f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(142f, -0.5f, 0f));

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
        // 6. NIVEL 3 — LA VALENCIANA (210u, 130s, 10 vetas, 4 gemas, 9 quebradizas, 2 móviles, 9 huecos, 14 spawners, 4 derrumbes)
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

            // Plataformas Móviles
            CrearPlataformaMovil(new Vector3(83f, 0.4f, 0f), new Vector3(88f, 0.4f, 0f), 2.5f, layerSuelo);
            CrearPlataformaMovil(new Vector3(166f, 0.3f, 0f), new Vector3(173f, 0.3f, 0f), 2.8f, layerSuelo);

            // Plataformas fijas con escalones alcanzables (ΔY <= 1.3u)
            CrearPlataformaFija(new Vector3(18f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(35f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(47f, 0.5f, 0f), layerSuelo);  // Sobre hueco 3
            CrearPlataformaFija(new Vector3(64f, 0.4f, 0f), layerSuelo);
            CrearPlataformaFija(new Vector3(68f, 1.4f, 0f), layerSuelo);  // Escalón alto
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

            // 5 Checkpoints
            CrearCheckpoint(new Vector3(32f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(72f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(114f, -0.5f, 0f));
            CrearCheckpoint(new Vector3(158f, -0.5f, 0f));
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
        // CONSTRUCCIÓN PROCEDURAL DE TERRENOS POR NIVEL
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

                // Estalactitas periódicas
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
        // HELPERS DE INSTANCIACIÓN DE ELEMENTOS
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
                    Debug.LogWarning($"[LevelBuilder] CompositeCollider2D en '{tm.name}' tiene 0 caminos. Usando colisión directa en TilemapCollider2D.");
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
            rectSup.sizeDelta = new Vector2(0f, 90f);

            // 3 Corazones
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
                rectH.sizeDelta = new Vector2(48f, 48f);
                rectH.anchoredPosition = new Vector2(40f + (i * 56f), 0f);
                hearts[i] = hObj.AddComponent<Image>();
                hearts[i].sprite = sprCorazonLleno;
            }

            // Reloj en el centro
            GameObject relojObj = new GameObject("Elemento_Reloj");
            relojObj.transform.SetParent(panelSup.transform, false);
            var rectReloj = relojObj.AddComponent<RectTransform>();
            rectReloj.anchorMin = new Vector2(0.5f, 0.5f);
            rectReloj.anchorMax = new Vector2(0.5f, 0.5f);
            rectReloj.sizeDelta = new Vector2(180f, 50f);

            GameObject iconoReloj = new GameObject("Icono_Reloj");
            iconoReloj.transform.SetParent(relojObj.transform, false);
            var rectIr = iconoReloj.AddComponent<RectTransform>();
            rectIr.anchorMin = new Vector2(0f, 0.5f);
            rectIr.anchorMax = new Vector2(0f, 0.5f);
            rectIr.pivot = new Vector2(0f, 0.5f);
            rectIr.sizeDelta = new Vector2(40f, 40f);
            rectIr.anchoredPosition = new Vector2(0f, 0f);
            var imgReloj = iconoReloj.AddComponent<Image>();
            imgReloj.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Reloj.png");

            GameObject textoReloj = new GameObject("Texto_Reloj");
            textoReloj.transform.SetParent(relojObj.transform, false);
            var rectTr = textoReloj.AddComponent<RectTransform>();
            rectTr.anchorMin = new Vector2(0f, 0f);
            rectTr.anchorMax = new Vector2(1f, 1f);
            rectTr.offsetMin = new Vector2(48f, 0f);
            rectTr.offsetMax = Vector2.zero;
            var tmpReloj = textoReloj.AddComponent<TextMeshProUGUI>();
            tmpReloj.text = "01:30";
            tmpReloj.fontSize = 32f;
            tmpReloj.alignment = TextAlignmentOptions.MidlineLeft;
            tmpReloj.fontStyle = FontStyles.Bold;

            // Plata y Puntaje a la derecha
            GameObject plataObj = new GameObject("Elemento_Plata");
            plataObj.transform.SetParent(panelSup.transform, false);
            var rectPlata = plataObj.AddComponent<RectTransform>();
            rectPlata.anchorMin = new Vector2(1f, 0.5f);
            rectPlata.anchorMax = new Vector2(1f, 0.5f);
            rectPlata.pivot = new Vector2(1f, 0.5f);
            rectPlata.sizeDelta = new Vector2(300f, 50f);
            rectPlata.anchoredPosition = new Vector2(-120f, 0f);

            GameObject iconoPlata = new GameObject("Icono_Plata");
            iconoPlata.transform.SetParent(plataObj.transform, false);
            var ipRect = iconoPlata.AddComponent<RectTransform>();
            ipRect.anchorMin = new Vector2(0f, 0.5f);
            ipRect.anchorMax = new Vector2(0f, 0.5f);
            ipRect.pivot = new Vector2(0f, 0.5f);
            ipRect.sizeDelta = new Vector2(40f, 40f);
            var imgPlata = iconoPlata.AddComponent<Image>();
            imgPlata.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Plata.png");

            GameObject textoPlata = new GameObject("Texto_Plata");
            textoPlata.transform.SetParent(plataObj.transform, false);
            var tpRect = textoPlata.AddComponent<RectTransform>();
            tpRect.anchorMin = new Vector2(0f, 0f);
            tpRect.anchorMax = new Vector2(0.5f, 1f);
            tpRect.offsetMin = new Vector2(48f, 0f);
            tpRect.offsetMax = Vector2.zero;
            var tmpPlata = textoPlata.AddComponent<TextMeshProUGUI>();
            tmpPlata.text = $"0 / {totalVetas}";
            tmpPlata.fontSize = 30f;
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
            tmpScore.fontSize = 30f;
            tmpScore.fontStyle = FontStyles.Bold;
            tmpScore.alignment = TextAlignmentOptions.MidlineRight;

            // Botón Pausa (esquina sup derecha)
            GameObject btnPausaObj = new GameObject("Boton_Pausa");
            btnPausaObj.transform.SetParent(panelSup.transform, false);
            var bpRect = btnPausaObj.AddComponent<RectTransform>();
            bpRect.anchorMin = new Vector2(1f, 0.5f);
            bpRect.anchorMax = new Vector2(1f, 0.5f);
            bpRect.pivot = new Vector2(1f, 0.5f);
            bpRect.sizeDelta = new Vector2(48f, 48f);
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

            // Menú de Pausa
            CrearMenuPausaEnCanvas(canvasObj, btnPausa);

            // Controles Táctiles para Móvil
            CrearControlesTactiles(canvasObj, jugador.GetComponent<PlayerInputHandler>());

            // Sistemas Persistentes
            CrearSistemasPersistentes();
        }

        private static void CrearMenuPausaEnCanvas(GameObject canvasObj, Button btnPausaIcono)
        {
            var pauseComp = canvasObj.AddComponent<PauseMenu>();

            GameObject panelPausa = new GameObject("Panel_Pausa");
            panelPausa.transform.SetParent(canvasObj.transform, false);
            var rectP = panelPausa.AddComponent<RectTransform>();
            rectP.sizeDelta = new Vector2(450f, 320f);
            var imgP = panelPausa.AddComponent<Image>();
            imgP.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            imgP.type = Image.Type.Sliced;

            // Título Pausa
            GameObject tObj = new GameObject("Texto_Pausa");
            tObj.transform.SetParent(panelPausa.transform, false);
            var rectT = tObj.AddComponent<RectTransform>();
            rectT.anchoredPosition = new Vector2(0f, 100f);
            rectT.sizeDelta = new Vector2(400f, 45f);
            var tmpT = tObj.AddComponent<TextMeshProUGUI>();
            tmpT.text = "PAUSA";
            tmpT.fontSize = 32f;
            tmpT.alignment = TextAlignmentOptions.Center;
            tmpT.fontStyle = FontStyles.Bold;
            tmpT.color = new Color(1f, 0.9f, 0.4f);

            Button btnResume = CrearBotonMadera(panelPausa.transform, "Boton_Continuar", "Continuar", new Vector2(0f, 30f), new Vector2(250f, 45f));
            Button btnRestart = CrearBotonMadera(panelPausa.transform, "Boton_Reiniciar", "Reiniciar Nivel", new Vector2(0f, -25f), new Vector2(250f, 45f));
            Button btnMenu = CrearBotonMadera(panelPausa.transform, "Boton_MenuPrincipal", "Menú Principal", new Vector2(0f, -80f), new Vector2(250f, 45f));

            SerializedObject so = new SerializedObject(pauseComp);
            so.FindProperty("pausePanel").objectReferenceValue = panelPausa;
            so.FindProperty("resumeButton").objectReferenceValue = btnResume;
            so.FindProperty("restartButton").objectReferenceValue = btnRestart;
            so.FindProperty("mainMenuButton").objectReferenceValue = btnMenu;
            so.FindProperty("pauseIconButton").objectReferenceValue = btnPausaIcono;
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

            // Botón Izquierda (esquina inferior izquierda)
            GameObject btnLeft = CrearBotonTactil(touchRoot.transform, "Boton_Izquierda", "Assets/Sprites/UI/Boton_Izquierda.png",
                new Vector2(140f, 140f), new Vector2(180f, 180f), new Vector2(0f, 0f));

            // Botón Derecha (al lado del botón izquierda)
            GameObject btnRight = CrearBotonTactil(touchRoot.transform, "Boton_Derecha", "Assets/Sprites/UI/Boton_Derecha.png",
                new Vector2(340f, 140f), new Vector2(180f, 180f), new Vector2(0f, 0f));

            // Botón Saltar (esquina inferior derecha)
            GameObject btnJump = CrearBotonTactil(touchRoot.transform, "Boton_Saltar", "Assets/Sprites/UI/Boton_Saltar.png",
                new Vector2(-160f, 160f), new Vector2(200f, 200f), new Vector2(1f, 0f));

            // EventTriggers para PointerDown / PointerUp
            AgregarEventTrigger(btnLeft, () => touch.OnLeftDown(), () => touch.OnLeftUp());
            AgregarEventTrigger(btnRight, () => touch.OnRightDown(), () => touch.OnRightUp());
            AgregarEventTrigger(btnJump, () => touch.OnJumpDown(), () => touch.OnJumpUp());

            touchRoot.SetActive(false); // TouchControls lo activará automáticamente en móvil
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
            tmp.fontSize = 20f;
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
