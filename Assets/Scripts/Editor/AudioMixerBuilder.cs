using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace ElMinero.Editor
{
    public static class AudioMixerBuilder
    {
        [MenuItem("Herramientas/El Minero/Crear Mezclador Principal")]
        public static AudioMixer ObtenerOCrearMezclador()
        {
            string path = "Assets/Audios/MezcladorPrincipal.mixer";
            AudioMixer mixer = AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
            if (mixer != null)
            {
                return mixer;
            }

            try
            {
                Assembly editorAsm = typeof(UnityEditor.Editor).Assembly;
                Type controllerType = editorAsm.GetType("UnityEditor.Audio.AudioMixerController");
                Type groupType = editorAsm.GetType("UnityEditor.Audio.AudioMixerGroupController");

                if (controllerType != null)
                {
                    MethodInfo createMethod = controllerType.GetMethod("CreateMixerControllerAtPath", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (createMethod != null)
                    {
                        object controller = createMethod.Invoke(null, new object[] { path });
                        if (controller != null)
                        {
                            // Master group
                            PropertyInfo masterGroupProp = controllerType.GetProperty("masterGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            object masterGroup = masterGroupProp?.GetValue(controller);

                            MethodInfo createNewGroupMethod = controllerType.GetMethod("CreateNewGroup", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { typeof(string), typeof(bool) }, null);
                            MethodInfo addChildMethod = controllerType.GetMethod("AddChildToParent", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            MethodInfo addExposedParam = controllerType.GetMethod("AddExposedParam", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                            // Crear grupo Musica
                            object musicaGroup = null;
                            if (createNewGroupMethod != null)
                            {
                                musicaGroup = createNewGroupMethod.Invoke(controller, new object[] { "Musica", true });
                                if (musicaGroup != null && masterGroup != null && addChildMethod != null)
                                {
                                    addChildMethod.Invoke(controller, new object[] { musicaGroup, masterGroup });
                                }
                            }

                            // Crear grupo Efectos
                            object efectosGroup = null;
                            if (createNewGroupMethod != null)
                            {
                                efectosGroup = createNewGroupMethod.Invoke(controller, new object[] { "Efectos", true });
                                if (efectosGroup != null && masterGroup != null && addChildMethod != null)
                                {
                                    addChildMethod.Invoke(controller, new object[] { efectosGroup, masterGroup });
                                }
                            }

                            // Exponer parámetros VolMusica y VolEfectos
                            Type exposedParamType = editorAsm.GetType("UnityEditor.Audio.ExposedAudioParameter");
                            if (exposedParamType != null && addExposedParam != null)
                            {
                                // Helper para exponer volumen de un grupo
                                ExponerVolumenDeGrupo(controller, musicaGroup, "VolMusica", editorAsm);
                                ExponerVolumenDeGrupo(controller, efectosGroup, "VolEfectos", editorAsm);
                            }

                            AssetDatabase.SaveAssets();
                            AssetDatabase.Refresh();
                            Debug.Log("[El Minero] MezcladorPrincipal.mixer creado exitosamente con grupos Musica y Efectos.");
                            return AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[El Minero] No se pudo crear AudioMixer por reflection: {ex.Message}");
            }

            return AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
        }

        private static void ExponerVolumenDeGrupo(object controller, object group, string paramName, Assembly editorAsm)
        {
            if (controller == null || group == null) return;
            try
            {
                Type groupType = group.GetType();
                MethodInfo getGUIDMethod = groupType.GetMethod("GetGUIDForVolume", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (getGUIDMethod != null)
                {
                    GUID volumeGUID = (GUID)getGUIDMethod.Invoke(group, null);

                    Type exposedParamType = editorAsm.GetType("UnityEditor.Audio.ExposedAudioParameter");
                    object exposedParam = Activator.CreateInstance(exposedParamType);
                    exposedParamType.GetField("guid", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(exposedParam, volumeGUID);
                    exposedParamType.GetField("name", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)?.SetValue(exposedParam, paramName);

                    MethodInfo addExposedParam = controller.GetType().GetMethod("AddExposedParam", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    addExposedParam?.Invoke(controller, new object[] { exposedParam });
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[El Minero] Error al exponer parámetro {paramName}: {ex.Message}");
            }
        }
    }
}
