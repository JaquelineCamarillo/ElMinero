using System.IO;
using UnityEngine;
using UnityEditor;
using TMPro;

namespace ElMinero.Editor
{
    public static class FontAssetBuilder
    {
        [MenuItem("Herramientas/El Minero/Crear TMP Font Assets", priority = 15)]
        public static void GenerarFontAssets()
        {
            GenerarFontAssetPara("Assets/Fuentes/PressStart2P-Regular.ttf", "Assets/Fuentes/PressStart2P SDF.asset");
            GenerarFontAssetPara("Assets/Fuentes/PixelifySans.ttf", "Assets/Fuentes/PixelifySans SDF.asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[El Minero] Font Assets generados correctamente.");
        }

        private static void GenerarFontAssetPara(string ttfPath, string assetPath)
        {
            if (File.Exists(assetPath))
            {
                Debug.Log($"[FontAssetBuilder] El asset {assetPath} ya existe.");
                return;
            }

            Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                Debug.LogWarning($"[FontAssetBuilder] No se pudo cargar la fuente en {ttfPath}");
                return;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 32, 4, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512);
            if (fontAsset != null)
            {
                AssetDatabase.CreateAsset(fontAsset, assetPath);
                Debug.Log($"[FontAssetBuilder] Creado {assetPath}");
            }
        }

        public static TMP_FontAsset ObtenerPressStart2P()
        {
            TMP_FontAsset f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fuentes/PressStart2P SDF.asset");
            if (f == null)
            {
                GenerarFontAssets();
                f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fuentes/PressStart2P SDF.asset");
            }
            return f;
        }

        public static TMP_FontAsset ObtenerPixelifySans()
        {
            TMP_FontAsset f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fuentes/PixelifySans SDF.asset");
            if (f == null)
            {
                GenerarFontAssets();
                f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fuentes/PixelifySans SDF.asset");
            }
            return f;
        }
    }
}
