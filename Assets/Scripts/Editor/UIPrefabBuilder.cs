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
    /// Construye paneles estandarizados según las especificaciones del GDD y paquete v2.
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
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[El Minero] Prefabs de UI creados exitosamente.");
        }

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
            Sprite spIconoFlechaAbajo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Iconos/Icono_FlechaAbajo.png");

            Sprite spBotonVolver = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver.png");
            Sprite spBotonVolverRes = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Resaltado.png");
            Sprite spBotonVolverPres = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Boton_Volver_Presionado.png");

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            // 1. Contenedor principal OptionsPanel
            GameObject panelRoot = new GameObject("OptionsPanel");
            var rectRoot = panelRoot.AddComponent<RectTransform>();
            rectRoot.sizeDelta = new Vector2(460f, 480f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);
            rectRoot.pivot = new Vector2(0.5f, 0.5f);

            var imgFondo = panelRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;

            var optionsScript = panelRoot.AddComponent<OptionsPanel>();

            // Botón Volver (Redondo 32x32, esquina superior izquierda con ignoreLayout)
            GameObject goVolver = new GameObject("Boton_Volver");
            goVolver.transform.SetParent(panelRoot.transform, false);
            var rectVolver = goVolver.AddComponent<RectTransform>();
            rectVolver.anchorMin = new Vector2(0f, 1f);
            rectVolver.anchorMax = new Vector2(0f, 1f);
            rectVolver.pivot = new Vector2(0f, 1f);
            rectVolver.anchoredPosition = new Vector2(16f, -16f);
            rectVolver.sizeDelta = new Vector2(36f, 36f);

            var leVolver = goVolver.AddComponent<LayoutElement>();
            leVolver.ignoreLayout = true;

            var imgVolver = goVolver.AddComponent<Image>();
            imgVolver.sprite = spBotonVolver;
            var btnVolver = goVolver.AddComponent<Button>();
            ConfigurarSpriteSwap(btnVolver, spBotonVolver, spBotonVolverRes, spBotonVolverPres);

            // Vertical Layout Group dentro del panel
            var vlg = panelRoot.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(24, 24, 24, 24);
            vlg.spacing = 14f;
            vlg.childAlignment = TextAnchor.UpperCenter;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            // Título: "OPCIONES" (Press Start 2P, 24px)
            GameObject goTitulo = new GameObject("Titulo");
            goTitulo.transform.SetParent(panelRoot.transform, false);
            var tmpTitulo = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTitulo.text = "OPCIONES";
            tmpTitulo.fontSize = 22f;
            tmpTitulo.font = fontTitulo;
            tmpTitulo.alignment = TextAlignmentOptions.Center;
            tmpTitulo.color = Color.white;
            goTitulo.AddComponent<LayoutElement>().minHeight = 35f;

            // Fila Música (Horizontal: Icono + Texto + Slider)
            GameObject filaMusica = new GameObject("Fila_Musica");
            filaMusica.transform.SetParent(panelRoot.transform, false);
            var hlgMusica = filaMusica.AddComponent<HorizontalLayoutGroup>();
            hlgMusica.spacing = 10f;
            hlgMusica.childAlignment = TextAnchor.MiddleLeft;
            hlgMusica.childControlWidth = false;
            hlgMusica.childControlHeight = false;
            hlgMusica.childForceExpandWidth = false;
            filaMusica.AddComponent<LayoutElement>().minHeight = 36f;

            CrearIcono(filaMusica.transform, "Icono_Musica", spIconoMusica, new Vector2(24f, 24f));
            CrearTexto(filaMusica.transform, "Label", "Música:", fontTexto, 18f, new Vector2(75f, 30f));
            Slider sliderMusica = CrearSlider(filaMusica.transform, "Slider_Musica", spSliderFondo, spSliderRelleno, spSliderManija, new Vector2(260f, 26f));

            // Fila Efectos (Horizontal: Icono + Texto + Slider)
            GameObject filaEfectos = new GameObject("Fila_Efectos");
            filaEfectos.transform.SetParent(panelRoot.transform, false);
            var hlgEfectos = filaEfectos.AddComponent<HorizontalLayoutGroup>();
            hlgEfectos.spacing = 10f;
            hlgEfectos.childAlignment = TextAnchor.MiddleLeft;
            hlgEfectos.childControlWidth = false;
            hlgEfectos.childControlHeight = false;
            hlgEfectos.childForceExpandWidth = false;
            filaEfectos.AddComponent<LayoutElement>().minHeight = 36f;

            CrearIcono(filaEfectos.transform, "Icono_Efectos", spIconoEfectos, new Vector2(24f, 24f));
            CrearTexto(filaEfectos.transform, "Label", "Efectos:", fontTexto, 18f, new Vector2(75f, 30f));
            Slider sliderEfectos = CrearSlider(filaEfectos.transform, "Slider_Efectos", spSliderFondo, spSliderRelleno, spSliderManija, new Vector2(260f, 26f));

            // Toggle Música
            Toggle toggleMusica = CrearToggle(panelRoot.transform, "Toggle_Musica", "Activar Música", spToggleFondo, spToggleCheck, fontTexto);
            panelRoot.transform.Find("Toggle_Musica").gameObject.AddComponent<LayoutElement>().minHeight = 32f;

            // Fila Calidad Gráfica (Dropdown)
            GameObject filaCalidad = new GameObject("Fila_Calidad");
            filaCalidad.transform.SetParent(panelRoot.transform, false);
            var hlgCalidad = filaCalidad.AddComponent<HorizontalLayoutGroup>();
            hlgCalidad.spacing = 10f;
            hlgCalidad.childAlignment = TextAnchor.MiddleLeft;
            hlgCalidad.childControlWidth = false;
            hlgCalidad.childControlHeight = false;
            filaCalidad.AddComponent<LayoutElement>().minHeight = 36f;

            CrearTexto(filaCalidad.transform, "Label", "Calidad:", fontTexto, 18f, new Vector2(90f, 30f));
            TMP_Dropdown dropdownCalidad = CrearDropdown(filaCalidad.transform, "Dropdown_Calidad", spBotonMadera, spIconoFlechaAbajo, fontTexto, new Vector2(260f, 36f));

            // Botón Controles
            Button btnControles = CrearBotonMadera(panelRoot.transform, "Boton_Controles", "Ver Controles", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 18f, 38f);

            // Botón Borrar Progreso
            Button btnBorrar = CrearBotonMadera(panelRoot.transform, "Boton_BorrarProgreso", "Borrar Progreso", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 16f, 38f);

            // Subpanel de Controles (Modal superpuesto)
            GameObject panelControles = CrearSubpanelModal(panelRoot.transform, "Subpanel_Controles", spPanelMadera, new Vector2(430f, 430f));
            GameObject contTexto = new GameObject("Texto_Controles");
            contTexto.transform.SetParent(panelControles.transform, false);
            var rectCt = contTexto.AddComponent<RectTransform>();
            rectCt.anchorMin = Vector2.zero;
            rectCt.anchorMax = Vector2.one;
            rectCt.offsetMin = new Vector2(20f, 60f);
            rectCt.offsetMax = new Vector2(-20f, -20f);
            var tmpCt = contTexto.AddComponent<TextMeshProUGUI>();
            tmpCt.font = fontTexto;
            tmpCt.fontSize = 15f;
            tmpCt.text = "<b><size=18>CONTROLES DEL JUEGO</size></b>\n\n" +
                         "<b>TECLADO:</b>\n" +
                         "• <b>A / D</b> o <b>Flechas</b> : Moverse\n" +
                         "• <b>Espacio</b> : Saltar (mantén para más altura)\n" +
                         "• <b>Escape / P</b> : Pausa\n\n" +
                         "<b>GAMEPAD:</b>\n" +
                         "• <b>Stick / D-Pad</b> : Moverse\n" +
                         "• <b>Botón Sur (A/X)</b> : Saltar\n" +
                         "• <b>Start</b> : Pausa\n\n" +
                         "<b>PANTALLA TÁCTIL:</b>\n" +
                         "• Botones virtuales izquierda / derecha / salto.";
            tmpCt.color = Color.white;

            Button btnCerrarControles = CrearBotonMadera(panelControles.transform, "Boton_CerrarControles", "Cerrar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 16f, 36f);
            var rectBcc = btnCerrarControles.GetComponent<RectTransform>();
            rectBcc.anchorMin = new Vector2(0.5f, 0f);
            rectBcc.anchorMax = new Vector2(0.5f, 0f);
            rectBcc.anchoredPosition = new Vector2(0f, 30f);
            rectBcc.sizeDelta = new Vector2(160f, 36f);
            panelControles.SetActive(false);

            // Modal Confirmar Borrado
            GameObject modalBorrar = CrearSubpanelModal(panelRoot.transform, "Modal_ConfirmarBorrado", spPanelMadera, new Vector2(380f, 220f));
            GameObject textoMb = new GameObject("Texto_Confirmar");
            textoMb.transform.SetParent(modalBorrar.transform, false);
            var rectMb = textoMb.AddComponent<RectTransform>();
            rectMb.anchorMin = new Vector2(0f, 0.35f);
            rectMb.anchorMax = new Vector2(1f, 1f);
            rectMb.offsetMin = new Vector2(20f, 0f);
            rectMb.offsetMax = new Vector2(-20f, -15f);
            var tmpMb = textoMb.AddComponent<TextMeshProUGUI>();
            tmpMb.font = fontTexto;
            tmpMb.fontSize = 16f;
            tmpMb.alignment = TextAlignmentOptions.Center;
            tmpMb.text = "¿Seguro que deseas borrar el progreso?\nSe perderán las estrellas y récords.";
            tmpMb.color = Color.white;

            Button btnSi = CrearBotonMadera(modalBorrar.transform, "Boton_Si", "Sí, borrar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 15f, 36f);
            var rectSi = btnSi.GetComponent<RectTransform>();
            rectSi.anchorMin = new Vector2(0.28f, 0.2f);
            rectSi.anchorMax = new Vector2(0.28f, 0.2f);
            rectSi.sizeDelta = new Vector2(130f, 36f);

            Button btnNo = CrearBotonMadera(modalBorrar.transform, "Boton_No", "Cancelar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 15f, 36f);
            var rectNo = btnNo.GetComponent<RectTransform>();
            rectNo.anchorMin = new Vector2(0.72f, 0.2f);
            rectNo.anchorMax = new Vector2(0.72f, 0.2f);
            rectNo.sizeDelta = new Vector2(130f, 36f);
            modalBorrar.SetActive(false);

            // Aviso Progreso Borrado
            GameObject avisoBorrado = new GameObject("Aviso_Borrado");
            avisoBorrado.transform.SetParent(panelRoot.transform, false);
            var rectAv = avisoBorrado.AddComponent<RectTransform>();
            rectAv.sizeDelta = new Vector2(320f, 40f);
            rectAv.anchoredPosition = new Vector2(0f, -180f);
            var tmpAv = avisoBorrado.AddComponent<TextMeshProUGUI>();
            tmpAv.font = fontTexto;
            tmpAv.fontSize = 16f;
            tmpAv.alignment = TextAlignmentOptions.Center;
            tmpAv.text = "¡Progreso restablecido!";
            tmpAv.color = new Color(1f, 0.85f, 0.4f);
            avisoBorrado.AddComponent<LayoutElement>().ignoreLayout = true;
            avisoBorrado.SetActive(false);

            // Asignar referencias en SerializedObject de OptionsPanel
            SerializedObject so = new SerializedObject(optionsScript);
            so.FindProperty("sliderMusica").objectReferenceValue = sliderMusica;
            so.FindProperty("sliderEfectos").objectReferenceValue = sliderEfectos;
            so.FindProperty("toggleMusica").objectReferenceValue = toggleMusica;
            so.FindProperty("dropdownCalidad").objectReferenceValue = dropdownCalidad;
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
            PrefabUtility.SaveAsPrefabAsset(panelRoot, prefabPath);
            GameObject.DestroyImmediate(panelRoot);
            Debug.Log($"[UIPrefabBuilder] Creado prefab {prefabPath}");

            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        // ====================================================================
        // HELPERS UI
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
            tmp.fontSize = tamano;
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

            // Background
            GameObject goBg = new GameObject("Background");
            goBg.transform.SetParent(goSlider.transform, false);
            var rectBg = goBg.AddComponent<RectTransform>();
            rectBg.anchorMin = new Vector2(0f, 0.25f);
            rectBg.anchorMax = new Vector2(1f, 0.75f);
            rectBg.offsetMin = Vector2.zero;
            rectBg.offsetMax = Vector2.zero;
            var imgBg = goBg.AddComponent<Image>();
            imgBg.sprite = spFondo;
            imgBg.type = Image.Type.Sliced;

            // Fill Area
            GameObject goFillArea = new GameObject("Fill Area");
            goFillArea.transform.SetParent(goSlider.transform, false);
            var rectFa = goFillArea.AddComponent<RectTransform>();
            rectFa.anchorMin = new Vector2(0f, 0.25f);
            rectFa.anchorMax = new Vector2(1f, 0.75f);
            rectFa.offsetMin = new Vector2(5f, 0f);
            rectFa.offsetMax = new Vector2(-15f, 0f);

            GameObject goFill = new GameObject("Fill");
            goFill.transform.SetParent(goFillArea.transform, false);
            var rectFill = goFill.AddComponent<RectTransform>();
            rectFill.offsetMin = Vector2.zero;
            rectFill.offsetMax = Vector2.zero;
            var imgFill = goFill.AddComponent<Image>();
            imgFill.sprite = spRelleno;
            imgFill.type = Image.Type.Sliced;

            // Handle Slide Area
            GameObject goHandleArea = new GameObject("Handle Slide Area");
            goHandleArea.transform.SetParent(goSlider.transform, false);
            var rectHa = goHandleArea.AddComponent<RectTransform>();
            rectHa.anchorMin = Vector2.zero;
            rectHa.anchorMax = Vector2.one;
            rectHa.offsetMin = new Vector2(10f, 0f);
            rectHa.offsetMax = new Vector2(-10f, 0f);

            GameObject goHandle = new GameObject("Handle");
            goHandle.transform.SetParent(goHandleArea.transform, false);
            var rectHandle = goHandle.AddComponent<RectTransform>();
            rectHandle.sizeDelta = new Vector2(20f, 0f);
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
            rect.sizeDelta = new Vector2(300f, 32f);

            var toggle = goToggle.AddComponent<Toggle>();

            // Background
            GameObject goBg = new GameObject("Background");
            goBg.transform.SetParent(goToggle.transform, false);
            var rectBg = goBg.AddComponent<RectTransform>();
            rectBg.anchorMin = new Vector2(0f, 0.5f);
            rectBg.anchorMax = new Vector2(0f, 0.5f);
            rectBg.anchoredPosition = new Vector2(15f, 0f);
            rectBg.sizeDelta = new Vector2(26f, 26f);
            var imgBg = goBg.AddComponent<Image>();
            imgBg.sprite = spFondo;

            // Checkmark
            GameObject goCheck = new GameObject("Checkmark");
            goCheck.transform.SetParent(goBg.transform, false);
            var rectCheck = goCheck.AddComponent<RectTransform>();
            rectCheck.anchorMin = Vector2.zero;
            rectCheck.anchorMax = Vector2.one;
            rectCheck.offsetMin = Vector2.zero;
            rectCheck.offsetMax = Vector2.zero;
            var imgCheck = goCheck.AddComponent<Image>();
            imgCheck.sprite = spCheck;

            // Label
            GameObject goLabel = new GameObject("Label");
            goLabel.transform.SetParent(goToggle.transform, false);
            var rectLabel = goLabel.AddComponent<RectTransform>();
            rectLabel.anchorMin = new Vector2(0f, 0f);
            rectLabel.anchorMax = new Vector2(1f, 1f);
            rectLabel.offsetMin = new Vector2(40f, 0f);
            rectLabel.offsetMax = Vector2.zero;
            var tmpLabel = goLabel.AddComponent<TextMeshProUGUI>();
            tmpLabel.font = font;
            tmpLabel.text = etiqueta;
            tmpLabel.fontSize = 18f;
            tmpLabel.alignment = TextAlignmentOptions.MidlineLeft;
            tmpLabel.color = Color.white;

            toggle.graphic = imgCheck;
            toggle.targetGraphic = imgBg;

            return toggle;
        }

        public static TMP_Dropdown CrearDropdown(Transform padre, string nombre, Sprite spFondo, Sprite spFlecha, TMP_FontAsset font, Vector2 tamano)
        {
            GameObject goDropdown = new GameObject(nombre);
            goDropdown.transform.SetParent(padre, false);
            var rect = goDropdown.AddComponent<RectTransform>();
            rect.sizeDelta = tamano;

            var imgBg = goDropdown.AddComponent<Image>();
            imgBg.sprite = spFondo;
            imgBg.type = Image.Type.Sliced;

            var dropdown = goDropdown.AddComponent<TMP_Dropdown>();

            // Caption Text
            GameObject goLabel = new GameObject("Caption_Text");
            goLabel.transform.SetParent(goDropdown.transform, false);
            var rectLabel = goLabel.AddComponent<RectTransform>();
            rectLabel.anchorMin = Vector2.zero;
            rectLabel.anchorMax = Vector2.one;
            rectLabel.offsetMin = new Vector2(15f, 0f);
            rectLabel.offsetMax = new Vector2(-35f, 0f);
            var tmpLabel = goLabel.AddComponent<TextMeshProUGUI>();
            tmpLabel.font = font;
            tmpLabel.fontSize = 18f;
            tmpLabel.alignment = TextAlignmentOptions.MidlineLeft;
            tmpLabel.color = Color.white;

            // Flecha
            GameObject goArrow = new GameObject("Arrow");
            goArrow.transform.SetParent(goDropdown.transform, false);
            var rectArrow = goArrow.AddComponent<RectTransform>();
            rectArrow.anchorMin = new Vector2(1f, 0.5f);
            rectArrow.anchorMax = new Vector2(1f, 0.5f);
            rectArrow.anchoredPosition = new Vector2(-18f, 0f);
            rectArrow.sizeDelta = new Vector2(20f, 20f);
            var imgArrow = goArrow.AddComponent<Image>();
            imgArrow.sprite = spFlecha;
            imgArrow.preserveAspect = true;

            dropdown.captionText = tmpLabel;
            dropdown.targetGraphic = imgBg;

            return dropdown;
        }

        public static Button CrearBotonMadera(Transform padre, string nombre, string texto, Sprite normal, Sprite res, Sprite pres, TMP_FontAsset font, float fontSize, float altura)
        {
            GameObject goBtn = new GameObject(nombre);
            goBtn.transform.SetParent(padre, false);
            var rect = goBtn.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(380f, altura);

            var img = goBtn.AddComponent<Image>();
            img.sprite = normal;
            img.type = Image.Type.Sliced;

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
            tmpText.fontSize = fontSize;
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
            img.color = new Color(0.95f, 0.95f, 0.95f, 0.98f);

            var le = goModal.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            return goModal;
        }
    }
}
