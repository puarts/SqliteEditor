using SqliteEditor.Plugins;
using SqliteEditor.ViewModels;
using SqliteEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor.Plugins.SkillRowEditPlugins
{
    public class SkillRowEditPlugin : RowEditPluginBase<SkillRowEditWindow, SkillRowViewModel>
    {
        public SkillRowEditPlugin()
            : base("スキル編集", "skills", (row, table) => new SkillRowViewModel(row, table))
        {
        }
    }
}
