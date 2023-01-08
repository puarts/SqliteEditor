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
        public static string ConvertEnumToDisplayName(object source)
        {
            string internalFormat = source.ToString()!;
            var displayAttribute = source.GetType().GetField(internalFormat)?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? internalFormat;
        }

        public static string ConvertEnumToDisplayName<TEnum>(TEnum source)
            where TEnum : struct, Enum
        {
            string internalFormat = source.ToString();
            var displayAttribute = source.GetType().GetField(internalFormat)?.GetCustomAttribute<DisplayAttribute>();
            return displayAttribute?.Name ?? internalFormat;
        }

        public static object ConvertDisplayNameToEnum(Type enumType, string displayName)
        {
            foreach (string enumName in Enum.GetNames(enumType))
            {
                var displayAttribute = enumType.GetField(enumName)?.GetCustomAttribute<DisplayAttribute>();
                if ((displayAttribute is not null && displayAttribute.Name == displayName)
                    || displayName == enumName)
                {
                    if (Enum.TryParse(enumType, enumName, out object? value))
                    {
                        return value ?? throw new Exception();
                    }
                }
            }

            throw new Exception();
        }

        public static TEnum ConvertDisplayNameToEnum<TEnum>(string displayName)
            where TEnum : struct, Enum
        {
            return (TEnum)ConvertDisplayNameToEnum(typeof(TEnum), displayName);
        }
    }
}
