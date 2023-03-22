﻿using Reactive.Bindings;
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
        [Display(Name = "隊長")]
        Captain,
    }

    public enum AssistType
    {
        [Display(Name = "")]
        None,
        [Display(Name = "Refresh")]
        Refresh,
        [Display(Name = "Move")]
        Move,
        [Display(Name = "Rally")]
        Rally,
        [Display(Name = "DonorHeal")]
        DonorHeal,
        [Display(Name = "Heal")]
        Heal,
        [Display(Name = "Restore")]
        Restore,
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
            Effectives.AddNewItemsWhile(() => Effectives.Count < 2);
            InvalidateEffectives.AddNewItemsWhile(() => InvalidateEffectives.Count < 2);
            MustLearn.AddNewItemsWhile(() => MustLearn.Count < 2);

            _ = WeaponType.Subscribe(x =>
            {
                if ((SkillRowEditPlugins.SkillType)x != SkillRowEditPlugins.SkillType.None)
                {
                    SkillType.Value = SkillRowEditPlugins.SkillType.Weapon;
                }
            }).AddTo(Disposable);

            _ = Inherit.Subscribe(x =>
            {
                if ((SkillRowEditPlugins.SkillType)SkillType.Value == SkillRowEditPlugins.SkillType.Weapon)
                {
                    Sp.Value = x == true ? "300" : "400";
                }
            }).AddTo(Disposable);
        }

        public LabeledStringCollectionViewModel MustLearn { get; } = new("下位スキル");
        public ReactiveProperty<bool?> Inherit { get; } = new();
        public LabeledIntStringViewModel Sp { get; } = new("SP");
        public LabeledIntStringViewModel Count { get; } = new("奥義カウント");
        public LabeledIntStringViewModel Might { get; } = new("威力");
        public LabeledIntStringViewModel MightRefine { get; } = new("錬成後の威力");
        public LabeledEnumViewModel SkillType { get; } = new(typeof(SkillType), "スキル種");
        public LabeledEnumCollectionViewModel Effectives { get; } = new(typeof(EffectiveType), "特効", EffectiveType.None);
        public LabeledEnumCollectionViewModel InvalidateEffectives { get; } = new(typeof(EffectiveType), "特効無効", EffectiveType.None);

        public LabeledBoolViewModel HasKillerEffect { get; } = new("キラー効果");
        public LabeledEnumViewModel WeaponType { get; } = new LabeledEnumViewModel(typeof(WeaponType), "武器種");

        protected override void RegisterProperties()
        {
            RegisterProperties(new Dictionary<string, object>()
            {
                { "name", new LabeledStringViewModel("名前") },
                { "english_name", new LabeledStringViewModel("英語名") },
                { "description", new LabeledDescriptionViewModel("説明") },
                { "refine_description", new LabeledDescriptionViewModel("説明(錬成)") },
                { "special_refine_description", new LabeledDescriptionViewModel("説明(特殊錬成)") },
                { "can_status_refine", new LabeledBoolViewModel("ステータス錬成") },
                { "special_refine_hp", new LabeledIntStringViewModel("特殊錬成後のHP+") },
                { "must_learn", MustLearn },
                { "release_date", new LabeledDateTimeViewModel("リリース日") },
                { "refined_date", new LabeledDateTimeViewModel("錬成日") },
                { "type", SkillType },
                { "weapon_type", WeaponType },
                { "assist_type", new LabeledEnumViewModel(typeof(AssistType), "補助種") },
                { "sp", Sp },
                { "count", Count },
                { "might", Might },
                { "might_refine", MightRefine },
                { "effective", Effectives },
                { "invalidate_effective", InvalidateEffectives },
                { "hp", new LabeledIntStringViewModel("HP") },
                { "atk", new LabeledIntStringViewModel("攻撃") },
                { "spd", new LabeledIntStringViewModel("速さ") },
                { "def", new LabeledIntStringViewModel("守備") },
                { "res", new LabeledIntStringViewModel("魔防") },
                { "wrathful_staff", new LabeledBoolViewModel("神罰") },
                { "disable_counter", new LabeledBoolViewModel("反撃不可") },
                { "all_dist_counter", new LabeledBoolViewModel("全距離反撃") },
                { "counteratk_count", new LabeledIntStringViewModel("反撃時の攻撃回数") },
                { "atk_count", new LabeledIntStringViewModel("攻撃回数") },

            });
        }

        protected override void SyncFromSourceCore()
        {
            var inheritStr = GetStringValue("inherit");
            Inherit.Value = inheritStr == "可" ? true : inheritStr == "不可" ? false : null;

            //Effectives.Clear();
            //foreach (var value in ConvertToArray(GetStringValue("effective")).Select(x => EnumUtility.ConvertDisplayNameToEnum<EffectiveType>(x)))
            //{
            //    Effectives.Add(new ReactiveProperty<EffectiveType>(value));
            //}

            HasKillerEffect.Value = GetIntValueAsString("cooldown_count") == "-1" ? true : false;
        }

        protected override void WriteBackToSourceCore()
        {
            WriteToCell("inherit", Inherit.Value is null ? DBNull.Value : Inherit.Value.Value ? "可" : "不可");
            //WriteToCell("effective", ConvertToString(Effectives.Select(x => EnumUtility.ConvertEnumToDisplayName(x.Value))));
            WriteToCell("cooldown_count", HasKillerEffect.Value ?? false ? "-1" : DBNull.Value);
        }
    }
}
