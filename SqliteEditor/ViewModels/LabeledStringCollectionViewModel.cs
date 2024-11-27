using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class LabeledStringCollectionViewModel : ObservableCollection<ReactiveProperty<string>>, IPropertyViewModel
    {
        public LabeledStringCollectionViewModel(string label)
        {
            Label = label;
        }

        public string Label { get; }

        public void AddNewItemsWhile(Func<bool> condition)
        {
            while (condition())
            {
                var item = new ReactiveProperty<string>();
                Add(item);
            }
        }
        public ReactiveProperty<bool> IsVisible { get; } = new(true);
    }
}
