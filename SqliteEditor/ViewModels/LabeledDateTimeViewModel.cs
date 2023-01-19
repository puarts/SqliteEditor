﻿using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels
{
    public class LabeledDateTimeViewModel : ReactiveProperty<DateTime?>
    {
        public LabeledDateTimeViewModel(string label)
        {
            Label = label;
        }

        public string Label { get; }
    }
}
