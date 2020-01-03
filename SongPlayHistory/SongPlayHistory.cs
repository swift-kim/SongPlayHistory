using BS_Utils.Utilities;
using HMUI;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace SongPlayHistory
{
    /// <summary>
    /// Monobehaviours (scripts) are added to GameObjects.
    /// For a full list of Messages a Monobehaviour can receive from the game, see https://docs.unity3d.com/ScriptReference/MonoBehaviour.html.
    /// </summary>
    public class SongPlayHistory : MonoBehaviour
    {
        public static SongPlayHistory Instance;

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
                Logger.Log?.Debug($"Unable to create UI: {ex.Message}");
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
            // Search the target container.
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
            var maxComboRect = playerStatsContainer.GetComponentsInChildren<RectTransform>().FirstOrDefault(x => x.name == "MaxCombo");
            var highscoreRect = playerStatsContainer.GetComponentsInChildren<RectTransform>().FirstOrDefault(x => x.name == "Highscore");
            var maxRankRect = playerStatsContainer.GetComponentsInChildren<RectTransform>().FirstOrDefault(x => x.name == "MaxRank");

            // Create our rect.
            var playCountRect = Instantiate(maxComboRect, statsRect);
            var playCountTitleTMP = playCountRect.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
            var playCountValueTMP = playCountRect.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            playCountRect.name = "PlayCount";
            playCountTitleTMP.SetText("Play Count");
            playCountValueTMP.SetText("0");

            // Resize and translate components.
            maxComboRect?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 16.0f);
            highscoreRect?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 15f, 16.0f);
            maxRankRect?.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 23f, 16.0f);
            playCountRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, 6.0f, 16.0f);
        }
    }
}
