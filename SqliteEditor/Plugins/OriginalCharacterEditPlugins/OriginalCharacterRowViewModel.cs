using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Extensions;
using SqliteEditor.Plugins.HeroRowEditPlugins;
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
            Series.AddNewItemsWhile(() => Series.Count < 2);
            Tags.AddNewItemsWhile(() => Tags.Count < 2);
            OtherNames.AddNewItemsWhile(() => OtherNames.Count < 2);
        }

        public LabeledEnumCollectionViewModel Series { get; } = new(typeof(OriginType), "シリーズ", "");
        public LabeledStringCollectionViewModel Tags { get; } = new("タグ");
        public LabeledStringCollectionViewModel OtherNames { get; } = new("別称");

        protected override void RegisterProperties()
        {
            RegisterProperties(new Dictionary<string, IPropertyViewModel>()
            {
                { "name", new LabeledStringViewModel("名前") },
                { "fullname", new LabeledStringViewModel("フルネーム") },
                { "english_name", new LabeledStringViewModel("英語名") },
                { "series", Series },
                { "sex", new LabeledEnumViewModel(typeof(SexType), "性別") },
                { "cv", new LabeledStringViewModel("声優") },
                { "birthday", new LabeledStringViewModel("誕生日") },
                { "thumb", new LabeledStringViewModel("サムネール") },
                { "playable", new LabeledBoolViewModel("プレイアブル") },
                { "tags", Tags },
                { "other_names", OtherNames },
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
