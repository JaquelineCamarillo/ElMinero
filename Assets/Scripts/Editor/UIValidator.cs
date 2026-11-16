using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

namespace ElMinero.Editor
{
    /// <summary>
    /// Validador de interfaz de usuario según la especificación de la Sección A7.
    /// Recorre prefabs de UI y Canvas de todas las escenas, verificando:
    /// 1. Elementos dentro del área interior del panel padre (-48px por lado).
    /// 2. Texto sin desbordamiento (WordWrap, AutoSize min 24).
    /// 3. Fuentes Pixelify Sans o Press Start 2P (nunca LiberationSans).
    /// 4. Caracteres compatibles (sin ▶ ◀ ♥ ✓ ★).
    /// Genera capturas de pantalla de la interfaz en Docs/Capturas/.
    /// </summary>
    public static class UIValidator
    {
        private static readonly string[] EscenasUI = new string[]
        {
            "Assets/Scenes/Splash.unity",
            "Assets/Scenes/MenuPrincipal.unity",
            "Assets/Scenes/Intro.unity",
            "Assets/Scenes/Nivel1_MinaRayas.unity",
            "Assets/Scenes/Nivel2_ElNopal.unity",
            "Assets/Scenes/Nivel3_LaValenciana.unity",
            "Assets/Scenes/Final_Templo.unity"
        };

        private static readonly string[] PrefabsUI = new string[]
        {
            "Assets/Prefabs/UI/OptionsPanel.prefab",
            "Assets/Prefabs/UI/LevelSelectPanel.prefab",
            "Assets/Prefabs/UI/CreditsPanel.prefab",
            "Assets/Prefabs/UI/LevelStartCard.prefab",
            "Assets/Prefabs/UI/PauseMenu.prefab",
            "Assets/Prefabs/UI/ResultsPanel.prefab",
            "Assets/Prefabs/UI/GameOverPanel.prefab",
            "Assets/Prefabs/UI/HUD.prefab"
        };

        [MenuItem("Herramientas/El Minero/Validar interfaz", priority = 25)]
        public static bool ValidarInterfaz()
        {
            int advertencias = 0;
            int correcciones = 0;

            TMP_FontAsset fontTitulo = FontAssetBuilder.ObtenerPressStart2P();
            TMP_FontAsset fontTexto = FontAssetBuilder.ObtenerPixelifySans();

            Debug.Log("[UIValidator] === INICIANDO VALIDACIÓN DE INTERFAZ ===");

            // 1. Validar Prefabs de UI
            foreach (string prefabPath in PrefabsUI)
            {
                if (!File.Exists(prefabPath)) continue;

                GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
                if (root == null) continue;

                bool modificado = false;
                ValidarJerarquiaUI(root, prefabPath, fontTitulo, fontTexto, ref advertencias, ref correcciones, ref modificado);

                if (modificado)
                {
                    PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                }
                PrefabUtility.UnloadPrefabContents(root);
            }

            // 2. Validar Escenas
            foreach (string escenaPath in EscenasUI)
            {
                if (!File.Exists(escenaPath)) continue;

                var scene = EditorSceneManager.OpenScene(escenaPath, OpenSceneMode.Single);
                if (!scene.IsValid()) continue;

                bool escenaModificada = false;
                var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (var canvas in canvases)
                {
                    ValidarJerarquiaUI(canvas.gameObject, escenaPath, fontTitulo, fontTexto, ref advertencias, ref correcciones, ref escenaModificada);
                }

                if (escenaModificada)
                {
                    EditorSceneManager.SaveScene(scene);
                }
            }

            Debug.Log($"[UIValidator] Validación completada. Correcciones automáticas: {correcciones}, Advertencias restantes: {advertencias}");
            return advertencias == 0;
        }

