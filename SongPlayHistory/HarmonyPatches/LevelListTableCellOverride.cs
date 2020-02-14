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
        private static Image _thumbsUp;
        private static Image _thumbsDown;

        private class UserVote
        {
            public string key = null;
            public string voteType = null;
        }

        /// <summary>
        /// Called before applying this Harmony patch.
        /// </summary>
        public static bool Prepare()
        {
            if (_thumbsUp == null)
            {
                _thumbsUp = new GameObject("ThumbsUp").AddComponent<Image>();
                _thumbsUp.sprite = LoadSpriteFromResource("SongPlayHistory.Assets.ThumbsUp.png");
                _thumbsUp.color = Color.white;
                _thumbsUp.enabled = false;

                _thumbsDown = new GameObject("ThumbsDown").AddComponent<Image>();
                _thumbsDown.sprite = LoadSpriteFromResource("SongPlayHistory.Assets.ThumbsDown.png");
                _thumbsDown.color = Color.white;
                _thumbsDown.enabled = false;
            }
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
            catch (Exception ex) // IOException or JsonException
            {
                Logger.Log.Error(ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// Called after drawing a LevelListTableCell.
        /// </summary>
        [HarmonyAfter(new string[] { "com.kyle1413.BeatSaber.SongCore" })]
        public static void Postfix(LevelListTableCell __instance, IPreviewBeatmapLevel level, bool isFavorite, ref TextMeshProUGUI ____songNameText,
            ref TextMeshProUGUI ____authorText)
        {
            if (_voteData == null)
                return;

            if (!level.levelID.StartsWith("custom_level_"))
                return;

            var levelID = level.levelID.Replace("custom_level_", "").ToLower();

            if (_voteData.TryGetValue(levelID, out UserVote vote))
            {
                Image icon = null;
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
                Logger.Log.Error("Error while loading a Sprite from resource.\n" + ex.ToString());
                return null;
            }
        }
    }
}
