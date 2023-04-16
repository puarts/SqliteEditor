using Reactive.Bindings;
using SqliteEditor.Plugins.SkillRowEditPlugins;
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


        public void SetOrAdd(object value)
        {
            if (this.Any(x => x.Value.Equals(value)))
            {
                return;
            }

            var emptyItem = this.FirstOrDefault(x => x.Value.Equals(DefaultValue));
            if (emptyItem is null)
            {
                this.Add(new ReactiveProperty<object>(value));
            }
            else
            {
                int emptyIndex = this.IndexOf(emptyItem);
                this[emptyIndex].Value = value;
            }
        }
    }
}
