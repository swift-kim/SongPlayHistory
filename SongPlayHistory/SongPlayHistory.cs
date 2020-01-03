using BS_Utils.Utilities;
using HMUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    public class SongPlayHistory : MonoBehaviour
    {
        public static SongPlayHistory Instance;

        private TextMeshProUGUI _playCount;
        private HoverHint _playHistory;

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
                InitializeUI();
            }
            catch (Exception ex)
            {
                Logger.Log?.Debug($"Unable to initialize UI: {ex.Message}");
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
        /// <exception cref="InvalidOperationException">Fail fast if something goes wrong.</exception>
        private void InitializeUI()
        {
            // Find some existing components.
            var flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            var levelSelectionNavController = flowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            var levelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var standardLevelDetailView = levelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            var playerStatsContainer = standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");
            // Components hierarchy:
            //   Stats [RectTransform, LayoutElement]
            //   -- MaxCombo, Highscore, MaxRank [RectTransform]
            //   ---- Title, Value [RectTransform, TextMeshProUGUI, (LocalizedTextMeshProUGUI)]
            var statsRect = playerStatsContainer.GetComponentInChildren<RectTransform>();
            var maxComboRect = playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            var highscoreRect = playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            var maxRankRect = playerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");

            // Create our rect.
            var playCountRect = Instantiate(maxComboRect, statsRect);
            playCountRect.name = "PlayCount";
            var playCountTitleTMP = playCountRect.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
            playCountTitleTMP.SetText("Play Count");
            _playCount = playCountRect.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            _playCount.SetText("-");

            // Resize and translate rects.
            maxComboRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 16.0f);
            highscoreRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 15.0f, 16.0f);
            maxRankRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 23.0f, 16.0f);
            playCountRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 6.0f, 16.0f);

            // Add a hover hint.
            // FIXIT: Temporarily making use of invisible button to show a HoverHint.
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var button = Instantiate(playButton, statsRect);
            var buttonRect = button.transform as RectTransform;
            var wrapperRect = buttonRect.GetComponentsInChildren<RectTransform>().First(x => x.name == "Wrapper");
            wrapperRect.anchoredPosition = new Vector2(18.5f, -4.4f);
            wrapperRect.sizeDelta = statsRect.sizeDelta;
            var glowContainerRect = buttonRect.GetComponentsInChildren<RectTransform>().First(x => x.name == "GlowContainer");
            var strokeRect = buttonRect.GetComponentsInChildren<RectTransform>().First(x => x.name == "Stroke");
            var textRect = buttonRect.GetComponentsInChildren<RectTransform>().First(x => x.name == "Text");
            Destroy(glowContainerRect.gameObject);
            Destroy(strokeRect.gameObject);
            Destroy(textRect.gameObject);
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            var targetElement = button.GetComponentsInChildren<StackLayoutGroup>().First();
            var existingHint = targetElement.GetComponentsInChildren<HoverHint>().FirstOrDefault();
            if (existingHint != null)
                DestroyImmediate(existingHint);
            _playHistory = targetElement.gameObject.AddComponent<HoverHint>();
            _playHistory.SetPrivateField("_hoverHintController", hoverHintController);
            _playHistory.name = name;
            _playHistory.text = null; // 9 lines max
        }
    }
}
