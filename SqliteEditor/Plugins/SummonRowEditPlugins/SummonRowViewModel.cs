using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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

namespace SqliteEditor.Plugins.SummonRowEditPlugins
{
    public enum SummonType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "ピックアップ")]
        Pickup,
        [Display(Name = "新英雄")]
        New,
        [Display(Name = "超英雄")]
        Special,
        [Display(Name = "W超英雄")]
        DoubleSpecial,
        [Display(Name = "復刻新英雄")]
        RevivalNew,
        [Display(Name = "復刻超英雄")]
        RevivalSpecial,
        [Display(Name = "伝承英雄")]
        Legendary,
        [Display(Name = "神階英雄")]
        Mythic,
        [Display(Name = "伝承英雄Remix")]
        LegendRemix,
        [Display(Name = "英雄祭")]
        HeroFest,
        [Display(Name = "リバイバル")]
        Revival,
        [Display(Name = "ω超英雄")]
        OmegaSpecial,
    }

    public class SummonRowViewModel : RowEditViewModelBase
    {
        public SummonRowViewModel(DataRow source, TableViewModel tableViewModel)
            : base(source, tableViewModel)
        {
            Red.AddNewItemsWhile(() => Red.Count < 3);
            Blue.AddNewItemsWhile(() => Blue.Count < 3);
            Green.AddNewItemsWhile(() => Green.Count < 3);
            Colorless.AddNewItemsWhile(() => Colorless.Count < 3);
        }

        public LabeledStringCollectionViewModel Red { get; }= new LabeledStringCollectionViewModel("赤");
        public LabeledStringCollectionViewModel Blue { get; }= new LabeledStringCollectionViewModel("青");
        public LabeledStringCollectionViewModel Green{ get; }= new LabeledStringCollectionViewModel("緑");
        public LabeledStringCollectionViewModel Colorless { get; } = new LabeledStringCollectionViewModel("無");
        protected override void RegisterProperties()
        {
            RegisterProperties(new Dictionary<string, object>()
            {
                { "name", new LabeledStringViewModel("名前") },
                { "type", new LabeledEnumViewModel(typeof(SummonType), "種類") },
                { "begin_date", new LabeledDateTimeViewModel("開始日") },
                { "end_date", new LabeledDateTimeViewModel("終了日") },
                { "pickup_red", Red },
                { "pickup_blue", Blue },
                { "pickup_green", Green },
                { "pickup_gray", Colorless },
                { "thumb", new LabeledStringViewModel("サムネール") },
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
