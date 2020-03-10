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
                // LastNote = -1 (cleared), 0 (undefined), n (failed)
                var filteredList = config.ShowFailed ? scoreList : scoreList.Where(s => s.LastNote <= 0);

                // HoverHint max lines = 9
                var orderedList = filteredList.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore).Take(9);

                if (orderedList.Count() > 0)
                {
                    var maxScore = ScoreController.MaxRawScoreForNumberOfNotes(beatmap.beatmapData.notesCount);
                    var builder = new StringBuilder(200);

                    foreach (var s in orderedList)
                    {
                        var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(s.Date).LocalDateTime;
                        var denom = config.AverageAccuracy && s.LastNote > 0 ? ScoreController.MaxRawScoreForNumberOfNotes(s.LastNote) : maxScore;
                        var accuracy = s.RawScore / (float)denom * 100f;
                        var param = ConcatParam((Param)s.Param);
                        if (param.Length == 0 && s.RawScore != s.ModifiedScore)
                        {
                            param = "N/A";
                        }
                        var notesRemaining = beatmap.beatmapData.notesCount - s.LastNote;

                        builder.Append($"<size=3>{localDateTime.ToString("d")}</size>");
                        builder.Append($"<size=4><color=#96ceb4ff> {s.ModifiedScore}</color></size>");
                        builder.Append($"<size=2> {param} </size>");
                        builder.Append($"<size=4><color=#ffcc5cff>{accuracy:0.00}%</color></size>");
                        if (config.ShowFailed)
                        {
                            if (s.LastNote == -1)
                                builder.Append($"<size=3><color=#d0f5fcff> cleared</color></size>");
                            else if (s.LastNote == 0) // old record (success, failed, or practice)
                                builder.Append($"<size=3><color=#c7c7c7ff> unknown</color></size>");
                            else
                                builder.Append($"<size=3><color=#ff6f69ff> {notesRemaining} notes left</color></size>");
                        }
                        builder.AppendLine();
                    }

                    return builder.ToString();
                }
            }

            return "No record";
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

        private static string ConcatParam(Param param)
        {
            if (param == Param.None)
                return "";

            var mods = new List<string>();
            if (param.HasFlag(Param.SubmissionDisabled)) mods.Add("??");
            if (param.HasFlag(Param.DisappearingArrows)) mods.Add("DA");
            if (param.HasFlag(Param.GhostNotes)) mods.Add("GN");
            if (param.HasFlag(Param.FasterSong)) mods.Add("FS");
            if (param.HasFlag(Param.NoFail)) mods.Add("NF");
            if (param.HasFlag(Param.NoObstacles)) mods.Add("NO");
            if (param.HasFlag(Param.NoBombs)) mods.Add("NB");
            if (param.HasFlag(Param.SlowerSong)) mods.Add("SS");
            if (param.HasFlag(Param.NoArrows)) mods.Add("NA");
            if (param.HasFlag(Param.InstaFail)) mods.Add("IF");
            if (param.HasFlag(Param.BatteryEnergy)) mods.Add("BE");
            if (param.HasFlag(Param.FastNotes)) mods.Add("FN");
            if (param.HasFlag(Param.StrictAngles)) mods.Add("SA");
            if (mods.Count > 4)
            {
                mods = mods.Take(3).ToList();
                mods.Add("..");
            }

            return string.Join(",", mods);
        }

        public class UserVote
        {
            public string key = null;
            public string voteType = null;
        }
    }
}
