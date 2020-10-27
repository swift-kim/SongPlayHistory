using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    public class SPHController : MonoBehaviour
    {
        public static SPHController Instance { get; set; }

        private SPHUI _pluginUI;

        #region MonoBehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
            }

            DontDestroyOnLoad(this);
            Instance = this;
        }

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            var soloButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloButton");
            soloButton?.onClick.AddListener(() =>
            {
                // Fail fast when an error is encountered during initialization.
                Initialize();
            });
        }

        /// <summary>
        /// Called every frame if the script is enabled.
        /// </summary>
        private void Update()
        {
        }

        /// <summary>
        /// Called every frame after every other enabled script's Update().
        /// </summary>
        private void LateUpdate()
        {
        }

        /// <summary>
        /// Called when the script becomes enabled and active
        /// </summary>
        private void OnEnable()
        {
        }

        /// <summary>
        /// Called when the script becomes disabled or when it is being destroyed.
        /// </summary>
        private void OnDisable()
        {
        }

        /// <summary>
        /// Called when the script is being destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Instance = null;
        }
        #endregion

        private void Initialize()
        {
            // We don't have to re-initialize unless the menu scene is reloaded.
            if (_pluginUI != null)
            {
                return;
            }

            BeatSaberUI.Initialize();
            _pluginUI = new SPHUI();

            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDifficultyChanged;
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent += OnDifficultyChanged;
            BeatSaberUI.LevelDetailViewController.didChangeContentEvent -= OnContentChanged;
            BeatSaberUI.LevelDetailViewController.didChangeContentEvent += OnContentChanged;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;

            Plugin.Log?.Info("Initialization completed.");
        }

        private void OnDifficultyChanged(StandardLevelDetailViewController _, IDifficultyBeatmap beatmap)
        {
            Refresh();
        }

        private void OnContentChanged(StandardLevelDetailViewController _, StandardLevelDetailViewController.ContentType contentType)
        {
            if (contentType != StandardLevelDetailViewController.ContentType.Loading)
            {
                Refresh();
            }
        }

        private void OnPlayResultDismiss(ResultsViewController _)
        {
            // The user may have voted on this map.
            SPHModel.ScanVoteData();
            BeatSaberUI.ReloadSongList();

            Refresh();
        }

        private void Refresh()
        {
            Plugin.Log?.Info("Refreshing data...");

            var beatmap = BeatSaberUI.LevelDetailViewController.selectedDifficultyBeatmap;
            if (beatmap == null)
            {
                return;
            }

            try
            {
                _pluginUI.ShowRecords(beatmap, SPHModel.GetRecords(beatmap));

                if (PluginConfig.Instance.ShowPlayCounts)
                {
                    _pluginUI.ShowPlayCount(SPHModel.GetPlayCount(beatmap));
                }
            }
            catch (Exception ex) // Any UnityException
            {
                Plugin.Log?.Error(ex.ToString());
            }
        }
    }
}
