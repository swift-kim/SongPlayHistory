namespace SongPlayHistory
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public bool ShowPlayCounts { get; set; } = true;
        public bool ShowFailed { get; set; } = true;
        public bool AverageAccuracy { get; set; } = true;
        public bool SortByDate { get; set; } = false;
        public bool ShowVotes { get; set; } = true;
    }
}
