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
            // TODO: Selection bugs after installing/uninstalling patches.
            foreach (var image in __instance.GetComponentsInChildren<Image>())
            {
                // For performance reason, avoid using Linq.
                if (image.name == "Vote")
                {
                    image.color = ____selected ?
                        ____highlighted ? ____selectedHighlightElementsColor : Color.black :
                        ____beatmapCharacteristicImagesNormalColor;
                    break;
                }
            }
        }
    }
}
