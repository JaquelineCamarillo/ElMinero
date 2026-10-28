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
            // Forzar regeneración limpia si no tiene atlas
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(assetPath);
            if (existing != null && existing.atlasTextures != null && existing.atlasTextures.Length > 0 && existing.atlasTextures[0] != null)
            {
                return;
            }

            if (File.Exists(assetPath))
            {
                AssetDatabase.DeleteAsset(assetPath);
            }

            Font font = AssetDatabase.LoadAssetAtPath<Font>(ttfPath);
            if (font == null)
            {
                Debug.LogWarning($"[FontAssetBuilder] No se pudo cargar la fuente en {ttfPath}");
                return;
            }

            TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(font, 36, 4, UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA, 512, 512);
            if (fontAsset != null)
            {
                AssetDatabase.CreateAsset(fontAsset, assetPath);

                if (fontAsset.atlasTextures != null)
                {
                    for (int i = 0; i < fontAsset.atlasTextures.Length; i++)
                    {
                        if (fontAsset.atlasTextures[i] != null)
                        {
                            fontAsset.atlasTextures[i].name = fontAsset.name + " Atlas " + i;
                            AssetDatabase.AddObjectToAsset(fontAsset.atlasTextures[i], fontAsset);
                        }
                    }
                }
                else if (fontAsset.atlasTexture != null)
                {
                    fontAsset.atlasTexture.name = fontAsset.name + " Atlas";
                    AssetDatabase.AddObjectToAsset(fontAsset.atlasTexture, fontAsset);
                }

                if (fontAsset.material != null)
                {
                    fontAsset.material.name = fontAsset.name + " Material";
                    AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
                }

                EditorUtility.SetDirty(fontAsset);
                Debug.Log($"[FontAssetBuilder] Creado {assetPath} con texturas y material asociados.");
            }
        }

        public static TMP_FontAsset ObtenerPressStart2P()
        {
            GenerarFontAssets();
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fuentes/PressStart2P SDF.asset");
        }

        public static TMP_FontAsset ObtenerPixelifySans()
        {
            GenerarFontAssets();
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Fuentes/PixelifySans SDF.asset");
        }
    }
}
