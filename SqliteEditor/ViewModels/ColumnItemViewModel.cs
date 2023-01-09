using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class ColumnItemViewModel : CompositeDisposableBase
    {
        public ColumnItemViewModel(string columnName)
        {
            ColumnName = columnName;
        }

        public string ColumnName { get; }

        public ReactiveProperty<bool> IsVisible { get; } = new(true);
    }
}
