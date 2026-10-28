using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace ElMinero.Editor
{
    /// <summary>
    /// Genera proceduralmente todos los clips de animación (.anim) y Animator Controllers (.controller)
    /// del proyecto "El Minero" utilizando los sprites importados (paquetes v1 y v2).
    /// </summary>
    public static class AnimationGenerator
    {
        [MenuItem("Herramientas/El Minero/4. Crear animaciones y Animator Controllers", priority = 40)]
        public static void GenerarTodasLasAnimaciones()
        {
            EditorUtility.DisplayProgressBar("El Minero", "Generando animaciones y controladores...", 0.1f);

            try
            {
                // Asegurar que las carpetas existen
                string[] carpetas = new string[]
                {
                    "Assets/Animaciones/Jugador",
                    "Assets/Animaciones/Coleccionables",
                    "Assets/Animaciones/Obstaculos",
                    "Assets/Animaciones/Entorno",
                    "Assets/Animaciones/UI",
                    "Assets/Animaciones/Enemigos",
                    "Assets/Controladores/Jugador",
                    "Assets/Controladores/Coleccionables",
                    "Assets/Controladores/Obstaculos",
                    "Assets/Controladores/Entorno",
                    "Assets/Controladores/UI",
                    "Assets/Controladores/Enemigos"
                };

                foreach (var dir in carpetas)
                {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }

                // 1. Animaciones y Controlador del Jugador
                GenerarAnimacionesJugador();

                // 2. Animaciones y Controlador de Entorno (Antorcha y Malacate)
                GenerarAnimacionAntorcha();
                GenerarAnimacionMalacate();

                // 3. Animaciones y Controlador de Coleccionables (Veta, Gema y Minerales Distintivos)
                GenerarAnimacionesColeccionables();
                GenerarAnimacionesMineralesDistintivos();
                GenerarAnimacionCheckpointCapilla();

                // 4. Animaciones y Controlador de Obstáculos (Roca, Plataforma Quebradiza y Fuego)
                GenerarAnimacionesObstaculos();
                GenerarAnimacionFuego();

                // 5. Animación de UI (Insignia x2)
                GenerarAnimacionUI();

                // 6. Animaciones y Controladores de Enemigos (Murciélagos, Araña, Alacrán, Vagoneta)
                GenerarAnimacionesEnemigos();

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Debug.Log("[El Minero] ¡Todas las animaciones y controladores generados exitosamente!");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }

        // ====================================================================
        // 1. JUGADOR (Minero)
        // ====================================================================
        private static void GenerarAnimacionesJugador()
        {
            Sprite[] idleSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Idle.png", 2);
            Sprite[] walkSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Caminar.png", 4);
            Sprite[] jumpSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Saltar.png", 1);
            Sprite[] fallSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Caer.png", 1);
            Sprite[] hurtSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Golpe.png", 1);
            Sprite[] dieSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Morir.png", 3);
            Sprite[] winSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Celebrar.png", 2);

            AnimationClip clipIdle = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Idle.anim", idleSprites, 3, true);
            AnimationClip clipWalk = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Caminar.anim", walkSprites, 10, true);
            AnimationClip clipJump = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Saltar.anim", jumpSprites, 1, false);
            AnimationClip clipFall = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Caer.anim", fallSprites, 1, false);
            AnimationClip clipHurt = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Golpe.anim", hurtSprites, 6, false);
            AnimationClip clipDie = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Morir.anim", dieSprites, 6, false);
            AnimationClip clipWin = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Celebrar.anim", winSprites, 4, true);

            string ctrlPath = "Assets/Controladores/Jugador/Minero.controller";
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Dead", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Win", AnimatorControllerParameterType.Trigger);

            var rootSm = ctrl.layers[0].stateMachine;

            var stIdle = rootSm.AddState("Idle");
            stIdle.motion = clipIdle;
            rootSm.defaultState = stIdle;

            var stWalk = rootSm.AddState("Caminar");
            stWalk.motion = clipWalk;

            var stJump = rootSm.AddState("Saltar");
            stJump.motion = clipJump;

            var stFall = rootSm.AddState("Caer");
            stFall.motion = clipFall;

            var stHurt = rootSm.AddState("Golpe");
            stHurt.motion = clipHurt;

            var stDie = rootSm.AddState("Morir");
            stDie.motion = clipDie;

            var stWin = rootSm.AddState("Celebrar");
            stWin.motion = clipWin;

            // Idle <-> Walk
            var tIdleToWalk = stIdle.AddTransition(stWalk);
            tIdleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            tIdleToWalk.hasExitTime = false;
            tIdleToWalk.duration = 0f;

            var tWalkToIdle = stWalk.AddTransition(stIdle);
            tWalkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            tWalkToIdle.hasExitTime = false;
            tWalkToIdle.duration = 0f;

            // Transiciones a Saltando/Cayendo
            var tAnyToJump = rootSm.AddAnyStateTransition(stJump);
            tAnyToJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tAnyToJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VelocityY");
            tAnyToJump.hasExitTime = false;
            tAnyToJump.duration = 0f;

            var tAnyToFall = rootSm.AddAnyStateTransition(stFall);
            tAnyToFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tAnyToFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VelocityY");
            tAnyToFall.hasExitTime = false;
            tAnyToFall.duration = 0f;

            // Salir de caer a Idle
            var tFallToIdle = stFall.AddTransition(stIdle);
            tFallToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            tFallToIdle.hasExitTime = false;
            tFallToIdle.duration = 0f;

            var tJumpToIdle = stJump.AddTransition(stIdle);
            tJumpToIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            tJumpToIdle.hasExitTime = false;
            tJumpToIdle.duration = 0f;

            // Daño, muerte y victoria desde AnyState
            var tAnyToHurt = rootSm.AddAnyStateTransition(stHurt);
            tAnyToHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");
            tAnyToHurt.hasExitTime = false;
            tAnyToHurt.duration = 0f;

            var tHurtToIdle = stHurt.AddTransition(stIdle);
            tHurtToIdle.hasExitTime = true;
            tHurtToIdle.exitTime = 1.0f;
            tHurtToIdle.duration = 0f;

            var tAnyToDie = rootSm.AddAnyStateTransition(stDie);
            tAnyToDie.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
            tAnyToDie.hasExitTime = false;
            tAnyToDie.duration = 0f;

            var tAnyToWin = rootSm.AddAnyStateTransition(stWin);
            tAnyToWin.AddCondition(AnimatorConditionMode.If, 0f, "Win");
            tAnyToWin.hasExitTime = false;
            tAnyToWin.duration = 0f;
        }

        // ====================================================================
        // 2. ENTORNO (Antorcha y Malacate)
        // ====================================================================
        private static void GenerarAnimacionAntorcha()
        {
            Sprite[] sprites = CargarSpritesSub("Assets/Sprites/Entorno/Antorcha.png", 3);
            AnimationClip clip = CrearClipSprite("Assets/Animaciones/Entorno/AntorchaFuego.anim", sprites, 8, true);

            string ctrlPath = "Assets/Controladores/Entorno/Antorcha.controller";
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var state = ctrl.layers[0].stateMachine.AddState("Fuego");
            state.motion = clip;
            ctrl.layers[0].stateMachine.defaultState = state;
        }

        private static void GenerarAnimacionMalacate()
        {
            Sprite[] sprites = CargarSpritesSub("Assets/Sprites/Entorno/Malacate.png", 4);
            AnimationClip clip = CrearClipSprite("Assets/Animaciones/Entorno/MalacateGirar.anim", sprites, 8, true);

            string ctrlPath = "Assets/Controladores/Entorno/Malacate.controller";
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
            var state = ctrl.layers[0].stateMachine.AddState("Girar");
            state.motion = clip;
            ctrl.layers[0].stateMachine.defaultState = state;
        }

        // ====================================================================
        // 3. COLECCIONABLES (Veta, Gema y Minerales Distintivos)
        // ====================================================================
        private static void GenerarAnimacionesColeccionables()
        {
            Sprite[] vetaSprites = CargarSpritesSub("Assets/Sprites/Coleccionables/Veta_Plata.png", 3);
            AnimationClip clipVeta = CrearClipSprite("Assets/Animaciones/Coleccionables/VetaBrillo.anim", vetaSprites, 6, true);
            string ctrlVetaPath = "Assets/Controladores/Coleccionables/Veta.controller";
            AnimatorController ctrlVeta = AnimatorController.CreateAnimatorControllerAtPath(ctrlVetaPath);
            var stateVeta = ctrlVeta.layers[0].stateMachine.AddState("Brillo");
            stateVeta.motion = clipVeta;
            ctrlVeta.layers[0].stateMachine.defaultState = stateVeta;

            Sprite[] gemaSprites = CargarSpritesSub("Assets/Sprites/Coleccionables/Gema_Amatista.png", 3);
            AnimationClip clipGema = CrearClipSprite("Assets/Animaciones/Coleccionables/GemaBrillo.anim", gemaSprites, 6, true);
            string ctrlGemaPath = "Assets/Controladores/Coleccionables/Gema.controller";
            AnimatorController ctrlGema = AnimatorController.CreateAnimatorControllerAtPath(ctrlGemaPath);
            var stateGema = ctrlGema.layers[0].stateMachine.AddState("Brillo");
            stateGema.motion = clipGema;
            ctrlGema.layers[0].stateMachine.defaultState = stateGema;
        }

        private static void GenerarAnimacionesMineralesDistintivos()
        {
            // Aguilarita (N1)
            Sprite[] spAguilarita = CargarSpritesSub("Assets/Sprites/Coleccionables/Aguilarita.png", 3);
            AnimationClip clipAguilarita = CrearClipSprite("Assets/Animaciones/Coleccionables/AguilaritaBrillo.anim", spAguilarita, 6, true);
            CrearSimpleController("Assets/Controladores/Coleccionables/Aguilarita.controller", clipAguilarita);

            // Pepita de Oro (N2)
            Sprite[] spOro = CargarSpritesSub("Assets/Sprites/Coleccionables/Pepita_Oro.png", 3);
            AnimationClip clipOro = CrearClipSprite("Assets/Animaciones/Coleccionables/PepitaOroBrillo.anim", spOro, 6, true);
            CrearSimpleController("Assets/Controladores/Coleccionables/PepitaOro.controller", clipOro);

            // Pirargirita (N3)
            Sprite[] spPirargirita = CargarSpritesSub("Assets/Sprites/Coleccionables/Pirargirita.png", 3);
            AnimationClip clipPirargirita = CrearClipSprite("Assets/Animaciones/Coleccionables/PirargiritaBrillo.anim", spPirargirita, 6, true);
            CrearSimpleController("Assets/Controladores/Coleccionables/Pirargirita.controller", clipPirargirita);
        }

        private static void GenerarAnimacionCheckpointCapilla()
        {
            Sprite[] sprites = CargarSpritesSub("Assets/Sprites/Coleccionables/Checkpoint_Capilla.png", 2);
            if (sprites != null && sprites.Length >= 2)
            {
                AnimationClip clipApagado = CrearClipSprite("Assets/Animaciones/Coleccionables/CapillaApagada.anim", new Sprite[] { sprites[0] }, 1, false);
                AnimationClip clipEncendido = CrearClipSprite("Assets/Animaciones/Coleccionables/CapillaEncendida.anim", new Sprite[] { sprites[1] }, 1, false);

                string ctrlPath = "Assets/Controladores/Coleccionables/CheckpointCapilla.controller";
                AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);
                ctrl.AddParameter("Activar", AnimatorControllerParameterType.Trigger);

                var sm = ctrl.layers[0].stateMachine;
                var stApagado = sm.AddState("Apagado");
                stApagado.motion = clipApagado;
                sm.defaultState = stApagado;

                var stEncendido = sm.AddState("Encendido");
                stEncendido.motion = clipEncendido;

                var t = stApagado.AddTransition(stEncendido);
                t.AddCondition(AnimatorConditionMode.If, 0f, "Activar");
                t.hasExitTime = false;
                t.duration = 0f;
            }
        }

        // ====================================================================
        // 4. OBSTÁCULOS
        // ====================================================================
        private static void GenerarAnimacionesObstaculos()
        {
            // Roca rodando/girando
            Sprite[] rocaSprites = CargarSpritesSub("Assets/Sprites/Obstaculos/Roca.png", 4);
            AnimationClip clipRoca = CrearClipSprite("Assets/Animaciones/Obstaculos/RocaGirando.anim", rocaSprites, 12, true);
            CrearSimpleController("Assets/Controladores/Obstaculos/Roca.controller", clipRoca);

            // Plataforma Quebradiza
            Sprite[] platSprites = CargarSpritesSub("Assets/Sprites/Plataformas/Plataforma_Quebradiza.png", 3);
            if (platSprites != null && platSprites.Length >= 3)
            {
                AnimationClip clipNormal = CrearClipSprite("Assets/Animaciones/Obstaculos/PlataformaNormal.anim", new Sprite[] { platSprites[0] }, 1, false);
                AnimationClip clipTemblando = CrearClipSprite("Assets/Animaciones/Obstaculos/PlataformaTemblando.anim", new Sprite[] { platSprites[1], platSprites[0] }, 8, true);
                AnimationClip clipColapso = CrearClipSprite("Assets/Animaciones/Obstaculos/PlataformaColapso.anim", new Sprite[] { platSprites[2] }, 1, false);

                string ctrlPlatPath = "Assets/Controladores/Obstaculos/PlataformaQuebradiza.controller";
                AnimatorController ctrlPlat = AnimatorController.CreateAnimatorControllerAtPath(ctrlPlatPath);
                ctrlPlat.AddParameter("Shake", AnimatorControllerParameterType.Trigger);
                ctrlPlat.AddParameter("Collapse", AnimatorControllerParameterType.Trigger);
                ctrlPlat.AddParameter("Reset", AnimatorControllerParameterType.Trigger);

                var sm = ctrlPlat.layers[0].stateMachine;
                var stNormal = sm.AddState("Normal");
                stNormal.motion = clipNormal;
                sm.defaultState = stNormal;

                var stShake = sm.AddState("Temblando");
                stShake.motion = clipTemblando;

                var stCollapse = sm.AddState("Colapso");
                stCollapse.motion = clipColapso;

                var t1 = stNormal.AddTransition(stShake);
                t1.AddCondition(AnimatorConditionMode.If, 0f, "Shake");
                t1.hasExitTime = false;
                t1.duration = 0f;

                var t2 = stShake.AddTransition(stCollapse);
                t2.AddCondition(AnimatorConditionMode.If, 0f, "Collapse");
                t2.hasExitTime = false;
                t2.duration = 0f;

                var t3 = stCollapse.AddTransition(stNormal);
                t3.AddCondition(AnimatorConditionMode.If, 0f, "Reset");
                t3.hasExitTime = false;
                t3.duration = 0f;
            }
        }

        private static void GenerarAnimacionFuego()
        {
            Sprite[] fuegoSprites = CargarSpritesSub("Assets/Sprites/Obstaculos/Fuego.png", 4);
            AnimationClip clipFuego = CrearClipSprite("Assets/Animaciones/Obstaculos/FuegoArder.anim", fuegoSprites, 10, true);
            CrearSimpleController("Assets/Controladores/Obstaculos/Fuego.controller", clipFuego);
        }

        // ====================================================================
        // 5. UI (Insignia x2)
        // ====================================================================
        private static void GenerarAnimacionUI()
        {
            Sprite[] x2Sprites = CargarSpritesSub("Assets/Sprites/UI/Insignia_x2.png", 2);
            AnimationClip clipX2 = CrearClipSprite("Assets/Animaciones/UI/InsigniaX2Pulso.anim", x2Sprites, 4, true);
            CrearSimpleController("Assets/Controladores/UI/InsigniaX2.controller", clipX2);
        }

        // ====================================================================
        // 6. ENEMIGOS (v2)
        // ====================================================================
        private static void GenerarAnimacionesEnemigos()
        {
            // Murciélago (16x16, 4 cuadros, 10 fps)
            Sprite[] batSprites = CargarSpritesSub("Assets/Sprites/Enemigos/Murcielago.png", 4);
            AnimationClip clipBat = CrearClipSprite("Assets/Animaciones/Enemigos/MurcielagoVolar.anim", batSprites, 10, true);
            CrearSimpleController("Assets/Controladores/Enemigos/Murcielago.controller", clipBat);

            // Murciélago Rojo (16x16, 4 cuadros, 10 fps)
            Sprite[] redBatSprites = CargarSpritesSub("Assets/Sprites/Enemigos/Murcielago_Rojo.png", 4);
            AnimationClip clipRedBat = CrearClipSprite("Assets/Animaciones/Enemigos/MurcielagoRojoVolar.anim", redBatSprites, 10, true);
            CrearSimpleController("Assets/Controladores/Enemigos/MurcielagoRojo.controller", clipRedBat);

            // Araña (16x16, 3 cuadros, 6 fps)
            Sprite[] spiderSprites = CargarSpritesSub("Assets/Sprites/Enemigos/Arana.png", 3);
            AnimationClip clipSpider = CrearClipSprite("Assets/Animaciones/Enemigos/AranaBajar.anim", spiderSprites, 6, true);
            CrearSimpleController("Assets/Controladores/Enemigos/Arana.controller", clipSpider);

            // Alacrán: Caminar (4 cuadros, 8 fps) y Ataque (2 cuadros, 6 fps, no loop)
            Sprite[] alacranWalk = CargarSpritesSub("Assets/Sprites/Enemigos/Alacran_Caminar.png", 4);
            Sprite[] alacranAttack = CargarSpritesSub("Assets/Sprites/Enemigos/Alacran_Ataque.png", 2);

            AnimationClip clipAlacranWalk = CrearClipSprite("Assets/Animaciones/Enemigos/AlacranCaminar.anim", alacranWalk, 8, true);
            AnimationClip clipAlacranAttack = CrearClipSprite("Assets/Animaciones/Enemigos/AlacranAtaque.anim", alacranAttack, 6, false);

            string ctrlAlacranPath = "Assets/Controladores/Enemigos/Alacran.controller";
            AnimatorController ctrlAlacran = AnimatorController.CreateAnimatorControllerAtPath(ctrlAlacranPath);
            ctrlAlacran.AddParameter("Atacar", AnimatorControllerParameterType.Trigger);

            var smAlacran = ctrlAlacran.layers[0].stateMachine;
            var stWalk = smAlacran.AddState("Caminar");
            stWalk.motion = clipAlacranWalk;
            smAlacran.defaultState = stWalk;

            var stAttack = smAlacran.AddState("Ataque");
            stAttack.motion = clipAlacranAttack;

            var tAttack = stWalk.AddTransition(stAttack);
            tAttack.AddCondition(AnimatorConditionMode.If, 0f, "Atacar");
            tAttack.hasExitTime = false;
            tAttack.duration = 0f;

            var tBack = stAttack.AddTransition(stWalk);
            tBack.hasExitTime = true;
            tBack.exitTime = 1.0f;
            tBack.duration = 0f;

            // Vagoneta Desbocada (32x24, 2 cuadros, 12 fps)
            Sprite[] cartSprites = CargarSpritesSub("Assets/Sprites/Enemigos/Vagoneta_Desbocada.png", 2);
            AnimationClip clipCart = CrearClipSprite("Assets/Animaciones/Obstaculos/VagonetaRodar.anim", cartSprites, 12, true);
            CrearSimpleController("Assets/Controladores/Obstaculos/Vagoneta.controller", clipCart);
        }

        private static void CrearSimpleController(string path, AnimationClip defaultClip)
        {
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(path);
            var state = ctrl.layers[0].stateMachine.AddState("Default");
            state.motion = defaultClip;
            ctrl.layers[0].stateMachine.defaultState = state;
        }

        // ====================================================================
        // HELPERS
        // ====================================================================
        public static AnimationClip CrearClipSprite(string assetPath, Sprite[] sprites, float frameRate, bool loop)
        {
            if (sprites == null || sprites.Length == 0) return null;

            AnimationClip clip = new AnimationClip();
            clip.frameRate = frameRate;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                type = typeof(SpriteRenderer),
                path = "",
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Length];
            for (int i = 0; i < sprites.Length; i++)
            {
                keyframes[i] = new ObjectReferenceKeyframe
                {
                    time = i / frameRate,
                    value = sprites[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

            if (loop)
            {
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            AssetDatabase.CreateAsset(clip, assetPath);
            return clip;
        }

        private static Sprite[] CargarSpritesSub(string texturePath, int expectedCount)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(texturePath);
            List<Sprite> sprites = new List<Sprite>();
            foreach (var a in assets)
            {
                if (a is Sprite s)
                {
                    sprites.Add(s);
                }
            }

            if (sprites.Count == 0)
            {
                Sprite single = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                if (single != null) sprites.Add(single);
            }

            return sprites.ToArray();
        }
    }
}
