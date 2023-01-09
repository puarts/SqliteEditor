using Reactive.Bindings;
using SqliteEditor.Plugins.SkillRowEditPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SqliteEditor.ViewModels
{
    public class LabeledEnumViewModel : ReactiveProperty<object>
    {
        public LabeledEnumViewModel(Type enumType, string label)
        {
            EnumType = enumType;
            Label = label;
            EnumValues = Enum.GetValues(enumType);
        }

        public LabeledEnumViewModel(Type enumType, string label, object defaultValue)
            : this(enumType, label)
        {
            EnumType = enumType;
            Label = label;
            EnumValues = Enum.GetValues(enumType);
            Value = defaultValue;
        }

        public TEnum GetEnumValue<TEnum>() => (TEnum)Value;

        public Type EnumType { get; }
        public string Label { get; }

        public Array EnumValues { get; }
    }
}
