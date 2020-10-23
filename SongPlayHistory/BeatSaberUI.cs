using BS_Utils.Utilities;
using HMUI;
using System.Linq;
using UnityEngine;

namespace SongPlayHistory
{
    internal static class BeatSaberUI
    {
        public static ResultsViewController ResultsViewController { get; private set; }
        public static LevelStatsView LevelStatsView { get; private set; }
        public static StandardLevelDetailViewController LevelDetailViewController { get; private set; }
        public static LevelCollectionTableView LevelCollectionTableView { get; private set; }

        public static void Initialize()
        {
            var soloFreePlayFlowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().First();
            ResultsViewController = soloFreePlayFlowCoordinator.GetPrivateField<ResultsViewController>("_resultsViewController");

            var platformLeaderboardViewController = soloFreePlayFlowCoordinator.GetPrivateField<PlatformLeaderboardViewController>("_platformLeaderboardViewController");
            LevelStatsView = platformLeaderboardViewController.GetPrivateField<LevelStatsView>("_levelStatsView");

            var levelSelectionNavController = soloFreePlayFlowCoordinator.GetPrivateField<LevelSelectionNavigationController>("levelSelectionNavigationController");
            var levelCollectionNavController = levelSelectionNavController.GetPrivateField<LevelCollectionNavigationController>("_levelCollectionNavigationController");
            LevelDetailViewController = levelCollectionNavController.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
            var levelCollectionViewController = levelCollectionNavController.GetPrivateField<LevelCollectionViewController>("_levelCollectionViewController");
            LevelCollectionTableView = levelCollectionViewController.GetPrivateField<LevelCollectionTableView>("_levelCollectionTableView");
        }

        public static void ReloadSongList()
        {
            var levelCollection = LevelCollectionTableView.GetPrivateField<TableView>("_tableView");
            levelCollection.RefreshCellsContent();
        }
    }
}
