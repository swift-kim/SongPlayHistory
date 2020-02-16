using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SongPlayHistory
{
    public class SongPlayHistory : MonoBehaviour
    {
        public static SongPlayHistory Instance;
        private SongPlayHistoryUI _ui;

        internal static void OnLoad()
        {
            if (Instance != null)
                return;

            _ = new GameObject(nameof(SongPlayHistory)).AddComponent<SongPlayHistory>();
        }

        #region Monobehaviour Messages
        /// <summary>
        /// Only ever called once, mainly used to initialize variables.
        /// </summary>
        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);

            Instance = this;
        }

        /// <summary>
        /// Only ever called once on the first frame the script is Enabled. Start is called after any other script's Awake() and before Update().
        /// </summary>
        private void Start()
        {
            BeatSaber.SoloFreePlayButton.onClick.AddListener(() =>
            {
                // Fail fast when there's any error during initialization.
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
        }
        #endregion

        private void Initialize()
        {
            if (_ui != null)
                return;

            _ui = new SongPlayHistoryUI();

            // Install event handlers.
            BeatSaber.LevelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDidChangeDifficultyBeatmap;
            BeatSaber.LevelDetailViewController.didChangeDifficultyBeatmapEvent += OnDidChangeDifficultyBeatmap;
            BeatSaber.LevelDetailViewController.didPresentContentEvent -= OnDidPresentContent;
            BeatSaber.LevelDetailViewController.didPresentContentEvent += OnDidPresentContent;
            BeatSaber.ResultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaber.ResultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;
            BeatSaber.ResultsViewController.restartButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaber.ResultsViewController.restartButtonPressedEvent += OnPlayResultDismiss;

            Logger.Log.Debug("Initialization complete.");
        }

        private void OnDidChangeDifficultyBeatmap(StandardLevelDetailViewController _, IDifficultyBeatmap beatmap)
        {
            Refresh();
        }

        private void OnDidPresentContent(StandardLevelDetailViewController _, StandardLevelDetailViewController.ContentType contentType)
        {
            if (contentType != StandardLevelDetailViewController.ContentType.Loading)
            {
                Refresh();
                BeatSaber.RefreshSongList();
            }
        }

        private void OnPlayResultDismiss(ResultsViewController resultsViewController)
        {
            var lastBeatmap = resultsViewController.GetPrivateField<IDifficultyBeatmap>("_difficultyBeatmap");
            var lastResult = resultsViewController.GetPrivateField<LevelCompletionResults>("_levelCompletionResults");

            // Do not save failed records.
            if (lastResult.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                SongPlayHistoryModel.SaveRecord(lastBeatmap, lastResult);
            }

            // The user may have voted on this song.
            HarmonyPatches.LevelListTableCell_SetDataFromLevel.UpdateData();

            Refresh();
            BeatSaber.RefreshSongList();
        }

        internal void Refresh()
        {
            Logger.Log.Debug("Refreshing...");

            var beatmap = BeatSaber.LevelDetailViewController.selectedDifficultyBeatmap;
            if (beatmap == null)
                return;

            _ui.SetHoverText(SongPlayHistoryModel.GetRecords(beatmap));

            if (Plugin.Config.Value.ShowPlayCounts)
            {
                _ui.ShowPlayCount(SongPlayHistoryModel.GetPlayCount(beatmap));
            }
            else
            {
                _ui.UnshowPlayCount();
            }
        }
    }
}
