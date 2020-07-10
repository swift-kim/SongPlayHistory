using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace SongPlayHistory.UnitTest
{
    /// <summary>
    /// Unit tests for static file I/O methods in <see cref="SPHModel"/>.
    /// Only general non-Unity-related methods can be tested.
    /// It is not allowed to access any Unity class from a standalone test runner.
    /// </summary>
    public class SPHModelTest
    {
        public SPHModelTest(ITestOutputHelper output)
        {
            Plugin.Log = new TestLogger() { Output = output };

            var userDataDir = Path.Combine(Environment.CurrentDirectory, "UserData");
            if (Directory.Exists(userDataDir))
            {
                Directory.Delete(userDataDir, true);
            }
            Directory.CreateDirectory(userDataDir);
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
            records.Add("Level1", new List<Record> { new Record() });
            records.Add("Level2", new List<Record> { new Record(), new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            File.WriteAllText(SPHModel.DataFile, serialized);

            SPHModel.InitializeRecords();

            Assert.True(File.Exists(SPHModel.DataFile));
            Assert.True(SPHModel.Records.Count == 2);
            Assert.True(SPHModel.Records["Level1"].Count == 1);
            Assert.True(SPHModel.Records["Level2"].Count == 2);
        }

        [Fact]
        public void InitializeRecords_HasEmptyDataNoBackup()
        {
            File.WriteAllText(SPHModel.DataFile, string.Empty);

            SPHModel.InitializeRecords();

            Assert.True(SPHModel.Records.Count == 0);
        }

        [Fact]
        public void InitializeRecords_HasInvalidDataNoBackup()
        {
            File.WriteAllText(SPHModel.DataFile, "NOT_A_VALID_JSON");

            SPHModel.InitializeRecords();

            Assert.True(SPHModel.Records.Count == 0);
        }

        [Fact]
        public void InitializeRecords_RestoreFromBackup()
        {
            File.WriteAllText(SPHModel.DataFile, "NOT_A_VALID_JSON");

            var records = new Dictionary<string, IList<Record>>();
            records.Add("Level1", new List<Record> { new Record() });
            records.Add("Level2", new List<Record> { new Record(), new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            var backupFile = Path.ChangeExtension(SPHModel.DataFile, ".bak");
            File.WriteAllText(backupFile, serialized);

            SPHModel.InitializeRecords();

            Assert.True(File.Exists(backupFile));
            Assert.True(SPHModel.Records.Count == 2);
        }

        [Fact]
        public void InitializeRecords_FailToRestoreFromBackup()
        {
            File.WriteAllText(SPHModel.DataFile, string.Empty);

            var backupFile = Path.ChangeExtension(SPHModel.DataFile, ".bak");
            File.WriteAllText(backupFile, string.Empty);

            SPHModel.InitializeRecords();

            Assert.True(SPHModel.Records.Count == 0);
        }

        [Fact]
        public void SaveRecordsToFile_WriteData()
        {
            var records = SPHModel.Records = new Dictionary<string, IList<Record>>();

            SPHModel.SaveRecordsToFile();

            Assert.False(File.Exists(SPHModel.DataFile));

            records.Add("Level1", new List<Record> { new Record() });
            records.Add("Level2", new List<Record> { new Record(), new Record() });

            SPHModel.SaveRecordsToFile();

            Assert.True(File.Exists(SPHModel.DataFile));

            var text = File.ReadAllText(SPHModel.DataFile);
            Assert.Contains("Level1", text);
            Assert.Contains("Level2", text);
        }

        [Fact]
        public void BackupRecords_OverwriteBackup()
        {
            var backupFile = Path.ChangeExtension(SPHModel.DataFile, ".bak");
            File.WriteAllText(backupFile, string.Empty);

            var records = new Dictionary<string, IList<Record>>();
            records.Add("Level1", new List<Record> { new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            File.WriteAllText(SPHModel.DataFile, serialized);

            Assert.True(new FileInfo(SPHModel.DataFile).Length > new FileInfo(backupFile).Length);

            SPHModel.BackupRecords();

            Assert.True(new FileInfo(SPHModel.DataFile).Length == new FileInfo(backupFile).Length);
        }

        [Fact]
        public void BackupRecords_DoNotOverwriteBackup()
        {
            File.WriteAllText(SPHModel.DataFile, string.Empty);

            var records = new Dictionary<string, IList<Record>>();
            records.Add("Level1", new List<Record> { new Record() });

            var serialized = JsonConvert.SerializeObject(records);
            var backupFile = Path.ChangeExtension(SPHModel.DataFile, ".bak");
            File.WriteAllText(backupFile, serialized);

            SPHModel.BackupRecords();

            Assert.True(new FileInfo(SPHModel.DataFile).Length < new FileInfo(backupFile).Length);
        }
    }
}
