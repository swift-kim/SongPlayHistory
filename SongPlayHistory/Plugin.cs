using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using System;
using System.Reflection;

namespace SongPlayHistory
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public const string HarmonyId = "com.github.swift-kim.SongPlayHistory";

        public static Logger Log { get; internal set; }

        private static Harmony _harmony;

        [Init]
        public Plugin(Logger logger, Config conf)
        {
            Log = logger;
            _harmony = new Harmony(HarmonyId);

            PluginConfig.Instance = conf.Generated<PluginConfig>();
            BSMLSettings.instance.AddSettingsMenu("Song Play History", $"SongPlayHistory.Settings.bsml", SettingsController.instance);

            SPHModel.InitializeRecords();
        }

        [OnStart]
        public void OnStart()
        {
            // Init after the menu scene is loaded.
            BS_Utils.Utilities.BSEvents.lateMenuSceneLoadedFresh += (o) =>
            {
                _ = new UnityEngine.GameObject(nameof(SPHController)).AddComponent<SPHController>();
            };

            ApplyHarmonyPatches(PluginConfig.Instance.ShowVotes);
        }

        [OnExit]
        public void OnExit()
        {
            SPHModel.BackupRecords();
        }

        public static void ApplyHarmonyPatches(bool enabled)
        {
            try
            {
                if (enabled && !Harmony.HasAnyPatches(HarmonyId))
                {
                    Log.Info("Applying Harmony patches...");
                    _harmony.PatchAll(Assembly.GetExecutingAssembly());
                }
                else if (!enabled && Harmony.HasAnyPatches(HarmonyId))
                {
                    Log.Info("Removing Harmony patches...");
                    _harmony.UnpatchAll(HarmonyId);

                    SetDataFromLevelAsync.OnUnpatch();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while applying Harmony patches.\n" + ex.ToString());
            }
        }
    }
}
