using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteEditor.Utilities;
using SqliteEditorTest.Utilities;
using System.Data;
using System.Data.SQLite;
using System.Reflection;

namespace SqliteEditorTest;

[TestClass]
public class SqliteTest
{
    private string _testDbPath;

    public SqliteTest()
    {
        _testDbPath = GetTestDatabasePath();
    }

    [TestMethod]
    public void UpdateRecordTest()
    {
        using var dirCreator = new ScopedDirectoryCreator();
        var workDir = dirCreator.DirectoryPath;
        var outputDbPath = Path.Combine(workDir, "output.sqlite3");
        File.Copy(_testDbPath, outputDbPath);
        File.Exists(outputDbPath).Should().BeTrue();

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
            outputTable.Rows[1]["name"].Should().Be("hoge");
            outputTable.Rows[2]["name"].Should().NotBe("hoge");
        }
    }

    [TestMethod]
    public void ClearRowsTest()
    {
        using var dirCreator = new ScopedDirectoryCreator();
        var workDir = dirCreator.DirectoryPath;
        var outputDbPath = Path.Combine(workDir, "output.db");
        File.Copy(_testDbPath, outputDbPath);
        File.Exists(outputDbPath).Should().BeTrue();

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
        outputTable.Rows.Count.Should().Be(0);

        while (!isConnectionDisposed)
        {
            Thread.Sleep(100);
        }
    }

    [TestMethod]
    [DataRow("フィヨルム", 4)]
    [DataRow("エンブラ", 2)]
    public void SetTableTest(string testName, int actualCount)
    {
        using (var dirCreator = new ScopedDirectoryCreator())
        {
            var workDir = dirCreator.DirectoryPath;
            var outputDbPath = Path.Combine(workDir, "output.db");
            File.Copy(_testDbPath, outputDbPath);
            File.Exists(outputDbPath).Should().BeTrue();

            var query = $"select * from heroes where name like '%{testName}%'";
            var table = SqliteUtility.ReadTable(_testDbPath, query);

            SqliteUtility.WriteTable(outputDbPath, "heroes", table);

            var outputTable = SqliteUtility.ReadTable(outputDbPath, query);
            outputTable.Rows.Count.Should().Be(actualCount);
            foreach (DataRow row in outputTable.Rows)
            {
                var name = (string)row["name"];
                name.Should().Contain(testName);
            }
        }
    }

    [TestMethod]
    [DataRow("フィヨルム", 4)]
    [DataRow("エンブラ", 2)]
    public void GetTableTest(string testName, int actualCount)
    {
        var table = SqliteUtility.ReadTable(_testDbPath, $"select * from heroes where name like '%{testName}%'");
        table.Rows.Count.Should().Be(actualCount);

        foreach (DataRow row in table.Rows)
        {
            var name = (string)row["name"];
            name.Should().Contain(testName);
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