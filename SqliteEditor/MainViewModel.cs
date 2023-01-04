using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SqliteEditor
{
    public class MainViewModel : CompositeDisposableBase
    {
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
                DataTable.Value = Tables[index].DataTable;
            }).AddTo(Disposable);

            _ = OverwriteCommand.Subscribe(() =>
            {
                SaveDirtyTables();
            }).AddTo(Disposable);

            LoadApplicationSettings();
        }

        public ObservableCollection<TableViewModel> Tables { get; } = new ObservableCollection<TableViewModel>();

        public ReactiveProperty<DataTable?> DataTable { get; } = new ReactiveProperty<DataTable?>();

        public ReactiveProperty<int> SelectedTableIndex { get; } = new ReactiveProperty<int>(-1);

        public ReactiveProperty<string> Log { get; } = new ReactiveProperty<string>();

        public PathViewModel DatabasePath { get; } = new PathViewModel();

        public ReactiveCommand OverwriteCommand { get; } = new ReactiveCommand();

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
            SqliteUtility.SetTables(path, EnumerateDirtyTables());
        }

        private IEnumerable<DataTable> EnumerateDirtyTables()
        {
            return Tables.Where(x => x.IsDirty).Select(x => x.DataTable);
        }

        private void WriteLog(string message)
        {
            Log.Value += message + "\n";
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
                Tables.Add(new TableViewModel(table));
            }

            SelectedTableIndex.Value = Tables.Any() ? 0 : -1;
        }
    }
}
