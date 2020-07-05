using BS_Utils.Gameplay;
using BS_Utils.Utilities;
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
            var soloFreePlayButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloFreePlayButton");
            soloFreePlayButton.onClick.AddListener(() =>
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

            // Harmony can be used to patch StandardLevelDetailView.RefreshContent() but I'm too lazy to implement it.
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDifficultyChanged;
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent += OnDifficultyChanged;
            BeatSaberUI.LevelDetailViewController.didPresentContentEvent -= OnLevelDetailPresented;
            BeatSaberUI.LevelDetailViewController.didPresentContentEvent += OnLevelDetailPresented;

            // Don't use BSEvents.levelCleared and BSEvents.levelFailed as they are defective.
            BSEvents.gameSceneLoaded -= OnGameSceneLoaded;
            BSEvents.gameSceneLoaded += OnGameSceneLoaded;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.restartButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.restartButtonPressedEvent += OnPlayResultDismiss;

            Plugin.Log.Info("Initialization complete.");
        }

        private void OnDifficultyChanged(StandardLevelDetailViewController _, IDifficultyBeatmap beatmap)
        {
            Refresh();
        }

        private void OnLevelDetailPresented(StandardLevelDetailViewController _, StandardLevelDetailViewController.ContentType contentType)
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

        private void OnPlayResultDismiss(ResultsViewController resultsViewController)
        {
            if (_isPractice)
                return;

            var result = resultsViewController.GetPrivateField<LevelCompletionResults>("_levelCompletionResults");
            if (result.rawScore > 0)
            {
                // Actually there's no way to know if any custom modifier was applied if the user failed a song.
                var beatmap = BeatSaberUI.LevelDetailViewController.selectedDifficultyBeatmap;
                SPHModel.SaveRecord(beatmap, result, ScoreSubmission.WasDisabled);
            }
            Refresh();

            // The user may have voted on this song.
            SPHModel.ScanVoteData();
            BeatSaberUI.ReloadSongList();
        }

        private void Refresh()
        {
            Plugin.Log.Info("Refreshing data...");

            var beatmap = BeatSaberUI.LevelDetailViewController.selectedDifficultyBeatmap;
            if (beatmap == null)
                return;

            try
            {
                _pluginUI.ShowRecords(beatmap, SPHModel.GetRecords(beatmap));

                if (PluginConfig.Instance.ShowPlayCounts)
                {
                    _pluginUI.ShowPlayCount(SPHModel.GetPlayCount(beatmap));
                }
                else
                {
                    _pluginUI.UnshowPlayCount();
                }
            }
            catch (Exception ex) // Any UnityException
            {
                Plugin.Log.Error(ex.ToString());
            }
        }
    }
}
