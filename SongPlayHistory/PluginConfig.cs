using System.Collections.Generic;

namespace SongPlayHistory
{
    internal class PluginConfig
    {
        public bool RegenerateConfig = false;
        public bool ShowPlayCounts = true;
        public bool ShowVotes = true;
        public bool SortByDate = false;

        // User scores are stored in the config file.
        // Key: [levelID]___[difficulty]___[beatmapCharacteristicName] (e.g. PopStars___4___Standard)
        public Dictionary<string, IList<Score>> Scores = new Dictionary<string, IList<Score>>();
    }
}
