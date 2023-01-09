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

namespace SqliteEditor.Plugins.SkillRowEditPlugins
{
    public enum EffectiveType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "重装")]
        Armor,
        [Display(Name = "騎馬")]
        Caverly,
        [Display(Name = "飛行")]
        Flier,
        [Display(Name = "歩行")]
        Infantry,
        [Display(Name = "獣")]
        Beast,
        [Display(Name = "竜")]
        Dragon,
        [Display(Name = "魔法")]
        Tome,
        [Display(Name = "剣")]
        Sword,
        [Display(Name = "槍")]
        Lance,
        [Display(Name = "斧")]
        Axe,
        [Display(Name = "無属性弓")]
        ColorlessBow,
    }

    public enum SkillType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "武器")]
        Weapon,
        [Display(Name = "サポート")]
        Support,
        [Display(Name = "奥義")]
        Special,
        [Display(Name = "パッシブA")]
        PassiveA,
        [Display(Name = "パッシブB")]
        PassiveB,
        [Display(Name = "パッシブC")]
        PassiveC,
        [Display(Name = "聖印")]
        SacredSeal,
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
        [Display(Name = "竜石")]
        DragonStone,
        [Display(Name = "獣")]
        Beast,
        [Display(Name = "赤魔法")]
        RedTome,
        [Display(Name = "青魔法")]
        BlueTome,
        [Display(Name = "緑魔法")]
        GreenTome,
        [Display(Name = "無魔法")]
        ColorlessTome,
        [Display(Name = "暗器")]
        Dagger,
        [Display(Name = "弓")]
        Bow,
    }

    public class SkillRowViewModel : RowEditViewModelBase
    {
        public static Array SkillTypes => Enum.GetValues(typeof(SkillType));
        public static Array EffectiveTypes => Enum.GetValues(typeof(EffectiveType));


        public SkillRowViewModel(DataRow source, TableViewModel tableViewModel)
            : base(source, tableViewModel)
        {
            _ = AddEffectiveCommand.Subscribe(() =>
            {
                Effectives.Add(new ReactiveProperty<EffectiveType>());
            }).AddTo(Disposable);
            _ = RemoveEffectiveCommand.Subscribe(() =>
            {
                Effectives.RemoveAt(Effectives.Count - 1);
            }).AddTo(Disposable);
        }

        public LabeledStringViewModel Name { get; } = new("名前");
        public LabeledStringViewModel EnglishName { get; } = new("英語名");
        public LabeledStringViewModel MustLearn { get; } = new("下位スキル");
        public LabeledStringViewModel ReleaseDate { get; } = new("リリース日");
        public ReactiveProperty<bool?> Inherit { get; } = new();
        public LabeledStringViewModel Description { get; } = new("説明");
        public LabeledStringViewModel RefineDescription { get; } = new("説明(錬成)");
        public LabeledStringViewModel SpecialRefineDescription { get; } = new("説明(特殊錬成)");
        public LabeledIntStringViewModel Sp { get; } = new("SP");
        public LabeledIntStringViewModel Count { get; } = new("奥義カウント");
        public LabeledIntStringViewModel Might { get; } = new("威力");
        public LabeledIntStringViewModel MightRefine { get; } = new("錬成後の威力");
        public LabeledEnumViewModel SkillType { get; } = new(typeof(SkillType), "スキル種");
        public ObservableCollection<ReactiveProperty<EffectiveType>> Effectives { get; } = new();

        public LabeledBoolViewModel HasKillerEffect { get; } = new("キラー効果");


        public ReactiveCommand AddEffectiveCommand { get; } = new ReactiveCommand();
        public ReactiveCommand RemoveEffectiveCommand { get; } = new ReactiveCommand();

        protected override void RegisterProperties()
        {
            RegisterProperties(new Dictionary<string, object>()
            {
                { "name", Name },
                { "english_name", EnglishName },
                { "must_learn", MustLearn },
                { "release_date", ReleaseDate },
                { "type", SkillType },
                { "weapon_type", new LabeledEnumViewModel(typeof(WeaponType), "武器種") },
                { "sp", Sp },
                { "count", Count },
                { "might", Might },
                { "might_refine", MightRefine },
            });
        }

        protected override void SyncFromSourceCore()
        {
            var inheritStr = GetStringValue("inherit");
            Inherit.Value = inheritStr == "可" ? true : inheritStr == "不可" ? false : null;
            Description.Value = GetDescription("description");
            RefineDescription.Value = GetDescription("refine_description");
            SpecialRefineDescription.Value = GetDescription("special_refine_description");

            Effectives.Clear();
            foreach (var value in ConvertToArray(GetStringValue("effective")).Select(x => EnumUtility.ConvertDisplayNameToEnum<EffectiveType>(x)))
            {
                Effectives.Add(new ReactiveProperty<EffectiveType>(value));
            }

            HasKillerEffect.Value = GetIntValueAsString("cooldown_count") == "-1" ? true : false;
        }

        protected override void WriteBackToSourceCore()
        {
            WriteToCell("inherit", Inherit.Value is null ? DBNull.Value : Inherit.Value.Value ? "可" : "不可");
            WriteToCell("description", ConvertToDBDescription(Description.Value));
            WriteToCell("refine_description", ConvertToDBDescription(RefineDescription.Value));
            WriteToCell("special_refine_description", ConvertToDBDescription(SpecialRefineDescription.Value));
            WriteToCell("effective", ConvertToString(Effectives.Select(x => EnumUtility.ConvertEnumToDisplayName(x.Value))));
            WriteToCell("cooldown_count", HasKillerEffect.Value ? "-1" : DBNull.Value);
        }
    }
}
