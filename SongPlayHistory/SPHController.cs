using BS_Utils.Gameplay;
using BS_Utils.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    public class SPHController : MonoBehaviour
    {
        public static SPHController Instance { get; set; }

        private SPHUI _pluginUI;

        internal static void OnLoad()
        {
            if (Instance != null)
                return;

            _ = new GameObject(nameof(SPHController)).AddComponent<SPHController>();
        }

        #region MonoBehaviour Messages
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
        }
        #endregion

        private void Initialize()
        {
            // We don't have to re-initialize unless the menu scene is reloaded.
            if (_pluginUI != null)
                return;

            BeatSaberUI.Initialize();
            _pluginUI = new SPHUI();

            // Do not change these to BS_Utils.Utilities.BSEvents.* or BS_Utils.Plugin.LevelDidFinishEvent
            // unless you fully understand possible side effects (e.g. Auto Restart on Fail).
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDidChangeDifficultyBeatmap;
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent += OnDidChangeDifficultyBeatmap;
            BeatSaberUI.LevelDetailViewController.didPresentContentEvent -= OnDidPresentContent;
            BeatSaberUI.LevelDetailViewController.didPresentContentEvent += OnDidPresentContent;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.restartButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.restartButtonPressedEvent += OnPlayResultDismiss;

            Plugin.Log.Info("Initialization complete.");
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
            }
        }

        private void OnPlayResultDismiss(ResultsViewController resultsViewController)
        {
            if (resultsViewController.practice)
                return;

            var lastResult = resultsViewController.GetPrivateField<LevelCompletionResults>("_levelCompletionResults");
            if (lastResult.rawScore > 0)
            {
                var lastBeatmap = resultsViewController.GetPrivateField<IDifficultyBeatmap>("_difficultyBeatmap");
                // The values of ScoreSubmission.Disabled and ModString are automatically reset when a level is cleared.
                // Thus we use ScoreSubmission.WasDisabled to check if submission had been disabled during the last gameplay.
                bool submissionDisabled = ScoreSubmission.WasDisabled;

                SPHModel.SaveRecord(lastBeatmap, lastResult, submissionDisabled);
            }
            Refresh();

            // The user may have voted on this song.
            SPHModel.UpdateVoteData();
            BeatSaberUI.ReloadSongList();
        }

        internal void Refresh()
        {
            Plugin.Log.Info("Refreshing data...");

            var beatmap = BeatSaberUI.LevelDetailViewController.selectedDifficultyBeatmap;
            if (beatmap == null)
                return;

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
    }
}
