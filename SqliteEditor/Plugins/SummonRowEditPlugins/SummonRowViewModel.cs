using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Extensions;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SqliteEditor.Plugins.SummonRowEditPlugins;

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
    [Display(Name = "プレゼント")]
    Present,
}

public class SummonRowViewModel : RowEditViewModelBase
{
    public SummonRowViewModel(DataRow source, TableViewModel tableViewModel)
        : base(source, tableViewModel)
    {
        Red.AddNewItemsWhile(() => Red.Values.Count < 3);
        Blue.AddNewItemsWhile(() => Blue.Values.Count < 3);
        Green.AddNewItemsWhile(() => Green.Values.Count < 3);
        Colorless.AddNewItemsWhile(() => Colorless.Values.Count < 3);

        if (tableViewModel.DatabasePath != null && File.Exists(tableViewModel.DatabasePath))
        {
            var dbDirectoryPath = Path.GetDirectoryName(tableViewModel.DatabasePath)!;
            var heroDbPath = Path.Combine(dbDirectoryPath, "feh-heroes.sqlite3");
            var heroTable = SqliteUtility.ReadTable(heroDbPath, "select name,id,type,epithet from heroes where how_to_get is not null and how_to_get!=''");
            HeroIdCollectionViewModel[] heroIds = [
                Red, Blue, Green, Colorless
            ];
            foreach (DataRow row in heroTable.Rows)
            {
                var name = (string)row["name"];
                var id = System.Convert.ToInt32((Int64)row["id"]);
                var color = (string)row["type"];
                var epithet = (string)row["epithet"];
                var heroInfo = new HeroInfo(id, name, epithet);
                switch (color)
                {
                    case "赤": Red.AddHeroInfo(heroInfo); break;
                    case "青": Blue.AddHeroInfo(heroInfo); break;
                    case "緑": Green.AddHeroInfo(heroInfo); break;
                    case "無": Colorless.AddHeroInfo(heroInfo); break;
                }
            }

            foreach (var hero in heroIds)
            {
                hero.SyncSelectedIndices();
            }
        }
    }
    public HeroIdCollectionViewModel Red { get; }= new("赤");
    public HeroIdCollectionViewModel Blue { get; }= new("青");
    public HeroIdCollectionViewModel Green { get; }= new("緑");
    public HeroIdCollectionViewModel Colorless { get; } = new("無");
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
