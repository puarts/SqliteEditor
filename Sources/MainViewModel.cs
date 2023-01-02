using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SqliteEditor
{
    internal class MainViewModel
    {
        public MainViewModel()
        {
        }

        public ReactiveProperty<string> DatabasePath { get; } = new ReactiveProperty<string>();

        public string ApplicationSettingPath
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

        public void SaveApplicationSettings()
        {
            JsonUtility.WriteAsJson(CreateApplicationSetting(), ApplicationSettingPath);
        }

        private ApplicationSetting CreateApplicationSetting()
        {
            var setting = new ApplicationSetting();
            setting.DatabasePath = DatabasePath.Value;
            return setting;
        }
    }
}
