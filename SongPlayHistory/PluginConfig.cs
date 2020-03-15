using System.Collections.Generic;

namespace SongPlayHistory
{
    internal class PluginConfig
    {
        public bool RegenerateConfig = false;
        public bool ShowPlayCounts = true;
        public bool ShowFailed = false;
        public bool AverageAccuracy = true;
        public bool SortByDate = false;
        public bool ShowVotes = true;

        // Key: [levelID]___[difficulty]___[beatmapCharacteristicName] (e.g. PopStars___4___Standard)
        // TODO: Create a page (or button) to delete individual (or all) records.
        public Dictionary<string, IList<Score>> Scores = new Dictionary<string, IList<Score>>();
    }
}
