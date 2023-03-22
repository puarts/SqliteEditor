using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SqliteEditor.Views
{
    public class EnumDisplayConverter : IValueConverter
    {
        public static EnumDisplayConverter Instance { get; } = new EnumDisplayConverter();

        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null) return null;

            FieldInfo? field = value.GetType().GetField(value.ToString()!);
            DisplayAttribute? attr = field?.GetCustomAttribute<DisplayAttribute>();
            if (attr != null)
            {
                return attr.Name;
            }
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
