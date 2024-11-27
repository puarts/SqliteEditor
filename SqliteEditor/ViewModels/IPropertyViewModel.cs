﻿using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.ViewModels;

public interface IPropertyViewModel
{
    ReactiveProperty<bool> IsVisible { get; }
}
