using System;
using Harmony;
using TMPro;
using UnityEngine.UI;

namespace SongPlayHistory.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("SetDataFromLevelAsync", new Type[] { typeof(IPreviewBeatmapLevel), typeof(bool) })]
    public class LevelListTableCellOverride
    {
        private Image _thumbUp;
        private Image _thumbDown;

        [HarmonyAfter(new string[] { "com.kyle1413.BeatSaber.SongCore" })]
        static void Postfix(LevelListTableCell __instance, IPreviewBeatmapLevel level, bool isFavorite,
            ref TextMeshProUGUI ____songNameText, ref TextMeshProUGUI ____authorText)
        {

        }
    }
}
