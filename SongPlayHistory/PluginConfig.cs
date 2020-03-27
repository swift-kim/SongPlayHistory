using System;
using System.Collections.Generic;

namespace SongPlayHistory
{
    public class PluginConfig
    {
        public static PluginConfig Instance { get; set; }

        public bool RegenerateConfig { get; set; } = false;
        public bool ShowPlayCounts { get; set; } = true;
        public bool ShowFailed { get; set; } = false;
        public bool AverageAccuracy { get; set; } = true;
        public bool SortByDate { get; set; } = false;
        public bool ShowVotes { get; set; } = true;

        // Key: [levelID]___[difficulty]___[beatmapCharacteristicName] (e.g. PopStars___4___Standard)
        // TODO: Create a page (or button) to delete individual (or all) records.
        public Dictionary<string, IList<Score>> Scores { get; set; } = new Dictionary<string, IList<Score>>();
    }

    public class Score
    {
        public long Date = 0L;
        public int ModifiedScore = 0;
        public int RawScore = 0;
        public int LastNote = 0;
        public int Param = 0;
    }

    [Flags]
    public enum Param
    {
        None = 0x0000,
        BatteryEnergy = 0x0001,
        NoFail = 0x0002,
        InstaFail = 0x0004,
        NoObstacles = 0x0008,
        NoBombs = 0x0010,
        FastNotes = 0x0020,
        StrictAngles = 0x0040,
        DisappearingArrows = 0x0080,
        FasterSong = 0x0100,
        SlowerSong = 0x0200,
        NoArrows = 0x0400,
        GhostNotes = 0x0800,
        SubmissionDisabled = 0x10000,
    }
}
