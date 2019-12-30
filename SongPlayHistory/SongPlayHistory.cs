using BS_Utils.Utilities;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

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
                CreateUI();
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
        private void CreateUI()
        {
            // Find our target container.
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

            // Apply new sizes and positions.
            // Initial width = 72 (total), 24 (each)
            // Resized width = 54 (total), 18 (each)
            maxComboRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 18.0f);
            highscoreRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 18.0f, 18.0f);
            maxRankRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 36.0f, 18.0f);

            // Create our button.
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var button = Instantiate(playButton, statsRect);
            var buttonTextRect = button.GetComponentsInChildren<RectTransform>().First(x => x.name == "Text");
            if (buttonTextRect != null)
                Destroy(buttonTextRect.gameObject);
            button.name = name;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() =>
            {
                // TODO
            });
            var buttonImage = button.GetComponentsInChildren<Image>().First(x => x.name == "Icon");
            //buttonImage.sprite = null; // TODO
        }
    }
}
