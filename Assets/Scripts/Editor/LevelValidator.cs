using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEditor.SceneManagement;
using ElMinero.Niveles;
using ElMinero.Utilidades;
using ElMinero.Coleccionables;

namespace ElMinero.Editor
{
    /// <summary>
    /// Herramienta de Editor para validar niveles y plantar objetos flotantes al suelo o pared.
    /// Accesible desde el menú: Herramientas > El Minero > Validar niveles.
    /// </summary>
    public static class LevelValidator
    {
        private static readonly string[] EscenasNiveles = new string[]
        {
            "Assets/Scenes/Nivel1_MinaRayas.unity",
            "Assets/Scenes/Nivel2_ElNopal.unity",
            "Assets/Scenes/Nivel3_LaValenciana.unity"
        };

        [MenuItem("Herramientas/El Minero/Validar niveles", priority = 20)]
        [MenuItem("Herramientas/El Minero/Plantar objetos flotantes al suelo", priority = 21)]
        public static void ValidarTodosLosNiveles() => ValidarYCorregirTodosLosNiveles();
        public static void ValidarYCorregirTodosLosNiveles()
        {
            int totalCorregidos = 0;

            Sprite[] spCheckpoint = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Coleccionables/Checkpoint.png").OfType<Sprite>().ToArray();
            Sprite[] spCapilla = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Coleccionables/Checkpoint_Capilla.png").OfType<Sprite>().ToArray();

            foreach (string escenaPath in EscenasNiveles)
            {
                var scene = EditorSceneManager.OpenScene(escenaPath, OpenSceneMode.Single);
                if (!scene.IsValid()) continue;

                int corregidosEnEscena = 0;
                int layerSuelo = LayerMask.GetMask(Constantes.Layers.Suelo);

                // 1. Validar y plantar Checkpoints
                var checkpoints = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
                foreach (var cp in checkpoints)
                {
                    // Raycast hacia abajo buscando el suelo
                    RaycastHit2D hit = Physics2D.Raycast(cp.transform.position + Vector3.up * 0.5f, Vector2.down, 15f, layerSuelo);
                    if (hit.collider != null)
                    {
                        float nuevaY = hit.point.y;
                        if (Mathf.Abs(cp.transform.position.y - nuevaY) > 0.05f)
                        {
                            cp.transform.position = new Vector3(cp.transform.position.x, nuevaY, cp.transform.position.z);
                            EditorUtility.SetDirty(cp.gameObject);
                            corregidosEnEscena++;
                        }
                    }

                    // Configurar sprites rebanados
                    bool esCapilla = cp.gameObject.name.Contains("Capilla");
                    Sprite[] banco = esCapilla ? spCapilla : spCheckpoint;
                    if (banco.Length >= 2)
                    {
                        cp.ConfigurarSprites(banco[0], banco[1]);
                        var sr = cp.GetComponent<SpriteRenderer>();
                        if (sr != null && !cp.IsActivated) sr.sprite = banco[0];
                    }

                    // Asegurar Light2D cálida
                    cp.ConfigurarLuz2D();
                    EditorUtility.SetDirty(cp.gameObject);
                }

                // 2. Validar Bloques de Derrumbe
                var bloques = Object.FindObjectsByType<BloqueDerrumbe>(FindObjectsSortMode.None);
                foreach (var bloque in bloques)
                {
                    RaycastHit2D hit = Physics2D.Raycast(bloque.transform.position, Vector2.down, 40f, layerSuelo);
                    if (hit.collider != null)
                    {
                        SerializedObject so = new SerializedObject(bloque);
                        var propTarget = so.FindProperty("targetGroundPosition");
                        var col = bloque.GetComponent<Collider2D>();
                        float extY = col != null ? col.bounds.extents.y : 0.5f;
                        propTarget.vector3Value = new Vector3(bloque.transform.position.x, hit.point.y + extY, bloque.transform.position.z);
                        so.ApplyModifiedProperties();
                        EditorUtility.SetDirty(bloque.gameObject);
                        corregidosEnEscena++;
                    }
                }

                if (escenaPath.Contains("Nivel3"))
                {
                    ValidarNivel3ContraMapa();
                }

                if (corregidosEnEscena > 0)
                {
                    EditorSceneManager.SaveScene(scene);
                    Debug.Log($"[LevelValidator] {escenaPath}: {corregidosEnEscena} objetos flotantes corregidos y plantados al suelo.");
                    totalCorregidos += corregidosEnEscena;
                }
            }

            Debug.Log($"[LevelValidator] ¡Validación completada! Total de correcciones realizadas: {totalCorregidos}.");
        }

        /// <summary>
        /// Comprueba que la escena del nivel 3 tenga exactamente lo que dice Mapa_Nivel3.txt.
        /// </summary>
        private static void ValidarNivel3ContraMapa()
        {
            TextAsset txt = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/Settings/Niveles/Mapa_Nivel3.txt");
            if (txt == null)
            {
                Debug.LogWarning("[LevelValidator] Nivel 3: no se encontró Mapa_Nivel3.txt.");
                return;
            }
            var mapa = LevelBuilder.LeerMapa(txt);
            int vetasMapa = mapa.Buscar('v').Count;
            int gemasMapa = mapa.Buscar('g').Count;
            int cpMapa = mapa.Buscar('C').Count;

            int vetas = Object.FindObjectsByType<SilverVein>(FindObjectsSortMode.None).Length;
            int gemas = Object.FindObjectsByType<AmethystGem>(FindObjectsSortMode.None).Length;
            int cps = Object.FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).Length;
            int jefes = Object.FindObjectsByType<ElMinero.Jefe.BossColoso>(FindObjectsSortMode.None).Length;
            int elevadores = Object.FindObjectsByType<ElMinero.Plataformas.MineElevator>(FindObjectsSortMode.None).Length;
            int muros = Object.FindObjectsByType<ElMinero.Obstaculos.FireWallChase>(FindObjectsSortMode.None).Length;
            int fuegosEstaticos = Object.FindObjectsByType<ElMinero.Obstaculos.FireHazard>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;

            bool ok = true;
            if (vetas != vetasMapa) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: {vetas} vetas en escena, {vetasMapa} en el mapa."); }
            if (gemas != gemasMapa) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: {gemas} gemas en escena, {gemasMapa} en el mapa."); }
            if (cps != cpMapa) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: {cps} checkpoints en escena, {cpMapa} en el mapa."); }
            if (jefes != 1) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: se esperaba 1 jefe y hay {jefes}."); }
            if (elevadores != 1) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: se esperaba 1 elevador y hay {elevadores}."); }
            if (muros != 1) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: se esperaba 1 muro de fuego y hay {muros}."); }
            if (fuegosEstaticos != 0) { ok = false; Debug.LogWarning($"[LevelValidator] Nivel 3: quedan {fuegosEstaticos} FireHazard estáticos."); }

            if (ok) Debug.Log($"[LevelValidator] Nivel 3 coincide con el mapa: {vetas} vetas, {gemas} gemas, {cps} checkpoints, jefe, elevador y muro de fuego.");
        }
    }
}
