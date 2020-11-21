using BS_Utils.Utilities;
using HMUI;
using System.Linq;
using UnityEngine;

namespace SongPlayHistory
{
    internal static class BeatSaberUI
    {
        public static ResultsViewController ResultsViewController { get; private set; }

        public static LevelStatsView LeaderboardLevelStatsView { get; private set; }

        public static StandardLevelDetailViewController LevelDetailViewController { get; private set; }

        public static LevelCollectionViewController LevelCollectionViewController { get; private set; }

        public static LevelCollectionTableView LevelCollectionTableView { get; private set; }

        private static LevelSelectionFlowCoordinator _flowCoordinator;

        public static bool IsValid => _flowCoordinator != null;

        public static bool IsSolo
        {
            get
            {
                return _flowCoordinator == null || _flowCoordinator is SoloFreePlayFlowCoordinator;
            }
            set
            {
                if (value)
                {
                    _flowCoordinator = Resources.FindObjectsOfTypeAll<SoloFreePlayFlowCoordinator>().LastOrDefault();

                    ResultsViewController = _flowCoordinator?.GetPrivateField<ResultsViewController>("_resultsViewController");
                    var leaderboardViewController = _flowCoordinator?.GetPrivateField<PlatformLeaderboardViewController>("_platformLeaderboardViewController");
                    LeaderboardLevelStatsView = leaderboardViewController?.GetPrivateField<LevelStatsView>("_levelStatsView");
                }
                else
                {
                    var parent = Resources.FindObjectsOfTypeAll<GameServerLobbyFlowCoordinator>().LastOrDefault();
                    _flowCoordinator = parent?.GetPrivateField<MultiplayerLevelSelectionFlowCoordinator>("multiplayerLevelSelectionFlowCoordinator");
                }

                var levelSelectionNavController = _flowCoordinator?.GetPrivateField<LevelSelectionNavigationController>("levelSelectionNavigationController");
                var levelCollectionNavController = levelSelectionNavController?.GetPrivateField<LevelCollectionNavigationController>("_levelCollectionNavigationController");
                LevelDetailViewController = levelCollectionNavController?.GetPrivateField<StandardLevelDetailViewController>("_levelDetailViewController");
                LevelCollectionViewController = levelCollectionNavController?.GetPrivateField<LevelCollectionViewController>("_levelCollectionViewController");
                LevelCollectionTableView = LevelCollectionViewController?.GetPrivateField<LevelCollectionTableView>("_levelCollectionTableView");
            }
        }

        public static void ReloadSongList()
        {
            if (IsValid)
            {
                LevelCollectionTableView?.GetPrivateField<TableView>("_tableView")?.RefreshCellsContent();
            }
        }
    }
}
