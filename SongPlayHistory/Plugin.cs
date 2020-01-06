using BS_Utils.Utilities;
using IPA;
using IPA.Config;
using IPA.Utilities;
using Newtonsoft.Json;
using System.IO;
using UnityEngine.SceneManagement;

namespace SongPlayHistory
{
    public class Plugin : IBeatSaberPlugin
    {
        internal static string Name => "SongPlayHistory";

        internal static IConfigProvider ConfigProvider;
        internal static Ref<PluginConfig> Config;

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
        }

        public void OnApplicationStart()
        {
            BSEvents.OnLoad();
            BSEvents.menuSceneLoadedFresh += OnMenuLoadedFresh;
        }

        private void OnMenuLoadedFresh()
        {
            Logger.Log?.Debug("OnMenuLoadedFresh");
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
    }
}
