using BeatSaberMarkupLanguage;
using BS_Utils.Utilities;
using HMUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    internal class SPHUI
    {
        private LevelStatsView _levelStatsView;
        private LevelStatsView LevelStatsView
        {
            get
            {
                if (!BeatSaberUI.IsValid)
                {
                    return null;
                }
                if (BeatSaberUI.IsSolo)
                {
                    if (_levelStatsView != null)
                    {
                        UnityEngine.Object.Destroy(_levelStatsView.gameObject);
                        _levelStatsView = null;
                    }
                    return BeatSaberUI.LeaderboardLevelStatsView;
                }
                else
                {
                    if (_levelStatsView == null)
                    {
                        var template = Resources.FindObjectsOfTypeAll<LevelStatsView>().First();
                        _levelStatsView = UnityEngine.Object.Instantiate(template, BeatSaberUI.LevelCollectionTableView.transform);
                        var transform = _levelStatsView.transform as RectTransform;
                        transform.anchorMin = new Vector2(0f, 0f);
                        transform.anchorMax = new Vector2(1f, 0f);
                        transform.anchoredPosition = new Vector2(0f, -16f); // Need some margin for SongBrowser.
                        transform.sizeDelta = new Vector2(0f, 8f);
                    }
                    return _levelStatsView;
                }
            }
        }

        private HoverHint HoverHint
        {
            get
            {
                if (LevelStatsView == null)
                {
                    return null;
                }
                var hoverHint = LevelStatsView.GetComponentsInChildren<HoverHint>().FirstOrDefault(x => x.name == "HoverArea");
                if (!BeatSaberUI.IsSolo)
                {
                    // Unsupported for now.
                    var hiddenButton = hoverHint?.GetComponentInParent<Button>();
                    if (hiddenButton != null)
                    {
                        UnityEngine.Object.Destroy(hiddenButton.gameObject);
                    }
                    return null;
                }
                if (hoverHint == null)
                {
                    var template = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
                    var hiddenButton = UnityEngine.Object.Instantiate(template, LevelStatsView.transform, false);
                    hiddenButton.name = "HoverArea";
                    (hiddenButton.transform as RectTransform).sizeDelta = new Vector2(68f, 8f);
                    foreach (var image in hiddenButton.GetComponentsInChildren<ImageView>())
                    {
                        image.color = Color.clear;
                    }
                    hiddenButton.onClick.RemoveAllListeners();
                    hiddenButton.SetButtonText("");

                    hoverHint = hiddenButton.GetComponentInChildren<HoverHint>();
                    var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
                    hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
                    hoverHint.text = "";
                }
                return hoverHint;
            }
        }

        private RectTransform PlayCount
        {
            get
            {
                if (LevelStatsView == null)
                {
                    return null;
                }
                var playCount = LevelStatsView.GetComponentsInChildren<RectTransform>().FirstOrDefault(x => x.name == "PlayCount");
                if (playCount == null)
                {
                    var maxCombo = LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
                    var highscore = LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
                    var maxRank = LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");

                    playCount = UnityEngine.Object.Instantiate(maxCombo, LevelStatsView.transform);
                    playCount.name = "PlayCount";
                    var playCountTitle = playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
                    playCountTitle.SetText("Play Count");

                    const float w = 0.225f;
                    (maxCombo.transform as RectTransform).anchorMin = new Vector2(0f, .5f);
                    (maxCombo.transform as RectTransform).anchorMax = new Vector2(1 * w, .5f);
                    (highscore.transform as RectTransform).anchorMin = new Vector2(1 * w, .5f);
                    (highscore.transform as RectTransform).anchorMax = new Vector2(2 * w, .5f);
                    (maxRank.transform as RectTransform).anchorMin = new Vector2(2 * w, .5f);
                    (maxRank.transform as RectTransform).anchorMax = new Vector2(3 * w, .5f);
                    (playCount.transform as RectTransform).anchorMin = new Vector2(3 * w, .5f);
                    (playCount.transform as RectTransform).anchorMax = new Vector2(4 * w, .5f);
                }
                return playCount;
            }
        }

        public void SetRecords(IDifficultyBeatmap beatmap, List<Record> records)
        {
            if (HoverHint == null || beatmap == null)
            {
                return;
            }

            if (records?.Count > 0)
            {
                List<Record> truncated = records.Take(10).ToList();

                var notesCount = beatmap.beatmapData.cuttableNotesType;
                var maxScore = ScoreModel.MaxRawScoreForNumberOfNotes(notesCount);
                var builder = new StringBuilder(200);

                static string ConcatParam(Param param)
                {
                    if (param == Param.None)
                    {
                        return "";
                    }

                    var mods = new List<string>();
                    if (param.HasFlag(Param.SubmissionDisabled)) mods.Add("??");
                    if (param.HasFlag(Param.Multiplayer)) mods.Add("MULTI");
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
                        mods = mods.Take(3).ToList(); // Truncate
                        mods.Add("..");
                    }

                    return string.Join(",", mods);
                }

                static string Space(int len)
                {
                    var space = string.Concat(Enumerable.Repeat("_", len));
                    return $"<size=1><color=#00000000>{space}</color></size>";
                }

                foreach (var r in truncated)
                {
                    var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(r.Date).LocalDateTime;
                    var adjMaxScore = ScoreModel.MaxRawScoreForNumberOfNotes(r.LastNote);
                    var denom = PluginConfig.Instance.AverageAccuracy && r.LastNote > 0 ? adjMaxScore : maxScore;
                    var accuracy = r.RawScore / (float)denom * 100f;
                    var param = ConcatParam((Param)r.Param);
                    if (param.Length == 0 && r.RawScore != r.ModifiedScore)
                    {
                        param = "?!";
                    }
                    var notesRemaining = notesCount - r.LastNote;

                    builder.Append(Space(truncated.Count - truncated.IndexOf(r) - 1));
                    builder.Append($"<size=2.5><color=#1a252bff>{localDateTime:d}</color></size>");
                    builder.Append($"<size=3.5><color=#0f4c75ff> {r.ModifiedScore}</color></size>");
                    builder.Append($"<size=3.5><color=#368cc6ff> {accuracy:0.00}%</color></size>");
                    if (param.Length > 0)
                    {
                        builder.Append($"<size=2><color=#1a252bff> {param}</color></size>");
                    }
                    if (PluginConfig.Instance.ShowFailed)
                    {
                        if (r.LastNote == -1)
                            builder.Append($"<size=2.5><color=#1a252bff> cleared</color></size>");
                        else if (r.LastNote == 0) // old record (success, fail, or practice)
                            builder.Append($"<size=2.5><color=#584153ff> unknown</color></size>");
                        else
                            builder.Append($"<size=2.5><color=#ff5722ff> +{notesRemaining} notes</color></size>");
                    }
                    builder.Append(Space(truncated.IndexOf(r)));
                    builder.AppendLine();
                }

                HoverHint.text = builder.ToString();
            }
            else
            {
                HoverHint.text = "No record";
            }
        }

        public void SetStats(IDifficultyBeatmap beatmap, PlayerLevelStatsData stats)
        {
            if (beatmap == null || stats == null)
            {
                return;
            }

            static void SetValue(RectTransform column, string value)
            {
                if (column == null)
                {
                    return;
                }
                var text = column.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
                text.SetText(value);
            }

            if (!BeatSaberUI.IsSolo && LevelStatsView != null)
            {
                var maxCombo = LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
                var highscore = LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
                var maxRank = LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");
                var notesCount = beatmap.beatmapData.cuttableNotesType;
                var maxScore = ScoreModel.MaxRawScoreForNumberOfNotes(notesCount);
                var estimatedAcc = stats.highScore / (float)maxScore * 100f;
                SetValue(maxCombo, stats.validScore ? $"{stats.maxCombo}" : "-");
                SetValue(highscore, stats.validScore ? $"{stats.highScore} ({estimatedAcc:0.00}%)" : "-");
                SetValue(maxRank, stats.validScore ? RankModel.GetRankName(stats.maxRank) : "-");
            }
            SetValue(PlayCount, stats.validScore ? stats.playCount.ToString() : "-");
        }
    }
}
