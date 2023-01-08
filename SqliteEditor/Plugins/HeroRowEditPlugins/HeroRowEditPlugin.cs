using SqliteEditor.Plugins;
using SqliteEditor.Plugins.SkillRowEditPlugins;
using SqliteEditor.ViewModels;
using SqliteEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor.Plugins.HeroRowEditPlugins
{
    public class HeroRowEditPlugin : RowEditPluginBase<HeroRowEditWindow, HeroRowViewModel>
    {
        public HeroRowEditPlugin()
            : base("英雄編集", "heroes", (row, table) => new HeroRowViewModel(row, table))
        {
        }
    }
}
