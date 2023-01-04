using SqliteEditor.ViewModels;
using SqliteEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor.Plugins
{
    public class SkillRowEditPlugin : IRowEditPlugin
    {
        public string MenuHeader => "スキル編集";

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
            SkillRowEditWindow window = new();
            var row = table.DataTable.Rows[rowIndex];
            window.Owner = Application.Current.MainWindow;
            window.DataContext = new SkillRowViewModel(row, table);
            window.Show();
        }

        public bool CanExecute(TableViewModel tableViewModel)
        {
            return tableViewModel.TableName == "skills";
        }
    }
}
