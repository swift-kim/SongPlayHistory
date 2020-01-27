using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    public class PlayerStatsUIOverload : MonoBehaviour
    {
        public static PlayerStatsUIOverload Instance;

        private StandardLevelDetailViewController _levelDetailViewController;
        private GameObject _playerStatsContainer;
        private RectTransform _playCountRect;

        internal static void OnLoad()
        {
            if (Instance != null)
                return;

            _ = new GameObject(nameof(PlayerStatsUIOverload)).AddComponent<PlayerStatsUIOverload>();
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
                try
                {
                    Initialize();
                }
                catch (Exception ex)
                {
                    Logger.Log?.Debug(ex);
                }
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
            // Find components of our interest.
            var flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            var resultsViewController = flowCoordinator.GetPrivateField<ResultsViewController>("_resultsViewController");
            var levelSelectionNavController = flowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            _levelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var standardLevelDetailView = _levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            _playerStatsContainer = standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");

            // Create a button.
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var moreButton = Instantiate(playButton, _playerStatsContainer.transform);
            moreButton.name = "MoreButton";
            moreButton.SetButtonText("...");
            moreButton.SetButtonTextSize(2.0f);
            moreButton.onClick.RemoveAllListeners();
            moreButton.onClick.AddListener(() =>
            {
                var viewController = BeatSaberUI.CreateViewController<LocalDashboardController>();
                flowCoordinator.InvokeMethod("PresentViewController", new object[] { viewController, null, false });
            });
            (moreButton.transform as RectTransform).sizeDelta = new Vector2(6.0f, 6.0f);
            (moreButton.transform as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 0.0f, 6.0f);

            // Install event handlers.
            _levelDetailViewController.didChangeDifficultyBeatmapEvent += OnDidChangeDifficultyBeatmap;
            _levelDetailViewController.didPresentContentEvent += OnDidPresentContent;
            resultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;
            resultsViewController.restartButtonPressedEvent += OnPlayResultDismiss;

            Logger.Log?.Debug($"Finished initializing {name}.");
        }

        internal void Refresh()
        {
            var beatmap = _levelDetailViewController?.selectedDifficultyBeatmap;
            if (beatmap == null)
                return;

            if (_playerStatsContainer == null)
                return;

            var config = Plugin.Config?.Value;
            if (config == null)
            {
                Logger.Log?.Warn($"The config provider is not initialized.");
                return;
            }

            // Read scores for the currently selected song from the plugin config.
            // TODO: Deprecated.
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            if (config.Scores.TryGetValue(difficulty, out IList<Score> scoreList))
            {
                var scores = scoreList.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore).Take(8);
                if (scores.Count() > 0)
                {
                    StringBuilder builder = new StringBuilder(200);
                    builder.AppendLine("Date / Score / Rank");

                    foreach (var score in scores)
                    {
                        var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(score.Date).LocalDateTime;
                        builder.AppendLine($"{localDateTime.ToString("g")} / {score.ModifiedScore} / {(RankModel.Rank)score.Rank}");
                    }

                    //StatsHoverHint.text = builder.ToString();
                }
            }

            // What are inside PlayerStatsContainer:
            // - Stats [RectTransform/LayoutElement]
            //   - MaxCombo, Highscore, MaxRank [RectTransform]
            //     - Title, Value [RectTransform/(Localized)TextMeshProUGUI]
            var maxCombo = _playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            var highscore = _playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            var maxRank = _playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");

            if (config.ShowPlayCounts)
            {
                if (_playCountRect == null)
                {
                    _playCountRect = Instantiate(maxCombo, _playerStatsContainer.transform);
                    _playCountRect.name = "PlayCount";
                    var playCountTitle = _playCountRect.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
                    playCountTitle.SetText("Play Count");

                    // Resize and align. The initial width is 64.
                    maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 16.5f);
                    highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 14.5f, 16.5f);
                    maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 31.0f, 16.5f);
                    _playCountRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 47.5f, 16.5f);
                }

                var playCountValue = _playCountRect.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
                playCountValue.SetText("-");

                // Read play counts for the selected song from PlayerDataModelSO.
                var playerDataModel = _levelDetailViewController?.GetPrivateField<PlayerDataModelSO>("_playerDataModel");
                if (playerDataModel?.playerData == null)
                    return;

                var statsList = playerDataModel.playerData.levelsStatsData;
                var stat = statsList?.FirstOrDefault(x => x.levelID == beatmap.level.levelID && x.difficulty == beatmap.difficulty);
                if (stat == null)
                {
                    Logger.Log?.Warn($"{nameof(PlayerLevelStatsData)} not found for {beatmap.level.levelID} - {beatmap.difficulty}.");
                }
                else
                {
                    playCountValue.SetText(stat.playCount > 0 ? stat.playCount.ToString() : "-");
                }
            }
            else if (_playCountRect != null)
            {
                Destroy(_playCountRect.gameObject);
                _playCountRect = null;

                // Resize and align.
                maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 21.4f);
                highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 21.4f, 21.3f);
                maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 42.7f, 21.3f);
            }
        }

        private void OnDidChangeDifficultyBeatmap(StandardLevelDetailViewController _, IDifficultyBeatmap beatmap)
        {
            Refresh();
        }

        private void OnDidPresentContent(StandardLevelDetailViewController _, StandardLevelDetailViewController.ContentType contentType)
        {
            Refresh();
        }

        private void OnPlayResultDismiss(ResultsViewController resultsViewController)
        {
            // Get the last play result.
            var lastResult = resultsViewController.GetPrivateField<LevelCompletionResults>("_levelCompletionResults");
            var lastBeatmap = resultsViewController.GetPrivateField<IDifficultyBeatmap>("_difficultyBeatmap");
            var unixDateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var beatmapCharacteristicName = lastBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{lastBeatmap.level.levelID}___{(int)lastBeatmap.difficulty}___{beatmapCharacteristicName}";
            var score = new Score
            {
                Date = unixDateTime,
                ModifiedScore = lastResult.modifiedScore,
                RawScore = lastResult.rawScore,
                Rank = (int)lastResult.rank,
                FullCombo = lastResult.fullCombo,
                MissedCount = lastResult.missedCount,
                MaxCombo = lastResult.maxCombo
            };

            // Save the result to the plugin config file.
            var config = Plugin.Config?.Value;
            if (config == null || Plugin.ConfigProvider == null)
            {
                Logger.Log?.Warn($"The config provider is not initialized. Unable to save scores.");
            }
            else
            {
                if (!config.Scores.ContainsKey(difficulty))
                {
                    config.Scores.Add(difficulty, new List<Score>());
                }
                config.Scores[difficulty].Add(score);

                Plugin.ConfigProvider.Store(config);
            }

            Refresh();
        }
    }
}
