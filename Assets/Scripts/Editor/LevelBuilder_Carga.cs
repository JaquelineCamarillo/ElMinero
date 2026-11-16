using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using ElMinero.UI;

namespace ElMinero.Editor
{
    /// <summary>
    /// Escena "Splash" convertida en pantalla de carga: es lo primero que se ve al abrir
    /// el juego. Fondo de la Mina de Rayas, logo, barra de carga con marco de madera sobre
    /// la que el minero empuja una vagoneta, porcentaje, consejos y crédito de la autora.
    /// No tiene botones ni menú de opciones.
    /// </summary>
    public static partial class LevelBuilder
    {
        public static void ConstruirSplash()
        {
            SpriteImportSetup.ConfigurarSpritesMenu();

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camObj = new GameObject("Main Camera");
            camObj.tag = "MainCamera";
            camObj.transform.position = new Vector3(0f, 0f, -10f);
            var cam = camObj.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.07f, 0.05f, 0.11f);
            camObj.AddComponent<AudioListener>();
            camObj.AddComponent<UniversalAdditionalCameraData>();

            GameObject canvasObj = CrearCanvasBase("Canvas_Carga");
            var ctrl = canvasObj.AddComponent<SplashController>();
            TMP_FontAsset fTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fTexto = FontAssetBuilder.ObtenerPixelifySans();

