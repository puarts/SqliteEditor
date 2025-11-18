using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqliteEditor.Utilities;
using SqliteEditorTest.Utilities;
using System;
using System.Data;
using System.Data.SQLite;
using System.Reflection;
using System.Windows.Documents;
using System.IO;
using System.Text.RegularExpressions;

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
    public void Test2()
    {
        var directoryPath = @"F:\trunk\Websites\fire-emblem.fun\images\fe-if\captures\支援会話";

        if (!Directory.Exists(directoryPath))
        {
            Console.WriteLine($"Directory does not exist: {directoryPath}");
            return;
        }

        // Pattern: "name1-name2YYYY-MM-DD_hh-mm-ss.mp4"
        // Capture the leading "name1-name2" part as group 1.
        var pattern = new Regex(@"^(.+?-.+?)(\d{4}-\d{2}-\d{2}[_ ]\d{2}-\d{2}-\d{2})\.mp4$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        foreach (var filePath in Directory.EnumerateFiles(directoryPath, "*.mp4", SearchOption.TopDirectoryOnly))
        {
            var fileName = Path.GetFileName(filePath);
            if (fileName is null)
                continue;

            var m = pattern.Match(fileName);
            if (!m.Success)
                continue; // not matching the expected pattern

            var folderName = m.Groups[1].Value.Trim();
            if (string.IsNullOrEmpty(folderName))
                continue;

            var destDir = Path.Combine(directoryPath, folderName);

            try
            {
                // Create directory if it doesn't exist
                if (!Directory.Exists(destDir))
                {
                    Directory.CreateDirectory(destDir);
                }

                var destPath = Path.Combine(destDir, fileName);

                // If destination file already exists, skip moving to avoid overwrite
                if (File.Exists(destPath))
                {
                    Console.WriteLine($"Destination already exists, skipping: {destPath}");
                    continue;
                }

                File.Move(filePath, destPath);
                Console.WriteLine($"Moved: {fileName} -> {destDir}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to move file {fileName}: {ex.Message}");
            }
        }
    }

    [TestMethod]
    public void Test()
    {
        var root = @"F:\trunk\Websites\fire-emblem.fun\images\fe-the-sacred-stones\captures\support";
        var dbPath = @"F:\trunk\Websites\fire-emblem.fun\db\fe-the-sacred-stones.sqlite3";
        var table = SqliteUtility.ReadTable(dbPath, $"select * from support");
        foreach (DataRow row in table.Rows)
        {
            var name1 = (string)row["name1"];
            var name2 = (string)row["name2"];
            if (string.IsNullOrEmpty(name1) || string.IsNullOrEmpty(name2))
                continue;

            // アルファベット順で先に来る方を先頭にする
            string combinedName;
            if (string.Compare(name1, name2, StringComparison.OrdinalIgnoreCase) <= 0)
            {
                combinedName = $"{name1}-{name2}";
            }
            else
            {
                combinedName = $"{name2}-{name1}";
            }

            var dirPath = Path.Combine(root, combinedName);

            if (!Directory.Exists(dirPath))
            {
                Console.WriteLine(dirPath);
                _ = Directory.CreateDirectory(dirPath);
            }
        }
    }


    [TestMethod]
    public void CopyValuesForSpecifiedColumnTest()
    {
        using var dirCreator = new ScopedDirectoryCreator();
        var workDir = dirCreator.DirectoryPath;
        var outputDbPath = Path.Combine(workDir, "output.db");
        File.Copy(_testDbPath, outputDbPath);
        File.Exists(outputDbPath).Should().BeTrue();

        string columnName = "type";
        {
            var sourceTable = SqliteUtility.ReadTable(_testDbPath, $"select * from heroes");

            foreach (DataRow row in sourceTable.Rows)
            {
                row[columnName] = "hoge"; // Set the type column for testing
                var value = row[columnName];
                value.Should().Be("hoge");
            }

            var destTable = SqliteUtility.ReadTable(outputDbPath, $"select * from heroes");

            SqliteUtility.CopyValuesForSpecifiedColumn(sourceTable, destTable, columnName, "name");
            SqliteUtility.WriteTable(outputDbPath, "heroes", destTable);
        }

        {
            var outputTable = SqliteUtility.ReadTable(outputDbPath, "select * from heroes");
            foreach (DataRow row in outputTable.Rows)
            {
                var name = row["name"]?.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    continue; // Skip rows where name is null
                }
                var value = row[columnName];
                value.Should().Be("hoge");
            }
        }
    }

    [TestMethod]
    public void CopyValuesForSpecifiedColumnTestByFilePath()
    {
        using var dirCreator = new ScopedDirectoryCreator();
        var workDir = dirCreator.DirectoryPath;
        var sourceDbPath = Path.Combine(workDir, "source.db");
        var outputDbPath = Path.Combine(workDir, "output2.db");
        File.Copy(_testDbPath, sourceDbPath);
        File.Exists(sourceDbPath).Should().BeTrue();
        File.Copy(_testDbPath, outputDbPath);
        File.Exists(outputDbPath).Should().BeTrue();
        string columnName = "type";
        {
            var sourceTable = SqliteUtility.ReadTable(sourceDbPath, $"select * from heroes");
            foreach (DataRow row in sourceTable.Rows)
            {
                row[columnName] = "hoge"; // Set the type column for testing
            }

            SqliteUtility.WriteTable(sourceDbPath, "heroes", sourceTable);

            SqliteUtility.CopyValuesForSpecifiedColumn(
                sourceDbPath, outputDbPath, "heroes", columnName, "name");

        }

        {
            var outputTable = SqliteUtility.ReadTable(outputDbPath, "select * from heroes");
            foreach (DataRow row in outputTable.Rows)
            {
                var name = row["name"]?.ToString();
                if (string.IsNullOrEmpty(name))
                {
                    continue; // Skip rows where name is null
                }
                var value = row[columnName];
                value.Should().Be("hoge");
            }
        }
    }

    [TestMethod]
    public void CopySpecifiedColumn()
    {
        string sourceDbPath = @"F:\trunk\Websites\fire-emblem.fun\db\feh-original_heroes_source.sqlite3";
        string outputDbPath = @"F:\trunk\Websites\fire-emblem.fun\db\feh-original_heroes.sqlite3";
        SqliteUtility.CopyValuesForSpecifiedColumn(
            sourceDbPath, outputDbPath, "original_heroes", "self_pronoun", "id");
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