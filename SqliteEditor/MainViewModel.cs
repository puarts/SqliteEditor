using Microsoft.Win32;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Plugins;
using SqliteEditor.SkillRowEditPlugins;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using SqliteEditor.Views;
using System;
using System.Collections.Generic;
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

namespace SqliteEditor
{
    public class MainViewModel : CompositeDisposableBase
    {
        private List<IRowEditPlugin> _plugins = new List<IRowEditPlugin>();

        public MainViewModel()
        {
            _ = DatabasePath.ActualPath.Subscribe(path =>
            {
                SyncSqliteInfo();
            }).AddTo(Disposable);

            _ = SelectedTableIndex.Subscribe(index =>
            {
                if (index < 0)
                {
                    return;
                }
                var table = Tables[index];
                SelectedTable.Value = table;
                DataTable.Value = table.DataTable;
                SchemaTable.Value = table.Schema;
            }).AddTo(Disposable);

            _ = OverwriteCommand.Subscribe(() =>
            {
                SaveDirtyTables();
            }).AddTo(Disposable);

            _ = AddRowCommand.Subscribe(() =>
            {
                AddRowToSelectedTable();
            }).AddTo(Disposable);

            LoadApplicationSettings();
            AddRowEditPlugin(new SkillRowEditPlugin());
        }

        public ObservableCollection<MenuItemVIewModel> EditRowMenus { get; } = new ObservableCollection<MenuItemVIewModel>();

        private ReactiveProperty<TableViewModel> SelectedTable { get; } = new ReactiveProperty<TableViewModel>();

        public ObservableCollection<TableViewModel> Tables { get; } = new ObservableCollection<TableViewModel>();

        public ReactiveProperty<DataTable?> DataTable { get; } = new ReactiveProperty<DataTable?>();
        public ReactiveProperty<DataTable?> SchemaTable { get; } = new ReactiveProperty<DataTable?>();

        public ReactiveProperty<int> SelectedTableIndex { get; } = new ReactiveProperty<int>(-1);
        public ReactiveProperty<DataRowView?> SelectedRow { get; } = new ReactiveProperty<DataRowView?>();

        public ReactiveProperty<string> Log { get; } = new ReactiveProperty<string>();

        public PathViewModel DatabasePath { get; } = new PathViewModel();

        public ReactiveCommand OverwriteCommand { get; } = new ReactiveCommand();

        public ReactiveCommand AddRowCommand { get; } = new ReactiveCommand();

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
            var setting = new ApplicationSetting();
            setting.DatabasePath = DatabasePath.ActualPath.Value;
            return setting;
        }

        public void SaveDirtyTables()
        {
            var path = DatabasePath.Path.Value;
            var dirtyTables = Tables.Where(x => x.IsDirty).ToArray();
            SqliteUtility.SetTables(path, dirtyTables.Select(x => x.DataTable));
            foreach (var table in dirtyTables)
            {
                table.UpdateDirtySource();
            }

            WriteLog($"変更内容を保存しました。\"{path}\"");
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
            tableViewModel.UpdateDirty();
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
                var table = SqliteUtility.GetTable(path, $"select * from {name}");
                table.TableName = name;

                var schemaTable = SqliteUtility.GetTableSchema(path, name);
                Tables.Add(new TableViewModel(table, schemaTable));
            }

            SelectedTableIndex.Value = Tables.Any() ? 0 : -1;
            WriteLog($"データベースを開きました。\"{path}\"");
        }

        private void AddRowEditPlugin(IRowEditPlugin plugin)
        {
            _plugins.Add(plugin);
            var command = new ReactiveCommand(SelectedTable.Select(x => plugin.CanExecute(x)));
            _ = command.Subscribe(() =>
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
                plugin.ShowEditWindow(table, rowIndex);
            }).AddTo(Disposable);
            EditRowMenus.Add(new MenuItemVIewModel(plugin.MenuHeader, command));
        }
    }
}
