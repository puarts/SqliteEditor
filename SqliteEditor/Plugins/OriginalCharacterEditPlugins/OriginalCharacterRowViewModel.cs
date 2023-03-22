using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Extensions;
using SqliteEditor.Plugins.SkillRowEditPlugins;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor.Plugins.OriginalCharacterRowEditPlugins
{
    public class OriginalCharacterRowViewModel : RowEditViewModelBase
    {
        public OriginalCharacterRowViewModel(DataRow source, TableViewModel tableViewModel)
            : base(source, tableViewModel)
        {
        }

        protected override void RegisterProperties()
        {
            RegisterProperties(new Dictionary<string, object>()
            {
                { "name", new LabeledStringViewModel("名前") },
                { "cv", new LabeledStringViewModel("声優") },
                { "description", new LabeledDescriptionViewModel("概要") },
                { "details", new LabeledDescriptionViewModel("詳細") },
            });
        }

        protected override void SyncFromSourceCore()
        {
        }

        protected override void WriteBackToSourceCore()
        {
        }
    }
}
