using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqliteEditor.ViewModels
{
    public class MenuItemVIewModel
    {
        public MenuItemVIewModel(string header, ICommand command) 
        {
            Header = header;
            Command = command;
        }

        public string Header { get; }
        public ICommand Command { get; }
    }
}
