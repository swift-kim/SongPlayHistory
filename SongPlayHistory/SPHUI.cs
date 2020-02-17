using BS_Utils.Utilities;
using HMUI;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
{
    internal class SPHUI
    {
        private HoverHint _hoverHint;
        private RectTransform _maxCombo;
        private RectTransform _highscore;
        private RectTransform _maxRank;
        private RectTransform _playCount;

        public SPHUI()
        {
            var playButton = Resources.FindObjectsOfTypeAll<Button>().First(x => x.name == "PlayButton");
            var hiddenButton = Object.Instantiate(playButton, BeatSaber.PlayerStatsContainer.transform);
            (hiddenButton.transform as RectTransform).SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 0.0f, 70.0f);
            foreach (var t in hiddenButton.GetComponentsInChildren<Transform>().Where(x => new[] { "BG", "GlowContainer", "Stroke", "Text" }.Contains(x.name)))
            {
                Object.Destroy(t.gameObject);
            }
            var hoverHintController = Resources.FindObjectsOfTypeAll<HoverHintController>().First();
            var hoverHintHolder = hiddenButton.GetComponentsInChildren<StackLayoutGroup>().First();
            _hoverHint = hoverHintHolder.gameObject.AddComponent<HoverHint>();
            _hoverHint.SetPrivateField("_hoverHintController", hoverHintController);
            _hoverHint.text = "History";

            _maxCombo = BeatSaber.PlayerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxCombo");
            _highscore = BeatSaber.PlayerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "Highscore");
            _maxRank = BeatSaber.PlayerStatsContainer.GetComponentsInChildren<RectTransform>().First(x => x.name == "MaxRank");
        }

        public void SetHoverText(string text)
        {
            _hoverHint.text = text;
        }

        public void ShowPlayCount(int count)
        {
            if (_playCount == null)
            {
                _playCount = Object.Instantiate(_maxCombo, BeatSaber.PlayerStatsContainer.transform);
                _playCount.name = "PlayCount";
                var playCountTitle = _playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Title");
                playCountTitle.SetText("Play Count");

                // Resize and align.
                // The original width is 70 but we only use 66 here (may be changed later).
                _maxCombo.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, -2.0f, 16.5f);
                _highscore.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 14.5f, 16.5f);
                _maxRank.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 31.0f, 16.5f);
                _playCount.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 47.5f, 16.5f);
            }

            var playCountValue = _playCount.GetComponentsInChildren<TextMeshProUGUI>().First(x => x.name == "Value");
            playCountValue.SetText(count > 0 ? count.ToString() : "-");
        }

        public void UnshowPlayCount()
        {
            if (_playCount != null)
            {
                Object.Destroy(_playCount.gameObject);
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
