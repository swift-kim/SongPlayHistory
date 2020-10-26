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
        private readonly HoverHint _hoverHint;
        private readonly RectTransform _maxCombo;
        private readonly RectTransform _highscore;
        private readonly RectTransform _maxRank;
        private RectTransform _playCount;

        public SPHUI()
        {
            _maxCombo = BeatSaberUI.LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            _highscore = BeatSaberUI.LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            _maxRank = BeatSaberUI.LevelStatsView.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");

            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var hiddenButton = UnityEngine.Object.Instantiate(playButton, BeatSaberUI.LevelStatsView.transform);
            hiddenButton.name = "HoverArea";
            (hiddenButton.transform as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, 68f);
            foreach (var image in hiddenButton.GetComponentsInChildren<ImageView>())
            {
                image.color = Color.clear;
            }
            UnityEngine.Object.Destroy(hiddenButton.GetComponentInChildren<TextMeshProUGUI>());

            _hoverHint = hiddenButton.GetComponentInChildren<HoverHint>();
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            _hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            _hoverHint.text = "";
        }

        public void ShowRecords(IDifficultyBeatmap beatmap, List<Record> records)
        {
            if (records?.Count > 0)
            {
                List<Record> truncated = records.Take(10).ToList();

                var notesCount = beatmap.beatmapData.cuttableNotesType;
                var maxScore = ScoreModel.MaxRawScoreForNumberOfNotes(notesCount);
                var builder = new StringBuilder(200);

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

                _hoverHint.text = builder.ToString();
            }
            else
            {
                _hoverHint.text = "No record";
            }
        }

        private static string ConcatParam(Param param)
        {
            if (param == Param.None)
                return "";

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

        public void ShowPlayCount(int count)
        {
            if (_playCount == null)
            {
                _playCount = UnityEngine.Object.Instantiate(_maxCombo, BeatSaberUI.LevelStatsView.transform);
                _playCount.name = "PlayCount";
                var playCountTitle = _playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
                playCountTitle.SetText("Play Count");

                float width = 17f;
                _maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0f, width);
                _highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, width, width);
                _maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, width * 2, width);
                _playCount.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, width * 3, width);
            }

            var playCountValue = _playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            playCountValue.SetText(count > 0 ? count.ToString() : "-");
        }
    }
}
