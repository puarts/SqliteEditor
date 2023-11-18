using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using SqliteEditor.Utilities;
using SqliteEditorTest.Utilities;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace SqliteEditorTest
{
    public class SqliteTest
    {
        private string _testDbPath;

        public SqliteTest()
        {
            _testDbPath = GetTestDatabasePath();
        }

        [Fact]
        public void UpdateRecordTest()
        {
            using var dirCreator = new ScopedDirectoryCreator();
            var workDir = dirCreator.DirectoryPath;
            var outputDbPath = Path.Combine(workDir, "output.db");
            File.Copy(_testDbPath, outputDbPath);
            File.Exists(outputDbPath).IsTrue();

            var query = $"select * from heroes";
            var table = SqliteUtility.ReadTable(_testDbPath, query);
            var schema = SqliteUtility.GetTableSchema(_testDbPath, "heroes");

            {
                var row = table.Rows[1];
                row.BeginEdit();
                row["name"] = "hoge";
                row.EndEdit();
                SqliteUtility.UpdateRecordById(outputDbPath, "heroes", row, schema);
            }

            {
                var outputTable = SqliteUtility.ReadTable(outputDbPath, query);
                outputTable.Rows[1]["name"].Is("hoge");
                outputTable.Rows[2]["name"].IsNot("hoge");
            }
        }

        [Fact]
        public void ClearRowsTest()
        {
            using var dirCreator = new ScopedDirectoryCreator();
            var workDir = dirCreator.DirectoryPath;
            var outputDbPath = Path.Combine(workDir, "output.db");
            File.Copy(_testDbPath, outputDbPath);
            File.Exists(outputDbPath).IsTrue();

            bool isConnectionDisposed = false;
            using (SQLiteConnection connection = new($"Data Source={outputDbPath};Version=3;"))
            {
                connection.Disposed += (sender, e) =>
                {
                    isConnectionDisposed = true;
                };

                connection.Open();
                try
                {
                    string sql = "delete from heroes";

                    using SQLiteCommand com = new SQLiteCommand(sql, connection);
                    com.ExecuteNonQuery();
                }
                finally
                {
                    connection.Close();
                }
            }

            var outputTable = SqliteUtility.ReadTable(outputDbPath, "select * from heroes");
            outputTable.Rows.Count.Is(0);

            while (!isConnectionDisposed)
            {
                Thread.Sleep(100);
            }
        }

        [Theory]
        [InlineData("�t�B������", 4)]
        [InlineData("�G���u��", 2)]
        public void SetTableTest(string testName, int actualCount)
        {
            using (var dirCreator = new ScopedDirectoryCreator())
            {
                var workDir = dirCreator.DirectoryPath;
                var outputDbPath = Path.Combine(workDir, "output.db");
                File.Copy(_testDbPath, outputDbPath);
                File.Exists(outputDbPath).IsTrue();

                var query = $"select * from heroes where name like '%{testName}%'";
                var table = SqliteUtility.ReadTable(_testDbPath, query);

                SqliteUtility.WriteTable(outputDbPath, "heroes", table);

                var outputTable = SqliteUtility.ReadTable(outputDbPath, query);
                outputTable.Rows.Count.Is(actualCount);
                foreach (DataRow row in outputTable.Rows)
                {
                    var name = (string)row["name"];
                    Assert.Contains(testName, name);
                }
            }
        }

        [Theory]
        [InlineData("�t�B������", 4)]
        [InlineData("�G���u��", 2)]
        public void GetTableTest(string testName, int actualCount)
        {
            var table = SqliteUtility.ReadTable(_testDbPath, $"select * from heroes where name like '%{testName}%'");
            table.Rows.Count.Is(actualCount);

            foreach (DataRow row in table.Rows)
            {
                var name = (string)row["name"];
                Assert.Contains(testName, name);
            }
        }

        private static string GetTestDatabasePath()
        {
            return Path.Combine(GetResourceRoot(), "test.sqlite3");
        }

        private static string GetResourceRoot()
        {
            var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new Exception();
            return Path.Combine(assemblyDir, "../../../Resources");
        }
    }
}