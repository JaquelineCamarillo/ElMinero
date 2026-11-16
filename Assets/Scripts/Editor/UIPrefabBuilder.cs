using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;
using ElMinero.UI;

namespace ElMinero.Editor
{
    /// <summary>
    /// Generador procedural de prefabs de interfaz gráfica (UI) para "El Minero".
    /// Implementa estrictamente la especificación de diseño canónico para resolución de referencia
    /// 1920×1080 (match 0.5), escala 9-slice con pixelsPerUnitMultiplier = 1.5625 (1 píxel del sprite = 4 px en 1920×1080), tamaños mínimos de tipografía
    /// mayores o iguales a 26 pt y botones de navegación estandarizados.
    /// </summary>
    public static class UIPrefabBuilder
    {
        private const string PrefabUIPath = "Assets/Prefabs/UI/";

        [MenuItem("Herramientas/El Minero/Crear Prefabs de UI", priority = 65)]
        public static void CrearTodosLosPrefabsUI()
        {
            FontAssetBuilder.GenerarFontAssets();

            if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }

            CrearOptionsPanelPrefab();
            CrearCreditsPanelPrefab();
            CrearLevelSelectPanelPrefab();
            CrearResultsPanelPrefab();
            CrearGameOverPanelPrefab();
            CrearLevelStartCardPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[El Minero] ¡Todos los prefabs de UI creados exitosamente en Assets/Prefabs/UI/!");
        }

