using BeatSaberMarkupLanguage.Attributes;

namespace SongPlayHistory
{
    public class SettingsController : PersistentSingleton<SettingsController>
    {
        [UIValue("show-play-counts")]
        public bool ShowPlayCounts
        {
            get => Plugin.Config.Value.ShowPlayCounts;
            set => Plugin.Config.Value.ShowPlayCounts = value;
        }

        [UIValue("show-failed")]
        public bool ShowFailed
        {
            get => Plugin.Config.Value.ShowFailed;
            set => Plugin.Config.Value.ShowFailed = value;
        }

        [UIValue("average-accuracy")]
        public bool AverageAccuracy
        {
            get => Plugin.Config.Value.AverageAccuracy;
            set => Plugin.Config.Value.AverageAccuracy = value;
        }

        [UIValue("sort-by-date")]
        public bool SortByDate
        {
            get => Plugin.Config.Value.SortByDate;
            set => Plugin.Config.Value.SortByDate = value;
        }

        [UIValue("show-votes")]
        public bool ShowVotes
        {
            get => Plugin.Config.Value.ShowVotes;
            set => Plugin.Config.Value.ShowVotes = value;
        }

        [UIAction("#apply")]
        public void OnApply()
        {
            Logger.Log.Info("Applying settings...");

            Plugin.ConfigProvider.Store(Plugin.Config.Value);
            Plugin.ApplyHarmonyPatch(ShowVotes);
        }
    }
}
