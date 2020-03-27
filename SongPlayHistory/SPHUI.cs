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
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var hiddenButton = UnityEngine.Object.Instantiate(playButton, BeatSaberUI.PlayerStatsContainer.transform);
            (hiddenButton.transform as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 70.0f);
            hiddenButton.name = "HoverArea";
            string[] gone = { "BG", "GlowContainer", "Stroke", "Text" };
            foreach (var t in hiddenButton.GetComponentsInChildren<RectTransform>().Where(x => gone.Contains(x.name)))
            {
                UnityEngine.Object.Destroy(t.gameObject);
            }
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            var hoverHintHolder = hiddenButton.GetComponentsInChildren<StackLayoutGroup>().First();
            _hoverHint = hoverHintHolder.gameObject.AddComponent<HoverHint>();
            _hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            _hoverHint.text = "";

            _maxCombo = BeatSaberUI.PlayerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            _highscore = BeatSaberUI.PlayerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            _maxRank = BeatSaberUI.PlayerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");
        }

        public void ShowRecords(IDifficultyBeatmap beatmap, List<Score> records)
        {
            if (records?.Count() > 0)
            {
                var maxScore = ScoreController.MaxRawScoreForNumberOfNotes(beatmap.beatmapData.notesCount);
                var builder = new StringBuilder(200);

                // HoverHint max lines = 9
                foreach (var r in records.Take(9))
                {
                    var localDateTime = DateTimeOffset.FromUnixTimeMilliseconds(r.Date).LocalDateTime;
                    var adjMaxScore = ScoreController.MaxRawScoreForNumberOfNotes(r.LastNote);
                    var denom = PluginConfig.Instance.AverageAccuracy && r.LastNote > 0 ? adjMaxScore : maxScore;
                    var accuracy = r.RawScore / (float)denom * 100f;
                    var param = ConcatParam((Param)r.Param);
                    if (param.Length == 0 && r.RawScore != r.ModifiedScore)
                    {
                        param = "N/A";
                    }
                    var notesRemaining = beatmap.beatmapData.notesCount - r.LastNote;

                    builder.Append($"<size=3>{localDateTime.ToString("d")}</size>");
                    builder.Append($"<size=4><color=#96ceb4ff> {r.ModifiedScore}</color></size>");
                    if (param.Length > 0)
                        builder.Append($"<size=2> {param}</size>");
                    builder.Append($"<size=4><color=#ffcc5cff> {accuracy:0.00}%</color></size>");
                    if (PluginConfig.Instance.ShowFailed)
                    {
                        if (r.LastNote == -1)
                            builder.Append($"<size=3><color=#d0f5fcff> cleared</color></size>");
                        else if (r.LastNote == 0) // old record (success, failed, or practice)
                            builder.Append($"<size=3><color=#c7c7c7ff> unknown</color></size>");
                        else
                            builder.Append($"<size=3><color=#ff6f69ff> {notesRemaining} notes left</color></size>");
                    }
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
                _playCount = UnityEngine.Object.Instantiate(_maxCombo, BeatSaberUI.PlayerStatsContainer.transform);
                _playCount.name = "PlayCount";
                var playCountTitle = _playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
                playCountTitle.SetText("Play Count");

                // Resize and align center. These values may be changed later.
                _maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 17.0f);
                _highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 15.0f, 17.0f);
                _maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 32.0f, 16.0f);
                _playCount.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 48.0f, 16.0f);
            }

            var playCountValue = _playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            playCountValue.SetText(count > 0 ? count.ToString() : "-");
        }

        public void UnshowPlayCount()
        {
            if (_playCount != null)
            {
                UnityEngine.Object.Destroy(_playCount.gameObject);
                _playCount = null;

                // The MenuScene is not always reloaded on saving the config.
                // In that case we have to manually restore original values.
                _maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 23.4f);
                _highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 23.4f, 23.3f);
                _maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 46.7f, 23.3f);
            }
        }
    }
}
