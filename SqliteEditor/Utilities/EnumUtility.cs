using SqliteEditor.SkillRowEditPlugins;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SqliteEditor.Utilities
{
    public static class EnumUtility
    {
        public static string ConvertEnumToDisplayName<TEnum>(TEnum source)
            where TEnum : struct, Enum
        {
            string internalFormat = source.ToString();
            var displayAttribute = source.GetType().GetField(internalFormat)?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? internalFormat;
        }

        public static TEnum ConvertDisplayNameToEnum<TEnum>(string displayName)
            where TEnum : struct, Enum
        {
            var type = typeof(TEnum);
            foreach (string enumName in Enum.GetNames(type))
            {
                var displayAttribute = type.GetField(enumName)?.GetCustomAttribute<DisplayAttribute>();
                if ((displayAttribute is not null && displayAttribute.Name == displayName)
                    || displayName == enumName)
                {
                    if (Enum.TryParse(enumName, out TEnum value))
                    {
                        return value;
                    }
                }
            }

            throw new Exception();
        }
    }
}
