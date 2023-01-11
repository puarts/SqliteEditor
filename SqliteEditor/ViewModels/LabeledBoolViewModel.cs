using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class LabeledBoolViewModel : ReactiveProperty<bool?>
    {
        public LabeledBoolViewModel(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }
}
