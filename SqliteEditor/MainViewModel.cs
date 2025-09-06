using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Extensions;
using SqliteEditor.Plugins;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using SqliteEditor.Views;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;

namespace SqliteEditor
{
    public enum RowFilterMode
    {
        NameFilter,
        AnyFilter
    }

    public class MainViewModel : CompositeDisposableBase
    {
        private List<IRowEditPlugin> _plugins = new();
        private List<ICommand> _openEditWindowCommands = new();

        public MainViewModel()
        {
            _ = DatabasePath.ActualPath.Subscribe(path =>
            {
                SyncSqliteInfo();
            }).AddTo(Disposer);

            _ = SelectedTableIndex.Subscribe(index =>
            {
                if (index < 0)
                {
                    return;
                }
                var table = Tables[index];
                SelectedTable.Value = table;
                SchemaTable.Value = table.Schema;
            }).AddTo(Disposer);

            _ = OverwriteCommand.Subscribe(() =>
            {
                SaveDirtyTables();
            }).AddTo(Disposer);

            _ = AddRowCommand.Subscribe(() =>
            {
                AddRowToSelectedTable();
            }).AddTo(Disposer);

            _ = OpenEditRowWindowCommand.Subscribe(() =>
            {
                var validCommand = _openEditWindowCommands.FirstOrDefault(x => x.CanExecute(null));
                if (validCommand is null)
                {
                    WriteError($"現在のテーブルを編集できる編集メニューがありません。");
                    return;
                }
                validCommand.Execute(null);
            }).AddTo(Disposer);

            _ = UpdateCurrentRecordCommand.Subscribe(() =>
            {
                var rowView = SelectedRow.Value;
                if (rowView is null) return;

                var selectedTable = SelectedTable.Value;
                SqliteUtility.UpdateRecord(
                    DatabasePath.ActualPath.Value,
                    selectedTable.TableName,
                    rowView.Row,
                    selectedTable.Schema);
            }).AddTo(Disposer);

            _ = DuplicateCurrentRecordCommand.Subscribe(() =>
            {
                var rowView = SelectedRow.Value;
                if (rowView is null) return;
                var selectedTable = SelectedTable.Value;
                var newRow = selectedTable.DataTable.NewRow();
                foreach (DataColumn col in selectedTable.DataTable.Columns)
                {
                    if (col.ColumnName == "id" && col.DataType == typeof(long))
                    {
                        // id列があってlong型なら新しいidを振る
                        var newId = EstimateNewRowPrimaryKeyValue(selectedTable.DataTable, "id");
                        newRow["id"] = newId;
                    }
                    else
                    {
                        newRow[col.ColumnName] = rowView.Row[col];
                    }
                }
                selectedTable.DataTable.Rows.Add(newRow);
                SqliteUtility.AddRow(DatabasePath.ActualPath.Value, selectedTable.TableName, newRow);
            }).AddTo(Disposer);

            _ = SelectedRow.Subscribe(rowView =>
            {
                ResetEditPluginViewModel(rowView);
            }).AddTo(Disposer);

            _ = OpenInputCsvToolCommand.Subscribe(() =>
            {
            }).AddTo(Disposer);

            _ = OpenStringConversionInfoWindowCommand.Subscribe(() =>
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var window = new StringConversionInfoWindow()
                    {
                        DataContext = this
                    };
                    window.Owner = Application.Current.MainWindow;
                    window.ShowDialog();
                    SyncStringConversionInfosToPlugins();
                });
            }).AddTo(Disposer);

            _ = AutoSetSkillInheritCommand.Subscribe(() =>
            {
                if (SelectedTable.Value is null) return;

                var table = SelectedTable.Value;
                if (table.TableName != "skills")
                {
                    WriteError("skills テーブルを選択してください。");
                    return;
                }

                try
                {
                    var nameColumn = table.EnumerateColumns().First(x => x.ColumnName == "name");
                    var inheritColumn = table.EnumerateColumns().First(x => x.ColumnName == "inherit");
                    var typeColumn = table.EnumerateColumns().First(x => x.ColumnName == "type");
                    var rows = table.EnumerateRows().ToArray();
                    var targetRows = rows.Where(x => x[inheritColumn] is DBNull or "").ToArray();
                    foreach (var row in targetRows)
                    {
                        if (row[inheritColumn] is not DBNull and not "") continue;

                        var name = (string)row[nameColumn];
                        var typeStr = row[typeColumn] as string;
                        row[inheritColumn] = FehSkillUtility.EstimateInheritance(name, typeStr) ? "可" : "不可";
                    }

                    var mustLearnColumn = table.EnumerateColumns().First(x => x.ColumnName == "must_learn");
                    foreach (var row in targetRows)
                    {
                        if (row[mustLearnColumn] is not DBNull and not "") continue;

                        var name = (string)row[nameColumn];
                        var lastChar = name[^1];
                        if (char.IsNumber(lastChar))
                        {
                            int num = int.Parse(lastChar.ToString());
                            int lowerNum = num - 1;
                            if (lowerNum >= 1)
                            {
                                string baseName = name.Substring(0, name.Length - 1);
                                string lowerSkillName = $"{baseName}{lowerNum}";
                                row[mustLearnColumn] = $"|{lowerSkillName}|";
                            }
                        }
                        else if (lastChar == '+')
                        {
                            string lowerSkillName = name.Substring(0, name.Length - 1);
                            row[mustLearnColumn] = $"|{lowerSkillName}|";
                        }
                    }

                    var inheritWeaponTypeColumn = table.EnumerateColumns().First(x => x.ColumnName == "inheritable_weapon_type");
                    var inheritMoveTypeColumn = table.EnumerateColumns().First(x => x.ColumnName == "inheritable_move_type");
                    foreach (var row in targetRows)
                    {
                        if (row[inheritColumn] is not "可"
                            || row[typeColumn] is not string typeStr
                            || (!typeStr.Contains("パッシブ") && !typeStr.Contains("サポート"))
                            || (row[inheritWeaponTypeColumn] is not DBNull and not ""
                                || row[inheritMoveTypeColumn] is not DBNull and not "")) continue;

                        var name = (string)row[nameColumn];
                        var lastChar = name[^1];
                        if (char.IsNumber(lastChar))
                        {
                            // 鬼神の一撃3など
                            string baseName = name.Substring(0, name.Length - 1);
                            var partName = baseName
                                .Replace("攻撃", "")
                                .Replace("速さ", "")
                                .Replace("守備", "")
                                .Replace("魔防", "")
                                .Replace("鬼神", "")
                                .Replace("飛燕", "")
                                .Replace("金剛", "")
                                .Replace("明鏡", "")
                                .Replace("攻速", "")
                                .Replace("攻守", "")
                                .Replace("攻魔", "")
                                .Replace("速守", "")
                                .Replace("速魔", "")
                                .Replace("守魔", "")
                                ;
                            var existingRow = rows.FirstOrDefault(x => ((string)x[nameColumn]).Contains(partName)
                                && (x[inheritWeaponTypeColumn] is not DBNull and not "" || x[inheritMoveTypeColumn] is not DBNull and not ""));
                            if (existingRow is not null)
                            {
                                row[inheritWeaponTypeColumn] = existingRow[inheritWeaponTypeColumn];
                                row[inheritMoveTypeColumn] = existingRow[inheritMoveTypeColumn];
                            }
                        }
                        else if (name.Contains('・'))
                        {
                            // 露払い・攻め立てなど
                            var split = name.Split('・');
                            var existingRow = rows.FirstOrDefault(x => IsSameKindSkill(x, split, nameColumn, inheritWeaponTypeColumn, inheritMoveTypeColumn));
                            if (existingRow is not null)
                            {
                                row[inheritWeaponTypeColumn] = existingRow[inheritWeaponTypeColumn];
                                row[inheritMoveTypeColumn] = existingRow[inheritMoveTypeColumn];
                            }


                            static bool IsSameKindSkill(
                                DataRow x,
                                string[] split,
                                DataColumn nameColumn,
                                DataColumn inheritWeaponTypeColumn,
                                DataColumn inheritMoveTypeColumn)
                            {
                                var baseName = split[0];
                                var suffix = split[split.Length - 1];
                                if (!((string)x[nameColumn]).StartsWith(baseName) && !((string)x[nameColumn]).EndsWith(suffix))
                                {
                                    return false;
                                }

                                return (x[inheritWeaponTypeColumn] is not DBNull and not "" || x[inheritMoveTypeColumn] is not DBNull and not "");
                            }

                        }
                        else
                        {
                            // 「鬼神金剛の掩撃」など
                            var partName = name
                                .Replace("攻撃", "")
                                .Replace("速さ", "")
                                .Replace("守備", "")
                                .Replace("魔防", "")
                                .Replace("鬼神", "")
                                .Replace("飛燕", "")
                                .Replace("金剛", "")
                                .Replace("明鏡", "")
                                .Replace("攻速", "")
                                .Replace("攻守", "")
                                .Replace("攻魔", "")
                                .Replace("速守", "")
                                .Replace("速魔", "")
                                .Replace("守魔", "")
                                ;
                            var existingRow = rows.FirstOrDefault(x => ((string)x[nameColumn]).Contains(partName)
                                && (x[inheritWeaponTypeColumn] is not DBNull and not "" || x[inheritMoveTypeColumn] is not DBNull and not ""));
                            if (existingRow is not null)
                            {
                                row[inheritWeaponTypeColumn] = existingRow[inheritWeaponTypeColumn];
                                row[inheritMoveTypeColumn] = existingRow[inheritMoveTypeColumn];
                            }
                        }
                    }

                    var spColumn = table.EnumerateColumns().First(x => x.ColumnName == "sp");
                    foreach (var row in targetRows)
                    {
                        if (row[spColumn] is not DBNull and not "") continue;
                        if (row[typeColumn] is not string typeStr || row[inheritColumn] is not string inheritStr) continue;

                        switch (typeStr)
                        {
                            case "武器":
                                {
                                    if (inheritStr == "不可")
                                    {
                                        row[spColumn] = 400;
                                    }
                                    else
                                    {
                                        row[spColumn] = 300;
                                    }
                                }
                                break;
                            case "サポート":
                                break;
                            case "奥義":
                                break;
                            case "パッシブA":
                            case "パッシブB":
                            case "パッシブC":
                                {
                                    if (inheritStr == "不可")
                                    {
                                        row[spColumn] = 300;
                                    }
                                    else
                                    {
                                        // ものによるけど大体300なのでとりあえず300にしておく
                                        row[spColumn] = 300;
                                    }
                                }
                                break;
                            case "響心":
                                row[spColumn] = 0;
                                break;
                        }
                    }

                    table.UpdateDirty();
                }
                catch (Exception e)
                {
                    WriteError(e.Message);
                }
            }).AddTo(Disposer);

            LoadApplicationSettings();

            {
                List<StringConversionInfo> defaultInfos = [
                    new(" ", ""),
                    new("增幅", "増幅"),
                    new("擊", "撃"),
                    new("守備魔防", "守備、魔防"),
                    new("攻撃速さ", "攻撃、速さ"),
                    new("－", "-"),
                    new("~", "～"),
                    new("備ー", "備-"),
                    new("撃ー", "撃-"),
                    new("さー", "さ-"),
                    new("防ー", "防-"),
                    new("Pー", "P-"),
                    new("ダメージー", "ダメージ-"),
                    new("カウントー", "カウント-"),
                    new("量ー", "量-"),
                    new("＋", "+"),
                    new("x", "×"),
                    new("（", "("),
                    new("）", ")"),
                    new("奥盖", "奥義"),
                    new("值", "値"),
                    new(",", "、"),
                    new(".", "、"),
                    new("天顔", "天脈"),
                    new("最大倍", "最大値"),
                    new("自身の奥義発動カウント変動量を無効", "自身の奥義発動カウント変動量-を無効"),
                    new("ダメージを%軽減","ダメージを○○%軽減"),
                    new("ダメージ+×", "ダメージ+○×"),
                    new("〇", "○"),
                    new(Environment.NewLine, "<br>"),
                ];
                foreach (var info  in defaultInfos)
                {
                    if (!StringConversionInfos.Contains(info))
                    {
                        StringConversionInfos.Add(info);
                    }
                }
            }

            AddRowEditPlugin(new Plugins.SkillRowEditPlugins.SkillRowEditPlugin());
            AddRowEditPlugin(new Plugins.HeroRowEditPlugins.HeroRowEditPlugin());
            AddRowEditPlugin(new Plugins.SummonRowEditPlugins.SummonRowEditPlugin());
            AddRowEditPlugin(new Plugins.OriginalCharacterRowEditPlugins.OriginalCharacterRowEditPlugin());

            SyncStringConversionInfosToPlugins();
        }

        public LabeledEnumViewModel RowFilterMode { get; } = new(typeof(RowFilterMode), "フィルターモード", SqliteEditor.RowFilterMode.AnyFilter);

        public ObservableCollection<MenuItemVIewModel> EditRowMenus { get; } = new ObservableCollection<MenuItemVIewModel>();

        public ReactiveProperty<TableViewModel> SelectedTable { get; } = new ReactiveProperty<TableViewModel>();

        public ObservableCollection<TableViewModel> Tables { get; } = new ObservableCollection<TableViewModel>();

        public ReactiveProperty<DataTable?> SchemaTable { get; } = new ReactiveProperty<DataTable?>();

        public ReactiveProperty<int> SelectedTableIndex { get; } = new ReactiveProperty<int>(-1);
        public ReactiveProperty<DataRowView?> SelectedRow { get; } = new ReactiveProperty<DataRowView?>();

        public ReactiveProperty<string> Log { get; } = new ReactiveProperty<string>();

        public PathViewModel DatabasePath { get; } = new PathViewModel();

        public ReactiveCommand OverwriteCommand { get; } = new ReactiveCommand();

        public ReactiveCommand AddRowCommand { get; } = new ReactiveCommand();
        public ReactiveCommand OpenEditRowWindowCommand { get; } = new ReactiveCommand();
        public ReactiveCommand UpdateCurrentRecordCommand { get; } = new ReactiveCommand();

        public ReactiveCommand DuplicateCurrentRecordCommand { get; } = new ReactiveCommand();
        public ReactiveCommand AutoSetSkillInheritCommand { get; } = new ReactiveCommand();

        public ReactiveCommand OpenInputCsvToolCommand { get; } = new ReactiveCommand();

        public ReactiveCommand OpenStringConversionInfoWindowCommand { get; } = new();

        public ObservableCollection<StringConversionInfo> StringConversionInfos { get; } = new();

        public static string ApplicationSettingPath
        {
            get
            {
                var assembly = Assembly.GetEntryAssembly();
                if (assembly is null)
                {
                    throw new Exception();
                }

                var dirPath = Path.GetDirectoryName(assembly.Location);
                if (dirPath is null)
                {
                    throw new Exception();
                }

                var path = Path.Combine(dirPath, "ApplicationSetting.json");
                return path;
            }
        }

        private void SyncStringConversionInfosToPlugins()
        {
            foreach (var plugin in _plugins)
            {
                plugin.SyncStringConversionInfosFrom(StringConversionInfos);
            }
        }

        public void LoadApplicationSettings()
        {
            var path = ApplicationSettingPath;
            var deserialized = JsonUtility.ReadJson<ApplicationSetting>(path);
            if (deserialized is null)
            {
                return;
            }

            if (deserialized.DatabasePath != null)
            {
                DatabasePath.Path.Value = deserialized.DatabasePath;
            }
            if (deserialized.StringConversionInfos != null)
            {
                StringConversionInfos.AddRange(deserialized.StringConversionInfos);
            }
        }

        public void SaveApplicationSettings()
        {
            JsonUtility.WriteAsJson(CreateApplicationSetting(), ApplicationSettingPath);
        }

        public TableViewModel? GetSelectedTableViewModel()
        {
            return SelectedTableIndex.Value < 0 ? null : Tables[SelectedTableIndex.Value];
        }

        public void UpdateSelectedTableDirty()
        {
            var tableViewModel = GetSelectedTableViewModel();
            tableViewModel?.UpdateDirty();
        }

        private ApplicationSetting CreateApplicationSetting()
        {
            var setting = new ApplicationSetting(DatabasePath.ActualPath.Value, 
                StringConversionInfos.ToImmutableList());
            return setting;
        }

        public void SaveDirtyTables()
        {
            var path = DatabasePath.Path.Value;
            WriteLog($"変更内容を保存しています..\"{path}\"");
            UpdateSelectedTableDirty();
            var dirtyTables = Tables.Where(x => x.IsDirty).ToArray();
            if (dirtyTables.Length > 0)
            {
                _ = Task.Run(() =>
                {
                    SqliteUtility.WriteTables(path, dirtyTables.Select(x => x.DataTable));
                    foreach (var table in dirtyTables)
                    {
                        WriteLog($"テーブル \"{table.TableName}\" の内容を保存しました。");
                        table.UpdateDirtySource();
                    }

                    WriteLog($"変更内容を保存しました。\"{path}\"");
                });
            }
            else
            {
                WriteLog("変更内容がないので保存をスキップしました。");
            }
        }

        public bool ShowConfirmSavingDialog(Window owner)
        {
            if (HasDirtyTable())
            {
                var result = MessageBox.Show(owner, "未保存の変更があります。保存しますか？", "変更の保存確認", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }

                if (result == MessageBoxResult.Yes)
                {
                    SaveDirtyTables();
                }
            }

            return true;
        }

        private bool HasDirtyTable()
        {
            return Tables.Any(x => x.IsDirty);
        }

        private void WriteLog(string message)
        {
            Log.Value += message + "\n";
        }

        private void WriteError(string message)
        {
            WriteLog("エラー:" + message);
        }

        private void AddRowToSelectedTable()
        {
            var tableViewModel = GetSelectedTableViewModel();
            if (tableViewModel is null)
            {
                return;
            }
            var dataTable = tableViewModel.DataTable;
            if (dataTable is null)
            {
                return;
            }
            var newRow = dataTable.NewRow();
            string? primaryKeyName = FindPrimaryKeyColumnName(tableViewModel);
            if (primaryKeyName is not null)
            {
                var newPkValue = EstimateNewRowPrimaryKeyValue(dataTable, primaryKeyName);
                newRow[primaryKeyName] = newPkValue;
            }

            dataTable.Rows.Add(newRow);

            var path = DatabasePath.Path.Value;
            var selectedTable = SelectedTable.Value;
            SqliteUtility.AddRow(path, selectedTable.TableName, newRow);
            //tableViewModel.UpdateDirty();
        }

        private static long EstimateNewRowPrimaryKeyValue(DataTable dataTable, string pkColName)
        {
            return EnumeratePrimaryKeyValues(dataTable, pkColName).Max() + 1;
        }

        private static IEnumerable<long> EnumeratePrimaryKeyValues(DataTable dataTable, string pkColName)
        {
            foreach (DataRow row in dataTable.Rows)
            {
                var value = (long)row[pkColName];
                yield return value;
            }
        }

        private static string? FindPrimaryKeyColumnName(TableViewModel tableViewModel)
        {
            var schema = tableViewModel.Schema;
            foreach (DataRow schemaRow in schema.Rows)
            {
                var pk = (long)schemaRow["pk"];
                if (pk == 1)
                {
                    return (string)schemaRow["name"];
                }
            }
            return null;
        }

        private void SyncSqliteInfo()
        {
            var path = DatabasePath.Path.Value;
            if (!File.Exists(path))
            {
                return;
            }

            Tables.Clear();
            foreach (var name in SqliteUtility.EnumerateTableNames(path))
            {
                var table = SqliteUtility.ReadTable(path, $"select * from {name}", (m, e) => WriteError(m + "\n" + e.Message));
                table.TableName = name;
                if (table.Columns.Contains("id"))
                {
                    // デフォルトでは新しいものを上に表示したいのでid降順ソート
                    table.DefaultView.Sort = "id DESC";
                }


                var schemaTable = SqliteUtility.GetTableSchema(path, name);
                Tables.Add(new TableViewModel(table, schemaTable, WriteError, this));
            }

            SelectedTableIndex.Value = Tables.Any() ? 0 : -1;
            WriteLog($"データベースを開きました。\"{path}\"");
        }

        private void ResetEditPluginViewModel(DataRowView? rowView)
        {
            if (rowView is null)
            {
                return;
            }
            var table = GetSelectedTableViewModel();
            if (table is null)
            {
                return;
            }

            var rowIndex = table.DataTable.Rows.IndexOf(rowView.Row);
            foreach (var plugin in _plugins)
            {
                plugin.ResetViewModel(table, rowIndex);
            }
        }

        private void AddRowEditPlugin(IRowEditPlugin plugin)
        {
            _plugins.Add(plugin);
            var command = new ReactiveCommand(SelectedTable.Select(x => plugin.CanExecute(x)));
            _ = command.Subscribe(() =>
            {
                try
                {
                    if (SelectedRow.Value is null)
                    {
                        return;
                    }
                    var table = GetSelectedTableViewModel();
                    if (table is null)
                    {
                        return;
                    }
                    var rowIndex = table.DataTable.Rows.IndexOf(SelectedRow.Value.Row);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        plugin.ShowEditWindow(table, rowIndex);
                    });
                }
                catch (Exception exception)
                {
                    WriteError(exception.Message +Environment.NewLine + exception.StackTrace);
                }
            }).AddTo(Disposer);
            _openEditWindowCommands.Add(command);
            EditRowMenus.Add(new MenuItemVIewModel(plugin.MenuHeader, command));
        }
    }
}
