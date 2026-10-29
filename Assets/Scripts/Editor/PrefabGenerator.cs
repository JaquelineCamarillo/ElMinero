using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering.Universal;
using TMPro;
using Unity.Cinemachine;
using ElMinero.Camara;
using ElMinero.Coleccionables;
using ElMinero.Enemigos;
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
    /// Genera todos los Prefabs requeridos por la arquitectura del proyecto (Sección 2 del GDD y paquete v2).
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
                                int layerEnemigo = LayerMask.NameToLayer(Constantes.Layers.Enemigo);
                if (layerEnemigo == -1) layerEnemigo = layerPeligro;

                int layerColeccionable = LayerMask.NameToLayer(Constantes.Layers.Coleccionable);
                if (layerColeccionable == -1) layerColeccionable = 11;

                // 1. Jugador
                CrearPrefabMinero(layerJugador, layerSuelo);

                // 2. Plataformas
                CrearPrefabPlataformaFija(layerSuelo);
                CrearPrefabPlataformaMovil(layerSuelo);
                CrearPrefabPlataformaQuebradiza(layerSuelo);
                CrearPrefabElevadorMalacate(layerSuelo);

                // 3. Obstáculos
                CrearPrefabRoca(layerPeligro);
                CrearPrefabBolaFuego(layerPeligro);
                CrearPrefabRockSpawner();
                CrearPrefabHuecoLetal();
                CrearPrefabZonaDerrumbe();
                CrearPrefabBloqueDerrumbe();
                CrearPrefabFuego(layerPeligro);
                CrearPrefabVagoneta(layerEnemigo);

                // 4. Coleccionables
                CrearPrefabVetaPlata(layerColeccionable);
                CrearPrefabGemaAmatista(layerColeccionable);
                CrearPrefabsMineralesDistintivos(layerColeccionable);

                // 5. Niveles y Entorno
                CrearPrefabCheckpoint();
                CrearPrefabCheckpointCapilla();
                CrearPrefabSalidaNivel();
                CrearPrefabAntorcha();
                CrearPrefabViga();
                CrearPrefabLetrero();

                // 6. Enemigos
                CrearPrefabMurcielago(layerEnemigo);
                CrearPrefabMurcielagoRojo(layerEnemigo);
                CrearPrefabArana(layerEnemigo);
                CrearPrefabAlacran(layerEnemigo, layerSuelo);

                // 7. Sistemas
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
                "Assets/Prefabs/Sistemas",
                "Assets/Prefabs/Enemigos"
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
            sr.sortingLayerName = Constantes.SortingLayers.Jugador;
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Jugador/Minero_Idle.png");

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Jugador/Minero.controller");

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 3.0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;

            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.8f, 1.6f);
            col.offset = new Vector2(0f, 0.8f);
            col.sharedMaterial = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>("Assets/Settings/SinFriccion.physicsMaterial2D");

            go.AddComponent<PlayerController>();
            go.AddComponent<PlayerHealth>();
            go.AddComponent<PlayerInputHandler>();
            go.AddComponent<PlayerAnimator>();

            // Nombre flotante
            GameObject nameGo = new GameObject("NameLabel");
            nameGo.transform.SetParent(go.transform);
            nameGo.transform.localPosition = new Vector3(0f, 1.9f, 0f);
            var tmp = nameGo.AddComponent<TextMeshPro>();
            tmp.text = "Refugio";
            tmp.fontSize = 2.5f;
            tmp.alignment = TextAlignmentOptions.Center;
            nameGo.AddComponent<NameLabel>();

            // Luz suave sobre el minero (lámpara del casco)
            GameObject lightGo = new GameObject("LuzMinero");
            lightGo.transform.SetParent(go.transform);
            lightGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var luz = lightGo.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightInnerRadius = 0.5f;
            luz.pointLightOuterRadius = 4.0f;
            luz.intensity = 1.1f;
            luz.color = new Color(1f, 0.92f, 0.7f);
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabPlataformaFija(int layerSuelo)
        {
            string path = "Assets/Prefabs/Plataformas/Plataforma_Fija.prefab";
            GameObject go = new GameObject("Plataforma_Fija");
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
            string path = "Assets/Prefabs/Plataformas/Plataforma_Movil.prefab";
            GameObject go = new GameObject("Plataforma_Movil");
            go.layer = layerSuelo;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Movil.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<MovingPlatform>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabPlataformaQuebradiza(int layerSuelo)
        {
            string path = "Assets/Prefabs/Plataformas/Plataforma_Quebradiza.prefab";
            GameObject go = new GameObject("Plataforma_Quebradiza");
            go.layer = layerSuelo;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.5f);

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/PlataformaQuebradiza.controller");

            go.AddComponent<BreakablePlatform>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabElevadorMalacate(int layerSuelo)
        {
            string path = "Assets/Prefabs/Plataformas/Elevador_Malacate.prefab";
            GameObject go = new GameObject("Elevador_Malacate");
            go.layer = layerSuelo;

            // Jaula del elevador
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Plataformas/Elevador_Jaula.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 0.4f);
            col.offset = new Vector2(0f, -0.4f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            // Rueda de malacate superior
            GameObject wheelGo = new GameObject("RuedaMalacate");
            wheelGo.transform.SetParent(go.transform);
            wheelGo.transform.localPosition = new Vector3(0f, 9f, 0f);
            var srWheel = wheelGo.AddComponent<SpriteRenderer>();
            srWheel.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Malacate.png");
            srWheel.sortingLayerName = Constantes.SortingLayers.Escenario;

            var elevator = go.AddComponent<MineElevator>();
            elevator.ConfigurarPuntos(Vector3.zero, new Vector3(0f, 8f, 0f), wheelGo.transform);

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabBolaFuego(int layerPeligro)
        {
            string path = "Assets/Prefabs/Obstaculos/Bola_Fuego.prefab";
            GameObject go = new GameObject("Bola_Fuego");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            Sprite spBola = null;
            var subSprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Obstaculos/Bola_Fuego.png").OfType<Sprite>().ToArray();
            if (subSprites.Length > 0) spBola = subSprites[0];
            if (spBola == null) spBola = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bola_Fuego.png");
            sr.sprite = spBola;
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.45f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;

            go.AddComponent<Fireball>();

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
            col.radius = 0.5f;

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 2.5f;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/Roca.controller");

            go.AddComponent<FallingRock>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabRockSpawner()
        {
            string path = "Assets/Prefabs/Obstaculos/RockSpawner.prefab";
            GameObject go = new GameObject("RockSpawner");
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
            col.size = new Vector2(2f, 4f);

            go.AddComponent<CaveInZone>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabBloqueDerrumbe()
        {
            string path = "Assets/Prefabs/Obstaculos/BloqueDerrumbe.prefab";
            GameObject go = new GameObject("BloqueDerrumbe");
            go.tag = Constantes.Tags.Hazard;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Bloque_Derrumbe.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1f, 1f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Dynamic;

            go.AddComponent<BloqueDerrumbe>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabFuego(int layerPeligro)
        {
            string path = "Assets/Prefabs/Obstaculos/Fuego.prefab";
            GameObject go = new GameObject("Fuego");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Obstaculos/Fuego.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/Fuego.controller");

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.9f);
            col.offset = new Vector2(0f, 0.45f);

            go.AddComponent<FireHazard>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabVagoneta(int layerPeligro)
        {
            string path = "Assets/Prefabs/Obstaculos/Vagoneta_Desbocada.prefab";
            GameObject go = new GameObject("Vagoneta_Desbocada");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Vagoneta_Desbocada.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Obstaculos/Vagoneta.controller");

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.8f, 1.2f);

            // Aviso "¡!"
            GameObject avisoGo = new GameObject("Aviso");
            avisoGo.transform.SetParent(go.transform);
            avisoGo.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            var tmp = avisoGo.AddComponent<TextMeshPro>();
            tmp.text = "¡!";
            tmp.fontSize = 4f;
            tmp.color = Color.red;
            tmp.alignment = TextAlignmentOptions.Center;

            var cart = go.AddComponent<RunawayCart>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabVetaPlata(int layerColeccionable)
        {
            string path = "Assets/Prefabs/Coleccionables/Veta_Plata.prefab";
            GameObject go = new GameObject("Veta_Plata");
            go.tag = Constantes.Tags.Collectible;
            go.layer = layerColeccionable;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Veta_Plata.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Coleccionables/Veta.controller");

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.AddComponent<SilverVein>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabGemaAmatista(int layerColeccionable)
        {
            string path = "Assets/Prefabs/Coleccionables/Gema_Amatista.prefab";
            GameObject go = new GameObject("Gema_Amatista");
            go.tag = Constantes.Tags.Collectible;
            go.layer = layerColeccionable;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Coleccionables/Gema_Amatista.png");
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Coleccionables/Gema.controller");

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            go.AddComponent<AmethystGem>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabsMineralesDistintivos(int layerColeccionable)
        {
            // Aguilarita (N1)
            CrearMineralPrefab("Aguilarita", "Assets/Sprites/Coleccionables/Aguilarita.png", "Assets/Controladores/Coleccionables/Aguilarita.controller",
                "Aguilarita (Ag₄SeS): mineral de plata y selenio. La encontró Ponciano Aguilar en la mina San Carlos, vecina de Rayas, y se describió en 1891.", 1, layerColeccionable);

            // Pepita de oro (N2)
            CrearMineralPrefab("Pepita_Oro", "Assets/Sprites/Coleccionables/Pepita_Oro.png", "Assets/Controladores/Coleccionables/PepitaOro.controller",
                "Pepita de oro: El Nopal no solo dio plata; también oro, zinc y cobre.", 2, layerColeccionable);

            // Pirargirita (N3)
            CrearMineralPrefab("Pirargirita", "Assets/Sprites/Coleccionables/Pirargirita.png", "Assets/Controladores/Coleccionables/Pirargirita.controller",
                "Pirargirita (Ag₃SbS₃): la 'plata roja' de Guanajuato, un mineral de plata de color rojo oscuro.", 3, layerColeccionable);
        }

        private static void CrearMineralPrefab(string nombre, string spritePath, string ctrlPath, string descripcion, int nivel, int layerColeccionable)
        {
            string path = $"Assets/Prefabs/Coleccionables/{nombre}.prefab";
            GameObject go = new GameObject(nombre);
            go.tag = Constantes.Tags.Collectible;
            go.layer = layerColeccionable;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ctrlPath);

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;

            var mineral = go.AddComponent<DistinctiveMineral>();
            mineral.ConfigurarMineral(nombre, descripcion, nivel);

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabCheckpoint()
        {
            string path = "Assets/Prefabs/Niveles/Checkpoint.prefab";
            GameObject go = new GameObject("Checkpoint");
            go.tag = Constantes.Tags.Checkpoint;

            var sr = go.AddComponent<SpriteRenderer>();
            var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Coleccionables/Checkpoint.png");
            Sprite spOff = null, spOn = null;
            foreach (var s in sprites)
            {
                if (s is Sprite sp)
                {
                    if (spOff == null) spOff = sp;
                    else if (spOn == null) spOn = sp;
                }
            }
            sr.sprite = spOff;
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1f, 2f);
            col.offset = new Vector2(0f, 1f);

            var luz = go.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightInnerRadius = 0.5f;
            luz.pointLightOuterRadius = 3f;
            luz.color = new Color(0.3f, 0.7f, 1f);
            luz.enabled = false;
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            go.AddComponent<Checkpoint>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabCheckpointCapilla()
        {
            string path = "Assets/Prefabs/Niveles/Checkpoint_Capilla.prefab";
            GameObject go = new GameObject("Checkpoint_Capilla");
            go.tag = Constantes.Tags.Checkpoint;

            var sr = go.AddComponent<SpriteRenderer>();
            var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Sprites/Coleccionables/Checkpoint_Capilla.png");
            Sprite spOff = null, spOn = null;
            foreach (var s in sprites)
            {
                if (s is Sprite sp)
                {
                    if (spOff == null) spOff = sp;
                    else if (spOn == null) spOn = sp;
                }
            }
            sr.sprite = spOff;
            sr.sortingLayerName = Constantes.SortingLayers.Objetos;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 2f);
            col.offset = new Vector2(0f, 1f);

            var luz = go.AddComponent<Light2D>();
            luz.lightType = Light2D.LightType.Point;
            luz.pointLightInnerRadius = 0.8f;
            luz.pointLightOuterRadius = 4f;
            luz.color = new Color(1f, 0.8f, 0.4f); // Luz cálida de velas
            luz.enabled = false;
            ElMineroBuilder.AsignarCapasALuz2D(luz);

            go.AddComponent<Checkpoint>();
            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabSalidaNivel()
        {
            string path = "Assets/Prefabs/Niveles/SalidaNivel.prefab";
            GameObject go = new GameObject("SalidaNivel");
            go.tag = Constantes.Tags.Exit;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Salida_Tunel.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.5f, 2.5f);
            col.offset = new Vector2(0f, 1.25f);

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

        private static void CrearPrefabLetrero()
        {
            string path = "Assets/Prefabs/Niveles/Letrero.prefab";
            GameObject go = new GameObject("Letrero");

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Entorno/Letrero.png");
            sr.sortingLayerName = Constantes.SortingLayers.Escenario;

            GameObject textGo = new GameObject("TextoLetrero");
            textGo.transform.SetParent(go.transform);
            textGo.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            var tmp = textGo.AddComponent<TextMeshPro>();
            tmp.text = "Aviso";
            tmp.fontSize = 2f;
            tmp.alignment = TextAlignmentOptions.Center;

            GuardarYDestruir(go, path);
        }

        // ====================================================================
        // ENEMIGOS
        // ====================================================================

        private static void CrearPrefabMurcielago(int layerPeligro)
        {
            string path = "Assets/Prefabs/Enemigos/Murcielago.prefab";
            GameObject go = new GameObject("Murcielago");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Murcielago.png");
            sr.sortingLayerName = Constantes.SortingLayers.Jugador;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Enemigos/Murcielago.controller");

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            go.AddComponent<BatPatrol>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabMurcielagoRojo(int layerPeligro)
        {
            string path = "Assets/Prefabs/Enemigos/Murcielago_Rojo.prefab";
            GameObject go = new GameObject("Murcielago_Rojo");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Murcielago_Rojo.png");
            sr.sortingLayerName = Constantes.SortingLayers.Jugador;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Enemigos/MurcielagoRojo.controller");

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            go.AddComponent<BatDiver>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabArana(int layerPeligro)
        {
            string path = "Assets/Prefabs/Enemigos/Arana.prefab";
            GameObject go = new GameObject("Arana");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Arana.png");
            sr.sortingLayerName = Constantes.SortingLayers.Jugador;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Enemigos/Arana.controller");

            var col = go.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.45f;

            // Hilo superior
            GameObject hiloGo = new GameObject("Hilo");
            hiloGo.transform.SetParent(go.transform);
            var srHilo = hiloGo.AddComponent<SpriteRenderer>();
            srHilo.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Hilo_Arana.png");
            srHilo.sortingLayerName = Constantes.SortingLayers.Escenario;
            srHilo.drawMode = SpriteDrawMode.Tiled;

            go.AddComponent<SpiderDrop>();

            GuardarYDestruir(go, path);
        }

        private static void CrearPrefabAlacran(int layerPeligro, int layerSuelo)
        {
            string path = "Assets/Prefabs/Enemigos/Alacran.prefab";
            GameObject go = new GameObject("Alacran");
            go.tag = Constantes.Tags.Hazard;
            go.layer = layerPeligro;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Enemigos/Alacran_Caminar.png");
            sr.sortingLayerName = Constantes.SortingLayers.Jugador;

            var anim = go.AddComponent<Animator>();
            anim.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Controladores/Enemigos/Alacran.controller");

            var col = go.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(1.2f, 0.6f);
            col.offset = new Vector2(0f, 0.3f);

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            go.AddComponent<ScorpionAI>();

            GuardarYDestruir(go, path);
        }

        // ====================================================================
        // SISTEMAS
        // ====================================================================

        private static void CrearPrefabSistemas()
        {
            string pathGm = "Assets/Prefabs/Sistemas/GameManager.prefab";
            GameObject goGm = new GameObject("GameManager");
            goGm.AddComponent<GameManager>();
            GuardarYDestruir(goGm, pathGm);

            string pathAm = "Assets/Prefabs/Sistemas/AudioManager.prefab";
            GameObject goAm = new GameObject("AudioManager");
            goAm.AddComponent<AudioManager>();
            GuardarYDestruir(goAm, pathAm);

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
