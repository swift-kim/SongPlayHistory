using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Logging;
using SongPlayHistory.HarmonyPatches;
using System;
using System.IO;
using System.Reflection;

namespace SongPlayHistory
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        public const string Name = "SongPlayHistory";
        public const string HarmonyId = "com.github.swift-kim.SongPlayHistory";

        public static Logger Log { get; private set; }

        private static Harmony _harmony;
        private static readonly string _configFile = Path.Combine(Environment.CurrentDirectory, "UserData", $"{Name}.json");
        private static readonly string _backupFile = Path.Combine(Environment.CurrentDirectory, "UserData", $"{Name}.bak");

        [Init]
        public Plugin(Logger logger, Config conf)
        {
            Log = logger;
            _harmony = new Harmony(HarmonyId);
            PluginConfig.Instance = conf.Generated<PluginConfig>();
        }

        [OnStart]
        public void OnStart()
        {
            BS_Utils.Utilities.BSEvents.OnLoad();
            BS_Utils.Utilities.BSEvents.menuSceneLoadedFresh += OnMenuLoadedFresh;

            ApplyHarmonyPatches(PluginConfig.Instance.ShowVotes);
        }

        [OnExit]
        public void OnExit()
        {
            BackupConfig();
        }

        private void OnMenuLoadedFresh()
        {
            BSMLSettings.instance.AddSettingsMenu("Song Play History", $"{Name}.Views.Settings.bsml", SettingsController.instance);
            SPHController.OnLoad();
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

                    // Do clean-up manually.
                    SetDataFromLevelAsync.OnUnpatch();
                }
            }
            catch (Exception ex)
            {
                Log.Error("Error while applying Harmony patches.\n" + ex.ToString());
            }
        }

        private static void BackupConfig()
        {
            if (!File.Exists(_configFile))
                return;

            try
            {
                if (File.Exists(_backupFile))
                {
                    // Compare file sizes instead of the last write time.
                    if (new FileInfo(_configFile).Length > new FileInfo(_backupFile).Length)
                    {
                        File.Copy(_configFile, _backupFile, true);
                    }
                    else
                    {
                        Log.Info("Did not overwrite the existing file.");
                    }
                }
                else
                {
                    File.Copy(_configFile, _backupFile);
                }
            }
            catch (IOException ex)
            {
                Log.Error(ex.ToString());
            }
        }
    }
}
