using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    public class SPHController : MonoBehaviour
    {
        public static SPHController Instance { get; set; }

        private SPHUI _pluginUI;
        private bool _isPractice;

        #region MonoBehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);

            DontDestroyOnLoad(this);
            Instance = this;
        }

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            var soloButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloButton");
            soloButton.onClick.AddListener(() =>
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
                return;

            BeatSaberUI.Initialize();
            _pluginUI = new SPHUI();

            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDifficultyChanged;
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent += OnDifficultyChanged;
            BeatSaberUI.LevelDetailViewController.didChangeContentEvent -= OnContentChanged;
            BeatSaberUI.LevelDetailViewController.didChangeContentEvent += OnContentChanged;

            BSEvents.gameSceneLoaded -= OnGameSceneLoaded;
            BSEvents.gameSceneLoaded += OnGameSceneLoaded;
            BS_Utils.Plugin.LevelDidFinishEvent -= OnLevelFinished;
            BS_Utils.Plugin.LevelDidFinishEvent += OnLevelFinished;
            BS_Utils.Plugin.MultiLevelDidFinishEvent -= OnMultilevelFinished;
            BS_Utils.Plugin.MultiLevelDidFinishEvent += OnMultilevelFinished;

            BeatSaberUI.ResultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;

            Plugin.Log?.Info("Initialization complete.");
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

        private void OnGameSceneLoaded()
        {
            var practiceSettings = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData?.practiceSettings;
            _isPractice = practiceSettings != null;
        }

        private void OnLevelFinished(StandardLevelScenesTransitionSetupDataSO _, LevelCompletionResults result)
        {
            if (_isPractice || Gamemode.IsPartyActive)
            {
                return;
            }
            SaveRecord(result, false);
            Refresh();
        }

        private void OnMultilevelFinished(MultiplayerLevelScenesTransitionSetupDataSO _, LevelCompletionResults result, Dictionary<string, LevelCompletionResults> __)
        {
            SaveRecord(result, true);
        }

        private void SaveRecord(LevelCompletionResults result, bool isMultiplayer)
        {
            if (result?.rawScore > 0)
            {
                // Actually there's no way to know if any custom modifier was applied if the user failed a song.
                var beatmap = BeatSaberUI.LevelDetailViewController.selectedDifficultyBeatmap;
                var submissionDisabled = ScoreSubmission.WasDisabled || ScoreSubmission.Disabled || ScoreSubmission.ProlongedDisabled;
                SPHModel.SaveRecord(beatmap, result, submissionDisabled, isMultiplayer);
            }
        }

        private void OnPlayResultDismiss(ResultsViewController _)
        {
            // The user may have voted on this map.
            SPHModel.ScanVoteData();
            BeatSaberUI.ReloadSongList();
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
