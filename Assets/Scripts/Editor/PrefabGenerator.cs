using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using TMPro;
using Unity.Cinemachine;
using ElMinero.Camara;
using ElMinero.Coleccionables;
using ElMinero.Entorno;
using ElMinero.Jugador;
using ElMinero.Managers;
using ElMinero.Niveles;
using ElMinero.Obstaculos;
using ElMinero.Plataformas;
using ElMinero.UI;
using ElMinero.Utilidades;

namespace ElMinero.Editor
{
    /// <summary>
    /// Genera todos los Prefabs requeridos por la arquitectura del proyecto (Sección 2 del GDD).
    /// </summary>
    public static class PrefabGenerator
    {
        [MenuItem("Herramientas/El Minero/6. Crear prefabs", priority = 60)]
        public static void CrearTodosLosPrefabs()
        {
            EditorUtility.DisplayProgressBar("El Minero", "Generando prefabs...", 0.1f);

            try
            {
                AsegurarCarpetas();

                int layerSuelo = LayerMask.NameToLayer(Constantes.Layers.Suelo);
                if (layerSuelo == -1) layerSuelo = 8;
                int layerJugador = LayerMask.NameToLayer(Constantes.Layers.Jugador);
                if (layerJugador == -1) layerJugador = 9;
                int layerPeligro = LayerMask.NameToLayer(Constantes.Layers.Peligro);
                if (layerPeligro == -1) layerPeligro = 10;
                int layerColeccionable = LayerMask.NameToLayer(Constantes.Layers.Coleccionable);
                if (layerColeccionable == -1) layerColeccionable = 11;

                // 1. Jugador
                CrearPrefabMinero(layerJugador, layerSuelo);

                // 2. Plataformas
                CrearPrefabPlataformaFija(layerSuelo);
                CrearPrefabPlataformaMovil(layerSuelo);
                CrearPrefabPlataformaQuebradiza(layerSuelo);

                // 3. Obstáculos
                CrearPrefabRoca(layerPeligro);
                CrearPrefabRockSpawner();
                CrearPrefabHuecoLetal();
                CrearPrefabZonaDerrumbe();
                CrearPrefabBloqueDerrumbe();

                // 4. Coleccionables
                CrearPrefabVetaPlata(layerColeccionable);
                CrearPrefabGemaAmatista(layerColeccionable);

                // 5. Niveles
                CrearPrefabCheckpoint();
                CrearPrefabSalidaNivel();
                CrearPrefabAntorcha();
                CrearPrefabViga();

                // 6. Sistemas
                CrearPrefabSistemas();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[El Minero] ¡Todos los prefabs creados y guardados en Assets/Prefabs/!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        private static void AsegurarCarpetas()
        {
            string[] dirs = new string[]
            {
                "Assets/Prefabs/Jugador",
                "Assets/Prefabs/Plataformas",
                "Assets/Prefabs/Obstaculos",
                "Assets/Prefabs/Coleccionables",
                "Assets/Prefabs/Niveles",
                "Assets/Prefabs/UI",
                "Assets/Prefabs/Sistemas"
            };

            foreach (var d in dirs)
            {
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            }
        }

        private static void CrearPrefabMinero(int layerJugador, int layerSuelo)
        {
            string path = "Assets/Prefabs/Jugador/Minero.prefab";
            GameObject go = new GameObject("Minero");
            go.tag = Constantes.Tags.Player;
            go.layer = layerJugador;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jugador/Minero_Idle.png");
            sr.sortingLayerName = Constantes.SortingLayers.Jugador;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
            rb.gravityScale = 3f;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.direction = CapsuleDirection2D.Vertical;
            col.size = new Vector2(0.75f, 1.4f);
            col.offset = new Vector2(0f, 0.7f);
            col.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Materiales/Fisica/SinFriccion.physicsMaterial2D");

            go.AddComponent<PlayerInputHandler>();
            var ctrl = go.AddComponent<PlayerController>();
            go.AddComponent<PlayerHealth>();

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Jugador/Minero.controller");
            go.AddComponent<PlayerAnimator>();

            SerializedObject soCtrl = new SerializedObject(ctrl);
            soCtrl.FindProperty("groundLayer").intValue = 1 << layerSuelo;
            soCtrl.FindProperty("spriteRenderer").objectReferenceValue = sr;
            soCtrl.ApplyModifiedProperties();

            // Etiqueta Nombre
            GameObject label = new GameObject("EtiquetaNombre");
            label.transform.SetParent(go.transform);
            label.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            var tmp = label.AddComponent<TextMeshPro>();
            tmp.text = "Refugio";
            tmp.fontSize = 3.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.sortingLayerID = SortingLayer.NameToID(Constantes.SortingLayers.UI);
            tmp.color = Color.yellow;
            label.AddComponent<NameLabel>();

            // Luz Casco
            GameObject luz = new GameObject("Luz_Casco");
            luz.transform.SetParent(go.transform);
            luz.transform.localPosition = new Vector3(0.2f, 1.2f, 0f);
            var l2d = luz.AddComponent<Light2D>();
            l2d.lightType = Light2D.LightType.Point;
            l2d.pointLightInnerRadius = 0.5f;
            l2d.pointLightOuterRadius = 4f;
            l2d.intensity = 1.0f;
            l2d.color = new Color(1f, 0.95f, 0.8f);
            ElMineroBuilder.AsignarCapasALuz2D(l2d);

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabPlataformaFija(int layerSuelo)
        {
            string path = "Assets/Prefabs/Plataformas/PlataformaFija.prefab";
            GameObject go = new GameObject("PlataformaFija");
            go.layer = layerSuelo;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Fija.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabPlataformaMovil(int layerSuelo)
        {
            string path = "Assets/Prefabs/Plataformas/PlataformaMovil.prefab";
            GameObject go = new GameObject("PlataformaMovil");
            go.layer = layerSuelo;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Movil.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            GameObject pA = new GameObject("PuntoA");
            pA.transform.SetParent(go.transform);
            pA.transform.localPosition = Vector3.zero;

            GameObject pB = new GameObject("PuntoB");
            pB.transform.SetParent(go.transform);
            pB.transform.localPosition = new Vector3(5f, 0f, 0f);

            var mov = go.AddComponent<MovingPlatform>();
            SerializedObject so = new SerializedObject(mov);
            so.FindProperty("pointA").objectReferenceValue = pA.transform;
            so.FindProperty("pointB").objectReferenceValue = pB.transform;
            so.FindProperty("speed").floatValue = 2.0f;
            so.ApplyModifiedProperties();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabPlataformaQuebradiza(int layerSuelo)
        {
            string path = "Assets/Prefabs/Plataformas/PlataformaQuebradiza.prefab";
            GameObject go = new GameObject("PlataformaQuebradiza");
            go.layer = layerSuelo;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            go.AddComponent<BreakablePlatform>();
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/PlataformaQuebradiza.controller");

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabRoca(int layerPeligro)
        {
            string path = "Assets/Prefabs/Obstaculos/Roca.prefab";
            GameObject go = new GameObject("Roca");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Roca.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;

            go.AddComponent<FallingRock>();
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/Roca.controller");

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabRockSpawner()
        {
            string path = "Assets/Prefabs/Obstaculos/GeneradorRocas.prefab";
            GameObject go = new GameObject("GeneradorRocas");
            go.AddComponent<RockSpawner>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabHuecoLetal()
        {
            string path = "Assets/Prefabs/Obstaculos/HuecoLetal.prefab";
            GameObject go = new GameObject("HuecoLetal");
            go.tag = Constantes.Tags.Hazard;
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(4f, 2f);
            go.AddComponent<DeathPit>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabZonaDerrumbe()
        {
            string path = "Assets/Prefabs/Obstaculos/ZonaDerrumbe.prefab";
            GameObject go = new GameObject("ZonaDerrumbe");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2.5f, 4f);
            go.AddComponent<CinemachineImpulseSource>();
            go.AddComponent<CaveInZone>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabBloqueDerrumbe()
        {
            string path = "Assets/Prefabs/Obstaculos/BloqueDerrumbe.prefab";
            GameObject go = new GameObject("BloqueDerrumbe");
            go.tag = Constantes.Tags.Hazard;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 3f);
            col.offset = new Vector2(0f, 1.5f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
            go.AddComponent<BloqueDerrumbe>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabVetaPlata(int layerColeccionable)
        {
            string path = "Assets/Prefabs/Coleccionables/VetaPlata.prefab";
            GameObject go = new GameObject("VetaPlata");
            go.tag = Constantes.Tags.Collectible;
            go.layer = layerColeccionable;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Veta_Plata.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.AddComponent<SilverVein>();
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Coleccionables/Veta.controller");

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabGemaAmatista(int layerColeccionable)
        {
            string path = "Assets/Prefabs/Coleccionables/GemaAmatista.prefab";
            GameObject go = new GameObject("GemaAmatista");
            go.tag = Constantes.Tags.Collectible;
            go.layer = layerColeccionable;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Gema_Amatista.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.AddComponent<AmethystGem>();
            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Coleccionables/Gema.controller");

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabCheckpoint()
        {
            string path = "Assets/Prefabs/Niveles/Checkpoint.prefab";
            GameObject go = new GameObject("Checkpoint");
            go.tag = Constantes.Tags.Checkpoint;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);
            col.offset = new Vector2(0f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Checkpoint.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var luz = go.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightInnerRadius = 0.5f;
            luz.pointLightOuterRadius = 3f;
            luz.intensity = 1.2f;
            luz.color = new Color(0.4f, 0.9f, 1f);
            luz.enabled = false;
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            var cp = go.AddComponent<Checkpoint>();
            SerializedObject so = new SerializedObject(cp);
            so.FindProperty("checkpointLight").objectReferenceValue = luz;
            so.ApplyModifiedProperties();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabSalidaNivel()
        {
            string path = "Assets/Prefabs/Niveles/SalidaNivel.prefab";
            GameObject go = new GameObject("SalidaNivel");
            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(2f, 3f);
            col.offset = new Vector2(0f, 1.5f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Salida_Tunel.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            go.AddComponent<LevelExit>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabAntorcha()
        {
            string path = "Assets/Prefabs/Niveles/Antorcha.prefab";
            GameObject go = new GameObject("Antorcha");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Antorcha.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Entorno/Antorcha.controller");

            var luz = go.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightInnerRadius = 0.5f;
            luz.pointLightOuterRadius = 3.5f;
            luz.intensity = 1.3f;
            luz.color = new Color(1f, 0.82f, 0.48f);
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            go.AddComponent<FlickeringLight2D>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabViga()
        {
            string path = "Assets/Prefabs/Niveles/Viga.prefab";
            GameObject go = new GameObject("Viga");
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Viga_Horizontal.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabSistemas()
        {
            // GameManager
            string pathGm = "Assets/Prefabs/Sistemas/GameManager.prefab";
            GameObject goGm = new GameObject("GameManager");
            goGm.AddComponent<GameManager>();
            GuardarYDestruir(goGm, pathGm);

            // AudioManager
            string pathAm = "Assets/Prefabs/Sistemas/AudioManager.prefab";
            GameObject goAm = new GameObject("AudioManager");
            goAm.AddComponent<AudioManager>();
            GuardarYDestruir(goAm, pathAm);

            // CamaraNivel
            string pathCam = "Assets/Prefabs/Sistemas/CamaraNivel.prefab";
            GameObject goCam = new GameObject("Main Camera");
            goCam.tag = "MainCamera";
            var cam = goCam.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5.5f;
            goCam.AddComponent<AudioListener>();
            goCam.AddComponent<UniversalAdditionalCameraData>();
            goCam.AddComponent<CinemachineBrain>();
            goCam.AddComponent<CinemachineImpulseListener>();
            goCam.AddComponent<CameraShake>();
            goCam.AddComponent<CameraFollowSetup>();
            GuardarYDestruir(goCam, pathCam);
        }

        private static void GuardarYDestruir(GameObject go, string prefabPath)
        {
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            GameObject.DestroyImmediate(go);
        }
    }
}
