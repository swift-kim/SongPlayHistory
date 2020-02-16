using BS_Utils.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SongPlayHistory
{
    internal static class SongPlayHistoryModel
    {
        public static string GetRecords(IDifficultyBeatmap beatmap)
        {
            var config = Plugin.Config.Value;
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";

            if (config.Scores.TryGetValue(difficulty, out IList<Score> scoreList))
            {
                // Note: HoverHint max lines = 9
                var orderedList = scoreList.OrderByDescending(s => config.SortByDate ? s.Date : s.ModifiedScore).Take(9);
                if (orderedList.Count() > 0)
                {
                    var maxRawScore = ScoreController.MaxRawScoreForNumberOfNotes(beatmap.beatmapData.notesCount);
                    StringBuilder builder = new StringBuilder(200);

                    foreach (var elem in orderedList)
                    {
                        var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(elem.Date).LocalDateTime;
                        var modifiedScore = elem.ModifiedScore + (elem.RawScore != elem.ModifiedScore ? "*" : "");
                        var accuracy = elem.RawScore / (float)maxRawScore * 100f;
                        builder.AppendLine($"[{localDateTime.ToString("g")}] {modifiedScore} ({accuracy:0.00}%)");
                    }

                    return builder.ToString();
                }
            }

            return "No record";
        }

        public static void SaveRecord(IDifficultyBeatmap beatmap, LevelCompletionResults record)
        {
            var beatmapCharacteristicName = beatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;
            var difficulty = $"{beatmap.level.levelID}___{(int)beatmap.difficulty}___{beatmapCharacteristicName}";
            var newScore = new Score
            {
                Date = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                ModifiedScore = record.modifiedScore,
                RawScore = record.rawScore,
            };

            var config = Plugin.Config.Value;
            if (!config.Scores.ContainsKey(difficulty))
            {
                config.Scores.Add(difficulty, new List<Score>());
            }
            config.Scores[difficulty].Add(newScore);

            Plugin.ConfigProvider.Store(config);
        }

        public static int GetPlayCount(IDifficultyBeatmap beatmap)
        {
            var playerDataModel = BeatSaber.LevelDetailViewController.GetPrivateField<PlayerDataModelSO>("_playerDataModel");
            var statsList = playerDataModel.playerData.levelsStatsData;
            var stat = statsList?.FirstOrDefault(x => x.levelID == beatmap.level.levelID && x.difficulty == beatmap.difficulty);
            if (stat == null)
            {
                Logger.Log.Warn($"{nameof(PlayerLevelStatsData)} not found for {beatmap.level.levelID} - {beatmap.difficulty}.");
                return -1;
            }
            return stat.playCount;
        }
    }
}
