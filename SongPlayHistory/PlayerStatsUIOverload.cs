using BS_Utils.Utilities;
using HMUI;
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

        private bool _isInitialized = false;
        private StandardLevelDetailViewController _levelDetailViewController;
        private GameObject _playerStatsContainer;
        private HoverHint _hoverHint;
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
            if (_isInitialized)
                return;

            // Find components of our interest.
            var flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            var resultsViewController = flowCoordinator.GetPrivateField<ResultsViewController>("_resultsViewController");
            var levelSelectionNavController = flowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            _levelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var standardLevelDetailView = _levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            _playerStatsContainer = standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");

            // Create a virtual button for score display.
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var hiddenButton = Instantiate(playButton, _playerStatsContainer.transform);
            (hiddenButton.transform as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 70.0f);
            foreach (var tf in hiddenButton.GetComponentsInChildren<Transform>())
            {
                if (new[] { "BG", "GlowContainer", "Stroke", "Text" }.Contains(tf.name))
                    Destroy(tf.gameObject);
            }
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            var hoverHintHolder = hiddenButton.GetComponentsInChildren<StackLayoutGroup>().First();
            _hoverHint = hoverHintHolder.gameObject.AddComponent<HoverHint>();
            _hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            _hoverHint.name = name;

            // Install event handlers.
            _levelDetailViewController.didChangeDifficultyBeatmapEvent -= OnDidChangeDifficultyBeatmap;
            _levelDetailViewController.didChangeDifficultyBeatmapEvent += OnDidChangeDifficultyBeatmap;
            _levelDetailViewController.didPresentContentEvent -= OnDidPresentContent;
            _levelDetailViewController.didPresentContentEvent += OnDidPresentContent;
            resultsViewController.continueButtonPressedEvent -= OnPlayResultDismiss;
            resultsViewController.continueButtonPressedEvent += OnPlayResultDismiss;
            resultsViewController.restartButtonPressedEvent -= OnPlayResultDismiss;
            resultsViewController.restartButtonPressedEvent += OnPlayResultDismiss;

            Logger.Log?.Debug($"Finished initializing {name}.");
            _isInitialized = true;
        }

        internal void Refresh()
        {
            Logger.Log?.Info("Refreshing...");

            var beatmap = _levelDetailViewController?.selectedDifficultyBeatmap;
            if (beatmap == null)
                return;

            // _hoverHint cannot be null here.
            _hoverHint.text = "No record";

            var config = Plugin.Config?.Value;
            if (config == null)
            {
                Logger.Log?.Warn($"The config provider is not initialized.");
                return;
            }

            // Read scores for the currently selected song.
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            if (config.Scores.TryGetValue(difficulty, out IList<Score> scoreList))
            {
                // Note: Max lines = 9
                var orderedList = scoreList.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore).Take(9);
                if (orderedList.Count() > 0)
                {
                    var maxRawScore = ScoreController.MaxRawScoreForNumberOfNotes(beatmap.beatmapData.notesCount);
                    StringBuilder builder = new StringBuilder(200);

                    foreach (var elem in orderedList)
                    {
                        var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(elem.Date).LocalDateTime;
                        var modifiedScore = elem.ModifiedScore + (elem.RawScore != elem.ModifiedScore ? "*" : "");
                        var accuracy = elem.RawScore / (float)maxRawScore * 100f;
                        builder.AppendLine($"[{localDateTime.ToString("g")}] {modifiedScore} ({accuracy:0.00}%)");
                    }

                    _hoverHint.text = builder.ToString();
                }
            }

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

                    // Resize and align.
                    // The original width is 70 but we only use 66 here (may be changed later).
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

                // The MenuScene is not always reloaded on saving the config.
                // In that case we have to manually restore original values.
                maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 23.4f);
                highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 23.4f, 23.3f);
                maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 46.7f, 23.3f);
            }
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
            var lastResult = resultsViewController.GetPrivateField<LevelCompletionResults>("_levelCompletionResults");

            // Do not save failed records.
            if (lastResult.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared)
            {
                var lastBeatmap = resultsViewController.GetPrivateField<IDifficultyBeatmap>("_difficultyBeatmap");
                var unixDateTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var beatmapCharacteristicName = lastBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
                var difficulty = $"{lastBeatmap.level.levelID}___{(int)lastBeatmap.difficulty}___{beatmapCharacteristicName}";
                var score = new Score
                {
                    Date = unixDateTime,
                    ModifiedScore = lastResult.modifiedScore,
                    RawScore = lastResult.rawScore,
                };

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
            }

            Refresh();
        }
    }
}
