using System.Collections.Generic;

namespace SongPlayHistory
{
    internal class PluginConfig
    {
        public bool RegenerateConfig = false;
        public bool HideFailedScores = true;
        public bool ShowPlayCounts = true;
        public bool SortByDate = false;
        public long LastUpdated = 0L;

        // User scores are stored in the plugin config file.
        // Key: [levelID]___[difficulty]___[beatmapCharacteristicName] (e.g. PopStars___4___Standard)
        public Dictionary<string, IList<Score>> Scores = new Dictionary<string, IList<Score>>();
    }
}