        private static void ValidarJerarquiaUI(
            GameObject root,
            string contexto,
            TMP_FontAsset fontTitulo,
            TMP_FontAsset fontTexto,
            ref int advertencias,
            ref int correcciones,
            ref bool modificado)
        {
            var textos = root.GetComponentsInChildren<TMP_Text>(true);
            foreach (var t in textos)
            {
                // A) Verificar y corregir fuentes (nunca LiberationSans)
                if (t.font == null || t.font.name.Contains("Liberation"))
                {
                    bool esTitulo = t.name.Contains("Titulo") || t.fontSize >= 44f || t.gameObject.name.Contains("Title");
                    t.font = esTitulo ? fontTitulo : fontTexto;
                    correcciones++;
                    modificado = true;
                }

                // B) Filtrar caracteres no compatibles (▶ ◀ ♥ ✓ ★ •)
                if (!string.IsNullOrEmpty(t.text))
                {
                    string original = t.text;
                    string filtrado = original
                        .Replace("•", "-")
                        .Replace("▶", "")
                        .Replace("◀", "")
                        .Replace("♥", "")
                        .Replace("✓", "")
                        .Replace("★", "");

                    if (filtrado != original)
                    {
                        t.text = filtrado;
                        correcciones++;
                        modificado = true;
                    }

                    // Validar si algún otro caracter falta en la fuente.
                    // - Se quitan las etiquetas de texto enriquecido (<b>, <size=44>...), que no se dibujan.
                    // - Las fuentes son dinámicas: un carácter que todavía no está en el atlas se intenta
                    //   agregar desde el archivo .ttf (tryAddCharacter). Solo se avisa si la fuente de verdad no lo tiene.
                    string visible = System.Text.RegularExpressions.Regex.Replace(t.text, "<[^>]*>", "");
                    if (t.font != null && !t.font.HasCharacters(visible, out uint[] faltantesU, true, true))
                    {
                        if (faltantesU != null && faltantesU.Length > 0)
                        {
                            foreach (char c in faltantesU.Distinct().Select(u => (char)u))
                            {
                                if (char.IsWhiteSpace(c) || char.IsControl(c)) continue;
                                Debug.LogWarning($"[UIValidator] Carácter no soportado '{c}' (\\u{(int)c:X4}) en {contexto} -> {t.gameObject.name}");
                                advertencias++;
                            }
                        }
                    }
                }

                // C) Desbordamiento de texto: asegurar auto-size o word-wrap
                if (t.textWrappingMode == TextWrappingModes.NoWrap && t.text.Length > 25 && !t.gameObject.name.Contains("Version"))
                {
                    t.textWrappingMode = TextWrappingModes.Normal;
                    correcciones++;
                    modificado = true;
                }

                if (!t.enableAutoSizing && t.fontSize >= 32f)
                {
                    t.enableAutoSizing = true;
                    t.fontSizeMin = Mathf.Min(24f, t.fontSize);
                    t.fontSizeMax = t.fontSize;
                    correcciones++;
                    modificado = true;
                }

                t.ForceMeshUpdate();

                // D) Contención en el marco padre (no aplica a contenido con desplazamiento, como los créditos)
                if (t.GetComponentInParent<RectMask2D>(true) != null) continue;
                RectTransform rectT = t.rectTransform;
                RectTransform panelPadre = BuscarPanelPadre(rectT);
                if (panelPadre != null && panelPadre != rectT)
                {
                    if (!EstaDentroDelMarco(rectT, panelPadre, 48f))
                    {
                        // Si está fuera pero es un botón o texto manejado por LayoutGroup, el LayoutGroup lo ajustará
                        var layout = panelPadre.GetComponent<LayoutGroup>();
                        if (layout == null)
                        {
                            // Intentar contener reduciendo o ajustando anchoredPosition
                            AjustarContencion(rectT, panelPadre, 48f);
                            correcciones++;
                            modificado = true;
                        }
                    }
                }
            }

            var botones = root.GetComponentsInChildren<Button>(true);
            foreach (var b in botones)
            {
                var txt = b.GetComponentInChildren<TMP_Text>(true);
                if (txt != null)
                {
                    if (txt.font == null || txt.font.name.Contains("Liberation"))
                    {
                        txt.font = fontTexto;
                        correcciones++;
                        modificado = true;
                    }
                }

                RectTransform rectB = b.GetComponent<RectTransform>();
                RectTransform panelPadre = BuscarPanelPadre(rectB);
                if (panelPadre != null && panelPadre != rectB)
                {
                    var layout = panelPadre.GetComponent<LayoutGroup>();
                    if (layout == null && !EstaDentroDelMarco(rectB, panelPadre, 48f))
                    {
                        AjustarContencion(rectB, panelPadre, 48f);
                        correcciones++;
                        modificado = true;
                    }
                }
            }
        }

        private static RectTransform BuscarPanelPadre(RectTransform rt)
        {
            Transform p = rt.parent;
            while (p != null)
            {
                if (p.GetComponent<Canvas>() != null) return null;
                var img = p.GetComponent<Image>();
                if (img != null && (img.type == Image.Type.Sliced || p.name.Contains("Panel") || p.name.Contains("Card")))
                {
                    return p.GetComponent<RectTransform>();
                }
                p = p.parent;
            }
            return null;
        }

        private static bool EstaDentroDelMarco(RectTransform hijo, RectTransform panel, float margen)
        {
            Vector3[] hijoCorners = new Vector3[4];
            Vector3[] panelCorners = new Vector3[4];
            hijo.GetWorldCorners(hijoCorners);
            panel.GetWorldCorners(panelCorners);

            float margenWorld = margen * panel.lossyScale.x;
            float minX = panelCorners[0].x + margenWorld;
            float maxX = panelCorners[2].x - margenWorld;
            float minY = panelCorners[0].y + margenWorld;
            float maxY = panelCorners[2].y - margenWorld;

            // Tolerancia de 8px en escala de mundo para evitar falsos positivos por layout
            float tol = 8f * panel.lossyScale.x;

            return (hijoCorners[0].x >= minX - tol) &&
                   (hijoCorners[2].x <= maxX + tol) &&
                   (hijoCorners[0].y >= minY - tol) &&
                   (hijoCorners[2].y <= maxY + tol);
        }

