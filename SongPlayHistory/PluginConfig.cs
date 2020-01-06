using System.Collections.Generic;

namespace SongPlayHistory
{
    internal class PluginConfig
    {
        public bool RegenerateConfig = false;
        public bool HideFailedScores = true;
        public bool ShowPlayCounts = true;
        public int SchemaVersion = 1;
        public long LastUpdated = 0L;

        // Using [levelID]___[difficulty]___[beatmapCharacteristicName] as a key for fast (de)serialization.
        public Dictionary<string, IList<Score>> Scores = new Dictionary<string, IList<Score>>();

        public class Score
        {
            public long date;
            public int modifiedScore;
            public int rawScore;
            public int rank;
            public bool fullCombo;
            public bool cleared;
            public int maxCombo;
        }
    }
}