            // ---------------- Fondo de la mina (las 3 capas del Nivel 1, oscurecidas)
            foreach (string capa in new[] { "Capa0_Lejana", "Capa1_Media", "Capa2_Cercana" })
            {
                Image f = CrearImagenUI(canvasObj.transform, "Fondo_" + capa, $"Assets/Sprites/Fondos/MinaRayas/Fondo_MinaRayas_{capa}.png", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
                Estirar(f.rectTransform);
                f.color = new Color(0.85f, 0.82f, 0.9f);
            }

            // Polvo (lo crea el controlador aquí dentro)
            GameObject zonaMotas = new GameObject("Polvo");
            zonaMotas.transform.SetParent(canvasObj.transform, false);
            var rtMotas = zonaMotas.AddComponent<RectTransform>();
            Estirar(rtMotas);

            Image vineta = CrearImagenUI(canvasObj.transform, "Vineta", RutaMenuUI + "Menu_Vineta.png", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            Estirar(vineta.rectTransform);

            // ---------------- Contenido (aparece con fundido)
            GameObject contenido = new GameObject("Contenido");
            contenido.transform.SetParent(canvasObj.transform, false);
            var rtCont = contenido.AddComponent<RectTransform>();
            Estirar(rtCont);
            var grupo = contenido.AddComponent<CanvasGroup>();
            grupo.blocksRaycasts = false;

            Image brillo = CrearImagenUI(contenido.transform, "Brillo_Logo", RutaMenuUI + "Menu_Resplandor.png", new Vector2(0f, 190f), new Vector2(1150f, 440f), new Vector2(0.5f, 0.5f));
            brillo.color = new Color(1f, 0.78f, 0.35f, 0.2f);

            Sprite logoGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero_Grande.png");
            if (logoGrande == null) logoGrande = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/UI/Logo_ElMinero.png");
            Image logo = CrearImagenUI(contenido.transform, "Logo_ElMinero", null, new Vector2(0f, 190f), new Vector2(873f, 168f), new Vector2(0.5f, 0.5f));
            logo.sprite = logoGrande;
            logo.preserveAspect = true;

            // ---------------- Barra de carga: marco de madera + riel oscuro + relleno dorado
            Image marco = CrearImagenUI(contenido.transform, "Marco_Barra", "Assets/Sprites/UI/Boton_Madera.png", new Vector2(0f, -110f), new Vector2(1040f, 88f), new Vector2(0.5f, 0.5f));
            marco.type = Image.Type.Sliced;
            marco.pixelsPerUnitMultiplier = 1.5625f;

            Image riel = CrearImagenUI(marco.transform, "Riel_Barra", "Assets/Sprites/UI/Slider_Fondo.png", Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            riel.type = Image.Type.Sliced;
            riel.pixelsPerUnitMultiplier = 1.5625f;
            var rtRiel = riel.rectTransform;
            rtRiel.anchorMin = Vector2.zero;
            rtRiel.anchorMax = Vector2.one;
            rtRiel.offsetMin = new Vector2(16f, 16f);
            rtRiel.offsetMax = new Vector2(-16f, -16f);

            Image relleno = CrearImagenUI(riel.transform, "Relleno_Barra", "Assets/Sprites/UI/Slider_Relleno.png", Vector2.zero, Vector2.zero, new Vector2(0f, 0.5f));
            relleno.type = Image.Type.Sliced;
            relleno.pixelsPerUnitMultiplier = 1.5625f;
            var rtRel = relleno.rectTransform;
            rtRel.anchorMin = Vector2.zero;
            rtRel.anchorMax = new Vector2(0f, 1f);
            rtRel.offsetMin = Vector2.zero;
            rtRel.offsetMax = Vector2.zero;

            // Vagoneta y minero que avanzan sobre el marco (base a 44 px del centro del riel)
            Image vagoneta = CrearImagenUI(riel.transform, "Vagoneta", "Assets/Sprites/Entorno/Vagoneta_Deco.png", new Vector2(-504f, 44f), new Vector2(96f, 72f), new Vector2(0.5f, 0f));
            vagoneta.preserveAspect = true;
            Sprite[] caminar = EditorSpriteUtil.Cuadros("Assets/Sprites/Jugador/Minero_Caminar.png");
            Image minero = CrearImagenUI(riel.transform, "Minero", null, new Vector2(-570f, 44f), new Vector2(96f, 96f), new Vector2(0.5f, 0f));
            minero.sprite = caminar.Length > 0 ? caminar[0] : null;
            minero.preserveAspect = true;

            // Textos bajo la barra
            TextMeshProUGUI cargando = CrearTextoCarga(contenido.transform, "Texto_Cargando", fTitulo, "CARGANDO", 26f,
                new Vector2(-520f, -190f), new Vector2(0f, 0.5f), TextAlignmentOptions.MidlineLeft, new Color(1f, 0.84f, 0.43f));
            TextMeshProUGUI porcentaje = CrearTextoCarga(contenido.transform, "Texto_Porcentaje", fTitulo, "0%", 26f,
                new Vector2(520f, -190f), new Vector2(1f, 0.5f), TextAlignmentOptions.MidlineRight, new Color(1f, 0.84f, 0.43f));
            TextMeshProUGUI consejo = CrearTextoCarga(contenido.transform, "Texto_Consejo", fTexto, "", 32f,
                new Vector2(0f, -290f), new Vector2(0.5f, 0.5f), TextAlignmentOptions.Center, new Color(0.95f, 0.9f, 0.8f));
            consejo.rectTransform.sizeDelta = new Vector2(1400f, 50f);

            // Crédito de la autora (abajo)
            Image logoAutora = CrearImagenUI(contenido.transform, "Logo_Autora", "Assets/Sprites/UI/Logo_Autora.png", new Vector2(40f, 28f), new Vector2(72f, 72f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            logoAutora.preserveAspect = true;
            CrearTextoBarra(contenido.transform, "Texto_Autora", fTexto, "Juana Jaqueline Camarillo Oláez  -  UTNG  -  Creación de Videojuegos",
                new Vector2(0f, 0f), new Vector2(128f, 46f), TextAlignmentOptions.BottomLeft, new Color(0.8f, 0.76f, 0.88f, 0.85f));

            // Fundido final a negro
            Image negro = CrearImagenUI(canvasObj.transform, "Fundido_Negro", null, Vector2.zero, Vector2.zero, new Vector2(0.5f, 0.5f));
            Estirar(negro.rectTransform);
            negro.color = new Color(0f, 0f, 0f, 0f);
            negro.transform.SetAsLastSibling();

            var so = new SerializedObject(ctrl);
            so.FindProperty("barraRelleno").objectReferenceValue = relleno;
            so.FindProperty("zonaBarra").objectReferenceValue = rtRiel;
            so.FindProperty("minero").objectReferenceValue = minero.rectTransform;
            so.FindProperty("imagenMinero").objectReferenceValue = minero;
            AsignarCuadros(so, "cuadrosMinero", caminar);
            so.FindProperty("vagoneta").objectReferenceValue = vagoneta.rectTransform;
            so.FindProperty("textoCargando").objectReferenceValue = cargando;
            so.FindProperty("textoPorcentaje").objectReferenceValue = porcentaje;
            so.FindProperty("textoConsejo").objectReferenceValue = consejo;
            so.FindProperty("logo").objectReferenceValue = logo.rectTransform;
            so.FindProperty("grupoContenido").objectReferenceValue = grupo;
            so.FindProperty("fundidoNegro").objectReferenceValue = negro;
            so.FindProperty("spriteMota").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(RutaMenuUI + "Menu_Luciernaga.png");
            so.FindProperty("zonaMotas").objectReferenceValue = rtMotas;
            so.ApplyModifiedProperties();

            CrearSistemasPersistentes();
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/Splash.unity");
            Debug.Log("[LevelBuilder] Escena Splash.unity (pantalla de carga) guardada.");
        }

        private static TextMeshProUGUI CrearTextoCarga(Transform padre, string nombre, TMP_FontAsset font, string texto, float tam,
                                                       Vector2 pos, Vector2 pivote, TextAlignmentOptions alin, Color color)
        {
            GameObject go = new GameObject(nombre);
            go.transform.SetParent(padre, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = pivote;
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(500f, 44f);
            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.font = font;
            tmp.text = texto;
            tmp.fontSize = tam;
            tmp.alignment = alin;
            tmp.color = color;
            tmp.raycastTarget = false;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return tmp;
        }
    }
}
