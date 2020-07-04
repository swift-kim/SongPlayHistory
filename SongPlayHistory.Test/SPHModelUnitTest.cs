using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace SongPlayHistory.Test
{
    [TestClass]
    public class SPHModelUnitTest
    {
        public readonly string UserDataDir = Path.Combine(Environment.CurrentDirectory, "UserData");

        [TestInitialize]
        public void Initialize()
        {
            if (Directory.Exists(UserDataDir))
            {
                Directory.Delete(UserDataDir, true);
            }
            Directory.CreateDirectory(UserDataDir);
        }

        [TestMethod]
        public void ReadOrMigrateRecords_HasNoDataFile()
        {
            SPHModel.ReadOrMigrateRecords();

            Assert.IsFalse(File.Exists(SPHModel.DataFile));
            Assert.IsTrue(SPHModel.Records.Count == 0);
        }

        [TestMethod]
        public void ReadOrMigrateRecords_MigrateFromConfig()
        {
            var configFile = Path.Combine(UserDataDir, $"{Plugin.Name}.json");

            var records = new Dictionary<string, IList<Record>>();
            records.Add("level1", new List<Record> { new Record() });
            records.Add("level2", new List<Record> { new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            File.WriteAllText(configFile, $"{{\"Scores\":{serialized}}}\n");

            SPHModel.ReadOrMigrateRecords();

            Assert.IsTrue(File.Exists(configFile));
            Assert.IsTrue(File.Exists(SPHModel.DataFile));
            Assert.IsTrue(SPHModel.Records.Count == 2);
        }
    }
}
