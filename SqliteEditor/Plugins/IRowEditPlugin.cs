﻿using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqliteEditor.Plugins
{
    public interface IRowEditPlugin
    {
        string MenuHeader { get; }
        void ShowEditWindow(TableViewModel tableViewModel, int rowIndex);
        bool CanExecute(TableViewModel tableViewModel);
    }
}
