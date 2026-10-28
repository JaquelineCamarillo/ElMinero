using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace ElMinero.Editor
{
    /// <summary>
    /// Genera proceduralmente todos los clips de animación (.anim) y Animator Controllers (.controller)
    /// del proyecto "El Minero" utilizando los sprites importados.
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
                    "Assets/Controladores/Jugador",
                    "Assets/Controladores/Coleccionables",
                    "Assets/Controladores/Obstaculos",
                    "Assets/Controladores/Entorno",
                    "Assets/Controladores/UI"
                };

                foreach (var dir in carpetas)
                {
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                }

                // 1. Animaciones y Controlador del Jugador
                GenerarAnimacionesJugador();

                // 2. Animaciones y Controlador de Entorno (Antorcha)
                GenerarAnimacionAntorcha();

                // 3. Animaciones y Controlador de Coleccionables (Veta y Gema)
                GenerarAnimacionesColeccionables();

                // 4. Animaciones y Controlador de Obstáculos (Roca y Plataforma Quebradiza)
                GenerarAnimacionesObstaculos();

                // 5. Animación de UI (Insignia x2)
                GenerarAnimacionUI();

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
            // Cargar sprites
            Sprite[] idleSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Idle.png", 2);
            Sprite[] walkSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Caminar.png", 4);
            Sprite[] jumpSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Saltar.png", 1);
            Sprite[] fallSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Caer.png", 1);
            Sprite[] hurtSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Golpe.png", 1);
            Sprite[] dieSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Morir.png", 3);
            Sprite[] winSprites = CargarSpritesSub("Assets/Sprites/Jugador/Minero_Celebrar.png", 2);

            // Crear clips
            AnimationClip clipIdle = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Idle.anim", idleSprites, 3, true);
            AnimationClip clipWalk = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Caminar.anim", walkSprites, 10, true);
            AnimationClip clipJump = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Saltar.anim", jumpSprites, 1, false);
            AnimationClip clipFall = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Caer.anim", fallSprites, 1, false);
            AnimationClip clipHurt = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Golpe.anim", hurtSprites, 6, false);
            AnimationClip clipDie = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Morir.anim", dieSprites, 6, false);
            AnimationClip clipWin = CrearClipSprite("Assets/Animaciones/Jugador/Minero_Celebrar.anim", winSprites, 4, true);

            // Crear Animator Controller
            string ctrlPath = "Assets/Controladores/Jugador/Minero.controller";
            AnimatorController ctrl = AnimatorController.CreateAnimatorControllerAtPath(ctrlPath);

            // Parámetros obligatorios del GDD
            ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            ctrl.AddParameter("VelocityY", AnimatorControllerParameterType.Float);
            ctrl.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Dead", AnimatorControllerParameterType.Trigger);
            ctrl.AddParameter("Win", AnimatorControllerParameterType.Trigger);

            var rootSm = ctrl.layers[0].stateMachine;

            // Estados
            var stateIdle = rootSm.AddState("Idle");
            stateIdle.motion = clipIdle;
            rootSm.defaultState = stateIdle;

            var stateWalk = rootSm.AddState("Caminar");
            stateWalk.motion = clipWalk;

            var stateJump = rootSm.AddState("Saltar");
            stateJump.motion = clipJump;

            var stateFall = rootSm.AddState("Caer");
            stateFall.motion = clipFall;

            var stateHurt = rootSm.AddState("Golpe");
            stateHurt.motion = clipHurt;

            var stateDie = rootSm.AddState("Morir");
            stateDie.motion = clipDie;

            var stateWin = rootSm.AddState("Celebrar");
            stateWin.motion = clipWin;

            // Transiciones Locomoción
            // Idle <-> Caminar
            var tIdleWalk = stateIdle.AddTransition(stateWalk);
            tIdleWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            tIdleWalk.hasExitTime = false;
            tIdleWalk.duration = 0f;

            var tWalkIdle = stateWalk.AddTransition(stateIdle);
            tWalkIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
            tWalkIdle.hasExitTime = false;
            tWalkIdle.duration = 0f;

            // Salto (desde Idle y Caminar)
            var tIdleJump = stateIdle.AddTransition(stateJump);
            tIdleJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tIdleJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VelocityY");
            tIdleJump.hasExitTime = false;
            tIdleJump.duration = 0f;

            var tWalkJump = stateWalk.AddTransition(stateJump);
            tWalkJump.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tWalkJump.AddCondition(AnimatorConditionMode.Greater, 0.1f, "VelocityY");
            tWalkJump.hasExitTime = false;
            tWalkJump.duration = 0f;

            // Caída (desde Saltar, Idle o Caminar si cae de repisa)
            var tJumpFall = stateJump.AddTransition(stateFall);
            tJumpFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VelocityY");
            tJumpFall.hasExitTime = false;
            tJumpFall.duration = 0f;

            var tIdleFall = stateIdle.AddTransition(stateFall);
            tIdleFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tIdleFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VelocityY");
            tIdleFall.hasExitTime = false;
            tIdleFall.duration = 0f;

            var tWalkFall = stateWalk.AddTransition(stateFall);
            tWalkFall.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsGrounded");
            tWalkFall.AddCondition(AnimatorConditionMode.Less, -0.1f, "VelocityY");
            tWalkFall.hasExitTime = false;
            tWalkFall.duration = 0f;

            // Aterrizaje
            var tFallIdle = stateFall.AddTransition(stateIdle);
            tFallIdle.AddCondition(AnimatorConditionMode.If, 0f, "IsGrounded");
            tFallIdle.hasExitTime = false;
            tFallIdle.duration = 0f;

            // AnyState -> Golpe (Hurt)
            var tAnyHurt = rootSm.AddAnyStateTransition(stateHurt);
            tAnyHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");
            tAnyHurt.hasExitTime = false;
            tAnyHurt.duration = 0f;

            var tHurtIdle = stateHurt.AddTransition(stateIdle);
            tHurtIdle.hasExitTime = true;
            tHurtIdle.exitTime = 1f;
            tHurtIdle.duration = 0.1f;

            // AnyState -> Morir (Dead)
            var tAnyDie = rootSm.AddAnyStateTransition(stateDie);
            tAnyDie.AddCondition(AnimatorConditionMode.If, 0f, "Dead");
            tAnyDie.hasExitTime = false;
            tAnyDie.duration = 0f;

            // AnyState -> Celebrar (Win)
            var tAnyWin = rootSm.AddAnyStateTransition(stateWin);
            tAnyWin.AddCondition(AnimatorConditionMode.If, 0f, "Win");
            tAnyWin.hasExitTime = false;
            tAnyWin.duration = 0f;
        }

        // ====================================================================
        // 2. ENTORNO (Antorcha)
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

        // ====================================================================
        // 3. COLECCIONABLES (Veta y Gema)
        // ====================================================================
        private static void GenerarAnimacionesColeccionables()
        {
            // Veta de Plata
            Sprite[] vetaSprites = CargarSpritesSub("Assets/Sprites/Coleccionables/Veta_Plata.png", 3);
            AnimationClip clipVeta = CrearClipSprite("Assets/Animaciones/Coleccionables/VetaBrillo.anim", vetaSprites, 6, true);
            string ctrlVetaPath = "Assets/Controladores/Coleccionables/Veta.controller";
            AnimatorController ctrlVeta = AnimatorController.CreateAnimatorControllerAtPath(ctrlVetaPath);
            var stateVeta = ctrlVeta.layers[0].stateMachine.AddState("Brillo");
            stateVeta.motion = clipVeta;
            ctrlVeta.layers[0].stateMachine.defaultState = stateVeta;

            // Gema de Amatista
            Sprite[] gemaSprites = CargarSpritesSub("Assets/Sprites/Coleccionables/Gema_Amatista.png", 3);
            AnimationClip clipGema = CrearClipSprite("Assets/Animaciones/Coleccionables/GemaBrillo.anim", gemaSprites, 6, true);
            string ctrlGemaPath = "Assets/Controladores/Coleccionables/Gema.controller";
            AnimatorController ctrlGema = AnimatorController.CreateAnimatorControllerAtPath(ctrlGemaPath);
            var stateGema = ctrlGema.layers[0].stateMachine.AddState("Brillo");
            stateGema.motion = clipGema;
            ctrlGema.layers[0].stateMachine.defaultState = stateGema;
        }

        // ====================================================================
        // 4. OBSTÁCULOS (Roca y Plataforma Quebradiza)
        // ====================================================================
        private static void GenerarAnimacionesObstaculos()
        {
            // Roca rodando/girando
            Sprite[] rocaSprites = CargarSpritesSub("Assets/Sprites/Obstaculos/Roca.png", 4);
            AnimationClip clipRoca = CrearClipSprite("Assets/Animaciones/Obstaculos/RocaGirando.anim", rocaSprites, 12, true);
            string ctrlRocaPath = "Assets/Controladores/Obstaculos/Roca.controller";
            AnimatorController ctrlRoca = AnimatorController.CreateAnimatorControllerAtPath(ctrlRocaPath);
            var stateRoca = ctrlRoca.layers[0].stateMachine.AddState("Girando");
            stateRoca.motion = clipRoca;
            ctrlRoca.layers[0].stateMachine.defaultState = stateRoca;

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

        // ====================================================================
        // 5. UI (Insignia x2)
        // ====================================================================
        private static void GenerarAnimacionUI()
        {
            Sprite[] x2Sprites = CargarSpritesSub("Assets/Sprites/UI/Insignia_x2.png", 2);
            AnimationClip clipX2 = CrearClipSprite("Assets/Animaciones/UI/InsigniaX2Pulso.anim", x2Sprites, 4, true);

            string ctrlX2Path = "Assets/Controladores/UI/InsigniaX2.controller";
            AnimatorController ctrlX2 = AnimatorController.CreateAnimatorControllerAtPath(ctrlX2Path);
            var stateX2 = ctrlX2.layers[0].stateMachine.AddState("Pulso");
            stateX2.motion = clipX2;
            ctrlX2.layers[0].stateMachine.defaultState = stateX2;
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
