using BeatSaberMarkupLanguage.Attributes;

namespace SongPlayHistory
{
    public class SettingsController : PersistentSingleton<SettingsController>
    {
        [UIValue("hide-failed-scores")]
        public bool HideFailedScores
        {
            get => Plugin.Config.Value.HideFailedScores;
            set => Plugin.Config.Value.HideFailedScores = value;
        }

        [UIValue("show-play-counts")]
        public bool ShowPlayCounts
        {
            get => Plugin.Config.Value.ShowPlayCounts;
            set => Plugin.Config.Value.ShowPlayCounts = value;
        }

        [UIValue("sort-by-date")]
        public bool SortByDate
        {
            get => Plugin.Config.Value.SortByDate;
            set => Plugin.Config.Value.SortByDate = value;
        }

        [UIAction("#apply")]
        public void OnApply()
        {
            Logger.Log?.Info("Saving configs...");
            Plugin.Config.Value.HideFailedScores = HideFailedScores;
            Plugin.Config.Value.ShowPlayCounts = ShowPlayCounts;
            Plugin.Config.Value.SortByDate = SortByDate;
            Plugin.ConfigProvider.Store(Plugin.Config.Value);
        }
    }
}
