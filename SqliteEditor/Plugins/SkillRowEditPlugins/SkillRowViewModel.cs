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

            _ = WeaponType.Subscribe(x =>
            {
                if ((SkillRowEditPlugins.SkillType)x != SkillRowEditPlugins.SkillType.None)
                {
                    SkillType.Value = SkillRowEditPlugins.SkillType.Weapon;
                }
            }).AddTo(Disposer);

            _ = Inherit.Subscribe(inherit =>
            {
                if ((SkillRowEditPlugins.SkillType)SkillType.Value == SkillRowEditPlugins.SkillType.Weapon)
                {
                    Sp.Value = !inherit.GetValueOrDefault() ? "400" :
                    Name.Value.EndsWith("+") || Name.Value.StartsWith("魔器・") ? 
                    "300" : "200";
                }
            }).AddTo(Disposer);

            _ = Description.Subscribe(value =>
            {
                var desc = ConvertToDBDescription(value);
                if (desc.StartsWith("奥義が発動しやすい(発動カウント-1)") || desc.Contains("<br>奥義が発動しやすい(発動カウント-1)"))
                {
                    HasKillerEffect.Value = true;
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
                    if (HasStatusAdd(desc, $"{name}特効<br>"))
                    {
                        Effectives.SetOrAdd(enumValue);
                    }
                }
            }).AddTo(Disposer);
        }

        private bool HasStatusAdd(string desc, string status)
        {
            return desc.StartsWith($"{status}+3<br") || desc.Contains($"<br>{status}+3<br>");
        }


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
        public LabeledEnumViewModel WeaponType { get; } = new LabeledEnumViewModel(typeof(WeaponType), "武器種");
        public LabeledDescriptionViewModel Description { get; } = new LabeledDescriptionViewModel("説明");
        public LabeledIntStringViewModel Atk { get; } = new LabeledIntStringViewModel("攻撃");
        public LabeledIntStringViewModel Spd { get; } = new LabeledIntStringViewModel("速さ");
        public LabeledIntStringViewModel Def { get; } = new LabeledIntStringViewModel("守備");
        public LabeledIntStringViewModel Res { get; } = new LabeledIntStringViewModel("魔防");
        public LabeledBoolViewModel WrathfullStaff { get; } = new LabeledBoolViewModel("神罰");
        public ObservableCollection<object> AutoBindProperties { get; } = new ObservableCollection<object>();

        public LabeledStringViewModel InheritableWeaponType { get; } = new LabeledStringViewModel("武器制限");

        public LabeledStringViewModel Name { get; } = new LabeledStringViewModel("名前");
        protected override void RegisterProperties()
        {
            var dict = new Dictionary<string, object>()
            {
                { "name", Name },
                { "english_name", new LabeledStringViewModel("英語名") },
                { "description", Description },
                { "refine_description", new LabeledDescriptionViewModel("説明(錬成)") },
                { "refine_description2", new LabeledDescriptionViewModel("説明(錬成2)") },
                { "special_refine_description", new LabeledDescriptionViewModel("説明(特殊錬成)") },
                { "can_status_refine", new LabeledBoolViewModel("ステータス錬成") },
                { "special_refine_hp", new LabeledIntStringViewModel("特殊錬成後のHP+") },
                { "must_learn", MustLearn },
                { "release_date", new LabeledDateTimeViewModel("リリース日") },
                { "refined_date", new LabeledDateTimeViewModel("錬成日") },
                { "type", SkillType },
                { "weapon_type", WeaponType },
                { "assist_type", new LabeledEnumViewModel(typeof(AssistType), "補助種") },
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
        }

        protected override void WriteBackToSourceCore()
        {
            WriteToCell("inherit", Inherit.Value is null ? DBNull.Value : Inherit.Value.Value ? "可" : "不可");
            //WriteToCell("effective", ConvertToString(Effectives.Select(x => EnumUtility.ConvertEnumToDisplayName(x.Value))));
            WriteToCell("cooldown_count", HasKillerEffect.Value ?? false ? "-1" : DBNull.Value);
        }
    }
}
