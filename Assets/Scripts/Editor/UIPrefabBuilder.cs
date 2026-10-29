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
    /// 1920×1080 (match 0.5), escala 9-slice con pixelsPerUnitMultiplier = 0.25, tamaños mínimos de tipografía
    /// mayores o iguales a 26 pt, selector de calidad con flechas (◀  Alta  ▶) y botones de navegación estandarizados.
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
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

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

            // Vertical Layout Group dentro del panel
            var vlg = panelRoot.AddComponent<VerticalLayoutGroup>();
            vlg.padding = new RectOffset(60, 60, 50, 50);
            vlg.spacing = 26f;
            vlg.childAlignment = TextAnchor.UpperCenter;
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

            // Fila Música
            GameObject filaMusica = new GameObject("Fila_Musica");
            filaMusica.transform.SetParent(panelRoot.transform, false);
            var hlgMusica = filaMusica.AddComponent<HorizontalLayoutGroup>();
            hlgMusica.spacing = 24f;
            hlgMusica.childAlignment = TextAnchor.MiddleCenter;
            hlgMusica.childControlWidth = false;
            hlgMusica.childControlHeight = false;
            filaMusica.AddComponent<LayoutElement>().minHeight = 64f;

            CrearIcono(filaMusica.transform, "Icono_Musica", spIconoMusica, new Vector2(56f, 56f));
            CrearTexto(filaMusica.transform, "Label", "Música:", fontTexto, 34f, new Vector2(220f, 50f));
            Slider sliderMusica = CrearSlider(filaMusica.transform, "Slider_Musica", spSliderFondo, spSliderRelleno, spSliderManija, new Vector2(560f, 44f));

            // Fila Efectos
            GameObject filaEfectos = new GameObject("Fila_Efectos");
            filaEfectos.transform.SetParent(panelRoot.transform, false);
            var hlgEfectos = filaEfectos.AddComponent<HorizontalLayoutGroup>();
            hlgEfectos.spacing = 24f;
            hlgEfectos.childAlignment = TextAnchor.MiddleCenter;
            hlgEfectos.childControlWidth = false;
            hlgEfectos.childControlHeight = false;
            filaEfectos.AddComponent<LayoutElement>().minHeight = 64f;

            CrearIcono(filaEfectos.transform, "Icono_Efectos", spIconoEfectos, new Vector2(56f, 56f));
            CrearTexto(filaEfectos.transform, "Label", "Efectos:", fontTexto, 34f, new Vector2(220f, 50f));
            Slider sliderEfectos = CrearSlider(filaEfectos.transform, "Slider_Efectos", spSliderFondo, spSliderRelleno, spSliderManija, new Vector2(560f, 44f));

            // Toggle Música
            Toggle toggleMusica = CrearToggle(panelRoot.transform, "Toggle_Musica", "Activar música de fondo", spToggleFondo, spToggleCheck, fontTexto);
            panelRoot.transform.Find("Toggle_Musica").gameObject.AddComponent<LayoutElement>().minHeight = 64f;

            // Fila Selector de Calidad (Sección D: ◀  Alta  ▶)
            GameObject filaCalidad = new GameObject("Fila_Calidad");
            filaCalidad.transform.SetParent(panelRoot.transform, false);
            var hlgCalidad = filaCalidad.AddComponent<HorizontalLayoutGroup>();
            hlgCalidad.spacing = 24f;
            hlgCalidad.childAlignment = TextAnchor.MiddleCenter;
            hlgCalidad.childControlWidth = false;
            hlgCalidad.childControlHeight = false;
            filaCalidad.AddComponent<LayoutElement>().minHeight = 70f;

            CrearTexto(filaCalidad.transform, "Label", "Calidad:", fontTexto, 34f, new Vector2(240f, 50f));

            // Botón Flecha Izquierda ◀
            Button btnCalidadAnt = CrearBotonMadera(filaCalidad.transform, "Boton_Calidad_Ant", "◀", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 60f);
            btnCalidadAnt.GetComponent<RectTransform>().sizeDelta = new Vector2(72f, 60f);

            // Texto Nombre Calidad
            GameObject goTxtCalidad = new GameObject("Texto_Calidad");
            goTxtCalidad.transform.SetParent(filaCalidad.transform, false);
            var rectTc = goTxtCalidad.AddComponent<RectTransform>();
            rectTc.sizeDelta = new Vector2(300f, 50f);
            var tmpCalidad = goTxtCalidad.AddComponent<TextMeshProUGUI>();
            tmpCalidad.font = fontTexto;
            tmpCalidad.fontSize = 34f;
            tmpCalidad.alignment = TextAlignmentOptions.Center;
            tmpCalidad.text = "Alta";
            tmpCalidad.color = new Color(1f, 0.9f, 0.5f);

            // Botón Flecha Derecha ▶
            Button btnCalidadSig = CrearBotonMadera(filaCalidad.transform, "Boton_Calidad_Sig", "▶", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 60f);
            btnCalidadSig.GetComponent<RectTransform>().sizeDelta = new Vector2(72f, 60f);

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

            // Subpanel de Controles (Modal superpuesto 1100x700)
            GameObject panelControles = CrearSubpanelModal(panelRoot.transform, "Subpanel_Controles", spPanelMadera, new Vector2(1100f, 700f));
            GameObject contTexto = new GameObject("Texto_Controles");
            contTexto.transform.SetParent(panelControles.transform, false);
            var rectCt = contTexto.AddComponent<RectTransform>();
            rectCt.anchorMin = Vector2.zero;
            rectCt.anchorMax = Vector2.one;
            rectCt.offsetMin = new Vector2(40f, 110f);
            rectCt.offsetMax = new Vector2(-40f, -40f);
            var tmpCt = contTexto.AddComponent<TextMeshProUGUI>();
            tmpCt.font = fontTexto;
            tmpCt.fontSize = 32f;
            tmpCt.text = "<b><size=44>CONTROLES DEL JUEGO</size></b>\n\n" +
                         "<b>TECLADO:</b>\n" +
                         "• <b>A / D</b> o <b>Flechas</b> : Moverse a los lados\n" +
                         "• <b>Espacio</b> : Saltar (mantén para mayor altura)\n" +
                         "• <b>Escape / P</b> : Menú de Pausa\n\n" +
                         "<b>GAMEPAD:</b>\n" +
                         "• <b>Stick Izquierdo / D-Pad</b> : Moverse\n" +
                         "• <b>Botón Sur (A/X)</b> : Saltar\n" +
                         "• <b>Start / Menú</b> : Pausa\n\n" +
                         "<b>PANTALLA TÁCTIL:</b>\n" +
                         "• Botones virtuales en pantalla.";
            tmpCt.color = Color.white;

            Button btnCerrarControles = CrearBotonMadera(panelControles.transform, "Boton_CerrarControles", "Cerrar", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            var rectBcc = btnCerrarControles.GetComponent<RectTransform>();
            rectBcc.anchorMin = new Vector2(0.5f, 0f);
            rectBcc.anchorMax = new Vector2(0.5f, 0f);
            rectBcc.anchoredPosition = new Vector2(0f, 50f);
            rectBcc.sizeDelta = new Vector2(360f, 88f);
            panelControles.SetActive(false);

            // Modal Confirmar Borrado (900x500)
            GameObject modalBorrar = CrearSubpanelModal(panelRoot.transform, "Modal_ConfirmarBorrado", spPanelMadera, new Vector2(900f, 500f));
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
            so.FindProperty("botonCalidadAnterior").objectReferenceValue = btnCalidadAnt;
            so.FindProperty("botonCalidadSiguiente").objectReferenceValue = btnCalidadSig;
            so.FindProperty("textoCalidad").objectReferenceValue = tmpCalidad;
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
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
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
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

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
            tmpTitulo.text = "CRÉDITOS";
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
                           "<b>TIPOGRAFÍAS</b>\n" +
                           "Press Start 2P (CodeMan38, SIL OFL 1.1)\n" +
                           "Pixelify Sans (Stefie Justprince, SIL OFL 1.1)\n\n" +
                           "<i>¡Gracias por jugar!</i>";

            SerializedObject so = new SerializedObject(credScript);
            so.FindProperty("botonVolver").objectReferenceValue = btnVolver;
            so.FindProperty("contenidoCreditos").objectReferenceValue = rectContent;
            so.ApplyModifiedProperties();

            string prefabPath = PrefabUIPath + "CreditsPanel.prefab";
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
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

            var selScript = panelRoot.AddComponent<LevelSelectPanel>();

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

            // Título: "SELECCIÓN DE NIVELES" (PressStart2P 60 pt)
            GameObject goTitulo = new GameObject("Titulo");
            goTitulo.transform.SetParent(panelRoot.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -60f);
            rectTit.sizeDelta = new Vector2(1200f, 70f);
            var tmpTitulo = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTitulo.text = "SELECCIÓN DE NIVELES";
            tmpTitulo.fontSize = 60f;
            tmpTitulo.font = fontTitulo;
            tmpTitulo.alignment = TextAlignmentOptions.Center;
            tmpTitulo.color = Color.white;

            // Fila de 3 tarjetas (420x640 cada una, espaciado 40 px)
            GameObject goFila = new GameObject("Fila_Tarjetas");
            goFila.transform.SetParent(panelRoot.transform, false);
            var rectFila = goFila.AddComponent<RectTransform>();
            rectFila.anchorMin = new Vector2(0.5f, 0.5f);
            rectFila.anchorMax = new Vector2(0.5f, 0.5f);
            rectFila.anchoredPosition = new Vector2(0f, -40f);
            rectFila.sizeDelta = new Vector2(1380f, 650f);

            var hlg = goFila.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 40f;
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childControlWidth = false;
            hlg.childControlHeight = false;

            // Construir 3 tarjetas canónicas
            var t1 = CrearTarjetaNivel(goFila.transform, 1, "Nivel1_MinaRayas", "1 · Mina Rayas", spMiniN1, spBotonNivel, spBotonNivelRes, spBotonNivelPres, spCandado, spEstrellaVacia, fontTitulo, fontTexto);
            var t2 = CrearTarjetaNivel(goFila.transform, 2, "Nivel2_ElNopal", "2 · El Nopal", spMiniN2, spBotonNivel, spBotonNivelRes, spBotonNivelPres, spCandado, spEstrellaVacia, fontTitulo, fontTexto);
            var t3 = CrearTarjetaNivel(goFila.transform, 3, "Nivel3_LaValenciana", "3 · La Valenciana", spMiniN3, spBotonNivel, spBotonNivelRes, spBotonNivelPres, spCandado, spEstrellaVacia, fontTitulo, fontTexto);

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
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

            var btn = goTarjeta.AddComponent<Button>();
            ConfigurarSpriteSwap(btn, spBoton, spBotonRes, spBotonPres);

            // Miniatura (360x200)
            GameObject goMini = new GameObject("Miniatura");
            goMini.transform.SetParent(goTarjeta.transform, false);
            var rectMini = goMini.AddComponent<RectTransform>();
            rectMini.anchorMin = new Vector2(0.5f, 1f);
            rectMini.anchorMax = new Vector2(0.5f, 1f);
            rectMini.anchoredPosition = new Vector2(0f, -140f);
            rectMini.sizeDelta = new Vector2(360f, 200f);
            var imgMini = goMini.AddComponent<Image>();
            imgMini.sprite = miniatura;
            imgMini.preserveAspect = true;

            // Título de la Mina (34 pt)
            GameObject goTitulo = new GameObject("Texto_TituloMina");
            goTitulo.transform.SetParent(goTarjeta.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -40f);
            rectTit.sizeDelta = new Vector2(380f, 60f);
            var tmpTit = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTit.font = fontTexto;
            tmpTit.fontSize = 34f;
            tmpTit.fontStyle = FontStyles.Bold;
            tmpTit.alignment = TextAlignmentOptions.Center;
            tmpTit.color = Color.white;
            tmpTit.text = nombreMina;

            // Fila de 3 Estrellas (56x56 cada una)
            GameObject goEstrellas = new GameObject("Fila_Estrellas");
            goEstrellas.transform.SetParent(goTarjeta.transform, false);
            var rectEst = goEstrellas.AddComponent<RectTransform>();
            rectEst.anchorMin = new Vector2(0.5f, 0.45f);
            rectEst.anchorMax = new Vector2(0.5f, 0.45f);
            rectEst.anchoredPosition = new Vector2(0f, -20f);
            rectEst.sizeDelta = new Vector2(240f, 60f);

            var hlgEst = goEstrellas.AddComponent<HorizontalLayoutGroup>();
            hlgEst.spacing = 18f;
            hlgEst.childAlignment = TextAnchor.MiddleCenter;

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
                iconos[i] = ie;
            }

            // Récords (30 pt)
            GameObject goRec = new GameObject("Texto_Record");
            goRec.transform.SetParent(goTarjeta.transform, false);
            var rectRec = goRec.AddComponent<RectTransform>();
            rectRec.anchorMin = new Vector2(0.5f, 0f);
            rectRec.anchorMax = new Vector2(0.5f, 0f);
            rectRec.anchoredPosition = new Vector2(0f, 120f);
            rectRec.sizeDelta = new Vector2(380f, 50f);
            var tmpRec = goRec.AddComponent<TextMeshProUGUI>();
            tmpRec.font = fontTexto;
            tmpRec.fontSize = 30f;
            tmpRec.alignment = TextAlignmentOptions.Center;
            tmpRec.color = new Color(0.95f, 0.9f, 0.75f);
            tmpRec.text = "Sin récord aún";

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
            rectIc.anchoredPosition = new Vector2(0f, 30f);
            rectIc.sizeDelta = new Vector2(80f, 80f);
            var imgLock = goIconoCandado.AddComponent<Image>();
            imgLock.sprite = spCandado;
            imgLock.preserveAspect = true;

            GameObject goEstado = new GameObject("Texto_Estado");
            goEstado.transform.SetParent(goTarjeta.transform, false);
            var rectEstad = goEstado.AddComponent<RectTransform>();
            rectEstad.anchorMin = new Vector2(0.5f, 0f);
            rectEstad.anchorMax = new Vector2(0.5f, 0f);
            rectEstad.anchoredPosition = new Vector2(0f, 50f);
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
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

            var resScript = panelRoot.AddComponent<ResultsPanel>();

            // Título: "¡NIVEL COMPLETADO!" (PressStart2P 60 pt)
            GameObject goTitulo = new GameObject("Texto_Titulo");
            goTitulo.transform.SetParent(panelRoot.transform, false);
            var rectTit = goTitulo.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -65f);
            rectTit.sizeDelta = new Vector2(1100f, 70f);
            var tmpTit = goTitulo.AddComponent<TextMeshProUGUI>();
            tmpTit.font = fontTitulo;
            tmpTit.fontSize = 60f;
            tmpTit.text = "¡NIVEL COMPLETADO!";
            tmpTit.alignment = TextAlignmentOptions.Center;
            tmpTit.color = new Color(1f, 0.85f, 0.4f);

            // Frase Estrellas (PixelifySans 34 pt)
            GameObject goFrase = new GameObject("Texto_FraseEstrellas");
            goFrase.transform.SetParent(panelRoot.transform, false);
            var rectFrase = goFrase.AddComponent<RectTransform>();
            rectFrase.anchorMin = new Vector2(0.5f, 1f);
            rectFrase.anchorMax = new Vector2(0.5f, 1f);
            rectFrase.anchoredPosition = new Vector2(0f, -145f);
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
            TextMeshProUGUI tVidas = CrearTextoStat(goStats.transform, "Texto_Vidas", "Vidas restantes: 3 ♥", fontTexto);
            TextMeshProUGUI tScore = CrearTextoStat(goStats.transform, "Texto_PuntosNivel", "Puntos nivel: 0", fontTexto);
            TextMeshProUGUI tTotal = CrearTextoStat(goStats.transform, "Texto_PuntosTotal", "Total acumulado: 0", fontTexto);

            // Botones: Siguiente nivel, Repetir, Menú (340x88 cada uno, text 34 pt)
            GameObject goBotones = new GameObject("Fila_Botones");
            goBotones.transform.SetParent(panelRoot.transform, false);
            var rectBtn = goBotones.AddComponent<RectTransform>();
            rectBtn.anchorMin = new Vector2(0.5f, 0f);
            rectBtn.anchorMax = new Vector2(0.5f, 0f);
            rectBtn.anchoredPosition = new Vector2(0f, 70f);
            rectBtn.sizeDelta = new Vector2(1080f, 88f);

            var hlgBtn = goBotones.AddComponent<HorizontalLayoutGroup>();
            hlgBtn.spacing = 30f;
            hlgBtn.childAlignment = TextAnchor.MiddleCenter;

            Button btnSig = CrearBotonMadera(goBotones.transform, "Boton_Siguiente", "Siguiente nivel", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnSig.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 88f);

            Button btnRep = CrearBotonMadera(goBotones.transform, "Boton_Repetir", "Repetir", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnRep.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 88f);

            Button btnMen = CrearBotonMadera(goBotones.transform, "Boton_Menu", "Menú", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 88f);
            btnMen.GetComponent<RectTransform>().sizeDelta = new Vector2(340f, 88f);

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
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

            var goScript = panelRoot.AddComponent<GameOverPanel>();

            // Título: "¡EL DERRUMBE TE ALCANZÓ!" (PressStart2P 56 pt en una sola línea)
            GameObject goTit = new GameObject("Texto_Titulo");
            goTit.transform.SetParent(panelRoot.transform, false);
            var rectTit = goTit.AddComponent<RectTransform>();
            rectTit.anchorMin = new Vector2(0.5f, 1f);
            rectTit.anchorMax = new Vector2(0.5f, 1f);
            rectTit.anchoredPosition = new Vector2(0f, -70f);
            rectTit.sizeDelta = new Vector2(1150f, 70f);
            var tmpTit = goTit.AddComponent<TextMeshProUGUI>();
            tmpTit.font = fontTitulo;
            tmpTit.fontSize = 56f;
            tmpTit.text = "¡EL DERRUMBE TE ALCANZÓ!";
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
            rectBtn.anchoredPosition = new Vector2(0f, 70f);
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
            rectRoot.sizeDelta = new Vector2(1100f, 650f);
            rectRoot.anchorMin = new Vector2(0.5f, 0.5f);
            rectRoot.anchorMax = new Vector2(0.5f, 0.5f);

            var imgFondo = cardRoot.AddComponent<Image>();
            imgFondo.sprite = spPanelMadera;
            imgFondo.type = Image.Type.Sliced;
            imgFondo.pixelsPerUnitMultiplier = 0.25f;

            var cardScript = cardRoot.AddComponent<LevelStartCard>();

            // Miniatura (400x220)
            GameObject goMini = new GameObject("Miniatura_Mina");
            goMini.transform.SetParent(cardRoot.transform, false);
            var rectMini = goMini.AddComponent<RectTransform>();
            rectMini.anchorMin = new Vector2(0.5f, 1f);
            rectMini.anchorMax = new Vector2(0.5f, 1f);
            rectMini.anchoredPosition = new Vector2(0f, -140f);
            rectMini.sizeDelta = new Vector2(400f, 220f);
            var imgMini = goMini.AddComponent<Image>();
            imgMini.preserveAspect = true;
            // No fondo blanco: si el sprite aún no está asignado, ocultar el color
            imgMini.color = Color.white;

            // Nombre Mina (PressStart2P 48 pt)
            GameObject goNombre = new GameObject("Texto_NombreMina");
            goNombre.transform.SetParent(cardRoot.transform, false);
            var rectNom = goNombre.AddComponent<RectTransform>();
            rectNom.anchorMin = new Vector2(0.5f, 1f);
            rectNom.anchorMax = new Vector2(0.5f, 1f);
            rectNom.anchoredPosition = new Vector2(0f, -40f);
            rectNom.sizeDelta = new Vector2(1000f, 60f);
            var tmpNom = goNombre.AddComponent<TextMeshProUGUI>();
            tmpNom.font = fontTitulo;
            tmpNom.fontSize = 48f;
            tmpNom.alignment = TextAlignmentOptions.Center;
            tmpNom.color = new Color(1f, 0.85f, 0.4f);

            // Dato Histórico (PixelifySans 32 pt)
            GameObject goDato = new GameObject("Texto_DatoHistorico");
            goDato.transform.SetParent(cardRoot.transform, false);
            var rectDato = goDato.AddComponent<RectTransform>();
            rectDato.anchorMin = new Vector2(0.5f, 0.5f);
            rectDato.anchorMax = new Vector2(0.5f, 0.5f);
            rectDato.anchoredPosition = new Vector2(0f, -30f);
            rectDato.sizeDelta = new Vector2(980f, 120f);
            var tmpDato = goDato.AddComponent<TextMeshProUGUI>();
            tmpDato.font = fontTexto;
            tmpDato.fontSize = 32f;
            tmpDato.alignment = TextAlignmentOptions.Center;
            tmpDato.color = Color.white;

            // Objetivo (PixelifySans 30 pt)
            GameObject goObj = new GameObject("Texto_Objetivo");
            goObj.transform.SetParent(cardRoot.transform, false);
            var rectObj = goObj.AddComponent<RectTransform>();
            rectObj.anchorMin = new Vector2(0.5f, 0.5f);
            rectObj.anchorMax = new Vector2(0.5f, 0.5f);
            rectObj.anchoredPosition = new Vector2(0f, -120f);
            rectObj.sizeDelta = new Vector2(980f, 50f);
            var tmpObj = goObj.AddComponent<TextMeshProUGUI>();
            tmpObj.font = fontTexto;
            tmpObj.fontSize = 30f;
            tmpObj.fontStyle = FontStyles.Italic;
            tmpObj.alignment = TextAlignmentOptions.Center;
            tmpObj.color = new Color(0.9f, 0.9f, 0.9f);

            // Botón ¡A escapar! (340x80, text 34 pt)
            Button btnEscapar = CrearBotonMadera(cardRoot.transform, "Boton_Escapar", "¡A escapar!", spBotonMadera, spBotonMaderaRes, spBotonMaderaPres, fontTexto, 34f, 80f);
            var rectBtn = btnEscapar.GetComponent<RectTransform>();
            rectBtn.anchorMin = new Vector2(0.5f, 0f);
            rectBtn.anchorMax = new Vector2(0.5f, 0f);
            rectBtn.anchoredPosition = new Vector2(0f, 50f);
            rectBtn.sizeDelta = new Vector2(340f, 80f);

            SerializedObject so = new SerializedObject(cardScript);
            so.FindProperty("miniaturaMina").objectReferenceValue = imgMini;
            so.FindProperty("textoNombreMina").objectReferenceValue = tmpNom;
            so.FindProperty("textoDatoHistorico").objectReferenceValue = tmpDato;
            so.FindProperty("textoObjetivo").objectReferenceValue = tmpObj;
            so.FindProperty("botonEscapar").objectReferenceValue = btnEscapar;
            so.ApplyModifiedProperties();

            string prefabPath = PrefabUIPath + "LevelStartCard.prefab";
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
            imgBg.pixelsPerUnitMultiplier = 0.25f;

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
            imgFill.pixelsPerUnitMultiplier = 0.25f;

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
            imgBg.pixelsPerUnitMultiplier = 0.25f;

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
            img.pixelsPerUnitMultiplier = 0.25f;

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
            img.pixelsPerUnitMultiplier = 0.25f;
            img.color = new Color(0.95f, 0.95f, 0.95f, 0.98f);

            var le = goModal.AddComponent<LayoutElement>();
            le.ignoreLayout = true;

            return goModal;
        }
    }
}
