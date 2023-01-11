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
        public LabeledEnumCollectionViewModel(Type enumType, string label)
        {
            EnumType = enumType;
            Label = label;
            EnumValues = Enum.GetValues(enumType);
        }

        public void AddNewItemsWhile(Func<bool> condition)
        {
            while (condition())
            {
                var item = new ReactiveProperty<object>();
                Add(item);
            }
        }

        public Type EnumType { get; }
        public string Label { get; }

        public Array EnumValues { get; }
    }
}
