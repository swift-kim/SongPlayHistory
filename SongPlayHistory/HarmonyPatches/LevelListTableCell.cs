using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("SetDataFromLevelAsync", new Type[] { typeof(IPreviewBeatmapLevel), typeof(bool) })]
    internal class SetDataFromLevelAsync
    {
        private static Sprite _thumbsUp;
        private static Sprite _thumbsDown;

        public static bool Prepare()
        {
            _thumbsUp ??= LoadSpriteFromResource($"SongPlayHistory.Assets.ThumbsUp.png");
            _thumbsDown ??= LoadSpriteFromResource($"SongPlayHistory.Assets.ThumbsDown.png");

            return SPHModel.ScanVoteData();
        }

        [HarmonyAfter(new string[] { "com.kyle1413.BeatSaber.SongCore" })]
        public static void Postfix(LevelListTableCell __instance, IPreviewBeatmapLevel level, bool isFavorite,
            Image[] ____beatmapCharacteristicImages,
            BeatmapCharacteristicSO[] ____beatmapCharacteristics,
            TextMeshProUGUI ____songNameText,
            TextMeshProUGUI ____authorText)
        {
            Image voteIcon = null;
            foreach (var image in __instance.GetComponentsInChildren<Image>())
            {
                // For performance reason, avoid using Linq.
                if (image.name == "Vote")
                {
                    voteIcon = image;
                    break;
                }
            }
            if (voteIcon == null)
            {
                voteIcon = UnityEngine.Object.Instantiate(____beatmapCharacteristicImages[0], __instance.transform);
                voteIcon.name = "Vote";
                voteIcon.rectTransform.sizeDelta = new Vector2(3f, 3f);
                voteIcon.color = new Color(1f, 1f, 1f, 0.5f);
            }
            voteIcon.enabled = false;

            if (SPHModel.Votes.TryGetValue(level.levelID.Replace("custom_level_", "").ToLower(), out var vote))
            {
                float pos = -1f;
                foreach (var d in level.previewDifficultyBeatmapSets)
                {
                    if (Array.IndexOf(____beatmapCharacteristics, d.beatmapCharacteristic) >= 0)
                    {
                        pos -= d.beatmapCharacteristic.serializedName == "90Degree" ? 3.125f : 4f;
                    }
                }
                voteIcon.enabled = true;
                voteIcon.sprite = vote.voteType == "Upvote" ? _thumbsUp : _thumbsDown;
                voteIcon.rectTransform.anchoredPosition = new Vector2(pos, 0f);

                pos -= 3.5f;
                ____songNameText.rectTransform.offsetMax = new Vector2(pos, ____songNameText.rectTransform.offsetMax.y);
                ____songNameText.SetText(____songNameText.text); // Force refresh.
                ____authorText.rectTransform.offsetMax = new Vector2(pos, ____authorText.rectTransform.offsetMax.y);
                ____authorText.SetText(____authorText.text); // Force refresh.
            }
        }

        public static void OnUnpatch()
        {
            // TableCells are re-used, so manually destroy what we've created.
            foreach (var image in Resources.FindObjectsOfTypeAll<Image>())
            {
                if (image.name == "Vote")
                {
                    UnityEngine.Object.Destroy(image.gameObject);
                }
            }
        }

        private static Sprite LoadSpriteFromResource(string resourcePath)
        {
            try
            {
                using var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourcePath);
                var resource = new byte[stream.Length];
                stream.Read(resource, 0, (int)stream.Length);

                var texture = new Texture2D(2, 2);
                texture.LoadImage(resource);

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                return sprite;
            }
            catch (Exception ex)
            {
                Plugin.Log?.Error("Error while loading a resource.\n" + ex.ToString());
                return null;
            }
        }
    }

    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("RefreshVisuals")]
    internal class RefreshVisuals
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
