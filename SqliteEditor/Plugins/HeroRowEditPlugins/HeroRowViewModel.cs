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

namespace SqliteEditor.Plugins.HeroRowEditPlugins
{
    public enum OriginType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "風花雪月")]
        ThreeHouses,
        [Display(Name = "ヒーローズ")]
        Heroes,
        [Display(Name = "Echoes")]
        Echoes,
        [Display(Name = "if")]
        Fates,
        [Display(Name = "覚醒")]
        Awakening,
        [Display(Name = "紋章の謎")]
        MysteryOfTheEmblem,
        [Display(Name = "暗黒竜と光の剣")]
        ShadowDragon,
        [Display(Name = "暁の女神")]
        RadiantDawn,
        [Display(Name = "蒼炎の軌跡")]
        PathOfRadiance,
        [Display(Name = "聖魔の光石")]
        TheSacredStones,
        [Display(Name = "烈火の剣")]
        TheBlazingBlade,
        [Display(Name = "封印の剣")]
        TheBindingBlade,
        [Display(Name = "トラキア776")]
        Thracia776,
        [Display(Name = "聖戦の系譜")]
        GenealogyOfTheHolyWar,
        [Display(Name = "幻影異聞録♯FE Encore")]
        SharpFEEncore,
        [Display(Name = "エンゲージ")]
        Engage,
    }
    public enum SexType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "男")]
        Male,
        [Display(Name = "女")]
        Female,
        [Display(Name = "性別不詳")]
        NonBinary,
    }
    public enum ColorType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "赤")]
        Red,
        [Display(Name = "青")]
        Blue,
        [Display(Name = "緑")]
        Green,
        [Display(Name = "無")]
        Colorless,
    }
    public enum MoveType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "重装")]
        Armor,
        [Display(Name = "歩行")]
        Infantry,
        [Display(Name = "飛行")]
        Flier,
        [Display(Name = "騎馬")]
        Cavarlry,
    }
    public enum HowToGetType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "ガチャ")]
        Gacha,
        [Display(Name = "超英雄")]
        SpecialHero,
        [Display(Name = "戦渦の連戦")]
        TempestTrials,
        [Display(Name = "大英雄戦")]
        GrandHeroBattle,
        [Display(Name = "伝承英雄ガチャ")]
        LegendaryHero,
        [Display(Name = "神階英雄ガチャ")]
        MythicHero,
        [Display(Name = "魔器英雄")]
        RearmedHero,
        [Display(Name = "特務機関")]
        OrderOfHeroes,
    }

    public enum WeaponType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "剣")]
        Sword,
        [Display(Name = "槍")]
        Lance,
        [Display(Name = "斧")]
        Axe,
        [Display(Name = "杖")]
        Staff,
        [Display(Name = "赤竜")]
        RedDragon,
        [Display(Name = "青竜")]
        BlueDragon,
        [Display(Name = "緑竜")]
        GreenDragon,
        [Display(Name = "無竜")]
        ColorlessDragon,
        [Display(Name = "赤獣")]
        RedBeast,
        [Display(Name = "青獣")]
        BlueBeast,
        [Display(Name = "緑獣")]
        GreenBeast,
        [Display(Name = "無獣")]
        ColorlessBeast,
        [Display(Name = "赤魔")]
        RedTome,
        [Display(Name = "青魔")]
        BlueTome,
        [Display(Name = "緑魔")]
        GreenTome,
        [Display(Name = "無魔")]
        ColorlessTome,
        [Display(Name = "赤暗器")]
        RedDagger,
        [Display(Name = "青暗器")]
        BlueDagger,
        [Display(Name = "緑暗器")]
        GreenDagger,
        [Display(Name = "暗器")]
        ColorlessDagger,
        [Display(Name = "赤弓")]
        RedBow,
        [Display(Name = "青弓")]
        BlueBow,
        [Display(Name = "緑弓")]
        GreenBow,
        [Display(Name = "弓")]
        ColorlessBow,
    }

    public enum RearityType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "星5")]
        Star5,
        [Display(Name = "星5|星4")]
        Star5_4,
        [Display(Name = "星4|星3")]
        Star4_3,
    }

    public class HeroRowViewModel : RowEditViewModelBase
    {
        public HeroRowViewModel(DataRow source, TableViewModel tableViewModel)
            : base(source, tableViewModel)
        {
            PureNames.AddNewItemsWhile(() => PureNames.Count < 2);
            Illustrators.AddNewItemsWhile(() => Illustrators.Count < 2);
            VoiceActors.AddNewItemsWhile(() => VoiceActors.Count < 2);
            Skills.AddNewItemsWhile(() => Skills.Count < 6);
            Sex.AddNewItemsWhile(() => Sex.Count < 2);
        }
        public LabeledStringViewModel Name { get; } = new("名前");
        public LabeledStringCollectionViewModel PureNames { get; } = new("純粋名");
        public LabeledStringCollectionViewModel Skills { get; } = new("習得スキル");
        public LabeledStringViewModel InternalId { get; } = new("内部ID");
        public LabeledStringViewModel EnglishName { get; } = new("英語名");
        public LabeledStringViewModel Epithet { get; } = new("称号");
        public LabeledStringViewModel EnglishEpithet { get; } = new("称号(英語)");
        public LabeledStringViewModel ReleaseDate { get; } = new("リリース日");
        public LabeledEnumViewModel Color { get; } = new LabeledEnumViewModel(typeof(ColorType), "属性");
        public LabeledEnumViewModel WeaponType { get; } = new LabeledEnumViewModel(typeof(WeaponType), "武器種");
        public LabeledEnumViewModel MoveType { get; } = new LabeledEnumViewModel(typeof(MoveType), "移動種");
        public LabeledEnumCollectionViewModel Sex { get; } = new(typeof(SexType), "性別", SexType.None) { TrimsSqliteArraySeparatorOnBothSide = true };


        public LabeledStringViewModel OfficialUrl { get; } = new("URL");

        public LabeledStringCollectionViewModel Illustrators { get; } = new("絵師");
        public LabeledStringCollectionViewModel VoiceActors { get; } = new("声優");


        protected override void RegisterProperties()
        {
            RegisterProperties(new Dictionary<string, object>()
            {
                { "name", Name },
                { "pure_name", PureNames },
                { "epithet", Epithet },
                { "origin", new LabeledEnumViewModel(typeof(OriginType), "出典") },
                { "how_to_get", new LabeledEnumViewModel(typeof(HowToGetType), "入手法") },
                { "type", Color },
                { "weapon_type", WeaponType },
                { "move_type", MoveType },
                { "sex", Sex },
                { "skills", Skills },
                { "rarity3", new LabeledEnumViewModel(typeof(RearityType), "レアリティ") },
                { "english_name", EnglishName },
                { "english_epithet", EnglishEpithet },
                { "release_date", ReleaseDate },
                { "cv", VoiceActors },
                { "illustrator", Illustrators },
                { "thumb", new LabeledStringViewModel("サムネール") },
                { "official_url", OfficialUrl },
                { "internal_id", InternalId },
                { "hp_5", new LabeledIntStringViewModel("LV40 HP") },
                { "atk_5", new LabeledIntStringViewModel("LV40 攻") },
                { "spd_5", new LabeledIntStringViewModel("LV40 速") },
                { "def_5", new LabeledIntStringViewModel("LV40 守") },
                { "res_5", new LabeledIntStringViewModel("LV40 魔") },
                { "hp_5_lv1", new LabeledIntStringViewModel("LV1 HP") },
                { "atk_5_lv1", new LabeledIntStringViewModel("LV1 攻") },
                { "spd_5_lv1", new LabeledIntStringViewModel("LV1 速") },
                { "def_5_lv1", new LabeledIntStringViewModel("LV1 守") },
                { "res_5_lv1", new LabeledIntStringViewModel("LV1 魔") },

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
