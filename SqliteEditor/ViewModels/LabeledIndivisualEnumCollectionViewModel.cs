using Reactive.Bindings;
using SqliteEditor.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SqliteEditor.ViewModels
{
    public class LabeledIndivisualEnumCollectionViewModel : ObservableCollection<EnumViewModel>, IPropertyViewModel
    {
        public LabeledIndivisualEnumCollectionViewModel(IEnumerable<EnumViewModel> enums, string label)
        {
            Label = label;
            foreach (var enumViewModel in enums)
            {
                Add(enumViewModel);
            }
        }

        public bool TrimsSqliteArraySeparatorOnBothSide { get; set; } = false;

        public string Label { get; }
        public ReactiveProperty<bool> IsVisible { get; } = new(true);

        public EnumViewModel? FindEnumViewModelFromValue(string value)
        {
            foreach (var item in this)
            {
                foreach (var enumValue in item.EnumValues)
                {
                    var valueStr = EnumUtility.ConvertEnumToDisplayName(enumValue);
                    if (value == valueStr)
                    {
                        return item;
                    }
                }
            }

            return null;
        }
    }
}
