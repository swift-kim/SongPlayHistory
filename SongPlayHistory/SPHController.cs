using BS_Utils.Utilities;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    public class SPHController : MonoBehaviour
    {
        public static SPHController Instance;
        private SPHUI _ui;

        internal static void OnLoad()
        {
            if (Instance != null)
                return;

            _ = new GameObject(nameof(SPHController)).AddComponent<SPHController>();
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
            var soloFreePlayButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloFreePlayButton");
            soloFreePlayButton.onClick.AddListener(() =>
            {
                // Fail fast if there's any error during initialization.
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
            if (_ui != null)
                return;

            BeatSaberUI.Initialize();
            _ui = new SPHUI();

            // Install event handlers.
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDidChangeDifficultyBeatmap;
            BeatSaberUI.LevelDetailViewController.didChangeDifficultyBeatmapEvent += OnDidChangeDifficultyBeatmap;
            BeatSaberUI.LevelDetailViewController.didPresentContentEvent -= OnDidPresentContent;
            BeatSaberUI.LevelDetailViewController.didPresentContentEvent += OnDidPresentContent;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.restartButtonPressedEvent -= OnPlayResultDismiss;
            BeatSaberUI.ResultsViewController.restartButtonPressedEvent += OnPlayResultDismiss;

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
            }
        }

        private void OnPlayResultDismiss(ResultsViewController resultsViewController)
        {
            var lastBeatmap = resultsViewController.GetPrivateField<IDifficultyBeatmap>("_difficultyBeatmap");
            var lastResult = resultsViewController.GetPrivateField<LevelCompletionResults>("_levelCompletionResults");

            // Do not save failed records.
            if (lastResult.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                SPHModel.SaveRecord(lastBeatmap, lastResult);
            }

            Refresh();

            // The user may have voted on this song.
            SPHModel.UpdateVoteData();
            BeatSaberUI.ReloadSongList();
        }

        internal void Refresh()
        {
            Logger.Log.Debug("Refreshing data...");

            var beatmap = BeatSaberUI.LevelDetailViewController.selectedDifficultyBeatmap;
            if (beatmap == null)
                return;

            _ui.SetHoverText(SPHModel.GetRecords(beatmap));

            if (Plugin.Config.Value.ShowPlayCounts)
            {
                _ui.ShowPlayCount(SPHModel.GetPlayCount(beatmap));
            }
            else
            {
                _ui.UnshowPlayCount();
            }
        }
    }
}
