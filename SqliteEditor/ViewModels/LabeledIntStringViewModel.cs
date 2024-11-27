﻿using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class LabeledIntStringViewModel : ReactiveProperty<string>, IPropertyViewModel
    {
        public LabeledIntStringViewModel(string label)
        {
            Label = label;
        }

        public string Label { get; }
        public ReactiveProperty<bool> IsVisible { get; } = new(true);
    }
}
