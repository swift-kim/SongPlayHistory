using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SongPlayHistory.UnitTest
{
    public class SPHModelTest
    {
        public readonly string UserDataDir = Path.Combine(Environment.CurrentDirectory, "UserData");

        public SPHModelTest()
        {
            if (Directory.Exists(UserDataDir))
            {
                Directory.Delete(UserDataDir, true);
            }
            Directory.CreateDirectory(UserDataDir);
        }

        [Fact]
        public void ReadOrMigrateRecords_HasNoDataFile()
        {
            SPHModel.ReadOrMigrateRecords();

            Assert.False(File.Exists(SPHModel.DataFile));
            Assert.True(SPHModel.Records.Count == 0);
        }

        [Fact]
        public void ReadOrMigrateRecords_ReadData()
        {
            var records = new Dictionary<string, IList<Record>>();
            records.Add("level1", new List<Record> { new Record() });
            records.Add("level2", new List<Record> { new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            File.WriteAllText(SPHModel.DataFile, $"{{\"Scores\":{serialized}}}");

            SPHModel.ReadOrMigrateRecords(); // Why NullReferenceException?

            Assert.True(File.Exists(SPHModel.DataFile));
            Assert.True(SPHModel.Records.Count == 2);
        }
    }
}
