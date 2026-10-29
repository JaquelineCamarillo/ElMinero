using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace ElMinero.Editor
{
    /// <summary>
    /// Automatización de Build Settings y compilación de Builds para múltiples plataformas
    /// (Windows Standalone .exe, WebGL y Android APK) según los requisitos del proyecto.
    /// </summary>
    public static class BuildSettingsSetup
    {
        private static readonly string[] EscenasDelJuego = new string[]
        {
            "Assets/Scenes/Splash.unity",
            "Assets/Scenes/MenuPrincipal.unity",
            "Assets/Scenes/Intro.unity",
            "Assets/Scenes/Nivel1_MinaRayas.unity",
            "Assets/Scenes/Nivel2_ElNopal.unity",
            "Assets/Scenes/Nivel3_LaValenciana.unity",
            "Assets/Scenes/Final.unity"
        };

        [MenuItem("Herramientas/El Minero/Builds/Configurar Build Settings", priority = 80)]
        public static void ConfigurarBuildSettings()
        {
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>();

            foreach (var path in EscenasDelJuego)
            {
                if (File.Exists(path))
                {
                    buildScenes.Add(new EditorBuildSettingsScene(path, true));
                    Debug.Log($"[BuildSettings] Escena agregada a Build Settings: {path}");
                }
                else
                {
                    Debug.LogWarning($"[BuildSettings] Escena no encontrada en disco: {path}");
                }
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();

            // Configurar Player Settings generales
            PlayerSettings.companyName = "UTNG - Juana Jaqueline Camarillo";
            PlayerSettings.productName = "El Minero";
            PlayerSettings.bundleVersion = "1.0.0";

            Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sprites/UI/Minero_Retrato.png");
            if (icon != null)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { icon });
            }

            AssetDatabase.SaveAssets();
            Debug.Log("[BuildSettings] Build Settings y Player Settings configurados exitosamente.");
        }

        [MenuItem("Herramientas/El Minero/Builds/Compilar para Windows (.exe)", priority = 81)]
        public static bool CompilarWindows()
        {
            ConfigurarBuildSettings();

            string dirBuild = "Builds/Windows";
            if (!Directory.Exists(dirBuild)) Directory.CreateDirectory(dirBuild);
            string exePath = Path.Combine(dirBuild, "ElMinero.exe");

            Debug.Log($"[Build] Iniciando compilación para Windows x64 en: {exePath}...");

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EscenasDelJuego,
                locationPathName = exePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[Build] Compilación Windows EXITOSA: {summary.totalSize / (1024 * 1024)} MB en {summary.totalTime.TotalSeconds:F1}s.");
                    return true;
                }
                else
                {
                    Debug.LogError($"[Build] Compilación Windows FALLIDA con resultado: {summary.result}. Errores: {summary.totalErrors}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Build] Excepción durante la compilación de Windows: {ex.Message}");
                return false;
            }
        }

        [MenuItem("Herramientas/El Minero/Builds/Compilar para WebGL", priority = 82)]
        public static bool CompilarWebGL()
        {
            ConfigurarBuildSettings();

            string dirBuild = "Builds/WebGL";
            if (!Directory.Exists(dirBuild)) Directory.CreateDirectory(dirBuild);

            Debug.Log($"[Build] Iniciando compilación para WebGL en: {dirBuild}...");

            // Ajustes recomendados para WebGL en Unity 6
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EscenasDelJuego,
                locationPathName = dirBuild,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[Build] Compilación WebGL EXITOSA: {summary.totalSize / (1024 * 1024)} MB en {summary.totalTime.TotalSeconds:F1}s.");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[Build] Compilación WebGL finalizó con resultado: {summary.result}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Build] El módulo de WebGL no está instalado o no está disponible en este entorno: {ex.Message}");
                return false;
            }
        }

        [MenuItem("Herramientas/El Minero/Builds/Compilar para Android (.apk)", priority = 83)]
        public static bool CompilarAndroid()
        {
            ConfigurarBuildSettings();

            string dirBuild = "Builds/Android";
            if (!Directory.Exists(dirBuild)) Directory.CreateDirectory(dirBuild);
            string apkPath = Path.Combine(dirBuild, "ElMinero.apk");

            Debug.Log($"[Build] Iniciando compilación para Android APK en: {apkPath}...");

            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.utng.elminero");
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = EscenasDelJuego,
                locationPathName = apkPath,
                target = BuildTarget.Android,
                options = BuildOptions.None
            };

            try
            {
                BuildReport report = BuildPipeline.BuildPlayer(options);
                BuildSummary summary = report.summary;

                if (summary.result == BuildResult.Succeeded)
                {
                    Debug.Log($"[Build] Compilación Android EXITOSA: {summary.totalSize / (1024 * 1024)} MB en {summary.totalTime.TotalSeconds:F1}s.");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[Build] Compilación Android finalizó con resultado: {summary.result}.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[Build] El módulo de Android Build Support no está instalado o no está configurado en este entorno: {ex.Message}");
                return false;
            }
        }

        [MenuItem("Herramientas/El Minero/Builds/Compilar todas las plataformas", priority = 84)]
        public static void CompilarTodasLasPlataformas()
        {
            Debug.Log("[Build] === COMPILACIÓN DE TODAS LAS PLATAFORMAS ===");
            bool winOk = CompilarWindows();
            bool webOk = CompilarWebGL();
            bool androidOk = CompilarAndroid();

            Debug.Log($"[Build] Resumen de Compilación: Windows={(winOk ? "OK" : "NO")}, WebGL={(webOk ? "OK" : "NO")}, Android={(androidOk ? "OK" : "NO")}");
        }
    }
}
