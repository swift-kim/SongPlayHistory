using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace SongPlayHistory.UnitTest
{
    public class SPHModelTest
    {
        public readonly string UserDataDir = Path.Combine(Environment.CurrentDirectory, "UserData");

        public SPHModelTest(ITestOutputHelper output)
        {
            Plugin.Log = new TestLogger() { Output = output };

            if (Directory.Exists(UserDataDir))
            {
                Directory.Delete(UserDataDir, true);
            }
            Directory.CreateDirectory(UserDataDir);
        }

        [Fact]
        public void InitializeRecords_HasNoDataFile()
        {
            SPHModel.InitializeRecords();

            Assert.False(File.Exists(SPHModel.DataFile));
            Assert.True(SPHModel.Records.Count == 0);
        }

        [Fact]
        public void InitializeRecords_ReadData()
        {
            var records = new Dictionary<string, IList<Record>>();
            records.Add("level1", new List<Record> { new Record() });
            records.Add("level2", new List<Record> { new Record(), new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            File.WriteAllText(SPHModel.DataFile, serialized);

            SPHModel.InitializeRecords();

            Assert.True(File.Exists(SPHModel.DataFile));
            Assert.True(SPHModel.Records.Count == 2);
            Assert.True(SPHModel.Records["level1"].Count == 1);
            Assert.True(SPHModel.Records["level2"].Count == 2);
        }
    }
}
