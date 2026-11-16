using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

namespace ElMinero.Editor
{
    /// <summary>
    /// Utilidades de editor para cargar cuadros de hojas de sprites en orden
    /// (por el número al final del nombre: Hoja_0, Hoja_1, ..., Hoja_10)
    /// y para obtener los Tile de un tileset por su índice real.
    /// </summary>
    public static class EditorSpriteUtil
    {
        private static int IndiceDe(string nombre)
        {
            Match m = Regex.Match(nombre, @"_(\d+)$");
            return m.Success ? int.Parse(m.Groups[1].Value) : 0;
        }

        /// <summary>Todos los sub-sprites de una textura, ordenados por índice.</summary>
        public static Sprite[] Cuadros(string rutaTextura)
        {
            var sprites = AssetDatabase.LoadAllAssetsAtPath(rutaTextura).OfType<Sprite>().ToList();
            if (sprites.Count == 0)
            {
                Sprite unico = AssetDatabase.LoadAssetAtPath<Sprite>(rutaTextura);
                if (unico != null) sprites.Add(unico);
            }
            return sprites.OrderBy(s => IndiceDe(s.name)).ToArray();
        }

        /// <summary>Un rango de cuadros (por ejemplo, del 2 al 3 de una hoja de 4).</summary>
        public static Sprite[] Cuadros(string rutaTextura, int inicio, int cantidad)
        {
            var todos = Cuadros(rutaTextura);
            var r = new List<Sprite>();
            for (int i = inicio; i < inicio + cantidad && i < todos.Length; i++) r.Add(todos[i]);
            return r.ToArray();
        }

        public static Sprite Primero(string rutaTextura)
        {
            var c = Cuadros(rutaTextura);
            return c.Length > 0 ? c[0] : null;
        }

        /// <summary>
        /// Crea (o actualiza) un Tile por cada sub-sprite del tileset, nombrado por su índice real:
        /// Assets/Tiles/{carpeta}/Tile_{prefijo}_{indice}.asset
        /// </summary>
        public static void CrearTilesPorIndice(string rutaTextura, string carpeta, string prefijo)
        {
            if (!Directory.Exists(carpeta)) Directory.CreateDirectory(carpeta);
            foreach (var s in Cuadros(rutaTextura))
            {
                string ruta = $"{carpeta}/Tile_{prefijo}_{IndiceDe(s.name)}.asset";
                Tile t = AssetDatabase.LoadAssetAtPath<Tile>(ruta);
                if (t == null)
                {
                    t = ScriptableObject.CreateInstance<Tile>();
                    t.sprite = s;
                    AssetDatabase.CreateAsset(t, ruta);
                }
                else
                {
                    t.sprite = s;
                    EditorUtility.SetDirty(t);
                }
            }
        }

        /// <summary>
        /// Diccionario índice → Tile de una carpeta, usando el nombre del sprite de cada Tile
        /// (así no importa en qué orden se crearon los assets).
        /// </summary>
        public static Dictionary<int, Tile> TilesPorIndice(string carpeta)
        {
            var dic = new Dictionary<int, Tile>();
            if (!Directory.Exists(carpeta)) return dic;
            foreach (string guid in AssetDatabase.FindAssets("t:Tile", new[] { carpeta }))
            {
                Tile t = AssetDatabase.LoadAssetAtPath<Tile>(AssetDatabase.GUIDToAssetPath(guid));
                if (t == null || t.sprite == null) continue;
                int idx = IndiceDe(t.sprite.name);
                if (!dic.ContainsKey(idx)) dic[idx] = t;
            }
            return dic;
        }
    }
}
