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

namespace SqliteEditor.Plugins.OriginalCharacterRowEditPlugins
{
    public class OriginalCharacterRowEditPlugin : RowEditPluginBase<OriginalCharacterRowEditWindow, OriginalCharacterRowViewModel>
    {
        public OriginalCharacterRowEditPlugin()
            : base("原作キャラ編集", "original_heroes", (row, table) => new OriginalCharacterRowViewModel(row, table))
        {
        }
    }
}
