using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.ViewControllers;
using TMPro;

namespace SongPlayHistory
{
    internal class LocalDashboardController : BSMLResourceViewController
    {
        public override string ResourceName => "SongPlayHistory.Views.LocalDashboard.bsml";

        [UIComponent("some-text")]
        private TextMeshProUGUI text;

        [UIAction("press")]
        private void ButtonPress()
        {
            text.text = "Hey look, the text changed";
        }
    }
}
