﻿using Reactive.Bindings;
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
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        [Display(Name = "響心")]
        Attuned,
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

            _ = Name.Subscribe(name =>
            {
                if (name == string.Empty) return;

                if (char.IsNumber(name.Last()))
                {
                    Inherit.Value = true;
                }
            }).AddTo(Disposer);

            _ = WeaponType.Subscribe(x =>
            {
                bool isWeapon = (SkillRowEditPlugins.SkillType)x != SkillRowEditPlugins.SkillType.None;
                if (isWeapon)
                {
                    SkillType.Value = SkillRowEditPlugins.SkillType.Weapon;
                }
            }).AddTo(Disposer);

            _ = SkillType.Subscribe(x =>
            {
                var skillType = (SkillRowEditPlugins.SkillType)x;
                bool isWeapon = skillType == SkillRowEditPlugins.SkillType.Weapon;
                var weaponProps = new IPropertyViewModel[]
                {
                    Might,
                    MightRefine,
                    CanStatusRefine,
                    SpecialRefineHp,
                    RefineDate,
                    RefineDescription,
                    RefineDescription2,
                    SpecialRefineDescription,
                };
                foreach (var prop in weaponProps)
                {
                    prop.IsVisible.Value = isWeapon;
                }

                Count.IsVisible.Value = skillType == SkillRowEditPlugins.SkillType.Special;
            }).AddTo(Disposer);

            _ = Inherit.Subscribe(inherit =>
            {
                var skillType = (SkillRowEditPlugins.SkillType)SkillType.Value;
                switch (skillType)
                {
                    case SkillRowEditPlugins.SkillType.Weapon:
                        Sp.Value = !inherit.GetValueOrDefault() ? "400" :
                                        Name.Value.EndsWith("+") || Name.Value.StartsWith("魔器・") ?
                                        "300" : "200";
                        break;
                    case SkillRowEditPlugins.SkillType.PassiveA:
                    case SkillRowEditPlugins.SkillType.PassiveB:
                    case SkillRowEditPlugins.SkillType.PassiveC:
                        if (!inherit.GetValueOrDefault())
                        {
                            Sp.Value = "300";
                        }
                        break;
                    case SkillRowEditPlugins.SkillType.Special:
                        if (!inherit.GetValueOrDefault())
                        {
                            Sp.Value = "500";
                        }
                        break;
                }
            }).AddTo(Disposer);

            _ = UpdateByDescriptionCommand.Subscribe(() =>
            {
                SyncFromDescription(Description.Value);
            });

            _ = Description.Subscribe(SyncFromDescription).AddTo(Disposer);
        }

        private void SyncFromDescription(string value)
        {
            var desc = ConvertToDBDescription(value);
            if (desc.StartsWith("奥義が発動しやすい(発動カウント-1)") || desc.Contains("<br>奥義が発動しやすい(発動カウント-1)"))
            {
                HasKillerEffect.Value = true;
            }
            else if (desc.StartsWith("奥義がとても発動しやすい(発動") || desc.Contains("<br>奥義がとても発動しやすい(発動"))
            {
                HasKillerEffect2.Value = true;
            }
            if (desc.StartsWith("杖は他の武器同様のダメージ計算になる") || desc.Contains("<br>杖は他の武器同様のダメージ計算になる"))
            {
                WrathfullStaff.Value = true;
            }
            if (HasStatusAdd(desc, "攻撃"))
            {
                Atk.Value = "3";
            }
            if (HasStatusAdd(desc, "速さ"))
            {
                Spd.Value = "3";
            }
            if (HasStatusAdd(desc, "守備"))
            {
                Def.Value = "3";
            }
            if (HasStatusAdd(desc, "魔防"))
            {
                Res.Value = "3";
            }

            var effectiveType = typeof(EffectiveType);
            foreach (var enumValue in Enum.GetValues<EffectiveType>())
            {
                MemberInfo[] memInfo = effectiveType.GetMember(enumValue.ToString());
                object[] attributes = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                var name = ((DisplayAttribute)attributes[0]).Name;
                if (desc.StartsWith($"{name}特効<br>") || desc.Contains($"<br>{name}特効") || desc.EndsWith($"{name}特効"))
                {
                    Effectives.SetOrAdd(enumValue);
                }
                if (desc.StartsWith($"{name}特効無効<br>") || desc.Contains($"<br>{name}特効無効") || desc.EndsWith($"{name}特効無効"))
                {
                    InvalidateEffectives.SetOrAdd(enumValue);
                }
            }
        }

        private bool HasStatusAdd(string desc, string status)
        {
            return desc.StartsWith($"{status}+3<br") || desc.Contains($"<br>{status}+3<br>");
        }

        public ReactiveCommand UpdateByDescriptionCommand { get; } = new();

        public LabeledStringCollectionViewModel MustLearn { get; } = new("下位スキル");
        public LabeledBoolViewModel Inherit { get; } = new("継承可否");
        public LabeledIntStringViewModel Sp { get; } = new("SP");
        public LabeledIntStringViewModel Count { get; } = new("奥義カウント");
        public LabeledIntStringViewModel Might { get; } = new("威力");
        public LabeledIntStringViewModel MightRefine { get; } = new("錬成後の威力");
        public LabeledEnumViewModel SkillType { get; } = new(typeof(SkillType), "スキル種");
        public LabeledEnumCollectionViewModel Effectives { get; } = new(typeof(EffectiveType), "特効", EffectiveType.None);
        public LabeledEnumCollectionViewModel InvalidateEffectives { get; } = new(typeof(EffectiveType), "特効無効", EffectiveType.None);

        public LabeledBoolViewModel HasKillerEffect { get; } = new("キラー効果");
        public LabeledBoolViewModel HasKillerEffect2 { get; } = new("キラー効果2");
        public LabeledEnumViewModel WeaponType { get; } = new LabeledEnumViewModel(typeof(WeaponType), "武器種");
        public LabeledDescriptionViewModel Description { get; } = new LabeledDescriptionViewModel("説明");
        public LabeledIntStringViewModel Atk { get; } = new LabeledIntStringViewModel("攻撃");
        public LabeledIntStringViewModel Spd { get; } = new LabeledIntStringViewModel("速さ");
        public LabeledIntStringViewModel Def { get; } = new LabeledIntStringViewModel("守備");
        public LabeledIntStringViewModel Res { get; } = new LabeledIntStringViewModel("魔防");
        public LabeledBoolViewModel WrathfullStaff { get; } = new LabeledBoolViewModel("神罰");
        public ObservableCollection<IPropertyViewModel> AutoBindProperties { get; } = new ObservableCollection<IPropertyViewModel>();

        public LabeledStringViewModel InheritableWeaponType { get; } = new LabeledStringViewModel("武器制限");

        public LabeledStringViewModel Name { get; } = new LabeledStringViewModel("名前");

        public LabeledBoolViewModel CanStatusRefine { get; } = new LabeledBoolViewModel("ステータス錬成");

        public LabeledIntStringViewModel SpecialRefineHp { get; } = new("特殊錬成後のHP+");

        public LabeledDateTimeViewModel RefineDate { get; } = new LabeledDateTimeViewModel("錬成日");

        public LabeledDescriptionViewModel RefineDescription { get; } = new LabeledDescriptionViewModel("説明(錬成)");
        public LabeledDescriptionViewModel RefineDescription2 { get; } = new LabeledDescriptionViewModel("説明(錬成2)");
        public LabeledDescriptionViewModel SpecialRefineDescription { get; } = new LabeledDescriptionViewModel("説明(特殊錬成)");

        protected override void RegisterProperties()
        {
            var dict = new Dictionary<string, IPropertyViewModel>()
            {
                { "type", SkillType },
                { "weapon_type", WeaponType },
                { "assist_type", new LabeledEnumViewModel(typeof(AssistType), "補助種") },
                { "name", Name },
                { "english_name", new LabeledStringViewModel("英語名") },
                { "description", Description },
                { "refine_description", RefineDescription },
                { "refine_description2", RefineDescription2 },
                { "special_refine_description", SpecialRefineDescription },
                { "can_status_refine", CanStatusRefine },
                { "special_refine_hp", SpecialRefineHp },
                { "must_learn", MustLearn },
                { "release_date", new LabeledDateTimeViewModel("リリース日") },
                { "refined_date", RefineDate },
                { "inheritable_weapon_type", InheritableWeaponType },
                { "inheritable_move_type", new LabeledStringViewModel("移動制限") },
                { "sp", Sp },
                { "count", Count },
                { "might", Might },
                { "might_refine", MightRefine },
                { "effective", Effectives },
                { "invalidate_effective", InvalidateEffectives },
                { "hp", new LabeledIntStringViewModel("HP") },
                { "atk", Atk },
                { "spd", Spd },
                { "def", Def },
                { "res", Res },
                { "wrathful_staff", WrathfullStaff },
                { "disable_counter", new LabeledBoolViewModel("反撃不可") },
                { "all_dist_counter", new LabeledBoolViewModel("全距離反撃") },
                { "counteratk_count", new LabeledIntStringViewModel("反撃時の攻撃回数") },
                { "atk_count", new LabeledIntStringViewModel("攻撃回数") },
            };
            RegisterProperties(dict);

            var bindProps = dict.Values.ToList();
            bindProps.Insert(bindProps.IndexOf(InheritableWeaponType), Inherit);
            bindProps.Add(HasKillerEffect);
            bindProps.Add(HasKillerEffect2);

            AutoBindProperties.AddRange(bindProps);
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
            HasKillerEffect2.Value = GetIntValueAsString("cooldown_count") == "-2" ? true : false;
        }

        protected override void WriteBackToSourceCore()
        {
            WriteToCell("inherit", Inherit.Value is null ? DBNull.Value : Inherit.Value.Value ? "可" : "不可");
            //WriteToCell("effective", ConvertToString(Effectives.Select(x => EnumUtility.ConvertEnumToDisplayName(x.Value))));
            WriteToCell("cooldown_count", HasKillerEffect.Value ?? false ? "-1" : HasKillerEffect2.Value ?? false ? "-2" : DBNull.Value);
        }
    }
}
