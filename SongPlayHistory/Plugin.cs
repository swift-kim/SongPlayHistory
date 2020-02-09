using BeatSaberMarkupLanguage.Settings;
using BS_Utils.Utilities;
using Harmony;
using IPA;
using IPA.Config;
using IPA.Utilities;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace SongPlayHistory
{
    public class Plugin : IBeatSaberPlugin
    {
        internal const string Name = "SongPlayHistory";
        internal const string HarmonyId = "com.github.swift-kim.SongPlayHistory";

        internal static IConfigProvider ConfigProvider;
        internal static Ref<PluginConfig> Config;
        internal static HarmonyInstance Harmony;

        public void Init(IPA.Logging.Logger logger, [IPA.Config.Config.Prefer("json")] IConfigProvider configProvider)
        {
            Logger.Log = logger;

            ConfigProvider = configProvider;
            Config = ConfigProvider.MakeLink<PluginConfig>((p, v) =>
            {
                if (v.Value == null || v.Value.RegenerateConfig)
                {
                    p.Store(v.Value = new PluginConfig() { RegenerateConfig = false });
                }
                Config = v;
            });

            Harmony = HarmonyInstance.Create(HarmonyId);
        }

        public void OnApplicationStart()
        {
            Logger.Log?.Debug("OnApplicationStart");

            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuLoadedFresh;

            ApplyHarmonyPatch(Config.Value.ShowVotes);
        }

        private void OnMenuLoadedFresh()
        {
            BSMLSettings.instance.AddSettingsMenu("Song Play History", $"{Name}.Views.Settings.bsml", SettingsController.instance);

            ConfigProvider.Store(Config.Value);
            SongPlayHistory.OnLoad();
        }

        public void OnApplicationQuit()
        {
        }

        /// <summary>
        /// Runs at a fixed intervalue, generally used for physics calculations. 
        /// </summary>
        public void OnFixedUpdate()
        {
        }

        /// <summary>
        /// This is called every frame.
        /// </summary>
        public void OnUpdate()
        {
        }

        /// <summary>
        /// Called when the active scene is changed.
        /// </summary>
        /// <param name="prevScene">The scene you are transitioning from.</param>
        /// <param name="nextScene">The scene you are transitioning to.</param>
        public void OnActiveSceneChanged(Scene prevScene, Scene nextScene)
        {
        }

        /// <summary>
        /// Called when the a scene's assets are loaded.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="sceneMode"></param>
        public void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
        }

        public void OnSceneUnloaded(Scene scene)
        {
        }

        public static void ApplyHarmonyPatch(bool enabled)
        {
            try
            {
                if (enabled)
                {
                    Logger.Log.Debug("Applying Harmony patches...");
                    Harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                else
                {
                    Logger.Log.Debug("Removing Harmony patches...");
                    Harmony.UnpatchAll(HarmonyId);
                }
            }
            catch (Exception ex)
            {
                Logger.Log.Error(ex.ToString());
            }
        }
    }
}
