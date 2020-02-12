using System;
using System.Collections.Generic;
using System.IO;
using Harmony;
using Newtonsoft.Json;
using TMPro;
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
        /// Called before applying this Harmony patch.
        /// </summary>
        public static bool Prepare()
        {
            return UpdateData();
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
                if (vote.voteType == "Upvote")
                {
                    ____authorText.SetText($"👍👍👍 { ____authorText.text}");
                }
                else
                {
                    ____authorText.SetText($"{ ____authorText.text} 👎👎👎");
                }
            }

            // TODO: Image
            // TODO: Not refreshed after finishing a song
        }
    }
}
