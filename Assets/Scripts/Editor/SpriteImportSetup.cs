using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.U2D.Sprites;
using UnityEngine.Tilemaps;

namespace ElMinero.Editor
{
    /// <summary>
    /// Configura automáticamente la importación de todos los sprites del proyecto:
    /// PPU = 16, Filter = Point, Compression = None, rebanado por celda con pivots exactos,
    /// 9-slice en UI, y genera los assets Tile para los Tilemaps.
    /// </summary>
    public static class SpriteImportSetup
    {
        private const int StandardPPU = 16;

        [MenuItem("Herramientas/El Minero/3. Configurar importación de sprites y crear Tile Palettes", priority = 30)]
        public static void ConfigurarTodosLosSprites()
        {
            EditorUtility.DisplayProgressBar("El Minero", "Configurando importación de sprites...", 0.1f);

            try
            {
                // 1. Sprites del Jugador (celdas de 32x32, pivot Bottom Center: 0.5, 0.0)
                ConfigurarHojasJugador();

                // 2. Sprites de Entorno
                ConfigurarSpritesEntorno();

                // 3. Sprites de Plataformas
                ConfigurarSpritesPlataformas();

                // 4. Sprites de Obstáculos
                ConfigurarSpritesObstaculos();

                // 5. Sprites de Coleccionables
                ConfigurarSpritesColeccionables();

                // 6. Sprites de UI (9-slice en paneles y botones)
                ConfigurarSpritesUI();

                // 7. Fondos Parallax (FullRect, Repeat)
                ConfigurarFondos();

                // 8. Tilesets y generación de Tile Assets
                ConfigurarTilesetsYCrearTiles();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[El Minero] ¡Todos los sprites y tilesets configurados correctamente con PPU = 16 y filtros Point!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ====================================================================
        // 1. JUGADOR (32x32, Pivot Bottom-Center: 0.5, 0.0)
        // ====================================================================
        private static void ConfigurarHojasJugador()
        {
            Vector2 pivotBottomCenter = new Vector2(0.5f, 0.0f);

            // Sprite sheets del minero
            SliceGrid("Assets/Sprites/Jugador/Minero_Idle.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Caminar.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Saltar.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Caer.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Golpe.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Morir.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Celebrar.png", 32, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
            SliceGrid("Assets/Sprites/Jugador/Minero_Retrato.png", 32, 32, new Vector2(0.5f, 0.5f), SpriteAlignment.Center);
        }

        // ====================================================================
        // 2. ENTORNO
        // ====================================================================
        private static void ConfigurarSpritesEntorno()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Viga_Horizontal.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Poste_Vertical.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Marco_Soporte.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Vagoneta_Deco.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Salida_Tunel.png", new Vector2(0.5f, 0.0f));
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Salida_Superficie.png", new Vector2(0.5f, 0.0f));

            // Antorcha: 3 cuadros de 16x16
            SliceGrid("Assets/Sprites/Entorno/Antorcha.png", 16, 16, pivotCenter, SpriteAlignment.Center);
        }

        // ====================================================================
        // 3. PLATAFORMAS
        // ====================================================================
        private static void ConfigurarSpritesPlataformas()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);
            ConfigurarSingleSprite("Assets/Sprites/Plataformas/Plataforma_Fija.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Plataformas/Plataforma_Movil.png", pivotCenter);

            // Plataforma quebradiza: 3 cuadros de 48x8
            SliceGrid("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png", 48, 8, pivotCenter, SpriteAlignment.Center);
        }

        // ====================================================================
        // 4. OBSTÁCULOS
        // ====================================================================
        private static void ConfigurarSpritesObstaculos()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);
            SliceGrid("Assets/Sprites/Obstaculos/Roca.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Obstaculos/Polvo.png", 8, 8, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Obstaculos/Advertencia_Sombra.png", 16, 8, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Obstaculos/Advertencia_PolvoTecho.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            ConfigurarSingleSprite("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Obstaculos/Picos.png", pivotCenter);
        }

