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
        protected readonly Dictionary<string, object> ColumnNameToReadPropertyDict = new();
        protected readonly Dictionary<string, object> ColumnNameToWritePropertyDict = new();

        protected RowEditViewModelBase(DataRow source, TableViewModel tableViewModel)
        {
            _source = source;
            _tableViewModel = tableViewModel;
            _ = UpdateCommand.Subscribe(() =>
            {
                WriteBackToSource();
                //UpdateRecord();
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
            ColumnNameToReadPropertyDict.Add(columnName, prop);
            ColumnNameToWritePropertyDict.Add(columnName, prop);
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
            foreach (var prop in ColumnNameToReadPropertyDict)
            {
                switch (prop.Value)
                {
                    case LabeledStringViewModel cast:
                        cast.Value = GetStringValue(prop.Key);
                        break;
                    case LabeledDescriptionViewModel cast:
                        cast.Value = GetDescription(prop.Key);
                        break;
                    case LabeledEnumViewModel cast:
                        cast.Value = EnumUtility.ConvertDisplayNameToEnum(cast.EnumType, GetStringValue(prop.Key));
                        break;
                    case LabeledStringCollectionViewModel cast:
                        cast.AddRange(
                            ConvertToArray(GetStringValue(prop.Key))
                            .Select(x => new ReactiveProperty<string>(x)));
                        break;
                    case LabeledEnumCollectionViewModel cast:
                        cast.AddRange(
                            ConvertToArray(GetStringValue(prop.Key))
                            .Select(x => new ReactiveProperty<object>(EnumUtility.ConvertDisplayNameToEnum(cast.EnumType, x))));
                        break;
                    case LabeledIndivisualEnumCollectionViewModel cast:
                        {
                            var valueArray = ConvertToArray(GetStringValue(prop.Key));
                            foreach (var value in ConvertToArray(GetStringValue(prop.Key)))
                            {
                                var enumViewModel = cast.FindEnumViewModelFromValue(value);
                                if (enumViewModel is null)
                                {
                                    continue;
                                }

                                enumViewModel.Value = EnumUtility.ConvertDisplayNameToEnum(enumViewModel.EnumType, value);
                            }
                        }
                        break;
                    case LabeledIntStringViewModel cast:
                        cast.Value = GetIntValueAsString(prop.Key);
                        break;
                    case LabeledBoolViewModel cast:
                        {
                            var sourceValue = _source[prop.Key];
                            cast.Value = sourceValue is DBNull or null ? null
                                : sourceValue is string sourceStrValue ? string.IsNullOrEmpty(sourceStrValue) ? null : bool.Parse(sourceStrValue)
                                : (bool?)sourceValue;
                        }
                        break;
                    case LabeledDateTimeViewModel cast:
                        cast.Value = _source[prop.Key] is DBNull ? null : (DateTime)_source[prop.Key];
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

        private void UpdateRecord()
        {
            SqliteUtility.UpdateRecord(
                _tableViewModel.DatabasePath,
                _tableViewModel.TableName,
                _source,
                _tableViewModel.Schema);
        }

        private void WriteBackToSource()
        {
            foreach (var prop in ColumnNameToWritePropertyDict)
            {
                switch (prop.Value)
                {
                    case LabeledStringViewModel cast:
                        WriteToCell(prop.Key, cast.Value);
                        break;
                    case LabeledDescriptionViewModel cast:
                        WriteToCell(prop.Key, ConvertToDBDescription(cast.Value));
                        break;
                    case LabeledEnumViewModel cast:
                        WriteToCell(prop.Key, EnumUtility.ConvertEnumToDisplayName(cast.Value));
                        break;
                    case LabeledStringCollectionViewModel cast:
                        WriteToCell(prop.Key, ConvertToString(cast.Select(x => x.Value)));
                        break;
                    case LabeledEnumCollectionViewModel cast:
                        {
                            var value = ConvertToString(cast.Where(x => x != cast.DefaultValue).Select(x => EnumUtility.ConvertEnumToDisplayName(x.Value)));
                            if (cast.TrimsSqliteArraySeparatorOnBothSide && value is string strValue)
                            {
                                value = strValue.Trim('|');
                            }
                            WriteToCell(prop.Key, value);
                        }
                        break;
                    case LabeledIndivisualEnumCollectionViewModel cast:
                        {
                            var value = ConvertToString(cast.Where(x => x != x.DefaultValue).Select(x => EnumUtility.ConvertEnumToDisplayName(x.Value)));
                            if (cast.TrimsSqliteArraySeparatorOnBothSide && value is string strValue)
                            {
                                value = strValue.Trim('|');
                            }
                            WriteToCell(prop.Key, value);
                        }
                        break;
                    case LabeledIntStringViewModel cast:
                        WriteToCell(prop.Key, ConvertStringToInt64DBValue(cast.Value));
                        break;
                    case LabeledBoolViewModel cast:
                        WriteToCell(prop.Key, ConvertBoolValueToCellValue(cast.Value));
                        break;
                    case LabeledDateTimeViewModel cast:
                        WriteToCell(prop.Key, cast.Value is null ? DBNull.Value : cast.Value);
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
            _source[columnName] = value ?? DBNull.Value;
        }

        protected string GetDescription(string columnName)
        {
            return GetStringValue(columnName).Replace("<br/>", Environment.NewLine);
        }
        protected string ConvertToDBDescription(string value)
        {
            return value
                .Replace(" ", "")
                .Replace("守備魔防", "守備、魔防")
                .Replace("〇", "○")
                .Replace("－", "-")
                .Replace("備ー", "備-")
                .Replace("撃ー", "撃-")
                .Replace("さー", "さ-")
                .Replace("防ー", "防-")
                .Replace("Pー", "P-")
                .Replace("＋", "+")
                .Replace("x", "×")
                .Replace("（", "(")
                .Replace("）", ")")
                .Replace(Environment.NewLine, "<br/>"); ;
        }

        private static object? ConvertBoolValueToCellValue(bool? value)
        {
            if (value != null && value.Value)
            {
                return "true";
            }
            return null;
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
