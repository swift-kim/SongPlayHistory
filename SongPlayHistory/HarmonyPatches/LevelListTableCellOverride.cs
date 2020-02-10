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
        private static Image _thumbUp;
        private static Image _thumbDown;

        private class UserVote
        {
            public string Key = null;
            public string VoteType = null;
        }

        static LevelListTableCellOverride()
        {
            // TODO: This should be done a bit earlier.
            CheckUpdate();
        }

        public static void CheckUpdate()
        {
            try
            {
                Logger.Log.Debug("Checking for changes in votedSongs.json...");

                if (_lastWritten != File.GetLastWriteTime(_voteFile))
                {
                    _lastWritten = File.GetLastWriteTime(_voteFile);

                    var text = File.ReadAllText(_voteFile);
                    _voteData = JsonConvert.DeserializeObject<Dictionary<string, UserVote>>(text);
                }
            }
            catch (Exception ex)
            {
                // Caught UnauthorizedAccessException, IOException, or JsonException.
                Logger.Log.Error(ex.ToString());
            }
        }

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
                if (vote.VoteType == "Upvote")
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