        // ====================================================================
        // 5. COLECCIONABLES
        // ====================================================================
        private static void ConfigurarSpritesColeccionables()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);
            SliceGrid("Assets/Sprites/Coleccionables/Veta_Plata.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Coleccionables/Gema_Amatista.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            // Checkpoint: 2 cuadros de 16x32 (apagado y encendido)
            SliceGrid("Assets/Sprites/Coleccionables/Checkpoint.png", 16, 32, new Vector2(0.5f, 0.0f), SpriteAlignment.BottomCenter);
        }

        // ====================================================================
        // 6. UI
        // ====================================================================
        private static void ConfigurarSpritesUI()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);

            // Iconos y botones simples
            string[] singleUi = new string[]
            {
                "Assets/Sprites/UI/Boton_Derecha.png",
                "Assets/Sprites/UI/Boton_Derecha_Presionado.png",
                "Assets/Sprites/UI/Boton_Izquierda.png",
                "Assets/Sprites/UI/Boton_Izquierda_Presionado.png",
                "Assets/Sprites/UI/Boton_Pausa.png",
                "Assets/Sprites/UI/Boton_Pausa_Presionado.png",
                "Assets/Sprites/UI/Boton_Saltar.png",
                "Assets/Sprites/UI/Boton_Saltar_Presionado.png",
                "Assets/Sprites/UI/Corazon_Lleno.png",
                "Assets/Sprites/UI/Corazon_Vacio.png",
                "Assets/Sprites/UI/Icono_Gema.png",
                "Assets/Sprites/UI/Icono_Plata.png",
                "Assets/Sprites/UI/Icono_Reloj.png",
                "Assets/Sprites/UI/Logo_ElMinero.png"
            };

            foreach (var path in singleUi)
            {
                ConfigurarSingleSprite(path, pivotCenter);
            }

            // Insignia x2: 2 cuadros de 32x16
            SliceGrid("Assets/Sprites/UI/Insignia_x2.png", 32, 16, pivotCenter, SpriteAlignment.Center);

            // 9-slice: Panel_Madera (borde 8)
            Configurar9Slice("Assets/Sprites/UI/Panel_Madera.png", new Vector4(8, 8, 8, 8));

            // 9-slice: Boton_Madera y Boton_Madera_Presionado (borde 4)
            Configurar9Slice("Assets/Sprites/UI/Boton_Madera.png", new Vector4(4, 4, 4, 4));
            Configurar9Slice("Assets/Sprites/UI/Boton_Madera_Presionado.png", new Vector4(4, 4, 4, 4));
        }

        // ====================================================================
        // 7. FONDOS PARALLAX
        // ====================================================================
        private static void ConfigurarFondos()
        {
            string[] fondos = new string[]
            {
                "Assets/Sprites/Fondos/Fondo_Menu.png",
                "Assets/Sprites/Fondos/MinaRayas/Fondo_MinaRayas_Capa0_Lejana.png",
                "Assets/Sprites/Fondos/MinaRayas/Fondo_MinaRayas_Capa1_Media.png",
                "Assets/Sprites/Fondos/MinaRayas/Fondo_MinaRayas_Capa2_Cercana.png",
                "Assets/Sprites/Fondos/ElNopal/Fondo_ElNopal_Capa0_Lejana.png",
                "Assets/Sprites/Fondos/ElNopal/Fondo_ElNopal_Capa1_Media.png",
                "Assets/Sprites/Fondos/ElNopal/Fondo_ElNopal_Capa2_Cercana.png",
                "Assets/Sprites/Fondos/LaValenciana/Fondo_LaValenciana_Capa0_Lejana.png",
                "Assets/Sprites/Fondos/LaValenciana/Fondo_LaValenciana_Capa1_Media.png",
                "Assets/Sprites/Fondos/LaValenciana/Fondo_LaValenciana_Capa2_Cercana.png"
            };

            foreach (var path in fondos)
            {
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = StandardPPU;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.wrapMode = TextureWrapMode.Repeat;

                var settings = new TextureImporterSettings();
                importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                importer.SetTextureSettings(settings);

                importer.SaveAndReimport();
            }
        }

        // ====================================================================
        // 8. TILESETS (128x48 -> 24 tiles de 16x16) Y CREACIÓN DE TILES
        // ====================================================================
        private static void ConfigurarTilesetsYCrearTiles()
        {
            string[] niveles = new string[] { "MinaRayas", "ElNopal", "LaValenciana" };

            foreach (var nivel in niveles)
            {
                string pathTileset = $"Assets/Tiles/{nivel}/Tileset_{nivel}.png";
                SliceGrid(pathTileset, 16, 16, new Vector2(0.5f, 0.5f), SpriteAlignment.Center);

                // Crear los assets Tile individuales (.asset)
                CrearTileAssets(nivel, pathTileset);
            }
        }

        private static void CrearTileAssets(string nivel, string texturePath)
        {
            var subSprites = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            int tileIndex = 0;

            foreach (var obj in subSprites)
            {
                if (obj is Sprite spr)
                {
                    string tileAssetPath = $"Assets/Tiles/{nivel}/Tile_{nivel}_{tileIndex}.asset";
                    Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(tileAssetPath);
                    if (tile == null)
                    {
                        tile = ScriptableObject.CreateInstance<Tile>();
                        tile.sprite = spr;
                        AssetDatabase.CreateAsset(tile, tileAssetPath);
                    }
                    else
                    {
                        tile.sprite = spr;
                        EditorUtility.SetDirty(tile);
                    }
                    tileIndex++;
                }
            }
        }

        // ====================================================================
        // HELPERS GENÉRICOS DE IMPORTACIÓN Y SLICING
        // ====================================================================

        public static void ConfigurarSingleSprite(string assetPath, Vector2 pivot)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = StandardPPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePivot = pivot;

            importer.SaveAndReimport();
        }

        public static void Configurar9Slice(string assetPath, Vector4 border)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = StandardPPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spriteBorder = border;

            importer.SaveAndReimport();
        }

        public static void SliceGrid(string assetPath, int cellWidth, int cellHeight, Vector2 pivot, SpriteAlignment alignment)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = StandardPPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;

            importer.SaveAndReimport();

            // Usar ISpriteEditorDataProvider oficial de Unity
            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null)
            {
                Debug.LogWarning($"[El Minero] No se pudo obtener SpriteEditorDataProvider para: {assetPath}");
                return;
            }

            dataProvider.InitSpriteEditorDataProvider();

            // Cargar textura para obtener dimensiones reales
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null) return;

            int texWidth = texture.width;
            int texHeight = texture.height;

            List<SpriteRect> rects = new List<SpriteRect>();
            int index = 0;

            // Recorrer de arriba hacia abajo (orden de lectura de spritesheets)
            for (int y = texHeight - cellHeight; y >= 0; y -= cellHeight)
            {
                for (int x = 0; x <= texWidth - cellWidth; x += cellWidth)
                {
                    SpriteRect sRect = new SpriteRect
                    {
                        name = $"{Path.GetFileNameWithoutExtension(assetPath)}_{index}",
                        rect = new Rect(x, y, cellWidth, cellHeight),
                        alignment = alignment,
                        pivot = pivot,
                        spriteID = GUID.Generate()
                    };

                    rects.Add(sRect);
                    index++;
                }
            }

            dataProvider.SetSpriteRects(rects.ToArray());
            dataProvider.Apply();

            importer.SaveAndReimport();
        }
    }
}
