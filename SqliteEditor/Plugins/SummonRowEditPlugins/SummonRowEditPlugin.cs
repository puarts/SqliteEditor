using SqliteEditor.Plugins;
using SqliteEditor.ViewModels;
using SqliteEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor.Plugins.SummonRowEditPlugins
{
    public class SummonRowEditPlugin : RowEditPluginBase<SummonRowEditWindow, SummonRowViewModel>
    {
        public SummonRowEditPlugin()
            : base("召喚イベント編集", "gacha", (row, table) => new SummonRowViewModel(row, table))
        {
        }
    }
}
