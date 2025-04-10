﻿using Reactive.Bindings;
using SqliteEditor.Plugins.SkillRowEditPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SqliteEditor.ViewModels
{
    public abstract class EnumViewModelBase : ReactiveProperty<object>, IPropertyViewModel
    {
        public EnumViewModelBase(Type enumType)
        {
            EnumType = enumType;
            EnumValues = Enum.GetValues(enumType);
        }

        public EnumViewModelBase(Type enumType, object defaultValue)
            : this(enumType)
        {
            Value = defaultValue;
        }

        public TEnum GetEnumValue<TEnum>() => (TEnum)Value;

        public Type EnumType { get; }

        public Array EnumValues { get; }
        public ReactiveProperty<bool> IsVisible { get; } = new(true);
    }

    public class EnumViewModel : EnumViewModelBase
    {
        public EnumViewModel(Type enumType)
            : base(enumType)
        {
            DefaultValue = System.Enum.GetValues(enumType).GetValue(0) ?? throw new Exception();
        }

        public EnumViewModel(Type enumType, object defaultValue)
            : base(enumType, defaultValue)
        {
            Value = defaultValue;
            DefaultValue = defaultValue;
        }

        public object DefaultValue { get; }
    }

    public class LabeledEnumViewModel : EnumViewModelBase
    {
        public LabeledEnumViewModel(Type enumType, string label, Func<bool>? isVisibleFunc = null)
            : base(enumType)
        {
            Label = label;
        }

        public LabeledEnumViewModel(Type enumType, string label, object defaultValue, Func<bool>? isVisibleFunc = null)
            : base(enumType, defaultValue)
        {
            Label = label;
        }

        public string Label { get; }
    }
}
