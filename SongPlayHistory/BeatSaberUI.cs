using BS_Utils.Utilities;
using HMUI;
using System.Linq;
using UnityEngine;

namespace SongPlayHistory
{
    internal static class BeatSaberUI
    {
        public static ResultsViewController ResultsViewController { get; private set; }
        public static LevelCollectionTableView LevelCollectionTableView { get; private set; }
        public static StandardLevelDetailViewController LevelDetailViewController { get; private set; }
        public static GameObject PlayerStatsContainer { get; private set; }

        public static void Initialize()
        {
            var soloFreePlayFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            ResultsViewController = soloFreePlayFlowCoordinator.GetPrivateField<ResultsViewController>("_resultsViewController");
            var levelSelectionNavController = soloFreePlayFlowCoordinator.GetPrivateField<LevelSelectionNavigationController>("_levelSelectionNavigationController");
            var levelCollectionViewController = levelSelectionNavController.GetPrivateField<LevelCollectionViewController>("_levelCollectionViewController");
            LevelCollectionTableView = levelCollectionViewController.GetPrivateField<LevelCollectionTableView>("_levelCollectionTableView");
            LevelDetailViewController = levelSelectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var standardLevelDetailView = LevelDetailViewController.GetPrivateField<StandardLevelDetailView>("_standardLevelDetailView");
            PlayerStatsContainer = standardLevelDetailView.GetPrivateField<GameObject>("_playerStatsContainer");
        }

        public static void ReloadSongList()
        {
            var levelCollection = LevelCollectionTableView.GetPrivateField<TableView>("_tableView");
            levelCollection.RefreshCellsContent();
        }
    }
}
