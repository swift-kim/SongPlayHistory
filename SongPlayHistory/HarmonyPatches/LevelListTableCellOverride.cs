using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SongPlayHistory.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("SetDataFromLevelAsync", new Type[] { typeof(IPreviewBeatmapLevel), typeof(bool) })]
    internal class LevelListTableCellOverride
    {
        private static string _voteFile = Path.Combine(Environment.CurrentDirectory, "UserData", "votedSongs.json");
        private static DateTime _lastWritten;
        private static Dictionary<string, UserVote> _voteData;
        private static Sprite _thumbsUp;
        private static Sprite _thumbsDown;

        private class UserVote
        {
            public string key = null;
            public string voteType = null;
        }

        public static bool Prepare()
        {
            _thumbsUp = _thumbsUp ?? LoadSpriteFromResource("SongPlayHistory.Assets.ThumbsUp.png");
            _thumbsDown = _thumbsDown ?? LoadSpriteFromResource("SongPlayHistory.Assets.ThumbsDown.png");

            return UpdateData();
        }

        public static bool UpdateData()
        {
            Logger.Log.Debug($"Checking for updates in {Path.GetFileName(_voteFile)}...");

            if (!File.Exists(_voteFile))
            {
                Logger.Log.Warn($"The file doesn't exist.");
                return false;
            }
            try
            {
                if (_lastWritten != File.GetLastWriteTime(_voteFile))
                {
                    _lastWritten = File.GetLastWriteTime(_voteFile);

                    var text = File.ReadAllText(_voteFile);
                    _voteData = JsonConvert.DeserializeObject<Dictionary<string, UserVote>>(text);

                    Logger.Log.Debug($"Update done.");
                }
                return true;
            }
            catch (Exception ex) // IOException, JsonException
            {
                Logger.Log.Error(ex.ToString());
                return false;
            }
        }

        [HarmonyAfter(new string[] { "com.kyle1413.BeatSaber.SongCore" })]
        public static void Postfix(LevelListTableCell __instance, IPreviewBeatmapLevel level,
            TextMeshProUGUI ____songNameText,
            TextMeshProUGUI ____authorText,
            Image[] ____beatmapCharacteristicImages)
        {
            // For performance reason, avoid using Linq.
            Image voteIcon = null;
            foreach (var i in __instance.GetComponentsInChildren<Image>())
            {
                if (i.name == "Vote")
                {
                    voteIcon = i;
                    break;
                }
            }
            if (voteIcon == null)
            {
                voteIcon = UnityEngine.Object.Instantiate(____beatmapCharacteristicImages[0], __instance.transform);
                voteIcon.name = "Vote";
                voteIcon.color = new Color(1f, 1f, 1f, 0.75f); // Color.white;
            }
            voteIcon.enabled = false;

            if (_voteData == null)
                return;

            if (_voteData.TryGetValue(level.levelID.Replace("custom_level_", "").ToLower(), out UserVote vote))
            {
                float num = -1f; // ____songNameText.rectTransform.offsetMax.x;

                foreach (var i in ____beatmapCharacteristicImages)
                {
                    if (i.enabled && i.rectTransform.anchoredPosition.x < num)
                    {
                        num -= i.rectTransform.sizeDelta.x + 0.5f;
                    }
                }
                num -= 5f; // icon.rectTransform.sizeDelta.x;

                voteIcon.enabled = true;
                voteIcon.sprite = vote.voteType == "Upvote" ? _thumbsUp : _thumbsDown;
                voteIcon.rectTransform.anchoredPosition = new Vector2(num, 0f);

                Logger.Log.Debug($"num={num}");

                ____songNameText.rectTransform.offsetMax = new Vector2(num, ____songNameText.rectTransform.offsetMax.y);
                ____authorText.rectTransform.offsetMax = new Vector2(num, ____authorText.rectTransform.offsetMax.y);

                ____songNameText.ForceMeshUpdate();
                ____authorText.ForceMeshUpdate();
            }

            // TODO: Icons not always white - see RefreshVisuals()
            // TODO: Not refreshed after finishing a song
        }

        private static Sprite LoadSpriteFromResource(string resourcePath)
        {
            try
            {
                var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourcePath);
                var resource = new byte[stream.Length];
                stream.Read(resource, 0, (int)stream.Length);

                var texture = new Texture2D(2, 2);
                texture.LoadImage(resource, false);

                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                return sprite;
            }
            catch (Exception ex)
            {
                Logger.Log.Error("Error while loading a resource.\n" + ex.ToString());
                return null;
            }
        }
    }
}
