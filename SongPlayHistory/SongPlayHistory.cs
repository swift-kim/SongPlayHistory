using BS_Utils.Utilities;
using HMUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static StandardLevelDetailViewController;

namespace SongPlayHistory
{
    public class SongPlayHistory : MonoBehaviour
    {
        public static SongPlayHistory Instance;

        private TextMeshProUGUI playCountValue;
        private HoverHint statsHoverHint; // The hint text should be in less than 10 lines.

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
            try
            {
                Initialize();
                Logger.Log?.Debug($"Finished initializing {name}.");
            }
            catch (Exception ex)
            {
                Logger.Log?.Debug($"Unable to initialize {name}: {ex.Message}");
                Logger.Log?.Debug(ex);
            }
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

        /// <summary>
        /// </summary>
        /// <exception cref="InvalidOperationException">Fail fast if anything goes wrong.</exception>
        private void Initialize()
        {
            // Note: I'm new to Unity so please correct my code if anything looks weird.
            // Reveal some existing components.
            var flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            var levelSelectionNavController = flowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            var levelCollectionViewController = levelSelectionNavController.GetPrivateField<LevelCollectionViewController>("_levelCollectionViewController");
            var levelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var standardLevelDetailView = levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            var statsContainer = standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");

            // What are inside statsContainer:
            // - Stats [RectTransform/LayoutElement]
            //   - MaxCombo, Highscore, MaxRank [RectTransform]
            //     - Title, Value [RectTransform/(Localized)TextMeshProUGUI]
            var maxCombo = statsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            var highscore = statsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            var maxRank = statsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");

            // Add our component.
            var playCount = Instantiate(maxCombo, statsContainer.transform);
            playCount.name = "PlayCount";
            var playCountTitle = playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
            playCountValue = playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            playCountTitle.SetText("Play Count");
            playCountValue.SetText("-");

            // Resize and align components.
            // The full width of statsRect is 72, but we need some padding at the left/right ends.
            maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 16.0f);
            highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 15.0f, 16.0f);
            maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 32.0f, 16.0f);
            playCount.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 6.0f, 16.0f);

            // Install handlers.
            levelCollectionViewController.didSelectLevelEvent += OnDidSelectLevelEvent;
            levelDetailViewController.didPresentContentEvent += OnDidPresentContentEvent;
            levelDetailViewController.didChangeDifficultyBeatmapEvent += OnDidChangeDifficultyBeatmapEvent;

            // Create a HoverHint.
            // TODO: Avoid the use of invisible button.
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var hiddenButton = Instantiate(playButton, statsContainer.transform);
            var hiddenButtonWrapper = hiddenButton.GetComponentsInChildren<RectTransform>().First(x => x.name == "Wrapper");
            var hiddenButtonStroke = hiddenButton.GetComponentsInChildren<RectTransform>().First(x => x.name == "Stroke");
            var hiddenButtonGlow = hiddenButton.GetComponentsInChildren<RectTransform>().First(x => x.name == "GlowContainer");
            var hiddenButtonText = hiddenButton.GetComponentsInChildren<RectTransform>().First(x => x.name == "Text");
            hiddenButtonWrapper.anchoredPosition = new Vector2(18.5f, -4.4f);
            hiddenButtonWrapper.sizeDelta = (statsContainer.transform as RectTransform).sizeDelta;
            Destroy(hiddenButtonStroke.gameObject);
            Destroy(hiddenButtonGlow.gameObject);
            Destroy(hiddenButtonText.gameObject);
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            var hoverHintHolder = hiddenButton.GetComponentsInChildren<StackLayoutGroup>().First();
            statsHoverHint = hoverHintHolder.gameObject.AddComponent<HoverHint>();
            statsHoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            statsHoverHint.name = name;
            statsHoverHint.text = "No record";
        }

        private void OnDidSelectLevelEvent(LevelCollectionViewController controller, IPreviewBeatmapLevel level)
        {
            Logger.Log?.Debug($"OnDidSelectLevelEvent {level.songName}");
        }

        private void OnDidPresentContentEvent(StandardLevelDetailViewController controller, ContentType type)
        {
            Logger.Log?.Debug($"OnDidPresentContentEvent {type}");
        }

        private void OnDidChangeDifficultyBeatmapEvent(StandardLevelDetailViewController controller, IDifficultyBeatmap beatmap)
        {
            Logger.Log?.Debug($"OnDidChangeDifficultyBeatmapEvent {beatmap.level.songName} {beatmap.difficulty}");
        }
    }
}
