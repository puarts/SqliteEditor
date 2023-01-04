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
                if (!File.Exists(path))
                {
                    return;
                }

                TableNames.Clear();
                foreach (var name in SqliteUtility.EnumerateTableNames(DatabasePath.Path.Value))
                {
                    TableNames.Add(name);
                }

                if (TableNames.Any())
                {
                    var tableName = TableNames.First();
                    var table = SqliteUtility.GetTable(DatabasePath.Path.Value, $"select * from {tableName}");
                    DataTable.Value = table;
                }
            }).AddTo(Disposable);

            LoadApplicationSettings();
            //DatabasePath.Path.Value = @"F:\trunk\Websites\puarts.com\db\feh-heroes.sqlite3";
        }

        public ObservableCollection<string> TableNames { get; } = new ObservableCollection<string>();

        public ReactiveProperty<DataTable?> DataTable { get; } = new ReactiveProperty<DataTable?>();

        public PathViewModel DatabasePath { get; } = new PathViewModel();

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

        private ApplicationSetting CreateApplicationSetting()
        {
            var setting = new ApplicationSetting();
            setting.DatabasePath = DatabasePath.ActualPath.Value;
            return setting;
        }
    }
}
