﻿using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqliteEditor.SkillRowEditPlugins
{
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

    public class SkillRowViewModel : CompositeDisposableBase
    {
        private readonly DataRow _source;
        private readonly TableViewModel _tableViewModel;

        public static Array SkillTypes => System.Enum.GetValues(typeof(SkillType));

        public SkillRowViewModel(DataRow source, TableViewModel tableViewModel)
        {
            _source = source;
            _tableViewModel = tableViewModel;
            _ = UpdateCommand.Subscribe(() =>
            {
                WriteBackToSource();
            }).AddTo(Disposable);

            SyncFromSource();
        }

        public ReactiveProperty<string> Name { get; } = new ReactiveProperty<string>();
        public ReactiveProperty<string> Description { get; } = new();
        public ReactiveProperty<string> Sp { get; } = new();
        public ReactiveProperty<SkillType> SkillType { get; } = new ReactiveProperty<SkillType>();

        

        public ReactiveCommand UpdateCommand { get; } = new ReactiveCommand();

        private void SyncFromSource()
        {
            Name.Value = (string)_source["name"];
            Sp.Value = _source["sp"].ToString()!;
            Description.Value = ((string)_source["description"]).Replace("<br/>", "\n");
            SkillType.Value = EnumUtility.ConvertDisplayNameToEnum<SkillType>((string)_source["type"]);
        }

        private void WriteBackToSource()
        {
            _source["name"] = Name.Value;
            _source["sp"] = long.Parse(Sp.Value);
            _source["description"] = Description.Value.Replace("\n", "<br/>");
            _source["type"] = EnumUtility.ConvertEnumToDisplayName(SkillType.Value);
            _tableViewModel.UpdateDirty();
        }
    }
}
