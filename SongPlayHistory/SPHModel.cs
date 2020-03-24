using BS_Utils.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SongPlayHistory
{
    internal static class SPHModel
    {
        private static readonly string _voteFile = Path.Combine(Environment.CurrentDirectory, "UserData", "votedSongs.json");
        private static DateTime _voteLastWritten;

        public static Dictionary<string, UserVote> VoteData;

        public static List<Score> GetRecords(IDifficultyBeatmap beatmap)
        {
            var config = Plugin.Config.Value;
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";

            if (config.Scores.TryGetValue(difficulty, out IList<Score> records))
            {
                // LastNote = -1 (cleared), 0 (undefined), n (failed)
                var filtered = config.ShowFailed ? records : records.Where(s => s.LastNote <= 0);
                var ordered = filtered.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore);
                return ordered.ToList();
            }

            return new List<Score>();
        }

        public static void SaveRecord(IDifficultyBeatmap beatmap, LevelCompletionResults record, bool submissionDisabled = false)
        {
            // We now keep failed records.
            var cleared = record.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared;

            // If submissionDisabled = true, we assume custom gameplay modifiers are applied.
            var param = ModsToParam(record.gameplayModifiers);
            param |= submissionDisabled ? Param.SubmissionDisabled : 0;

            var newScore = new Score
            {
                Date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ModifiedScore = record.modifiedScore,
                RawScore = record.rawScore,
                LastNote = cleared ? -1 : record.goodCutsCount + record.badCutsCount + record.missedCount,
                Param = (int)param,
            };

            var config = Plugin.Config.Value;
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";

            if (!config.Scores.ContainsKey(difficulty))
            {
                config.Scores.Add(difficulty, new List<Score>());
            }
            config.Scores[difficulty].Add(newScore);

            Plugin.ConfigProvider.Store(config);
            Logger.Log.Info($"Saved a new record {difficulty} ({record.modifiedScore}).");
        }

        public static int GetPlayCount(IDifficultyBeatmap beatmap)
        {
            var playerDataModel = BeatSaberUI.LevelDetailViewController.GetPrivateField<PlayerDataModelSO>("_playerDataModel");
            var statsList = playerDataModel.playerData.levelsStatsData;
            var stat = statsList?.FirstOrDefault(x => x.levelID == beatmap.level.levelID && x.difficulty == beatmap.difficulty);
            if (stat == null)
            {
                Logger.Log.Warn($"{nameof(PlayerLevelStatsData)} not found for {beatmap.level.levelID} - {beatmap.difficulty}.");
                return -1;
            }
            return stat.playCount;
        }

        public static bool UpdateVoteData()
        {
            Logger.Log.Info($"Scanning {Path.GetFileName(_voteFile)}...");

            if (!File.Exists(_voteFile))
            {
                Logger.Log.Warn("The file doesn't exist.");
                return false;
            }
            try
            {
                if (_voteLastWritten != File.GetLastWriteTime(_voteFile))
                {
                    _voteLastWritten = File.GetLastWriteTime(_voteFile);

                    var text = File.ReadAllText(_voteFile);
                    VoteData = JsonConvert.DeserializeObject<Dictionary<string, UserVote>>(text);

                    Logger.Log.Info("Update done.");
                }

                return true;
            }
            catch (Exception ex) // IOException, JsonException
            {
                Logger.Log.Error(ex.ToString());
                return false;
            }
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

        public class UserVote
        {
            public string key = null;
            public string voteType = null;
        }
    }
}