        // ====================================================================
        // 1. OPTIONS PANEL (Sección 3.4 & Correcciones B, D, E) - 1200x860
        // ====================================================================
        public static GameObject CrearOptionsPanelPrefab()
        {
            Sprite spPanelMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBotonMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera.png");
            Sprite spBotonMaderaRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Resaltado.png");
            Sprite spBotonMaderaPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Presionado.png");

            Sprite spSliderFondo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Slider_Fondo.png");
            Sprite spSliderRelleno = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Slider_Relleno.png");
            Sprite spSliderManija = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Slider_Manija.png");

            Sprite spToggleFondo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Toggle_Fondo.png");
            Sprite spToggleCheck = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Toggle_Check.png");

            Sprite spIconoMusica = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Iconos/Icono_Musica.png");
            Sprite spIconoEfectos = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Iconos/Icono_Efectos.png");

            Sprite spBotonVolver = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver.png");
            Sprite spBotonVolverRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Resaltado.png");
            Sprite spBotonVolverPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Presionado.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject panelRoot = new GameObject("OptionsPanel");
            var rectRoot = panelRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(1200f, 860f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);
            rectRoot.pivot = new Vector2(0.5f, 0.5f);

            var imgFondo = panelRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var optionsScript = panelRoot.AddComponent<OptionsPanel>();

            // Botón Volver (104x104, esquina superior izquierda a 36, -36)
            GameObject goVolver = new GameObject("Boton_Volver");
            goVolver.transform.SetParent(panelRoot.transform, false);
            var rectVolver = goVolver.AddComponent<RectTransform>();
            rectVolver.anchorMin = new Vector2(0f, 1f);
            rectVolver.anchorMax = new Vector2(0f, 1f);
            rectVolver.pivot = new Vector2(0f, 1f);
            rectVolver.anchoredPosition = new Vector2(36f, -36f);
            rectVolver.sizeDelta = new Vector2(104f, 104f);

            var leVolver = goVolver.AddComponent<LayoutElement>();
            leVolver.ignoreLayout = true;

            var imgVolver = goVolver.AddComponent<Image>();
            imgVolver.sprite = spBotonVolver;
            var btnVolver = goVolver.AddComponent<Button>();
            ConfigurarSpriteSwap(btnVolver, spBotonVolver, spBotonVolverRes, spBotonVolverPres);

            // Vertical Layout Group dentro del panel: centrado verticalmente
            var vlg = panelRoot.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(60, 60, 60, 60);
            vlg.spacing = 30f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Título: "OPCIONES" (PressStart2P 56 pt)
            GameObject goTitulo = new GameObject("Titulo");
            goTitulo.transform.SetParent(panelRoot.transform, false);
            var tmpTitulo = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTitulo.text = "OPCIONES";
            tmpTitulo.fontSize = 56f;
            tmpTitulo.font = fontTitulo;
            tmpTitulo.alignment = TextAlignmentOptions.Center;
            tmpTitulo.color = Color.white;
            goTitulo.AddComponent<LayoutElement>().minHeight = 80f;

            // Fila Música (margen izquierdo 96 px: icono 48 + 24 espacio + etiqueta 260 + slider)
            GameObject filaMusica = new GameObject("Fila_Musica");
            filaMusica.transform.SetParent(panelRoot.transform, false);
            var hlgMusica = filaMusica.AddComponent<HorizontalLayoutGroup>();
            hlgMusica.padding = new RectOffset(96, 60, 0, 0);
            hlgMusica.spacing = 24f;
            hlgMusica.childAlignment = TextAnchor.MiddleLeft;
            hlgMusica.childControlWidth = false;
            hlgMusica.childControlHeight = false;
            filaMusica.AddComponent<LayoutElement>().minHeight = 64f;

            CrearIcono(filaMusica.transform, "Icono_Musica", spIconoMusica, new Vector2(48f, 48f));
            CrearTexto(filaMusica.transform, "Label", "Música:", fontTexto, 34f, new Vector2(260f, 50f));
            Slider sliderMusica = CrearSlider(filaMusica.transform, "Slider_Musica", spSliderFondo, spSliderRelleno, spSliderManija, new Vector2(520f, 44f));

            // Fila Efectos (margen izquierdo 96 px: icono 48 + 24 espacio + etiqueta 260 + slider)
            GameObject filaEfectos = new GameObject("Fila_Efectos");
            filaEfectos.transform.SetParent(panelRoot.transform, false);
            var hlgEfectos = filaEfectos.AddComponent<HorizontalLayoutGroup>();
            hlgEfectos.padding = new RectOffset(96, 60, 0, 0);
            hlgEfectos.spacing = 24f;
            hlgEfectos.childAlignment = TextAnchor.MiddleLeft;
            hlgEfectos.childControlWidth = false;
            hlgEfectos.childControlHeight = false;
            filaEfectos.AddComponent<LayoutElement>().minHeight = 64f;

            CrearIcono(filaEfectos.transform, "Icono_Efectos", spIconoEfectos, new Vector2(48f, 48f));
            CrearTexto(filaEfectos.transform, "Label", "Efectos:", fontTexto, 34f, new Vector2(260f, 50f));
            Slider sliderEfectos = CrearSlider(filaEfectos.transform, "Slider_Efectos", spSliderFondo, spSliderRelleno, spSliderManija, new Vector2(520f, 44f));

            // Fila Toggle Música (margen izquierdo 96 px)
            GameObject filaToggle = new GameObject("Fila_Toggle");
            filaToggle.transform.SetParent(panelRoot.transform, false);
            var hlgToggle = filaToggle.AddComponent<HorizontalLayoutGroup>();
            hlgToggle.padding = new RectOffset(96, 60, 0, 0);
            hlgToggle.spacing = 24f;
            hlgToggle.childAlignment = TextAnchor.MiddleLeft;
            hlgToggle.childControlWidth = false;
            hlgToggle.childControlHeight = false;
            filaToggle.AddComponent<LayoutElement>().minHeight = 64f;

            Toggle toggleMusica = CrearToggle(filaToggle.transform, "Toggle_Musica", "Activar música de fondo", spToggleFondo, spToggleCheck, fontTexto);

            // Fila Botones de Acción (Controles y Borrar Progreso)
            GameObject filaAcciones = new GameObject("Fila_Acciones");
            filaAcciones.transform.SetParent(panelRoot.transform, false);
            var hlgAcciones = filaAcciones.AddComponent<HorizontalLayoutGroup>();
            hlgAcciones.spacing = 40f;
            hlgAcciones.childAlignment = TextAnchor.MiddleCenter;
            hlgAcciones.childControlWidth = false;
            hlgAcciones.childControlHeight = false;
            filaAcciones.AddComponent<LayoutElement>().minHeight = 90f;

            Button btnControles = CrearBotonMadera(filaAcciones.transform, "Boton_Controles", "Ver Controles", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnControles.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 88f);

            Button btnBorrar = CrearBotonMadera(filaAcciones.transform, "Boton_BorrarProgreso", "Borrar Progreso", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnBorrar.GetComponent<RectTransform>().sizeDelta = new Vector2(360f, 88f);

            // Subpanel de Controles (Modal superpuesto 1300x860, SetAsLastSibling)
            GameObject panelControles = CrearSubpanelModal(panelRoot.transform, "Subpanel_Controles", spPanelMadera, new Vector2(1300f, 860f));
            panelControles.transform.SetAsLastSibling();

            GameObject contArea = new GameObject("Area_Contenido");
            contArea.transform.SetParent(panelControles.transform, false);
            var rectArea = contArea.AddComponent<RectTransform>();
            rectArea.anchorMin = Vector2.zero;
            rectArea.anchorMax = Vector2.one;
            rectArea.offsetMin = new Vector2(72f, 150f);
            rectArea.offsetMax = new Vector2(-72f, -64f);

            var vlgArea = contArea.AddComponent<VerticalLayoutGroup>();
            vlgArea.spacing = 10f;
            vlgArea.childAlignment = TextAnchor.UpperLeft;
            vlgArea.childControlWidth = true;
            vlgArea.childControlHeight = true;
            vlgArea.childForceExpandWidth = true;
            vlgArea.childForceExpandHeight = false;

            // Título Controles (Press Start 2P 44, Auto Size 30-44, una sola línea)
            GameObject goTitCtrl = new GameObject("Titulo_Controles");
            goTitCtrl.transform.SetParent(contArea.transform, false);
            var tmpTitCtrl = goTitCtrl.AddComponent<TextMeshProUGUI>();
            tmpTitCtrl.font = fontTitulo;
            tmpTitCtrl.text = "CONTROLES DEL JUEGO";
            tmpTitCtrl.fontSize = 44f;
            tmpTitCtrl.enableAutoSizing = true;
            tmpTitCtrl.fontSizeMin = 30f;
            tmpTitCtrl.fontSizeMax = 44f;
            tmpTitCtrl.textWrappingMode = TextWrappingModes.NoWrap;
            tmpTitCtrl.alignment = TextAlignmentOptions.Center;
            tmpTitCtrl.color = Color.white;
            goTitCtrl.AddComponent<LayoutElement>().minHeight = 56f;

            // Secciones con subtítulos Pixelify Sans 32 y renglones en 28 con "-"
            CrearSeccionControles(contArea.transform, "TECLADO", new string[] {
                "- A / D o Flechas : Moverse a los lados",
                "- Espacio : Saltar (mantén para mayor altura)",
                "- Escape / P : Menú de Pausa"
            }, fontTexto);

            CrearSeccionControles(contArea.transform, "CONTROL (GAMEPAD)", new string[] {
                "- Stick Izquierdo / D-Pad : Moverse",
                "- Botón Sur (A/X) : Saltar",
                "- Start / Menú : Pausa"
            }, fontTexto);

            CrearSeccionControles(contArea.transform, "PANTALLA TÁCTIL", new string[] {
                "- Botones virtuales en pantalla"
            }, fontTexto);

            Button btnCerrarControles = CrearBotonMadera(panelControles.transform, "Boton_CerrarControles", "Cerrar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 84f);
            var rectBcc = btnCerrarControles.GetComponent<RectTransform>();
            rectBcc.anchorMin = new Vector2(0.5f, 0f);
            rectBcc.anchorMax = new Vector2(0.5f, 0f);
            rectBcc.pivot = new Vector2(0.5f, 0f);
            rectBcc.anchoredPosition = new Vector2(0f, 48f);
            rectBcc.sizeDelta = new Vector2(360f, 84f);
            panelControles.SetActive(false);

            // Modal Confirmar Borrado (900x500)
            GameObject modalBorrar = CrearSubpanelModal(panelRoot.transform, "Modal_ConfirmarBorrado", spPanelMadera, new Vector2(900f, 500f));
            modalBorrar.transform.SetAsLastSibling();
            GameObject textoMb = new GameObject("Texto_Confirmar");
            textoMb.transform.SetParent(modalBorrar.transform, false);
            var rectMb = textoMb.AddComponent<RectTransform>();
            rectMb.anchorMin = new Vector2(0f, 0.4f);
            rectMb.anchorMax = new Vector2(1f, 1f);
            rectMb.offsetMin = new Vector2(40f, 0f);
            rectMb.offsetMax = new Vector2(-40f, -40f);
            var tmpMb = textoMb.AddComponent<TextMeshProUGUI>();
            tmpMb.font = fontTexto;
            tmpMb.fontSize = 34f;
            tmpMb.alignment = TextAlignmentOptions.Center;
            tmpMb.text = "¿Seguro que deseas borrar el progreso?\nSe restablecerán las estrellas, récords y minerales.";
            tmpMb.color = Color.white;

            Button btnSi = CrearBotonMadera(modalBorrar.transform, "Boton_Si", "Sí, borrar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 32f, 80f);
            var rectSi = btnSi.GetComponent<RectTransform>();
            rectSi.anchorMin = new Vector2(0.3f, 0.2f);
            rectSi.anchorMax = new Vector2(0.3f, 0.2f);
            rectSi.sizeDelta = new Vector2(300f, 80f);

            Button btnNo = CrearBotonMadera(modalBorrar.transform, "Boton_No", "Cancelar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 32f, 80f);
            var rectNo = btnNo.GetComponent<RectTransform>();
            rectNo.anchorMin = new Vector2(0.7f, 0.2f);
            rectNo.anchorMax = new Vector2(0.7f, 0.2f);
            rectNo.sizeDelta = new Vector2(300f, 80f);
            modalBorrar.SetActive(false);

            // Aviso Progreso Borrado
            GameObject avisoBorrado = new GameObject("Aviso_Borrado");
            avisoBorrado.transform.SetParent(panelRoot.transform, false);
            var rectAv = avisoBorrado.AddComponent<RectTransform>();
            rectAv.sizeDelta = new Vector2(600f, 60f);
            rectAv.anchoredPosition = new Vector2(0f, -340f);
            var tmpAv = avisoBorrado.AddComponent<TextMeshProUGUI>();
            tmpAv.font = fontTexto;
            tmpAv.fontSize = 34f;
            tmpAv.alignment = TextAlignmentOptions.Center;
            tmpAv.text = "¡Progreso restablecido correctamente!";
            tmpAv.color = new Color(1f, 0.85f, 0.4f);
            avisoBorrado.AddComponent<LayoutElement>().ignoreLayout = true;
            avisoBorrado.SetActive(false);

            // Asignar referencias
            SerializedObject so = new SerializedObject(optionsScript);
            so.FindProperty("sliderMusica").objectReferenceValue = sliderMusica;
            so.FindProperty("sliderEfectos").objectReferenceValue = sliderEfectos;
            so.FindProperty("toggleMusica").objectReferenceValue = toggleMusica;
            so.FindProperty("botonControles").objectReferenceValue = btnControles;
            so.FindProperty("panelControles").objectReferenceValue = panelControles;
            so.FindProperty("botonCerrarControles").objectReferenceValue = btnCerrarControles;
            so.FindProperty("botonBorrarProgreso").objectReferenceValue = btnBorrar;
            so.FindProperty("modalConfirmarBorrar").objectReferenceValue = modalBorrar;
            so.FindProperty("botonConfirmarBorrarSi").objectReferenceValue = btnSi;
            so.FindProperty("botonConfirmarBorrarNo").objectReferenceValue = btnNo;
            so.FindProperty("avisoProgresoBorrado").objectReferenceValue = avisoBorrado;
            so.FindProperty("botonVolver").objectReferenceValue = btnVolver;
            so.ApplyModifiedProperties();

            string prefabPath = PrefabUIPath + "OptionsPanel.prefab";
            ElMineroBuilder.DecorarPanelesEn(panelRoot);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            GameObject.DestroyImmediate(panelRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static void CrearSeccionControles(Transform parent, string subtitulo, string[] lineas, TMP_FontAsset fontTexto)
        {
            GameObject goSub = new GameObject("Subtitulo_" + subtitulo.Replace(" ", "_"));
            goSub.transform.SetParent(parent, false);
            var tmpSub = goSub.AddComponent<TextMeshProUGUI>();
            tmpSub.font = fontTexto;
            tmpSub.fontSize = 32f;
            tmpSub.fontStyle = FontStyles.Bold;
            tmpSub.color = new Color(1f, 0.85f, 0.4f);
            tmpSub.text = subtitulo;
            goSub.AddComponent<LayoutElement>().minHeight = 36f;

            foreach (var l in lineas)
            {
                GameObject goLine = new GameObject("Linea");
                goLine.transform.SetParent(parent, false);
                var tmpLine = goLine.AddComponent<TextMeshProUGUI>();
                tmpLine.font = fontTexto;
                tmpLine.fontSize = 28f;
                tmpLine.color = Color.white;
                tmpLine.text = l;
                goLine.AddComponent<LayoutElement>().minHeight = 30f;
            }
        }

        // ====================================================================
        // 2. CREDITS PANEL (Sección 3.5) - 1200x860
        // ====================================================================
        public static GameObject CrearCreditsPanelPrefab()
        {
            Sprite spPanelMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBotonVolver = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver.png");
            Sprite spBotonVolverRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Resaltado.png");
            Sprite spBotonVolverPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Presionado.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject panelRoot = new GameObject("CreditsPanel");
            var rectRoot = panelRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(1200f, 860f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);
            rectRoot.pivot = new Vector2(0.5f, 0.5f);

            var imgFondo = panelRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var credScript = panelRoot.AddComponent<CreditsPanel>();

            // Botón Volver (104x104)
            GameObject goVolver = new GameObject("Boton_Volver");
            goVolver.transform.SetParent(panelRoot.transform, false);
            var rectVolver = goVolver.AddComponent<RectTransform>();
            rectVolver.anchorMin = new Vector2(0f, 1f);
            rectVolver.anchorMax = new Vector2(0f, 1f);
            rectVolver.pivot = new Vector2(0f, 1f);
            rectVolver.anchoredPosition = new Vector2(36f, -36f);
            rectVolver.sizeDelta = new Vector2(104f, 104f);
            var imgVolver = goVolver.AddComponent<Image>();
            imgVolver.sprite = spBotonVolver;
            var btnVolver = goVolver.AddComponent<Button>();
            ConfigurarSpriteSwap(btnVolver, spBotonVolver, spBotonVolverRes, spBotonVolverPres);

            // Título: "CRÉDITOS" (PressStart2P 56 pt)
            GameObject goTitulo = new GameObject("Titulo");
            goTitulo.transform.SetParent(panelRoot.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -50f);
            rectTit.sizeDelta = new Vector2(800f, 60f);
            var tmpTitulo = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTitulo.text = "CREDITOS";
            tmpTitulo.fontSize = 56f;
            tmpTitulo.font = fontTitulo;
            tmpTitulo.alignment = TextAlignmentOptions.Center;
            tmpTitulo.color = Color.white;

            // Scroll View de Créditos
            GameObject goScrollView = new GameObject("ScrollView");
            goScrollView.transform.SetParent(panelRoot.transform, false);
            var rectSv = goScrollView.AddComponent<RectTransform>();
            rectSv.anchorMin = Vector2.zero;
            rectSv.anchorMax = Vector2.one;
            rectSv.offsetMin = new Vector2(60f, 50f);
            rectSv.offsetMax = new Vector2(-60f, -140f);

            var scrollRect = goScrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            var imgMask = goScrollView.AddComponent<Image>();
            imgMask.color = new Color(0f, 0f, 0f, 0.01f);
            var mask = goScrollView.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            GameObject goContent = new GameObject("Content");
            goContent.transform.SetParent(goScrollView.transform, false);
            var rectContent = goContent.AddComponent<RectTransform>();
            rectContent.anchorMin = new Vector2(0f, 1f);
            rectContent.anchorMax = new Vector2(1f, 1f);
            rectContent.pivot = new Vector2(0.5f, 1f);
            rectContent.sizeDelta = new Vector2(0f, 800f);

            scrollRect.content = rectContent;

            GameObject goText = new GameObject("Texto_Creditos");
            goText.transform.SetParent(goContent.transform, false);
            var rectText = goText.AddComponent<RectTransform>();
            rectText.anchorMin = Vector2.zero;
            rectText.anchorMax = Vector2.one;
            rectText.offsetMin = Vector2.zero;
            rectText.offsetMax = Vector2.zero;

            var tmpCred = goText.AddComponent<TextMeshProUGUI>();
            tmpCred.font = fontTexto;
            tmpCred.fontSize = 32f;
            tmpCred.alignment = TextAlignmentOptions.Center;
            tmpCred.color = Color.white;
            tmpCred.text = "<b><size=44>EL MINERO</size></b>\n" +
                           "Un videojuego de plataformas 2D con rigor histórico\n\n" +
                           "<b>DISEÑO Y DESARROLLO</b>\n" +
                           "Juana Jaqueline Camarillo Oláez\n\n" +
                           "<b>INSTITUCIÓN</b>\n" +
                           "Universidad Tecnológica del Norte de Guanajuato (UTNG)\n" +
                           "GIDS6102-E · Creación de Videojuegos\n" +
                           "Profesor: Gabriel Barrón Rodríguez\n\n" +
                           "<b>HISTORIA Y MINERÍA</b>\n" +
                           "Mina de Rayas · El Nopal · La Valenciana\n" +
                           "Guanajuato, México — Siglo XIX\n\n" +
                           "<i>¡Gracias por jugar!</i>";

            SerializedObject so = new SerializedObject(credScript);
            so.FindProperty("botonVolver").objectReferenceValue = btnVolver;
            so.FindProperty("contenidoCreditos").objectReferenceValue = rectContent;
            so.ApplyModifiedProperties();

            string prefabPath = PrefabUIPath + "CreditsPanel.prefab";
            ElMineroBuilder.DecorarPanelesEn(panelRoot);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            GameObject.DestroyImmediate(panelRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // ====================================================================
        // 3. LEVEL SELECT PANEL (Sección 3.3) - 1500x900
        // ====================================================================
        public static GameObject CrearLevelSelectPanelPrefab()
        {
            Sprite spPanelMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBotonNivel = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Nivel.png");
            Sprite spBotonNivelRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Nivel_Resaltado.png");
            Sprite spBotonNivelPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Nivel_Presionado.png");
            Sprite spBotonVolver = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver.png");
            Sprite spBotonVolverRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Resaltado.png");
            Sprite spBotonVolverPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Presionado.png");
            Sprite spCandado = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Icono_Candado.png");

            Sprite spEstrellaLlena = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Estrella_Llena.png");
            Sprite spEstrellaVacia = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Estrella_Vacia.png");

            Sprite spMiniN1 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Miniatura_MinaRayas.png");
            Sprite spMiniN2 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Miniatura_ElNopal.png");
            Sprite spMiniN3 = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Miniatura_LaValenciana.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject panelRoot = new GameObject("LevelSelectPanel");
            var rectRoot = panelRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(1500f, 900f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);

            var imgFondo = panelRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var selScript = panelRoot.AddComponent<LevelSelectPanel>();

            // Botón Volver (96x96), dentro del marco interior
            GameObject goVolver = new GameObject("Boton_Volver");
            goVolver.transform.SetParent(panelRoot.transform, false);
            var rectVolver = goVolver.AddComponent<RectTransform>();
            rectVolver.anchorMin = new Vector2(0f, 1f);
            rectVolver.anchorMax = new Vector2(0f, 1f);
            rectVolver.pivot = new Vector2(0f, 1f);
            rectVolver.anchoredPosition = new Vector2(60f, -56f);
            rectVolver.sizeDelta = new Vector2(96f, 96f);
            var imgVolver = goVolver.AddComponent<Image>();
            imgVolver.sprite = spBotonVolver;
            var btnVolver = goVolver.AddComponent<Button>();
            ConfigurarSpriteSwap(btnVolver, spBotonVolver, spBotonVolverRes, spBotonVolverPres);

            // Título sobre una cinta roja con orillas doradas
            GameObject goCinta = new GameObject("Cinta_Titulo");
            goCinta.transform.SetParent(panelRoot.transform, false);
            var rectCinta = goCinta.AddComponent<RectTransform>();
            rectCinta.anchorMin = new Vector2(0.5f, 1f);
            rectCinta.anchorMax = new Vector2(0.5f, 1f);
            rectCinta.anchoredPosition = new Vector2(0f, -100f);
            rectCinta.sizeDelta = new Vector2(980f, 80f);
            var imgCinta = goCinta.AddComponent<Image>();
            imgCinta.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Decoracion/Cinta_Titulo.png");
            imgCinta.type = Image.Type.Sliced;
            imgCinta.pixelsPerUnitMultiplier = 1.5625f;
            imgCinta.raycastTarget = false;

            GameObject goTitulo = new GameObject("Titulo");
            goTitulo.transform.SetParent(goCinta.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = Vector2.zero;
            rectTit.anchorMax = Vector2.one;
            rectTit.offsetMin = new Vector2(70f, 6f);
            rectTit.offsetMax = new Vector2(-70f, -6f);
            var tmpTitulo = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTitulo.text = "SELECCION DE NIVELES";
            tmpTitulo.font = fontTitulo;
            tmpTitulo.enableAutoSizing = true;
            tmpTitulo.fontSizeMin = 24f;
            tmpTitulo.fontSizeMax = 42f;
            tmpTitulo.alignment = TextAlignmentOptions.Center;
            tmpTitulo.color = new Color(1f, 0.91f, 0.59f);
            tmpTitulo.raycastTarget = false;

            // Fila de 3 tarjetas (420x640 cada una, espaciado 40 px)
            GameObject goFila = new GameObject("Fila_Tarjetas");
            goFila.transform.SetParent(panelRoot.transform, false);
            var rectFila = goFila.AddComponent<RectTransform>();
            rectFila.anchorMin = new Vector2(0.5f, 0.5f);
            rectFila.anchorMax = new Vector2(0.5f, 0.5f);
            rectFila.anchoredPosition = new Vector2(0f, -55f);
            rectFila.sizeDelta = new Vector2(1380f, 650f);

            var hlg = goFila.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 40f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Construir 3 tarjetas canónicas
            var t1 = CrearTarjetaNivel(goFila.transform, 1, "Nivel1_MinaRayas", "Mina Rayas", spMiniN1, spBotonNivel, spBotonNivelRes, spBotonNivelPres, spCandado, spEstrellaVacia, fontTitulo, fontTexto);
            var t2 = CrearTarjetaNivel(goFila.transform, 2, "Nivel2_ElNopal", "El Nopal", spMiniN2, spBotonNivel, spBotonNivelRes, spBotonNivelPres, spCandado, spEstrellaVacia, fontTitulo, fontTexto);
            var t3 = CrearTarjetaNivel(goFila.transform, 3, "Nivel3_LaValenciana", "La Valenciana", spMiniN3, spBotonNivel, spBotonNivelRes, spBotonNivelPres, spCandado, spEstrellaVacia, fontTitulo, fontTexto);

            SerializedObject so = new SerializedObject(selScript);
            so.FindProperty("botonVolver").objectReferenceValue = btnVolver;
            so.FindProperty("spriteEstrellaLlena").objectReferenceValue = spEstrellaLlena;
            so.FindProperty("spriteEstrellaVacia").objectReferenceValue = spEstrellaVacia;

            var propTarjetas = so.FindProperty("tarjetas");
            propTarjetas.arraySize = 3;
            AsignarTarjetaAPropiedad(propTarjetas.GetArrayElementAtIndex(0), t1);
            AsignarTarjetaAPropiedad(propTarjetas.GetArrayElementAtIndex(1), t2);
            AsignarTarjetaAPropiedad(propTarjetas.GetArrayElementAtIndex(2), t3);
            so.ApplyModifiedProperties();

            string prefabPath = PrefabUIPath + "LevelSelectPanel.prefab";
            ElMineroBuilder.DecorarPanelesEn(panelRoot);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            GameObject.DestroyImmediate(panelRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static LevelSelectPanel.TarjetaNivelUI CrearTarjetaNivel(
            Transform padre,
            int numero,
            string escena,
            string nombreMina,
            Sprite miniatura,
            Sprite spBoton,
            Sprite spBotonRes,
            Sprite spBotonPres,
            Sprite spCandado,
            Sprite spEstrellaVacia,
            TMP_FontAsset fontTitulo,
            TMP_FontAsset fontTexto)
        {
            GameObject goTarjeta = new GameObject($"Tarjeta_Nivel_{numero}");
            goTarjeta.transform.SetParent(padre, false);
            var rectTar = goTarjeta.AddComponent<RectTransform>();
            rectTar.sizeDelta = new Vector2(420f, 640f);

            var imgFondo = goTarjeta.AddComponent<Image>();
            imgFondo.sprite = spBoton;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var btn = goTarjeta.AddComponent<Button>();
            ConfigurarSpriteSwap(btn, spBoton, spBotonRes, spBotonPres);

            Sprite spMarcoMini = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Decoracion/Marco_Miniatura.png");
            Sprite spMedalla = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Decoracion/Medalla.png");
            Sprite spBanda = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Campo_Texto.png");
            Sprite spSeparador = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Menu/Menu_Separador.png");

            // Miniatura (360x200) con marco de hierro
            GameObject goMini = new GameObject("Miniatura");
            goMini.transform.SetParent(goTarjeta.transform, false);
            var rectMini = goMini.AddComponent<RectTransform>();
            rectMini.anchorMin = new Vector2(0.5f, 1f);
            rectMini.anchorMax = new Vector2(0.5f, 1f);
            rectMini.anchoredPosition = new Vector2(0f, -192f);
            rectMini.sizeDelta = new Vector2(360f, 200f);
            var imgMini = goMini.AddComponent<Image>();
            imgMini.sprite = miniatura;
            imgMini.raycastTarget = false;

            GameObject goMarcoMini = new GameObject("Marco_Miniatura");
            goMarcoMini.transform.SetParent(goTarjeta.transform, false);
            var rectMarco = goMarcoMini.AddComponent<RectTransform>();
            rectMarco.anchorMin = new Vector2(0.5f, 1f);
            rectMarco.anchorMax = new Vector2(0.5f, 1f);
            rectMarco.anchoredPosition = new Vector2(0f, -192f);
            rectMarco.sizeDelta = new Vector2(384f, 224f);
            var imgMarco = goMarcoMini.AddComponent<Image>();
            imgMarco.sprite = spMarcoMini;
            imgMarco.type = Image.Type.Sliced;
            imgMarco.fillCenter = false;
            imgMarco.pixelsPerUnitMultiplier = 1.5625f;
            imgMarco.raycastTarget = false;

            // Nombre de la mina
            GameObject goTitulo = new GameObject("Texto_TituloMina");
            goTitulo.transform.SetParent(goTarjeta.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(20f, -48f);
            rectTit.sizeDelta = new Vector2(320f, 50f);
            var tmpTit = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTit.font = fontTexto;
            tmpTit.fontSize = 34f;
            tmpTit.enableAutoSizing = true;
            tmpTit.fontSizeMin = 24f;
            tmpTit.fontSizeMax = 34f;
            tmpTit.fontStyle = FontStyles.Bold;
            tmpTit.alignment = TextAlignmentOptions.Center;
            tmpTit.color = new Color(0.96f, 0.91f, 0.8f);
            tmpTit.text = nombreMina;
            tmpTit.raycastTarget = false;

            // Banda oscura para las estrellas
            GameObject goBanda = new GameObject("Banda_Estrellas");
            goBanda.transform.SetParent(goTarjeta.transform, false);
            var rectBanda = goBanda.AddComponent<RectTransform>();
            rectBanda.anchorMin = new Vector2(0.5f, 1f);
            rectBanda.anchorMax = new Vector2(0.5f, 1f);
            rectBanda.anchoredPosition = new Vector2(0f, -360f);
            rectBanda.sizeDelta = new Vector2(300f, 84f);
            var imgBanda = goBanda.AddComponent<Image>();
            imgBanda.sprite = spBanda;
            imgBanda.type = Image.Type.Sliced;
            imgBanda.pixelsPerUnitMultiplier = 1.5625f;
            imgBanda.raycastTarget = false;

            // Fila de 3 Estrellas (56x56 cada una)
            GameObject goEstrellas = new GameObject("Fila_Estrellas");
            goEstrellas.transform.SetParent(goTarjeta.transform, false);
            var rectEst = goEstrellas.AddComponent<RectTransform>();
            rectEst.anchorMin = new Vector2(0.5f, 1f);
            rectEst.anchorMax = new Vector2(0.5f, 1f);
            rectEst.anchoredPosition = new Vector2(0f, -360f);
            rectEst.sizeDelta = new Vector2(240f, 60f);

            var hlgEst = goEstrellas.AddComponent<HorizontalLayoutGroup>();
            hlgEst.spacing = 18f;
            hlgEst.childAlignment = TextAnchor.MiddleCenter;
            hlgEst.childControlWidth = false;
            hlgEst.childControlHeight = false;

            Image[] iconos = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject goE = new GameObject($"Estrella_{i + 1}");
                goE.transform.SetParent(goEstrellas.transform, false);
                var re = goE.AddComponent<RectTransform>();
                re.sizeDelta = new Vector2(56f, 56f);
                var ie = goE.AddComponent<Image>();
                ie.sprite = spEstrellaVacia;
                ie.preserveAspect = true;
                ie.raycastTarget = false;
                iconos[i] = ie;
            }

            // Separador dorado
            GameObject goSep = new GameObject("Separador");
            goSep.transform.SetParent(goTarjeta.transform, false);
            var rectSep = goSep.AddComponent<RectTransform>();
            rectSep.anchorMin = new Vector2(0.5f, 1f);
            rectSep.anchorMax = new Vector2(0.5f, 1f);
            rectSep.anchoredPosition = new Vector2(0f, -432f);
            rectSep.sizeDelta = new Vector2(320f, 20f);
            var imgSep = goSep.AddComponent<Image>();
            imgSep.sprite = spSeparador;
            imgSep.raycastTarget = false;

            // Récords (28 pt, dos renglones)
            GameObject goRec = new GameObject("Texto_Record");
            goRec.transform.SetParent(goTarjeta.transform, false);
            var rectRec = goRec.AddComponent<RectTransform>();
            rectRec.anchorMin = new Vector2(0.5f, 1f);
            rectRec.anchorMax = new Vector2(0.5f, 1f);
            rectRec.anchoredPosition = new Vector2(0f, -492f);
            rectRec.sizeDelta = new Vector2(360f, 76f);
            var tmpRec = goRec.AddComponent<TextMeshProUGUI>();
            tmpRec.font = fontTexto;
            tmpRec.fontSize = 28f;
            tmpRec.alignment = TextAlignmentOptions.Center;
            tmpRec.color = new Color(0.95f, 0.9f, 0.75f);
            tmpRec.text = "Sin récord aún";
            tmpRec.raycastTarget = false;

            // Medalla con el número del nivel (sobre la esquina)
            GameObject goMedalla = new GameObject("Medalla_Numero");
            goMedalla.transform.SetParent(goTarjeta.transform, false);
            var rectMed = goMedalla.AddComponent<RectTransform>();
            rectMed.anchorMin = new Vector2(0f, 1f);
            rectMed.anchorMax = new Vector2(0f, 1f);
            rectMed.pivot = new Vector2(0.5f, 0.5f);
            rectMed.anchoredPosition = new Vector2(26f, -26f);
            rectMed.sizeDelta = new Vector2(80f, 80f);
            var imgMed = goMedalla.AddComponent<Image>();
            imgMed.sprite = spMedalla;
            imgMed.raycastTarget = false;
            GameObject goNum = new GameObject("Numero");
            goNum.transform.SetParent(goMedalla.transform, false);
            var rectNum = goNum.AddComponent<RectTransform>();
            rectNum.anchorMin = Vector2.zero;
            rectNum.anchorMax = Vector2.one;
            rectNum.offsetMin = new Vector2(0f, 2f);
            rectNum.offsetMax = new Vector2(4f, 0f);
            var tmpNum = goNum.AddComponent<TextMeshProUGUI>();
            tmpNum.font = fontTitulo;
            tmpNum.fontSize = 30f;
            tmpNum.alignment = TextAlignmentOptions.Center;
            tmpNum.color = new Color(0.28f, 0.16f, 0.04f);
            tmpNum.text = numero.ToString();
            tmpNum.raycastTarget = false;

            // Efecto al pasar el ratón: crece un poco y el nombre se vuelve dorado
            var anim = goTarjeta.AddComponent<BotonMenuAnimado>();
            var soAnim = new SerializedObject(anim);
            soAnim.FindProperty("texto").objectReferenceValue = tmpTit;
            soAnim.FindProperty("escalaResaltado").floatValue = 1.04f;
            soAnim.ApplyModifiedPropertiesWithoutUndo();

            // Candado / Estado
            GameObject goLock = new GameObject("Contenedor_Candado");
            goLock.transform.SetParent(goTarjeta.transform, false);
            var rectLock = goLock.AddComponent<RectTransform>();
            rectLock.anchorMin = Vector2.zero;
            rectLock.anchorMax = Vector2.one;
            rectLock.offsetMin = Vector2.zero;
            rectLock.offsetMax = Vector2.zero;

            GameObject goIconoCandado = new GameObject("Icono_Candado");
            goIconoCandado.transform.SetParent(goLock.transform, false);
            var rectIc = goIconoCandado.AddComponent<RectTransform>();
            rectIc.anchorMin = new Vector2(0.5f, 0.5f);
            rectIc.anchorMax = new Vector2(0.5f, 0.5f);
            rectIc.anchoredPosition = new Vector2(0f, 128f); // sobre la miniatura
            rectIc.sizeDelta = new Vector2(96f, 96f);
            var imgLock = goIconoCandado.AddComponent<Image>();
            imgLock.sprite = spCandado;
            imgLock.preserveAspect = true;

            GameObject goEstado = new GameObject("Texto_Estado");
            goEstado.transform.SetParent(goTarjeta.transform, false);
            var rectEstad = goEstado.AddComponent<RectTransform>();
            rectEstad.anchorMin = new Vector2(0.5f, 0f);
            rectEstad.anchorMax = new Vector2(0.5f, 0f);
            rectEstad.anchoredPosition = new Vector2(0f, 56f);
            rectEstad.sizeDelta = new Vector2(380f, 50f);
            var tmpEstad = goEstado.AddComponent<TextMeshProUGUI>();
            tmpEstad.font = fontTexto;
            tmpEstad.fontSize = 32f;
            tmpEstad.fontStyle = FontStyles.Bold;
            tmpEstad.alignment = TextAlignmentOptions.Center;
            tmpEstad.color = new Color(0.4f, 1f, 0.5f);
            tmpEstad.text = "Disponible";

            LevelSelectPanel.TarjetaNivelUI t = new LevelSelectPanel.TarjetaNivelUI();
            t.numeroNivel = numero;
            t.nombreEscena = escena;
            t.botonSeleccion = btn;
            t.miniatura = imgMini;
            t.contenedorCandado = goLock;
            t.textoEstado = tmpEstad;
            t.iconosEstrellas = iconos;
            t.textoRecord = tmpRec;

            return t;
        }

        private static void AsignarTarjetaAPropiedad(SerializedProperty prop, LevelSelectPanel.TarjetaNivelUI t)
        {
            prop.FindPropertyRelative("numeroNivel").intValue = t.numeroNivel;
            prop.FindPropertyRelative("nombreEscena").stringValue = t.nombreEscena;
            prop.FindPropertyRelative("botonSeleccion").objectReferenceValue = t.botonSeleccion;
            prop.FindPropertyRelative("miniatura").objectReferenceValue = t.miniatura;
            prop.FindPropertyRelative("contenedorCandado").objectReferenceValue = t.contenedorCandado;
            prop.FindPropertyRelative("textoEstado").objectReferenceValue = t.textoEstado;
            prop.FindPropertyRelative("textoRecord").objectReferenceValue = t.textoRecord;

            var propEst = prop.FindPropertyRelative("iconosEstrellas");
            propEst.arraySize = t.iconosEstrellas.Length;
            for (int i = 0; i < t.iconosEstrellas.Length; i++)
            {
                propEst.GetArrayElementAtIndex(i).objectReferenceValue = t.iconosEstrellas[i];
            }
        }

        // ====================================================================
        // 4. RESULTS PANEL (Sección 4 & Correcciones B, G) - 1200x900
        // ====================================================================
        public static GameObject CrearResultsPanelPrefab()
        {
            Sprite spPanelMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBotonMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera.png");
            Sprite spBotonMaderaRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Resaltado.png");
            Sprite spBotonMaderaPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Presionado.png");

            Sprite spEstrellaLlena = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Estrella_Llena.png");
            Sprite spEstrellaVacia = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Estrella_Vacia.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject panelRoot = new GameObject("ResultsPanel");
            var rectRoot = panelRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(1200f, 900f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);

            var imgFondo = panelRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var resScript = panelRoot.AddComponent<ResultsPanel>();

            // Título: "¡NIVEL COMPLETADO!" (PressStart2P 60 pt)
            GameObject goTitulo = new GameObject("Texto_Titulo");
            goTitulo.transform.SetParent(panelRoot.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -82f);
            rectTit.sizeDelta = new Vector2(1040f, 70f);
            var tmpTit = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTit.font = fontTitulo;
            tmpTit.fontSize = 56f;
            tmpTit.text = "¡NIVEL COMPLETADO!";
            tmpTit.alignment = TextAlignmentOptions.Center;
            tmpTit.color = new Color(1f, 0.85f, 0.4f);

            // Frase Estrellas (PixelifySans 34 pt)
            GameObject goFrase = new GameObject("Texto_FraseEstrellas");
            goFrase.transform.SetParent(panelRoot.transform, false);
            var rectFrase = goFrase.AddComponent<RectTransform>();
            rectFrase.anchorMin = new Vector2(0.5f, 1f);
            rectFrase.anchorMax = new Vector2(0.5f, 1f);
            rectFrase.anchoredPosition = new Vector2(0f, -152f);
            rectFrase.sizeDelta = new Vector2(1000f, 50f);
            var tmpFrase = goFrase.AddComponent<TextMeshProUGUI>();
            tmpFrase.font = fontTexto;
            tmpFrase.fontSize = 34f;
            tmpFrase.alignment = TextAlignmentOptions.Center;
            tmpFrase.color = Color.white;
            tmpFrase.text = "¡Buen trabajo, minero!";

            // Fila de 3 Estrellas (128x128 cada una, separación 32 px)
            GameObject goEstrellas = new GameObject("Fila_Estrellas");
            goEstrellas.transform.SetParent(panelRoot.transform, false);
            var rectEst = goEstrellas.AddComponent<RectTransform>();
            rectEst.anchorMin = new Vector2(0.5f, 1f);
            rectEst.anchorMax = new Vector2(0.5f, 1f);
            rectEst.anchoredPosition = new Vector2(0f, -250f);
            rectEst.sizeDelta = new Vector2(480f, 130f);

            var hlgEst = goEstrellas.AddComponent<HorizontalLayoutGroup>();
            hlgEst.spacing = 32f;
            hlgEst.childAlignment = TextAnchor.MiddleCenter;

            Image[] imgEstrellas = new Image[3];
            for (int i = 0; i < 3; i++)
            {
                GameObject goE = new GameObject($"Estrella_{i + 1}");
                goE.transform.SetParent(goEstrellas.transform, false);
                var re = goE.AddComponent<RectTransform>();
                re.sizeDelta = new Vector2(128f, 128f);
                var ie = goE.AddComponent<Image>();
                ie.sprite = spEstrellaVacia;
                ie.preserveAspect = true;
                imgEstrellas[i] = ie;
            }

            // Banner Desbloqueo (34 pt)
            GameObject goBanner = new GameObject("Banner_Desbloqueo");
            goBanner.transform.SetParent(panelRoot.transform, false);
            var rectBan = goBanner.AddComponent<RectTransform>();
            rectBan.anchorMin = new Vector2(0.5f, 1f);
            rectBan.anchorMax = new Vector2(0.5f, 1f);
            rectBan.anchoredPosition = new Vector2(0f, -345f);
            rectBan.sizeDelta = new Vector2(900f, 50f);
            var tmpBan = goBanner.AddComponent<TextMeshProUGUI>();
            tmpBan.font = fontTexto;
            tmpBan.fontSize = 34f;
            tmpBan.fontStyle = FontStyles.Bold;
            tmpBan.alignment = TextAlignmentOptions.Center;
            tmpBan.text = "¡Nivel 2 desbloqueado!";
            tmpBan.color = new Color(0.4f, 1f, 0.5f);

            // Contenedor Grid de 2 Columnas para Estadísticas (Sección G: 520x56 cada celda, font 34 pt)
            GameObject goStats = new GameObject("Contenedor_Stats");
            goStats.transform.SetParent(panelRoot.transform, false);
            var rectSt = goStats.AddComponent<RectTransform>();
            rectSt.anchorMin = new Vector2(0.5f, 0.5f);
            rectSt.anchorMax = new Vector2(0.5f, 0.5f);
            rectSt.anchoredPosition = new Vector2(0f, -40f);
            rectSt.sizeDelta = new Vector2(1080f, 200f);

            var glgSt = goStats.AddComponent<GridLayoutGroup>();
            glgSt.cellSize = new Vector2(520f, 56f);
            glgSt.spacing = new Vector2(40f, 14f);
            glgSt.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glgSt.constraintCount = 2;
            glgSt.childAlignment = TextAnchor.MiddleCenter;

            TextMeshProUGUI tGemas = CrearTextoStat(goStats.transform, "Texto_Gemas", "Gemas: 0 / 0", fontTexto);
            TextMeshProUGUI tPlata = CrearTextoStat(goStats.transform, "Texto_Plata", "Plata: 0 / 0", fontTexto);
            TextMeshProUGUI tTiempo = CrearTextoStat(goStats.transform, "Texto_Tiempo", "Tiempo restante: 0.0s", fontTexto);
            TextMeshProUGUI tVidas = CrearTextoStat(goStats.transform, "Texto_Vidas", "Vidas restantes: 3", fontTexto);
            TextMeshProUGUI tScore = CrearTextoStat(goStats.transform, "Texto_PuntosNivel", "Puntos nivel: 0", fontTexto);
            TextMeshProUGUI tTotal = CrearTextoStat(goStats.transform, "Texto_PuntosTotal", "Total acumulado: 0", fontTexto);

            // Botones: Siguiente nivel, Repetir, Menú (340x88 cada uno, text 34 pt)
            GameObject goBotones = new GameObject("Fila_Botones");
            goBotones.transform.SetParent(panelRoot.transform, false);
            var rectBtn = goBotones.AddComponent<RectTransform>();
            rectBtn.anchorMin = new Vector2(0.5f, 0f);
            rectBtn.anchorMax = new Vector2(0.5f, 0f);
            rectBtn.anchoredPosition = new Vector2(0f, 122f); // por encima del marco interior y del remate
            rectBtn.sizeDelta = new Vector2(1020f, 88f);

            var hlgBtn = goBotones.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 30f;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;

            Button btnSig = CrearBotonMadera(goBotones.transform, "Boton_Siguiente", "Siguiente nivel", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnSig.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 88f);

            Button btnRep = CrearBotonMadera(goBotones.transform, "Boton_Repetir", "Repetir", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnRep.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 88f);

            Button btnMen = CrearBotonMadera(goBotones.transform, "Boton_Menu", "Menú", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnMen.GetComponent<RectTransform>().sizeDelta = new Vector2(320f, 88f);

            SerializedObject so = new SerializedObject(resScript);
            so.FindProperty("textoTitulo").objectReferenceValue = tmpTit;
            so.FindProperty("textoFraseEstrellas").objectReferenceValue = tmpFrase;
            so.FindProperty("bannerNuevoDesbloqueo").objectReferenceValue = goBanner;
            so.FindProperty("textoNuevoDesbloqueo").objectReferenceValue = tmpBan;
            so.FindProperty("spriteEstrellaLlena").objectReferenceValue = spEstrellaLlena;
            so.FindProperty("spriteEstrellaVacia").objectReferenceValue = spEstrellaVacia;

            var propEst = so.FindProperty("imagenesEstrellas");
            propEst.arraySize = 3;
            for (int i = 0; i < 3; i++) propEst.GetArrayElementAtIndex(i).objectReferenceValue = imgEstrellas[i];

            so.FindProperty("textoGemas").objectReferenceValue = tGemas;
            so.FindProperty("textoPlata").objectReferenceValue = tPlata;
            so.FindProperty("textoTiempo").objectReferenceValue = tTiempo;
            so.FindProperty("textoVidas").objectReferenceValue = tVidas;
            so.FindProperty("textoPuntajeNivel").objectReferenceValue = tScore;
            so.FindProperty("textoPuntajeTotal").objectReferenceValue = tTotal;

            so.FindProperty("botonSiguiente").objectReferenceValue = btnSig;
            so.FindProperty("textoBotonSiguiente").objectReferenceValue = btnSig.GetComponentInChildren<TextMeshProUGUI>();
            so.FindProperty("botonRepetir").objectReferenceValue = btnRep;
            so.FindProperty("botonMenu").objectReferenceValue = btnMen;
            so.ApplyModifiedProperties();

            panelRoot.SetActive(false);

            string prefabPath = PrefabUIPath + "ResultsPanel.prefab";
            ElMineroBuilder.DecorarPanelesEn(panelRoot);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            GameObject.DestroyImmediate(panelRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static TextMeshProUGUI CrearTextoStat(Transform padre, string nombre, string texto, TMP_FontAsset font)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = texto;
            tmp.fontSize = 34f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            return tmp;
        }

        // ====================================================================
        // 5. GAME OVER PANEL (Sección 4.2 & Corrección H) - 1200x760
        // ====================================================================
        public static GameObject CrearGameOverPanelPrefab()
        {
            Sprite spPanelMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBotonMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera.png");
            Sprite spBotonMaderaRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Resaltado.png");
            Sprite spBotonMaderaPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Presionado.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject panelRoot = new GameObject("GameOverPanel");
            var rectRoot = panelRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(1200f, 760f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);

            var imgFondo = panelRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var goScript = panelRoot.AddComponent<GameOverPanel>();

            // Título: "¡EL DERRUMBE TE ALCANZÓ!" (PressStart2P 56 pt en una sola línea)
            GameObject goTit = new GameObject("Texto_Titulo");
            goTit.transform.SetParent(panelRoot.transform, false);
            var rectTit = goTit.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -80f);
            rectTit.sizeDelta = new Vector2(1040f, 70f); // 1200 − 2 × 80 de margen: dentro del marco
            var tmpTit = goTit.AddComponent<TextMeshProUGUI>();
            tmpTit.font = fontTitulo;
            tmpTit.fontSize = 56f;
            tmpTit.text = "¡EL DERRUMBE TE ALCANZO!";
            tmpTit.enableAutoSizing = true;
            tmpTit.fontSizeMin = 30f;
            tmpTit.fontSizeMax = 52f;
            tmpTit.textWrappingMode = TextWrappingModes.NoWrap;
            tmpTit.alignment = TextAlignmentOptions.Center;
            tmpTit.color = new Color(0.95f, 0.25f, 0.25f);

            // Subtítulo (PixelifySans 34 pt)
            GameObject goSub = new GameObject("Texto_Subtitulo");
            goSub.transform.SetParent(panelRoot.transform, false);
            var rectSub = goSub.AddComponent<RectTransform>();
            rectSub.anchorMin = new Vector2(0.5f, 1f);
            rectSub.anchorMax = new Vector2(0.5f, 1f);
            rectSub.anchoredPosition = new Vector2(0f, -160f);
            rectSub.sizeDelta = new Vector2(1000f, 50f);
            var tmpSub = goSub.AddComponent<TextMeshProUGUI>();
            tmpSub.font = fontTexto;
            tmpSub.fontSize = 34f;
            tmpSub.alignment = TextAlignmentOptions.Center;
            tmpSub.color = Color.white;
            tmpSub.text = "La mina ha atrapado a Refugio…";

            // Motivo (PixelifySans 34 pt)
            GameObject goMot = new GameObject("Texto_Motivo");
            goMot.transform.SetParent(panelRoot.transform, false);
            var rectMot = goMot.AddComponent<RectTransform>();
            rectMot.anchorMin = new Vector2(0.5f, 0.5f);
            rectMot.anchorMax = new Vector2(0.5f, 0.5f);
            rectMot.anchoredPosition = new Vector2(0f, 30f);
            rectMot.sizeDelta = new Vector2(1000f, 60f);
            var tmpMot = goMot.AddComponent<TextMeshProUGUI>();
            tmpMot.font = fontTexto;
            tmpMot.fontSize = 34f;
            tmpMot.alignment = TextAlignmentOptions.Center;
            tmpMot.color = new Color(1f, 0.85f, 0.4f);
            tmpMot.text = "Te has quedado sin vidas";

            // Estadísticas (PixelifySans 34 pt)
            GameObject goStat = new GameObject("Texto_Estadisticas");
            goStat.transform.SetParent(panelRoot.transform, false);
            var rectStat = goStat.AddComponent<RectTransform>();
            rectStat.anchorMin = new Vector2(0.5f, 0.5f);
            rectStat.anchorMax = new Vector2(0.5f, 0.5f);
            rectStat.anchoredPosition = new Vector2(0f, -50f);
            rectStat.sizeDelta = new Vector2(1000f, 50f);
            var tmpStat = goStat.AddComponent<TextMeshProUGUI>();
            tmpStat.font = fontTexto;
            tmpStat.fontSize = 34f;
            tmpStat.alignment = TextAlignmentOptions.Center;
            tmpStat.color = new Color(0.9f, 0.9f, 0.9f);
            tmpStat.text = "Plata: 0  |  Gemas: 0  |  Puntos: 0";

            // Botones: Reintentar y Menú (340x88 cada uno, text 34 pt)
            GameObject goBotones = new GameObject("Fila_Botones");
            goBotones.transform.SetParent(panelRoot.transform, false);
            var rectBtn = goBotones.AddComponent<RectTransform>();
            rectBtn.anchorMin = new Vector2(0.5f, 0f);
            rectBtn.anchorMax = new Vector2(0.5f, 0f);
            rectBtn.anchoredPosition = new Vector2(0f, 122f);
            rectBtn.sizeDelta = new Vector2(800f, 88f);

            var hlgBtn = goBotones.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 50f;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;

            Button btnReintentar = CrearBotonMadera(goBotones.transform, "Boton_Reintentar", "Reintentar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnReintentar.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 88f);

            Button btnMenu = CrearBotonMadera(goBotones.transform, "Boton_Menu", "Menú Principal", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnMenu.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 88f);

            SerializedObject so = new SerializedObject(goScript);
            so.FindProperty("textoTitulo").objectReferenceValue = tmpTit;
            so.FindProperty("textoSubtitulo").objectReferenceValue = tmpSub;
            so.FindProperty("textoMotivo").objectReferenceValue = tmpMot;
            so.FindProperty("textoEstadisticas").objectReferenceValue = tmpStat;
            so.FindProperty("botonReintentar").objectReferenceValue = btnReintentar;
            so.FindProperty("botonMenu").objectReferenceValue = btnMenu;
            so.ApplyModifiedProperties();

            panelRoot.SetActive(false);

            string prefabPath = PrefabUIPath + "GameOverPanel.prefab";
            ElMineroBuilder.DecorarPanelesEn(panelRoot);
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            GameObject.DestroyImmediate(panelRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // ====================================================================
        // 6. LEVEL START CARD (Sección 3.6 & Corrección F) - 1100x650
        // ====================================================================
        public static GameObject CrearLevelStartCardPrefab()
        {
            Sprite spPanelMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Panel_Madera.png");
            Sprite spBotonMadera = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera.png");
            Sprite spBotonMaderaRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Resaltado.png");
            Sprite spBotonMaderaPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Madera_Presionado.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            GameObject cardRoot = new GameObject("LevelStartCard");
            var rectRoot = cardRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(1400f, 880f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);
            rectRoot.pivot = new Vector2(0.5f, 0.5f);

            var imgFondo = cardRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 1.5625f;

            var cardScript = cardRoot.AddComponent<LevelStartCard>();

            // Título dentro del marco: área superior con margen 56 y alto 100
            GameObject goNombre = new GameObject("Texto_NombreMina");
            goNombre.transform.SetParent(cardRoot.transform, false);
            var rectNom = goNombre.AddComponent<RectTransform>();
            rectNom.anchorMin = new Vector2(0f, 1f);
            rectNom.anchorMax = new Vector2(1f, 1f);
            rectNom.pivot = new Vector2(0.5f, 1f);
            rectNom.anchoredPosition = new Vector2(0f, -56f);
            rectNom.sizeDelta = new Vector2(-112f, 100f);
            var tmpNom = goNombre.AddComponent<TextMeshProUGUI>();
            tmpNom.font = fontTitulo;
            tmpNom.fontSize = 48f;
            tmpNom.enableAutoSizing = true;
            tmpNom.fontSizeMin = 32f;
            tmpNom.fontSizeMax = 48f;
            tmpNom.textWrappingMode = TextWrappingModes.NoWrap;
            tmpNom.alignment = TextAlignmentOptions.Center;
            tmpNom.color = new Color(1f, 0.85f, 0.4f);

            // Cuerpo en dos columnas (Horizontal Layout Group, padding 64, spacing 48)
            GameObject goCuerpo = new GameObject("Cuerpo_DosColumnas");
            goCuerpo.transform.SetParent(cardRoot.transform, false);
            var rectCuerpo = goCuerpo.AddComponent<RectTransform>();
            rectCuerpo.anchorMin = new Vector2(0f, 0f);
            rectCuerpo.anchorMax = new Vector2(1f, 1f);
            // Margen horizontal lo da el padding (64) del HorizontalLayoutGroup:
            // 64 + 380 + 48 + 760 + 64 = 1316 < 1400, así cabe dentro del marco.
            rectCuerpo.offsetMin = new Vector2(0f, 160f);
            rectCuerpo.offsetMax = new Vector2(0f, -170f);

            var hlg = goCuerpo.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(64, 64, 20, 20);
            hlg.spacing = 48f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            // Izquierda: Miniatura (380x216)
            GameObject goMini = new GameObject("Miniatura_Mina");
            goMini.transform.SetParent(goCuerpo.transform, false);
            var rectMini = goMini.AddComponent<RectTransform>();
            rectMini.sizeDelta = new Vector2(380f, 216f);
            var imgMini = goMini.AddComponent<Image>();
            imgMini.preserveAspect = true;
            imgMini.color = Color.white;
            var leMini = goMini.AddComponent<LayoutElement>();
            leMini.minWidth = 380f;
            leMini.preferredWidth = 380f;
            leMini.minHeight = 216f;
            leMini.preferredHeight = 216f;

            // Derecha: Columna de texto (Dato Histórico y Objetivo)
            GameObject goColDer = new GameObject("Columna_Derecha");
            goColDer.transform.SetParent(goCuerpo.transform, false);
            var rectColDer = goColDer.AddComponent<RectTransform>();
            rectColDer.sizeDelta = new Vector2(760f, 440f);
            var leColDer = goColDer.AddComponent<LayoutElement>();
            leColDer.minWidth = 700f;
            leColDer.preferredWidth = 760f;
            leColDer.minHeight = 400f;

            var vlgDer = goColDer.AddComponent<VerticalLayoutGroup>();
            vlgDer.spacing = 24f;
            vlgDer.childAlignment = TextAnchor.MiddleLeft;
            vlgDer.childControlWidth = true;
            vlgDer.childControlHeight = true;
            vlgDer.childForceExpandWidth = true;
            vlgDer.childForceExpandHeight = false;

            // Dato Histórico (Pixelify Sans 30, Word Wrap, Auto Size 24-30)
            GameObject goDato = new GameObject("Texto_DatoHistorico");
            goDato.transform.SetParent(goColDer.transform, false);
            var tmpDato = goDato.AddComponent<TextMeshProUGUI>();
            tmpDato.font = fontTexto;
            tmpDato.fontSize = 30f;
            tmpDato.textWrappingMode = TextWrappingModes.Normal;
            tmpDato.enableAutoSizing = true;
            tmpDato.fontSizeMin = 24f;
            tmpDato.fontSizeMax = 30f;
            tmpDato.alignment = TextAlignmentOptions.TopLeft;
            tmpDato.color = Color.white;
            var leDato = goDato.AddComponent<LayoutElement>();
            leDato.minHeight = 300f;
            leDato.preferredHeight = 330f;
            leDato.flexibleHeight = 1f;
            tmpDato.overflowMode = TextOverflowModes.Truncate;

            // Objetivo (28, cursiva, color #E0C080)
            GameObject goObj = new GameObject("Texto_Objetivo");
            goObj.transform.SetParent(goColDer.transform, false);
            var tmpObj = goObj.AddComponent<TextMeshProUGUI>();
            tmpObj.font = fontTexto;
            tmpObj.fontSize = 28f;
            tmpObj.fontStyle = FontStyles.Italic;
            tmpObj.alignment = TextAlignmentOptions.TopLeft;
            tmpObj.color = new Color(0.88f, 0.75f, 0.50f); // #E0C080
            tmpObj.text = "Objetivo: sube a la siguiente mina antes de que el derrumbe te alcance.";
            tmpObj.textWrappingMode = TextWrappingModes.Normal;
            var leObj = goObj.AddComponent<LayoutElement>();
            leObj.minHeight = 76f;
            leObj.preferredHeight = 76f;

            // Botón "¡A escapar!" (400x88, abajo-centro, pos (0, 48), dentro del marco)
            Button btnEscapar = CrearBotonMadera(cardRoot.transform, "Boton_Escapar", "¡A escapar!", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            var rectBtn = btnEscapar.GetComponent<RectTransform>();
            rectBtn.anchorMin = new Vector2(0.5f, 0f);
            rectBtn.anchorMax = new Vector2(0.5f, 0f);
            rectBtn.pivot = new Vector2(0.5f, 0f);
            rectBtn.anchoredPosition = new Vector2(0f, 48f);
            rectBtn.sizeDelta = new Vector2(400f, 88f);

            SerializedObject so = new SerializedObject(cardScript);
            so.FindProperty("miniaturaMina").objectReferenceValue = imgMini;
            so.FindProperty("textoNombreMina").objectReferenceValue = tmpNom;
            so.FindProperty("textoDatoHistorico").objectReferenceValue = tmpDato;
            so.FindProperty("textoObjetivo").objectReferenceValue = tmpObj;
            so.FindProperty("botonEscapar").objectReferenceValue = btnEscapar;
            so.ApplyModifiedProperties();

            string prefabPath = PrefabUIPath + "LevelStartCard.prefab";
            ElMineroBuilder.DecorarPanelesEn(cardRoot);
            PrefabUtility.SaveAsPrefabAsset(cardRoot, prefabPath);
            GameObject.DestroyImmediate(cardRoot);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // ====================================================================
        // HELPERS UI ESTANDARIZADOS
        // ====================================================================
        public static void ConfigurarSpriteSwap(Button btn, Sprite normal, Sprite res, Sprite pres)
        {
            btn.transition = Selectable.Transition.SpriteSwap;
            SpriteState ss = new SpriteState();
            ss.highlightedSprite = res != null ? res : normal;
            ss.pressedSprite = pres != null ? pres : normal;
            btn.spriteState = ss;
        }

        public static Image CrearIcono(Transform padre, string nombre, Sprite sprite, Vector2 tamano)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = tamano;
            var img = go.AddComponent<Image>();
            img.sprite = sprite;
            img.preserveAspect = true;
            return img;
        }

        public static TextMeshProUGUI CrearTexto(Transform padre, string nombre, string texto, TMP_FontAsset font, float tamano, Vector2 tamanoCaja)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rect = go.AddComponent<RectTransform>();
            rect.sizeDelta = tamanoCaja;
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = texto;
            tmp.fontSize = Mathf.Max(26f, tamano);
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.color = Color.white;
            return tmp;
        }

        public static Slider CrearSlider(Transform padre, string nombre, Sprite spFondo, Sprite spRelleno, Sprite spManija, Vector2 tamano)
        {
            GameObject goSlider = new GameObject(nombre);
            goSlider.transform.SetParent(padre, false);
            var rect = goSlider.AddComponent<RectTransform>();
            rect.sizeDelta = tamano;

            var slider = goSlider.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;

            GameObject goBg = new GameObject("Background");
            goBg.transform.SetParent(goSlider.transform, false);
            var rectBg = goBg.AddComponent<RectTransform>();
            rectBg.anchorMin = new Vector2(0f, 0.2f);
            rectBg.anchorMax = new Vector2(1f, 0.8f);
            rectBg.offsetMin = Vector2.zero;
            rectBg.offsetMax = Vector2.zero;
            var imgBg = goBg.AddComponent<Image>();
            imgBg.sprite = spFondo;
            imgBg.type = Image.Type.Sliced;
            imgBg.pixelsPerUnitMultiplier = 1.5625f;

            GameObject goFillArea = new GameObject("Fill Area");
            goFillArea.transform.SetParent(goSlider.transform, false);
            var rectFa = goFillArea.AddComponent<RectTransform>();
            rectFa.anchorMin = new Vector2(0f, 0.2f);
            rectFa.anchorMax = new Vector2(1f, 0.8f);
            rectFa.offsetMin = new Vector2(6f, 0f);
            rectFa.offsetMax = new Vector2(-20f, 0f);

            GameObject goFill = new GameObject("Fill");
            goFill.transform.SetParent(goFillArea.transform, false);
            var rectFill = goFill.AddComponent<RectTransform>();
            rectFill.offsetMin = Vector2.zero;
            rectFill.offsetMax = Vector2.zero;
            var imgFill = goFill.AddComponent<Image>();
            imgFill.sprite = spRelleno;
            imgFill.type = Image.Type.Sliced;
            imgFill.pixelsPerUnitMultiplier = 1.5625f;

            GameObject goHandleArea = new GameObject("Handle Slide Area");
            goHandleArea.transform.SetParent(goSlider.transform, false);
            var rectHa = goHandleArea.AddComponent<RectTransform>();
            rectHa.anchorMin = Vector2.zero;
            rectHa.anchorMax = Vector2.one;
            rectHa.offsetMin = new Vector2(20f, 0f);
            rectHa.offsetMax = new Vector2(-20f, 0f);

            GameObject goHandle = new GameObject("Handle");
            goHandle.transform.SetParent(goHandleArea.transform, false);
            var rectHandle = goHandle.AddComponent<RectTransform>();
            rectHandle.sizeDelta = new Vector2(60f, 60f); // Handle canónico 60x60
            var imgHandle = goHandle.AddComponent<Image>();
            imgHandle.sprite = spManija;
            imgHandle.preserveAspect = true;

            slider.fillRect = rectFill;
            slider.handleRect = rectHandle;
            slider.targetGraphic = imgHandle;
            slider.direction = Slider.Direction.LeftToRight;

            return slider;
        }

        public static Toggle CrearToggle(Transform padre, string nombre, string etiqueta, Sprite spFondo, Sprite spCheck, TMP_FontAsset font)
        {
            GameObject goToggle = new GameObject(nombre);
            goToggle.transform.SetParent(padre, false);
            var rect = goToggle.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(600f, 64f);

            var toggle = goToggle.AddComponent<Toggle>();

            GameObject goBg = new GameObject("Background");
            goBg.transform.SetParent(goToggle.transform, false);
            var rectBg = goBg.AddComponent<RectTransform>();
            rectBg.anchorMin = new Vector2(0f, 0.5f);
            rectBg.anchorMax = new Vector2(0f, 0.5f);
            rectBg.pivot = new Vector2(0f, 0.5f);
            rectBg.anchoredPosition = new Vector2(10f, 0f);
            rectBg.sizeDelta = new Vector2(64f, 64f); // Toggle canónico 64x64
            var imgBg = goBg.AddComponent<Image>();
            imgBg.sprite = spFondo;
            imgBg.type = Image.Type.Sliced;
            imgBg.pixelsPerUnitMultiplier = 1.5625f;

            GameObject goCheck = new GameObject("Checkmark");
            goCheck.transform.SetParent(goBg.transform, false);
            var rectCheck = goCheck.AddComponent<RectTransform>();
            rectCheck.anchorMin = new Vector2(0.5f, 0.5f);
            rectCheck.anchorMax = new Vector2(0.5f, 0.5f);
            rectCheck.sizeDelta = new Vector2(48f, 48f);
            var imgCheck = goCheck.AddComponent<Image>();
            imgCheck.sprite = spCheck;
            imgCheck.preserveAspect = true;

            GameObject goLabel = new GameObject("Label");
            goLabel.transform.SetParent(goToggle.transform, false);
            var rectLabel = goLabel.AddComponent<RectTransform>();
            rectLabel.anchorMin = new Vector2(0f, 0f);
            rectLabel.anchorMax = new Vector2(1f, 1f);
            rectLabel.offsetMin = new Vector2(90f, 0f);
            rectLabel.offsetMax = Vector2.zero;
            var tmpLabel = goLabel.AddComponent<TextMeshProUGUI>();
            tmpLabel.font = font;
            tmpLabel.text = etiqueta;
            tmpLabel.fontSize = 34f;
            tmpLabel.alignment = TextAlignmentOptions.MidlineLeft;
            tmpLabel.color = Color.white;

            toggle.graphic = imgCheck;
            toggle.targetGraphic = imgBg;

            return toggle;
        }

        public static Button CrearBotonMadera(Transform padre, string nombre, string texto, Sprite normal, Sprite res, Sprite pres, TMP_FontAsset font, float fontSize, float altura)
        {
            GameObject goBtn = new GameObject(nombre);
            goBtn.transform.SetParent(padre, false);
            var rect = goBtn.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(360f, altura);

            var img = goBtn.AddComponent<Image>();
            img.sprite = normal;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1.5625f;

            var btn = goBtn.AddComponent<Button>();
            ConfigurarSpriteSwap(btn, normal, res, pres);

            GameObject goText = new GameObject("Text");
            goText.transform.SetParent(goBtn.transform, false);
            var rectText = goText.AddComponent<RectTransform>();
            rectText.anchorMin = Vector2.zero;
            rectText.anchorMax = Vector2.one;
            rectText.offsetMin = Vector2.zero;
            rectText.offsetMax = Vector2.zero;
            var tmpText = goText.AddComponent<TextMeshProUGUI>();
            tmpText.font = font;
            tmpText.text = texto;
            tmpText.fontSize = Mathf.Max(26f, fontSize);
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;

            goBtn.AddComponent<LayoutElement>().minHeight = altura;

            return btn;
        }

        public static GameObject CrearSubpanelModal(Transform padre, string nombre, Sprite spFondo, Vector2 tamano)
        {
            GameObject goModal = new GameObject(nombre);
            goModal.transform.SetParent(padre, false);
            var rect = goModal.AddComponent<RectTransform>();
            rect.sizeDelta = tamano;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;

            var img = goModal.AddComponent<Image>();
            img.sprite = spFondo;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1.5625f;
            img.color = new Color(0.95f, 0.95f, 0.95f, 0.98f);

            var le = goModal.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            return goModal;
        }
    }
}
