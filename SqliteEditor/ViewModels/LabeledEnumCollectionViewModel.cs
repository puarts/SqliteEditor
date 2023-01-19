using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class LabeledEnumCollectionViewModel : ObservableCollection<ReactiveProperty<object>>
    {
        public LabeledEnumCollectionViewModel(Type enumType, string label, object defaultValue)
        {
            EnumType = enumType;
            Label = label;
            DefaultValue = defaultValue;
            EnumValues = Enum.GetValues(enumType);
        }

        public void AddNewItemsWhile(Func<bool> condition)
        {
            while (condition())
            {
                var item = new ReactiveProperty<object>(DefaultValue);
                Add(item);
            }
        }

        public bool TrimsSqliteArraySeparatorOnBothSide { get; set; } = false;

        public Type EnumType { get; }
        public string Label { get; }
        public object DefaultValue { get; }
        public Array EnumValues { get; }
    }
}
