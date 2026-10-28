using UnityEditor;
using UnityEngine;

namespace ElMinero.Editor
{
    /// <summary>
    /// AssetPostprocessor que asegura que todos los sprites y tiles del juego
    /// se importen con PPU = 16, FilterMode.Point y sin compresión.
    /// </summary>
    public class ElMineroSpritePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Sprites/") && !assetPath.StartsWith("Assets/Tiles/"))
                return;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
        }
    }
}
