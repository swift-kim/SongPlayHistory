using System;
using System.Reflection;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory
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
            Image ____favoritesBadgeImage,
            TextMeshProUGUI ____songBpmText)
        {
            if (float.TryParse(____songBpmText.text, out float bpm))
            {
                ____songBpmText.text = bpm.ToString("0");
            }

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
                voteIcon = UnityEngine.Object.Instantiate(____favoritesBadgeImage, __instance.transform);
                voteIcon.name = "Vote";
                voteIcon.rectTransform.sizeDelta = new Vector2(2.5f, 2.5f);
                voteIcon.color = new Color(1f, 1f, 1f, 0.3f);
            }
            voteIcon.enabled = false;

            if (!isFavorite && SPHModel.Votes.TryGetValue(level.levelID.Replace("custom_level_", "").ToLower(), out var vote))
            {
                voteIcon.sprite = vote.voteType == "Upvote" ? _thumbsUp : _thumbsDown;
                voteIcon.enabled = true;
            }
        }

        public static void OnUnpatch()
        {
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
}
