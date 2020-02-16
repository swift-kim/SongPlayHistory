using BS_Utils.Utilities;
using HMUI;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    internal static class BeatSaber
    {
        public static Button SoloFreePlayButton;
        public static ResultsViewController ResultsViewController;
        public static LevelCollectionTableView LevelCollectionTableView;
        public static StandardLevelDetailViewController LevelDetailViewController;
        public static GameObject PlayerStatsContainer;

        static BeatSaber()
        {
            // For now, we are sure these values never change after being initialized.
            SoloFreePlayButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "SoloFreePlayButton");
            var soloFreePlayFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            ResultsViewController = soloFreePlayFlowCoordinator.GetPrivateField<ResultsViewController>("_resultsViewController");
            var levelSelectionNavController = soloFreePlayFlowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            var levelCollectionViewController = levelSelectionNavController.GetPrivateField<LevelCollectionViewController>("_levelCollectionViewController");
            LevelCollectionTableView = levelCollectionViewController.GetPrivateField<LevelCollectionTableView>("_levelCollectionTableView");
            LevelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var standardLevelDetailView = LevelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            PlayerStatsContainer = standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");
        }

        public static void RefreshSongList()
        {
            var levelCollection = LevelCollectionTableView.GetPrivateField<TableView>("_tableView");
            levelCollection.ReloadData();
        }
    }
}
