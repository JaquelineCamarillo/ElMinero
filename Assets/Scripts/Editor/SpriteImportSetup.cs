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

                // 5. Sprites de Enemigos
                ConfigurarSpritesEnemigos();

                // 6. Sprites de Coleccionables y Minerales Distintivos
                ConfigurarSpritesColeccionables();

                // 7. Sprites de UI (9-slice en paneles, botones y sliders)
                ConfigurarSpritesUI();

                // 8. Fondos Parallax (FullRect, Repeat)
                ConfigurarFondos();

                // 9. Tilesets y generación de Tile Assets
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
            Vector2 pivotBottomCenter = new Vector2(0.5f, 0.0f);

            ConfigurarSingleSprite("Assets/Sprites/Entorno/Viga_Horizontal.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Poste_Vertical.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Marco_Soporte.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Vagoneta_Deco.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Salida_Tunel.png", pivotBottomCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Salida_Superficie.png", pivotBottomCenter);

            // Antorcha: 3 cuadros de 16x16
            SliceGrid("Assets/Sprites/Entorno/Antorcha.png", 16, 16, pivotCenter, SpriteAlignment.Center);

            // Nuevos elementos de entorno (v2)
            SliceGrid("Assets/Sprites/Entorno/Malacate.png", 32, 32, pivotCenter, SpriteAlignment.Center);
            ConfigurarTiledSprite("Assets/Sprites/Entorno/Cable.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Riel.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Letrero.png", pivotBottomCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Deco_VetaCobre.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Deco_VetaOro.png", pivotCenter);
            ConfigurarSingleSprite("Assets/Sprites/Entorno/Deco_VetaPlataRoja.png", pivotCenter);
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

            // Elevador vertical (v2): 48x24
            ConfigurarSingleSprite("Assets/Sprites/Plataformas/Elevador_Jaula.png", new Vector2(0.5f, 0.0f));
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

            // Fuego (v2): 4 cuadros de 16x16
            SliceGrid("Assets/Sprites/Obstaculos/Fuego.png", 16, 16, new Vector2(0.5f, 0.0f), SpriteAlignment.BottomCenter);
        }

        // ====================================================================
        // 5. ENEMIGOS (v2)
        // ====================================================================
        private static void ConfigurarSpritesEnemigos()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);
            Vector2 pivotBottomCenter = new Vector2(0.5f, 0.0f);

            // Murciélago (16x16, 4 cuadros)
            SliceGrid("Assets/Sprites/Enemigos/Murcielago.png", 16, 16, pivotCenter, SpriteAlignment.Center);

            // Murciélago Rojo (16x16, 4 cuadros)
            SliceGrid("Assets/Sprites/Enemigos/Murcielago_Rojo.png", 16, 16, pivotCenter, SpriteAlignment.Center);

            // Araña (16x16, 3 cuadros)
            SliceGrid("Assets/Sprites/Enemigos/Arana.png", 16, 16, pivotCenter, SpriteAlignment.Center);

            // Hilo de la Araña (2x16, Tiled FullRect)
            ConfigurarTiledSprite("Assets/Sprites/Enemigos/Hilo_Arana.png", new Vector2(0.5f, 1.0f));

            // Alacrán Caminar (32x16, 4 cuadros)
            SliceGrid("Assets/Sprites/Enemigos/Alacran_Caminar.png", 32, 16, pivotBottomCenter, SpriteAlignment.BottomCenter);

            // Alacrán Ataque (32x16, 2 cuadros)
            SliceGrid("Assets/Sprites/Enemigos/Alacran_Ataque.png", 32, 16, pivotBottomCenter, SpriteAlignment.BottomCenter);

            // Vagoneta Desbocada (32x24, 2 cuadros)
            SliceGrid("Assets/Sprites/Enemigos/Vagoneta_Desbocada.png", 32, 24, pivotCenter, SpriteAlignment.Center);
        }

        // ====================================================================
        // 6. COLECCIONABLES Y MINERALES DISTINTIVOS
        // ====================================================================
        private static void ConfigurarSpritesColeccionables()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);
            Vector2 pivotBottomCenter = new Vector2(0.5f, 0.0f);

            SliceGrid("Assets/Sprites/Coleccionables/Veta_Plata.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Coleccionables/Gema_Amatista.png", 16, 16, pivotCenter, SpriteAlignment.Center);

            // Checkpoint estándar: 2 cuadros de 16x32 (apagado y encendido)
            SliceGrid("Assets/Sprites/Coleccionables/Checkpoint.png", 16, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);

            // Minerales distintivos (v2: 3 cuadros de 16x16 cada uno)
            SliceGrid("Assets/Sprites/Coleccionables/Aguilarita.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Coleccionables/Pepita_Oro.png", 16, 16, pivotCenter, SpriteAlignment.Center);
            SliceGrid("Assets/Sprites/Coleccionables/Pirargirita.png", 16, 16, pivotCenter, SpriteAlignment.Center);

            // Checkpoint Capilla (v2: 2 cuadros de 24x32)
            SliceGrid("Assets/Sprites/Coleccionables/Checkpoint_Capilla.png", 24, 32, pivotBottomCenter, SpriteAlignment.BottomCenter);
        }

        // ====================================================================
        // 7. UI
        // ====================================================================
        private static void ConfigurarSpritesUI()
        {
            Vector2 pivotCenter = new Vector2(0.5f, 0.5f);

            // Iconos y botones simples del paquete 1
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
                "Assets/Sprites/UI/Logo_ElMinero.png",
                // Elementos UI del paquete v2
                "Assets/Sprites/UI/Estrella_Llena.png",
                "Assets/Sprites/UI/Estrella_Vacia.png",
                "Assets/Sprites/UI/Icono_Candado.png",
                "Assets/Sprites/UI/Miniatura_MinaRayas.png",
                "Assets/Sprites/UI/Miniatura_ElNopal.png",
                "Assets/Sprites/UI/Miniatura_LaValenciana.png",
                "Assets/Sprites/UI/Boton_Redondo.png",
                "Assets/Sprites/UI/Boton_Redondo_Resaltado.png",
                "Assets/Sprites/UI/Boton_Redondo_Presionado.png",
                "Assets/Sprites/UI/Boton_Volver.png",
                "Assets/Sprites/UI/Boton_Volver_Resaltado.png",
                "Assets/Sprites/UI/Boton_Volver_Presionado.png",
                "Assets/Sprites/UI/Slider_Manija.png",
                "Assets/Sprites/UI/Toggle_Fondo.png",
                "Assets/Sprites/UI/Toggle_Check.png",
                "Assets/Sprites/UI/Logo_Autora.png"
            };

            foreach (var path in singleUi)
            {
                ConfigurarSingleSprite(path, pivotCenter);
            }

            // Todos los iconos dentro de Assets/Sprites/UI/Iconos/
            if (Directory.Exists("Assets/Sprites/UI/Iconos"))
            {
                string[] iconFiles = Directory.GetFiles("Assets/Sprites/UI/Iconos", "*.png");
                foreach (var iconPath in iconFiles)
                {
                    string unityPath = iconPath.Replace('\\', '/');
                    ConfigurarSingleSprite(unityPath, pivotCenter);
                }
            }

            // Insignia x2: 2 cuadros de 32x16
            SliceGrid("Assets/Sprites/UI/Insignia_x2.png", 32, 16, pivotCenter, SpriteAlignment.Center);

            // 9-slice: Paneles y Botones
            Configurar9Slice("Assets/Sprites/UI/Panel_Madera.png", new Vector4(8, 8, 8, 8));
            Configurar9Slice("Assets/Sprites/UI/Boton_Madera.png", new Vector4(4, 4, 4, 4));
            Configurar9Slice("Assets/Sprites/UI/Boton_Madera_Presionado.png", new Vector4(4, 4, 4, 4));
            Configurar9Slice("Assets/Sprites/UI/Boton_Madera_Resaltado.png", new Vector4(4, 4, 4, 4));

            // Botón Nivel con borde 6
            Configurar9Slice("Assets/Sprites/UI/Boton_Nivel.png", new Vector4(6, 6, 6, 6));
            Configurar9Slice("Assets/Sprites/UI/Boton_Nivel_Resaltado.png", new Vector4(6, 6, 6, 6));
            Configurar9Slice("Assets/Sprites/UI/Boton_Nivel_Presionado.png", new Vector4(6, 6, 6, 6));
            Configurar9Slice("Assets/Sprites/UI/Boton_Nivel_Bloqueado.png", new Vector4(6, 6, 6, 6));

            // Sliders con borde 4
            Configurar9Slice("Assets/Sprites/UI/Slider_Fondo.png", new Vector4(4, 4, 4, 4));
            Configurar9Slice("Assets/Sprites/UI/Slider_Relleno.png", new Vector4(4, 4, 4, 4));
        }

        // ====================================================================
        // 8. FONDOS PARALLAX
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
        // 9. TILESETS (128x48 -> 24 tiles de 16x16) Y CREACIÓN DE TILES
        // ====================================================================
        private static void ConfigurarTilesetsYCrearTiles()
        {
            string[] niveles = new string[] { "MinaRayas", "ElNopal", "LaValenciana" };

            foreach (var nivel in niveles)
            {
                string pathTileset = $"Assets/Tiles/{nivel}/Tileset_{nivel}.png";
                SliceGrid(pathTileset, 16, 16, new Vector2(0.5f, 0.5f), SpriteAlignment.Center);

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
            importer.mipmapEnabled = false;

            importer.SaveAndReimport();
        }

        public static void ConfigurarTiledSprite(string assetPath, Vector2 pivot)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = StandardPPU;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.spritePivot = pivot;
            importer.mipmapEnabled = false;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);

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
            importer.mipmapEnabled = false;

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

            var factory = new SpriteDataProviderFactories();
            factory.Init();
            var dataProvider = factory.GetSpriteEditorDataProviderFromObject(importer);
            if (dataProvider == null)
            {
                Debug.LogWarning($"[El Minero] No se pudo obtener SpriteEditorDataProvider para: {assetPath}");
                return;
            }

            dataProvider.InitSpriteEditorDataProvider();

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null) return;

            int texWidth = texture.width;
            int texHeight = texture.height;

            List<SpriteRect> rects = new List<SpriteRect>();
            int index = 0;

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
