using BS_Utils.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SongPlayHistory
{
    internal static class SPHModel
    {
        private static readonly string _voteFile = Path.Combine(Environment.CurrentDirectory, "UserData", "votedSongs.json");
        private static DateTime _voteLastWritten;

        public static Dictionary<string, UserVote> VoteData;

        public static string GetRecords(IDifficultyBeatmap beatmap)
        {
            var config = Plugin.Config.Value;
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";

            if (config.Scores.TryGetValue(difficulty, out IList<Score> scoreList))
            {
                // LastNote = -1 (cleared), 0 (undefined), larger (failed)
                var filteredList = config.ShowFailed ? scoreList : scoreList.Where(x => x.LastNote <= 0);

                // HoverHint max lines = 9
                var orderedList = filteredList.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore).Take(9);

                if (orderedList.Count() > 0)
                {
                    var maxRawScore = ScoreController.MaxRawScoreForNumberOfNotes(beatmap.beatmapData.notesCount);
                    StringBuilder builder = new StringBuilder(200);

                    foreach (var elem in orderedList)
                    {
                        var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(elem.Date).LocalDateTime;
                        var modifiedScore = elem.ModifiedScore + (elem.RawScore != elem.ModifiedScore ? "*" : "");
                        var notesRemaining = beatmap.beatmapData.notesCount - elem.LastNote;

                        // TODO: Create a page (or button) to delete individual (or all) records.

                        // TODO: Clean up this mess.
                        // TODO: Make the display format configurable.
                        if (elem.LastNote > 0)
                        {
                            var accuracy = elem.RawScore / (float)ScoreController.MaxRawScoreForNumberOfNotes(elem.LastNote) * 100f;
                            builder.AppendLine($"[{localDateTime.ToString("d")}] {modifiedScore} ({accuracy:0.00}%, -{notesRemaining} notes)");
                        }
                        else
                        {
                            // TODO: Neatly deal with old records (LastNote = 0).
                            var accuracy = elem.RawScore / (float)maxRawScore * 100f;
                            builder.AppendLine($"[{localDateTime.ToString("d")}] {modifiedScore} ({accuracy:0.00}%)");
                        }
                    }

                    return builder.ToString();
                }
            }

            return "No record";
        }

        public static void SaveRecord(IDifficultyBeatmap beatmap, LevelCompletionResults record)
        {
            // We now keep failed records.
            var cleared = record.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared;
            var newScore = new Score
            {
                Date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ModifiedScore = record.modifiedScore,
                RawScore = record.rawScore,
                LastNote = cleared ? -1 : record.goodCutsCount + record.badCutsCount + record.missedCount,
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
            Logger.Log.Info($"Looking for changes in {Path.GetFileName(_voteFile)}...");

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

        public class UserVote
        {
            public string key = null;
            public string voteType = null;
        }
    }
}