        private static void AjustarContencion(RectTransform hijo, RectTransform panel, float margen)
        {
            Vector2 sizePanel = panel.rect.size;
            float maxW = Mathf.Max(100f, sizePanel.x - margen * 2f);
            float maxH = Mathf.Max(50f, sizePanel.y - margen * 2f);

            Vector2 sz = hijo.sizeDelta;
            if (sz.x > maxW) sz.x = maxW;
            if (sz.y > maxH) sz.y = maxH;
            hijo.sizeDelta = sz;
        }

        // ====================================================================
        // CAPTURA DE PANTALLAS DE INTERFAZ (Sección A7)
        // ====================================================================
        [MenuItem("Herramientas/El Minero/Capturar pantallas de UI", priority = 26)]
        public static void CapturarPantallasUI()
        {
            string outDir = "Docs/Capturas";
            if (!Directory.Exists(outDir))
            {
                Directory.CreateDirectory(outDir);
            }

            Debug.Log("[UIValidator] Generando capturas de pantalla de la interfaz...");

            // 1. Menú Principal, Opciones, Controles
            string menuScene = "Assets/Scenes/MenuPrincipal.unity";
            if (File.Exists(menuScene))
            {
                var scene = EditorSceneManager.OpenScene(menuScene, OpenSceneMode.Single);
                var cam = Camera.main;
                if (cam != null)
                {
                    RenderizarCamara(cam, Path.Combine(outDir, "Menu.png"));

                    // Opciones
                    var optionsPanel = UnityEngine.Object.FindAnyObjectByType<ElMinero.UI.OptionsPanel>(FindObjectsInactive.Include);
                    if (optionsPanel != null)
                    {
                        optionsPanel.gameObject.SetActive(true);
                        RenderizarCamara(cam, Path.Combine(outDir, "Opciones.png"));

                        // Controles
                        Transform subCtrl = optionsPanel.transform.Find("Subpanel_Controles");
                        if (subCtrl != null)
                        {
                            subCtrl.gameObject.SetActive(true);
                            RenderizarCamara(cam, Path.Combine(outDir, "Controles.png"));
                            subCtrl.gameObject.SetActive(false);
                        }
                        optionsPanel.gameObject.SetActive(false);
                    }
                }
            }

            // 2. Tarjetas de Inicio de Nivel (Rayas, El Nopal, La Valenciana)
            CapturarTarjetasNivel(outDir);

            // 3. Resultados y Game Over
            CapturarPanelesFinales(outDir);

            AssetDatabase.Refresh();
            Debug.Log($"[UIValidator] ¡Capturas guardadas exitosamente en {outDir}!");
        }

        private static void CapturarTarjetasNivel(string outDir)
        {
            string escena = "Assets/Scenes/Nivel1_MinaRayas.unity";
            if (!File.Exists(escena)) return;

            var scene = EditorSceneManager.OpenScene(escena, OpenSceneMode.Single);
            var cam = Camera.main;
            if (cam == null) return;

            var card = UnityEngine.Object.FindAnyObjectByType<ElMinero.UI.LevelStartCard>(FindObjectsInactive.Include);
            if (card == null) return;

            // Nivel 1: Mina Rayas
            Sprite min1 = Resources.Load<Sprite>("Miniaturas/Miniatura_MinaRayas");
            card.ConfigurarYMostrar("Nivel1_MinaRayas", "1 · Mina de Rayas", min1,
                "Mina de Rayas. Descubierta hacia 1550 por el arriero Juan Rayas. Con unos 400 m de profundidad, llegó a considerarse de las más profundas de su tiempo. Los mineros bajaban y subían por su tiro con un malacate, cables y poleas.",
                "Objetivo: sube a la siguiente mina antes de que el derrumbe te alcance.");
            RenderizarCamara(cam, Path.Combine(outDir, "Tarjeta_Nivel1.png"));

            // Nivel 2: El Nopal
            Sprite min2 = Resources.Load<Sprite>("Miniaturas/Miniatura_ElNopal");
            card.ConfigurarYMostrar("Nivel2_ElNopal", "2 · El Nopal", min2,
                "Mina El Nopal. Se trabaja desde 1868 y de ella se ha sacado plata, oro, zinc y cobre. En sus galerías hay una antigua capilla, y hoy la Universidad de Guanajuato la usa como mina-escuela.",
                "Objetivo: sube a la siguiente mina antes de que el derrumbe te alcance.");
            RenderizarCamara(cam, Path.Combine(outDir, "Tarjeta_Nivel2.png"));

            // Nivel 3: La Valenciana
            Sprite min3 = Resources.Load<Sprite>("Miniaturas/Miniatura_LaValenciana");
            card.ConfigurarYMostrar("Nivel3_LaValenciana", "3 · La Valenciana", min3,
                "La Valenciana. En el siglo XVIII fue de las minas de plata más productivas del mundo. Su dueño, Antonio de Obregón, fue nombrado Conde de la Valenciana en 1780 y mandó construir el Templo de San Cayetano junto a la mina. En 1810 sus mineros se unieron a la toma de la Alhóndiga de Granaditas, y en 1817, durante la guerra, la maquinaria fue incendiada.",
                "Objetivo: sube a la siguiente mina antes de que el derrumbe te alcance.");
            RenderizarCamara(cam, Path.Combine(outDir, "Tarjeta_Nivel3.png"));

            card.gameObject.SetActive(false);
        }

