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
    internal class LevelListTableCell_SetDataFromLevel
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
            Logger.Log.Debug($"Looking for changes in {Path.GetFileName(_voteFile)}...");

            if (!File.Exists(_voteFile))
            {
                Logger.Log.Warn("The file doesn't exist.");
                return false;
            }
            try
            {
                if (_lastWritten != File.GetLastWriteTime(_voteFile))
                {
                    _lastWritten = File.GetLastWriteTime(_voteFile);

                    var text = File.ReadAllText(_voteFile);
                    _voteData = JsonConvert.DeserializeObject<Dictionary<string, UserVote>>(text);

                    Logger.Log.Debug("Update done.");
                }

                return true;
            }
            catch (Exception ex) // IOException, JsonException
            {
                Logger.Log.Error(ex.ToString());
                return false;
            }
        }

        public static void Prefix(LevelListTableCell __instance, IPreviewBeatmapLevel level, bool isFavorite,
            string ____settingDataFromLevelId,
            Image[] ____beatmapCharacteristicImages,
            BeatmapCharacteristicSO[] ____beatmapCharacteristics)
        {
            Logger.Log.Debug($"Prefix ____settingDataFromLevelId={____settingDataFromLevelId} level.levelID={level.levelID} ({____settingDataFromLevelId== level.levelID})");
            if (____settingDataFromLevelId == level.levelID || _voteData == null)
                return;

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
                voteIcon.color = new Color(1f, 1f, 1f, 0.5f);
            }
            voteIcon.enabled = false;

            if (_voteData.TryGetValue(level.levelID.Replace("custom_level_", "").ToLower(), out UserVote vote))
            {
                float pos = -1f;

                foreach (var d in level.previewDifficultyBeatmapSets)
                {
                    if (Array.IndexOf(____beatmapCharacteristics, d.beatmapCharacteristic) >= 0)
                    {
                        pos -= 4f;
                    }
                }

                voteIcon.enabled = true;
                voteIcon.sprite = vote.voteType == "Upvote" ? _thumbsUp : _thumbsDown;
                voteIcon.rectTransform.anchoredPosition = new Vector2(pos, 0f);
            }
        }

        [HarmonyAfter(new string[] { "com.kyle1413.BeatSaber.SongCore" })]
        public static void Postfix(LevelListTableCell __instance, IPreviewBeatmapLevel level, bool isFavorite,
            string ____settingDataFromLevelId,
            TextMeshProUGUI ____songNameText,
            TextMeshProUGUI ____authorText)
        {
            Logger.Log.Debug($"Postfix ____settingDataFromLevelId={____settingDataFromLevelId} level.levelID={level.levelID} ({____settingDataFromLevelId == level.levelID})");
            if (_voteData.ContainsKey(level.levelID.Replace("custom_level_", "").ToLower()))
            {
                ____songNameText.rectTransform.offsetMax -= new Vector2(3.5f, 0);
                ____songNameText.SetText(____songNameText.text); // FIXME: Any better way to refresh?
                ____authorText.rectTransform.offsetMax -= new Vector2(3.5f, 0);
                ____authorText.SetText(____songNameText.text);
            }
        }

        private static Sprite LoadSpriteFromResource(string resourcePath)
        {
            try
            {
                var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resourcePath);
                var resource = new byte[stream.Length];
                stream.Read(resource, 0, (int)stream.Length);

                var texture = new Texture2D(2, 2);
                texture.LoadImage(resource);

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
