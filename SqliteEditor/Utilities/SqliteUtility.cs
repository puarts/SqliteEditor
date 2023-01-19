using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SqliteEditor.Utilities
{
    public static class SqliteUtility
    {
        public static void ExecuteNonQuery(string dbPath, string query)
        {
            // 接続先を指定
            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbPath };
            using var conn = new SQLiteConnection(sqlConnectionSb.ToString());
            using var command = conn.CreateCommand();

            // 接続
            conn.Open();

            // コマンドの実行処理
            command.CommandText = query;
            command.ExecuteNonQuery();
            //var value = command.ExecuteNonQuery();
            //MessageBox.Show($"更新されたレコード数は {value} です。");
        }

        public static IEnumerable<string> EnumerateTableNames(string dbPath)
        {
            string ListTableSql = $"SELECT name FROM sqlite_master WHERE type = 'table'";

            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbPath };
            using var connection = new SQLiteConnection(sqlConnectionSb.ToString());
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = ListTableSql;
            using (var reader = command.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var name = (string)reader["name"];
                        yield return name;
                    }
                }
            }
        }

        public static DataTable GetTableSchema(string dbPath, string tableName)
        {
            var sql = $"PRAGMA TABLE_INFO('{tableName}')";
            return ReadTable(dbPath, sql);
        }

        public static DataTable ReadTable(string dbPath, string sql)
        {
            DataTable table = new();
            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbPath };
            using (var connection = new SQLiteConnection(sqlConnectionSb.ToString()))
            {
                connection.Open();

                using (SQLiteCommand cmd = connection.CreateCommand())
                {
                    cmd.CommandText = sql;

                    using SQLiteDataReader reader = cmd.ExecuteReader();
                    try
                    {
                        var columns = reader.GetColumnSchema();
                        foreach (var column in columns)
                        {
                            if (column.DataType is not null)
                            {
                                table.Columns.Add(column.ColumnName, column.DataType);
                            }
                            else
                            {
                                table.Columns.Add(column.ColumnName);
                            }
                        }
                        while (reader.Read())
                        {
                            var row = table.NewRow();
                            List<object?> values = new();
                            foreach (var column in columns)
                            {
                                var cellValue = reader.GetValue(column.ColumnName);
                                row[column.ColumnName] = cellValue;
                            }
                            table.Rows.Add(row);
                        }
                    }
                    finally
                    {
                        reader.Close();
                    }

                    //SQLiteDataAdapter adapter = new(cmd);
                    //adapter.Fill(table);
                }
            }
            return table;
        }

        public static void WriteTables(string path, IEnumerable<DataTable> tables)
        {
            foreach (var table in tables)
            {
                SqliteUtility.WriteTable(path, table.TableName, table);
            }
        }

        private static string ConvertSqliteCellValue(object? source)
        {
            if (source is null or System.DBNull)
            {
                return "null";
            }
            string str;
            if (source is bool boolValue)
            {
                // 本当は 1、0にしないといけないけど、自分のDBの互換性のためにtrueにしておく
                return boolValue ? "'true'" : "null";
            }
            else if (source is DateTime dateTime)
            {
                return $"'{dateTime.ToString("yyyy-MM-dd")}'";
            }
            else
            {
                str = source.ToString()!.Replace("'", "''");
                return $"'{str}'";
            }
        }

        public static DataTable WriteTable(string dbPath, string tableName, DataTable table)
        {
            var sqlConnectionSb = new SQLiteConnectionStringBuilder { DataSource = dbPath };
            using (SQLiteConnection connection = new(sqlConnectionSb.ToString()))
            {
                connection.Open();
                try
                {
                    using SQLiteTransaction trans = connection.BeginTransaction();

                    try
                    {
                        var sql = "delete from " + tableName + ";";

                        foreach (DataRow row in table.Rows)
                        {
                            var values = string.Join(",", row.ItemArray.Select(x => ConvertSqliteCellValue(x))); 
                            sql += $"INSERT INTO {tableName} VALUES({values});";
                        }

                        using var command = connection.CreateCommand();
                        {
                            command.CommandText = sql;
                            command.ExecuteNonQuery();
                        }


                        //コミット
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
            return table;
        }
    }
}
