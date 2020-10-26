using BeatSaberMarkupLanguage.Attributes;

namespace SongPlayHistory
{
    public class SettingsController : PersistentSingleton<SettingsController>
    {
        [UIValue("show-play-counts")]
        public bool ShowPlayCounts
        {
            get => PluginConfig.Instance.ShowPlayCounts;
            set => PluginConfig.Instance.ShowPlayCounts = value;
        }

        [UIValue("show-failed")]
        public bool ShowFailed
        {
            get => PluginConfig.Instance.ShowFailed;
            set => PluginConfig.Instance.ShowFailed = value;
        }

        [UIValue("average-accuracy")]
        public bool AverageAccuracy
        {
            get => PluginConfig.Instance.AverageAccuracy;
            set => PluginConfig.Instance.AverageAccuracy = value;
        }

        [UIValue("sort-by-date")]
        public bool SortByDate
        {
            get => PluginConfig.Instance.SortByDate;
            set => PluginConfig.Instance.SortByDate = value;
        }

        [UIValue("show-votes")]
        public bool ShowVotes
        {
            get => PluginConfig.Instance.ShowVotes;
            set => PluginConfig.Instance.ShowVotes = value;
        }

        [UIAction("#apply")]
        public void OnApply()
        {
            Plugin.Instance?.ApplyHarmonyPatches(ShowVotes);
        }
    }
}
