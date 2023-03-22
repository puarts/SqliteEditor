using Reactive.Bindings;
using SqliteEditor.Plugins.SkillRowEditPlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SqliteEditor.ViewModels
{
    public abstract class EnumViewModelBase : ReactiveProperty<object>
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
    }

    public class EnumViewModel : EnumViewModelBase
    {
        public EnumViewModel(Type enumType)
            : base(enumType)
        {
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
        private readonly Func<bool>? _isVisibleFunc;
        private bool _isVisible = true;

        public LabeledEnumViewModel(Type enumType, string label, Func<bool>? isVisibleFunc = null)
            : base(enumType)
        {
            Label = label;
            this._isVisibleFunc = isVisibleFunc;
        }

        public LabeledEnumViewModel(Type enumType, string label, object defaultValue, Func<bool>? isVisibleFunc = null)
            : base(enumType, defaultValue)
        {
            Label = label;
            this._isVisibleFunc = isVisibleFunc;
        }

        public string Label { get; }

        public bool IsVisible { get => _isVisible; }

        public void UpdateVisibility()
        {
            _isVisible = _isVisibleFunc?.Invoke() ?? true;
        }
    }
}
