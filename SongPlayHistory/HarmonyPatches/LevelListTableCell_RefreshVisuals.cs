using Harmony;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("RefreshVisuals")]
    internal class LevelListTableCell_RefreshVisuals
    {
        public static void Postfix(LevelListTableCell __instance,
            bool ____selected,
            bool ____highlighted,
            Color ____beatmapCharacteristicImagesNormalColor,
            Color ____selectedHighlightElementsColor)
        {
            foreach (var image in __instance.GetComponentsInChildren<Image>())
            {
                // For performance reason, avoid using Linq.
                if (image.name != "Vote")
                    continue;

                if (____selected)
                    image.color = ____highlighted ? ____selectedHighlightElementsColor : Color.black;
                else
                    image.color = ____beatmapCharacteristicImagesNormalColor;

                break;
            }
        }
    }
}
