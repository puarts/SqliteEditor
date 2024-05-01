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
        [Display(Name = "外伝")]
        Gaiden,
        [Display(Name = "Echoes")]
        Echoes,
        [Display(Name = "if")]
        Fates,
        [Display(Name = "覚醒")]
        Awakening,
        [Display(Name = "紋章の謎")]
        MysteryOfTheEmblem,
        [Display(Name = "新・紋章の謎")]
        NewMysteryOfTheEmblem,
        [Display(Name = "暗黒竜と光の剣")]
        ShadowDragon,
        [Display(Name = "新・暗黒竜と光の剣")]
        NewShadowDragon,
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
        [Display(Name = "無双")]
        Warriors,
        [Display(Name = "無双 風花雪月")]
        WarriorsThreeHopes,
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
        [Display(Name = "紋章士英雄")]
        EmblemHero,
        [Display(Name = "魔器英雄")]
        RearmedHero,
        [Display(Name = "響心英雄")]
        AttunedHero,
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
        [Display(Name = "獣")]
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

    public enum RarityType
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

    public enum DuelType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "死闘205")]
        Duel205,
        [Display(Name = "死闘200")]
        Duel200,
        [Display(Name = "死闘195")]
        Duel195,
        [Display(Name = "死闘190")]
        Duel190,
        [Display(Name = "死闘185")]
        Duel185,
        [Display(Name = "死闘180")]
        Duel180,
    }

    public enum SpecialType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "比翼")]
        Duo,
        [Display(Name = "双界")]
        Resonant,
        [Display(Name = "伝承火")]
        LegendFire,
        [Display(Name = "伝承水")]
        LegendWater,
        [Display(Name = "伝承風")]
        LegendWind,
        [Display(Name = "伝承地")]
        LegendEarth,
        [Display(Name = "神階光")]
        MythicLight,
        [Display(Name = "神階闇")]
        MythicDark,
        [Display(Name = "神階天")]
        MythicAstra,
        [Display(Name = "神階理")]
        MythicAnima,
        [Display(Name = "魔器")]
        Rearmed,
        [Display(Name = "響心")]
        Attuned,
        [Display(Name = "開花")]
        Ascended,
        [Display(Name = "紋章士")]
        Emblem,
    }

    public enum StatusRevisionType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "Hp5_Atk3")]
        Hp5_Atk3,
        [Display(Name = "Hp5_Spd4")]
        Hp5_Spd4,
        [Display(Name = "Hp5_Def5")]
        Hp5_Def5,
        [Display(Name = "Hp5_Res5")]
        Hp5_Res5,
        [Display(Name = "Hp3_Atk2")]
        Hp3_Atk2,
        [Display(Name = "Hp3_Spd3")]
        Hp3_Spd3,
        [Display(Name = "Hp3_Def4")]
        Hp3_Def4,
        [Display(Name = "Hp3_Res4")]
        Hp3_Res4,
    }

    public enum SpecialEffectType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "ダブル")]
        PairUp,
        [Display(Name = "枠追加")]
        AddSlot,
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
            Origin.AddNewItemsWhile(() => Origin.Count < 2);

            _ = WeaponType.Subscribe(value =>
            {
                Color.Value = WeaponToColor((WeaponType)value);
            }).AddTo(Disposable);

            _ = HowToGet.Subscribe(value =>
            {
                var howToGet = (HowToGetType)value;
                switch (howToGet)
                {
                    case HowToGetType.TempestTrials:
                        Rarity.Value = RarityType.Star5_4;
                        break;
                    case HowToGetType.GrandHeroBattle:
                        Rarity.Value = RarityType.Star4_3;
                        break;
                    default:
                        break;
                }
            }).AddTo(Disposable);
        }

        private ColorType WeaponToColor(WeaponType weaponType)
        {
            switch (weaponType)
            {
                case HeroRowEditPlugins.WeaponType.Sword:
                case HeroRowEditPlugins.WeaponType.RedDragon:
                case HeroRowEditPlugins.WeaponType.RedBeast:
                case HeroRowEditPlugins.WeaponType.RedTome:
                case HeroRowEditPlugins.WeaponType.RedDagger:
                case HeroRowEditPlugins.WeaponType.RedBow:
                    return ColorType.Red;
                case HeroRowEditPlugins.WeaponType.Lance:
                case HeroRowEditPlugins.WeaponType.BlueDragon:
                case HeroRowEditPlugins.WeaponType.BlueBeast:
                case HeroRowEditPlugins.WeaponType.BlueTome:
                case HeroRowEditPlugins.WeaponType.BlueDagger:
                case HeroRowEditPlugins.WeaponType.BlueBow:
                    return ColorType.Blue;
                case HeroRowEditPlugins.WeaponType.Axe:
                case HeroRowEditPlugins.WeaponType.GreenDragon:
                case HeroRowEditPlugins.WeaponType.GreenBeast:
                case HeroRowEditPlugins.WeaponType.GreenTome:
                case HeroRowEditPlugins.WeaponType.GreenDagger:
                case HeroRowEditPlugins.WeaponType.GreenBow:
                    return ColorType.Green;
                case HeroRowEditPlugins.WeaponType.ColorlessBow:
                case HeroRowEditPlugins.WeaponType.ColorlessDragon:
                case HeroRowEditPlugins.WeaponType.ColorlessBeast:
                case HeroRowEditPlugins.WeaponType.ColorlessTome:
                case HeroRowEditPlugins.WeaponType.ColorlessDagger:
                case HeroRowEditPlugins.WeaponType.Staff:
                    return ColorType.Colorless;
                case HeroRowEditPlugins.WeaponType.None:
                default:
                    return ColorType.None;
            }
        }

        public LabeledStringCollectionViewModel PureNames { get; } = new("純粋名");
        public LabeledStringCollectionViewModel Skills { get; } = new("習得スキル");
        public LabeledEnumCollectionViewModel Sex { get; } = new(typeof(SexType), "性別", SexType.None) 
        {
            TrimsSqliteArraySeparatorOnBothSide = true 
        };
        public LabeledStringCollectionViewModel Illustrators { get; } = new("絵師");
        public LabeledStringCollectionViewModel VoiceActors { get; } = new("声優");

        public LabeledIndivisualEnumCollectionViewModel SpecialTypes { get; } = new LabeledIndivisualEnumCollectionViewModel(new EnumViewModel[] 
        {
            new(typeof(SpecialType), SpecialType.None),
            new(typeof(SpecialType), SpecialType.None),
        }, "特殊タイプ");

        public ObservableCollection<object> AutoBindProperties { get; } = new ObservableCollection<object>();

        public LabeledEnumViewModel Rarity { get; } = new LabeledEnumViewModel(typeof(RarityType), "レアリティ");
        public LabeledEnumViewModel HowToGet { get; } = new LabeledEnumViewModel(typeof(HowToGetType), "入手法");

        public LabeledEnumCollectionViewModel Origin { get; } = new LabeledEnumCollectionViewModel(typeof(OriginType), "出典", OriginType.None) { TrimsSqliteArraySeparatorOnBothSide = true };

        protected override void RegisterProperties()
        {
            var dict = new Dictionary<string, object>()
            {
                { "name", new LabeledStringViewModel("名前") },
                { "pure_name", PureNames },
                { "epithet", new LabeledStringViewModel("称号") },
                { "origin", Origin },
                { "how_to_get", HowToGet },
                { "type", Color },
                { "weapon_type", WeaponType },
                { "move_type", new LabeledEnumViewModel(typeof(MoveType), "移動種") },
                { "sex", Sex },
                { "skills", Skills },
                { "rarity3", Rarity },
                { "english_name", new LabeledStringViewModel("英語名") },
                { "english_epithet", new LabeledStringViewModel("称号(英語)") },
                { "release_date", new LabeledDateTimeViewModel("リリース日") },
                { "cv", VoiceActors },
                { "illustrator", Illustrators },
                { "thumb", new LabeledStringViewModel("サムネール") },
                { "special_type", new LabeledIndivisualEnumCollectionViewModel(new EnumViewModel[]
                    {
                        new(typeof(SpecialType), SpecialType.None),
                        new(typeof(DuelType), DuelType.None),
                        new(typeof(SpecialEffectType), SpecialEffectType.None),
                        new(typeof(StatusRevisionType), StatusRevisionType.None),
                    }, "特殊タイプ")
                },
                { "duo_skill", new LabeledDescriptionViewModel("比翼双界スキル") },
                { "description", new LabeledDescriptionViewModel("概要") },
                { "official_url", new LabeledStringViewModel("URL") },
                { "internal_id", new LabeledStringViewModel("内部ID") },
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
                { "resplendent", Resplendent },
                { "resplendent_date", new LabeledDateTimeViewModel("神装リリース日") },
                { "resplendent_url", new LabeledStringViewModel("神装URL") },
                { "resplendent_costume", new LabeledStringViewModel("神装衣装") },
            };
            RegisterProperties(dict);

            ResplendentProperties.AddRange(dict.Where(x => x.Key.StartsWith("resplendent_")).Select(x => x.Value));

            AutoBindProperties.AddRange(dict
                .Where(x => !x.Key.StartsWith("resplendent") && x.Key != "type")
                .Select(x => x.Value));
        }

        public LabeledEnumViewModel Color { get; } = new LabeledEnumViewModel(typeof(ColorType), "属性");
        public LabeledEnumViewModel WeaponType { get; } = new LabeledEnumViewModel(typeof(WeaponType), "武器種");

        public LabeledBoolViewModel Resplendent { get; } = new("神装");
        public ObservableCollection<object> ResplendentProperties { get; } = new();

        protected override void SyncFromSourceCore()
        {
        }
    }
}
