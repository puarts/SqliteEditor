using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Extensions;
using SqliteEditor.Plugins.SkillRowEditPlugins;
using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SqliteEditor.Plugins
{
    public abstract class RowEditViewModelBase : CompositeDisposableBase
    {
        private readonly DataRow _source;
        private readonly TableViewModel _tableViewModel;
        protected readonly Dictionary<string, object> ColumnNameToRowPropertyDict = new();

        protected RowEditViewModelBase(DataRow source, TableViewModel tableViewModel)
        {
            _source = source;
            _tableViewModel = tableViewModel;
            _ = UpdateCommand.Subscribe(() =>
            {
                WriteBackToSource();
            }).AddTo(Disposable);

            RegisterProperties();

            SyncFromSource();
        }

        public ObservableCollection<object> RowProperties { get; } = new ObservableCollection<object>();

        protected virtual void RegisterProperties()
        {
        }

        public ReactiveCommand UpdateCommand { get; } = new ReactiveCommand();

        protected void RegisterProp(string columnName, object prop)
        {
            ColumnNameToRowPropertyDict.Add(columnName, prop);
            RowProperties.Add(prop);
        }

        protected void RegisterProperties(Dictionary<string, object> props)
        {
            foreach (var prop in props)
            {
                RegisterProp(prop.Key, prop.Value);
            }
        }

        private void SyncFromSource()
        {
            foreach (var prop in ColumnNameToRowPropertyDict)
            {
                switch (prop.Value)
                {
                    case LabeledStringViewModel cast:
                        cast.Value = GetStringValue(prop.Key);
                        break;
                    case LabeledEnumViewModel cast:
                        cast.Value = EnumUtility.ConvertDisplayNameToEnum(cast.EnumType, GetStringValue(prop.Key));
                        break;
                    case LabeledStringCollectionViewModel cast:
                        cast.AddRange(ConvertToArray(GetStringValue(prop.Key)).Select(x => new ReactiveProperty<string>(x)));
                        break;
                    case LabeledIntStringViewModel cast:
                        cast.Value = GetIntValueAsString(prop.Key);
                        break;
                    default:
                        break;
                }
            }

            SyncFromSourceCore();
        }

        protected virtual void SyncFromSourceCore()
        {
        }

        protected string GetStringValue(string columnName)
        {
            return _source[columnName] is string str ? str : "";
        }

        protected string GetIntValueAsString(string columnName)
        {
            return _source[columnName].ToString()!;
        }

        private void WriteBackToSource()
        {
            foreach (var prop in ColumnNameToRowPropertyDict)
            {
                switch (prop.Value)
                {
                    case LabeledStringViewModel cast:
                        WriteToCell(prop.Key, cast.Value);
                        break;
                    case LabeledEnumViewModel cast:
                        WriteToCell(prop.Key, EnumUtility.ConvertEnumToDisplayName(cast.Value));
                        break;
                    case LabeledStringCollectionViewModel cast:
                        WriteToCell(prop.Key, ConvertToString(cast.Select(x => x.Value)));
                        break;
                    case LabeledIntStringViewModel cast:
                        WriteToCell(prop.Key, ConvertStringToInt64DBValue(cast.Value));
                        break;
                    default:
                        break;
                }
            }

            WriteBackToSourceCore();
            _tableViewModel.UpdateDirty();
        }

        protected virtual void WriteBackToSourceCore()
        {
        }

        protected void WriteToCell(string columnName, object? value)
        {
            _source[columnName] = value;
        }

        protected string GetDescription(string columnName)
        {
            return GetStringValue(columnName).Replace("<br/>", Environment.NewLine);
        }
        protected string ConvertToDBDescription(string value)
        {
            return value.Replace(" ", "").Replace(Environment.NewLine, "<br/>"); ;
        }

        protected static object? ConvertStringToInt64DBValue(string value)
        {
            return long.TryParse(value, out long numValue) ? numValue : System.DBNull.Value;
        }

        protected static string[] ConvertToArray(string value)
        {
            return value.Split('|', StringSplitOptions.RemoveEmptyEntries);
        }

        protected static object? ConvertToString(IEnumerable<string> values)
        {
            var nonEmptyValues = values.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (!nonEmptyValues.Any())
            {
                return DBNull.Value;
            }
            return "|" + string.Join('|', nonEmptyValues) + "|";
        }
    }
}