        private static void CapturarPanelesFinales(string outDir)
        {
            string escena = "Assets/Scenes/Nivel1_MinaRayas.unity";
            if (!File.Exists(escena)) return;

            var scene = EditorSceneManager.OpenScene(escena, OpenSceneMode.Single);
            var cam = Camera.main;
            if (cam == null) return;

            var results = UnityEngine.Object.FindAnyObjectByType<ElMinero.UI.ResultsPanel>(FindObjectsInactive.Include);
            // El panel de resultados guarda récords y desbloqueos: se respaldan y se restauran después de la captura
            string[] clavesInt = { "NivelDesbloqueado", "MejorPuntaje_Nivel1_MinaRayas", "Estrellas_Nivel1_MinaRayas", "Mineral_Nivel1_MinaRayas" };
            var respaldo = new Dictionary<string, int>();
            foreach (var k in clavesInt) if (PlayerPrefs.HasKey(k)) respaldo[k] = PlayerPrefs.GetInt(k);
            bool habiaTiempo = PlayerPrefs.HasKey("MejorTiempo_Nivel1_MinaRayas");
            float tiempoPrevio = PlayerPrefs.GetFloat("MejorTiempo_Nivel1_MinaRayas", 0f);
            if (results != null)
            {
                results.MostrarResultados("Nivel1_MinaRayas", 1, 2, 2, true, 6, 6, 45f, 180f, 3, 350);
                RenderizarCamara(cam, Path.Combine(outDir, "Resultados.png"));
                results.gameObject.SetActive(false);
            }

            var gameOver = UnityEngine.Object.FindAnyObjectByType<ElMinero.UI.GameOverPanel>(FindObjectsInactive.Include);
            if (gameOver != null)
            {
                gameOver.MostrarGameOver("¡El tiempo se ha agotado!", 2, 4, 120);
                RenderizarCamara(cam, Path.Combine(outDir, "GameOver.png"));
                gameOver.gameObject.SetActive(false);
            }

            foreach (var k in clavesInt)
            {
                if (respaldo.TryGetValue(k, out int v)) PlayerPrefs.SetInt(k, v); else PlayerPrefs.DeleteKey(k);
            }
            if (habiaTiempo) PlayerPrefs.SetFloat("MejorTiempo_Nivel1_MinaRayas", tiempoPrevio); else PlayerPrefs.DeleteKey("MejorTiempo_Nivel1_MinaRayas");
            PlayerPrefs.Save();
            Time.timeScale = 1f;
        }

        public static void RenderizarCamara(Camera cam, string outputPath)
        {
            if (cam == null) return;

            // Los Canvas en modo Overlay no salen en cam.Render(): se pasan
            // temporalmente a Screen Space - Camera para la captura.
            var canvases = UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            var modos = new List<(Canvas c, RenderMode m, Camera cam, float d)>();
            foreach (var c in canvases)
            {
                if (!c.isRootCanvas) continue;
                modos.Add((c, c.renderMode, c.worldCamera, c.planeDistance));
                if (c.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    c.renderMode = RenderMode.ScreenSpaceCamera;
                    c.worldCamera = cam;
                    c.planeDistance = 1f;
                }
            }
            Canvas.ForceUpdateCanvases();

            int width = 1920;
            int height = 1080;
            RenderTexture rt = new RenderTexture(width, height, 24);
            var prevTarget = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();

            RenderTexture.active = rt;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = null;
            foreach (var x in modos)
            {
                x.c.renderMode = x.m;
                x.c.worldCamera = x.cam;
                x.c.planeDistance = x.d;
            }
            UnityEngine.Object.DestroyImmediate(rt);

            byte[] bytes = tex.EncodeToPNG();
            UnityEngine.Object.DestroyImmediate(tex);

            File.WriteAllBytes(outputPath, bytes);
        }
    }
}
