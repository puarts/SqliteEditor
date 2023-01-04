using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SqliteEditor.Utilities
{
    public static class JsonUtility
    {
        public static T? ReadJson<T>(string path)
            where T : class
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var jsonText = File.ReadAllText(path);

            var deserialized = JsonSerializer.Deserialize<T>(jsonText);
            if (deserialized == null)
            {
                throw new Exception($"\"{path}\" の読み込みに失敗しました。");
            }

            return deserialized;
        }

        public static string? WriteAsJson<T>(T target, string path)
        {
            var savePath = path;
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true,
            };
            var jsonText = JsonSerializer.Serialize(target, options);
            File.WriteAllText(savePath, jsonText);
            return savePath;
        }
    }
}
