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
