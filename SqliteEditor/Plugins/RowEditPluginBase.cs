﻿using SqliteEditor.Plugins.SkillRowEditPlugins;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;

namespace SqliteEditor.Plugins
{
    public abstract class RowEditPluginBase<TWindow, TViewModel> : IRowEditPlugin
        where TWindow : Window, new()
        where TViewModel : RowEditViewModelBase
    {
        private readonly string _tableName;
        private readonly Func<DataRow, TableViewModel, TViewModel> _createViewModelFunc;
        private TWindow? _window;

        protected RowEditPluginBase(
            string menuHeader,
            string tableName,
            Func<DataRow, TableViewModel, TViewModel> createViewModelFunc)
        {
            MenuHeader = menuHeader;
            this._tableName = tableName;
            this._createViewModelFunc = createViewModelFunc;
        }

        public string MenuHeader { get; }

        public void ResetViewModel(TableViewModel tableViewModel, int rowIndex)
        {
            if (rowIndex < 0)
            {
                return;
            }
            var table = tableViewModel;
            if (table is null)
            {
                return;
            }
            if (_window is null)
            {
                return;
            }

            if (!CanExecute(tableViewModel))
            {
                return;
            }

            var row = table.DataTable.Rows[rowIndex];
            _window.DataContext = _createViewModelFunc(row, table);
        }

        public void ShowEditWindow(TableViewModel tableViewModel, int rowIndex)
        {
            if (rowIndex < 0)
            {
                return;
            }
            var table = tableViewModel;
            if (table is null)
            {
                return;
            }

            if (_window is not null)
            {
                return;
            }

            var row = table.DataTable.Rows[rowIndex];

            _window = new();
            _window.Closed += Window_Closed;
            _window.Owner = Application.Current.MainWindow;
            _window.DataContext = _createViewModelFunc(row, table);
            _window.Show();
        }

        private void Window_Closed(object? sender, EventArgs e)
        {
            if (_window is not null)
            {
                _window.Closed -= Window_Closed;
                _window = null;
            }
        }

        public bool CanExecute(TableViewModel tableViewModel)
        {
            return tableViewModel?.TableName == _tableName;
        }
    }
}
