using BS_Utils.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SongPlayHistory
{
    internal class Record
    {
        public long Date = 0L;
        public int ModifiedScore = 0;
        public int RawScore = 0;
        public int LastNote = 0;
        public int Param = 0;
    }

    [Flags]
    internal enum Param
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

    internal class UserVote
    {
        public string key = null;
        public string voteType = null;
    }

    internal static class SPHModel
    {
        public static Dictionary<string, IList<Record>> Records { get; set; } = new Dictionary<string, IList<Record>>();
        public static Dictionary<string, UserVote> Votes { get; private set; } = new Dictionary<string, UserVote>();

        private static readonly string _dataFile = Path.Combine(Environment.CurrentDirectory, "UserData", "SongPlayData.json");
        private static readonly string _voteFile = Path.Combine(Environment.CurrentDirectory, "UserData", "votedSongs.json");
        private static DateTime _voteLastWritten;

        public static List<Record> GetRecords(IDifficultyBeatmap beatmap)
        {
            var config = PluginConfig.Instance;
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";

            if (Records.TryGetValue(difficulty, out IList<Record> records))
            {
                // LastNote = -1 (cleared), 0 (undefined), n (failed)
                var filtered = config.ShowFailed ? records : records.Where(s => s.LastNote <= 0);
                var ordered = filtered.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore);
                return ordered.ToList();
            }

            return new List<Record>();
        }

        public static void SaveRecord(IDifficultyBeatmap beatmap, LevelCompletionResults result, bool submissionDisabled = false)
        {
            // We now keep failed records.
            var cleared = result.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared;

            // If submissionDisabled = true, we assume custom gameplay modifiers are applied.
            var param = ModsToParam(result.gameplayModifiers);
            param |= submissionDisabled ? Param.SubmissionDisabled : 0;

            var record = new Record
            {
                Date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ModifiedScore = result.modifiedScore,
                RawScore = result.rawScore,
                LastNote = cleared ? -1 : result.goodCutsCount + result.badCutsCount + result.missedCount,
                Param = (int)param
            };

            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";

            if (!Records.ContainsKey(difficulty))
            {
                Records.Add(difficulty, new List<Record>());
            }
            Records[difficulty].Add(record);
            SaveRecordsToFile();

            Plugin.Log.Info($"Saved a new record {difficulty} ({result.modifiedScore}).");
        }

        private static Param ModsToParam(GameplayModifiers mods)
        {
            Param param = Param.None;
            param |= mods.batteryEnergy ? Param.BatteryEnergy : 0;
            param |= mods.noFail ? Param.NoFail : 0;
            param |= mods.instaFail ? Param.InstaFail : 0;
            param |= mods.noObstacles ? Param.NoObstacles : 0;
            param |= mods.noBombs ? Param.NoBombs : 0;
            param |= mods.fastNotes ? Param.FastNotes : 0;
            param |= mods.strictAngles ? Param.StrictAngles : 0;
            param |= mods.disappearingArrows ? Param.DisappearingArrows : 0;
            param |= mods.songSpeed == GameplayModifiers.SongSpeed.Faster ? Param.FasterSong : 0;
            param |= mods.songSpeed == GameplayModifiers.SongSpeed.Slower ? Param.SlowerSong : 0;
            param |= mods.noArrows ? Param.NoArrows : 0;
            param |= mods.ghostNotes ? Param.GhostNotes : 0;
            return param;
        }

        public static int GetPlayCount(IDifficultyBeatmap beatmap)
        {
            var playerDataModel = BeatSaberUI.LevelDetailViewController.GetPrivateField<PlayerDataModel>("_playerDataModel");
            var statsList = playerDataModel.playerData.levelsStatsData;
            var stat = statsList?.FirstOrDefault(x => x.levelID == beatmap.level.levelID && x.difficulty == beatmap.difficulty);
            if (stat == null)
            {
                Plugin.Log.Warn($"{nameof(PlayerLevelStatsData)} not found for {beatmap.level.levelID} - {beatmap.difficulty}.");
                return -1;
            }
            return stat.playCount;
        }

        public static bool ScanVoteData()
        {
            Plugin.Log.Info($"Scanning {Path.GetFileName(_voteFile)}...");

            if (!File.Exists(_voteFile))
            {
                Plugin.Log.Warn("The file doesn't exist.");
                return false;
            }
            try
            {
                if (_voteLastWritten != File.GetLastWriteTime(_voteFile))
                {
                    _voteLastWritten = File.GetLastWriteTime(_voteFile);

                    var text = File.ReadAllText(_voteFile);
                    Votes = JsonConvert.DeserializeObject<Dictionary<string, UserVote>>(text);

                    Plugin.Log.Info("Update done.");
                }

                return true;
            }
            catch (Exception ex) // IOException, JsonException
            {
                Plugin.Log.Error(ex.ToString());
                return false;
            }
        }

        private static void SaveRecordsToFile()
        {
            try
            {
                if (Records.Count > 0)
                {
                    // This can be done synchronously because the overhead is small. (400 ms / 15 MB, 60 ms / 1 MB)
                    var serialized = JsonConvert.SerializeObject(Records, Formatting.Indented);
                    File.WriteAllText(_dataFile, serialized);
                }
            }
            catch (Exception ex) // IOException, JsonException
            {
                Plugin.Log.Error(ex.ToString());
            }
        }

        public static void ReadOrMigrateRecords()
        {
            // We should fail fast on an exception to prevent overwriting the existing records abnormally.
            var configFile = Path.Combine(Environment.CurrentDirectory, "UserData", $"{Plugin.Name}.json");
            if (File.Exists(configFile) && !File.Exists(_dataFile))
            {
                var config = JObject.Parse(File.ReadAllText(configFile));
                if (config.TryGetValue("Scores", out var token))
                {
                    Records = token.ToObject<Dictionary<string, IList<Record>>>();
                    SaveRecordsToFile();
                }
            }
            else if (File.Exists(_dataFile))
            {
                var text = File.ReadAllText(_dataFile);
                Records = JsonConvert.DeserializeObject<Dictionary<string, IList<Record>>>(text);
            }
        }

        public static void BackupRecords()
        {
            if (!File.Exists(_dataFile))
                return;

            var backupFile = Path.ChangeExtension(_dataFile, ".bak");
            try
            {
                if (File.Exists(backupFile))
                {
                    // Compare file sizes instead of the last modified.
                    if (new FileInfo(_dataFile).Length > new FileInfo(backupFile).Length)
                    {
                        File.Copy(_dataFile, backupFile, true);
                    }
                    else
                    {
                        Plugin.Log.Info("Did not overwrite the existing file.");
                    }
                }
                else
                {
                    File.Copy(_dataFile, backupFile);
                }
            }
            catch (IOException ex)
            {
                Plugin.Log.Error(ex.ToString());
            }
        }
    }
}
