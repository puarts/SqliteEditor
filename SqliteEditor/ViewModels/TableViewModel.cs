using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using SqliteEditor.Extensions;
using SqliteEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace SqliteEditor.ViewModels
{
    public class TableViewModel : CompositeDisposableBase
    {
        private bool _isDirty = false;
        private DataTable _dirtySourceTable;
        private DataTable _dataTable;
        private DataTable _schema;

        public TableViewModel(
            DataTable data, 
            DataTable schema,
            Action<string> errorLogAction,
            MainViewModel mainViewModel)
        {
            _dataTable = data;
            _schema = schema;
            _dirtySourceTable = data.Copy();

            foreach (DataColumn col in _dataTable.Columns)
            {
                var columnItem = new ColumnItemViewModel(col.ColumnName);
                ColumnItems.Add(columnItem);

                _ = columnItem.IsVisible.Subscribe(isVisible =>
                {
                    col.ColumnMapping = isVisible ? MappingType.Element : MappingType.Hidden;
                }).AddTo(Disposable);
            }

            _ = RowFilter.StartWith("").CombineLatest(RowNameFilter.StartWith("")).Subscribe(args =>
            {
                var filter = args.First;
                var nameFilter = args.Second;
                var view = _dataTable.DefaultView;
                try
                {
                    List<string> actualFilters = new List<string>();
                    if (!string.IsNullOrEmpty(filter))
                    {
                        actualFilters.Add(filter);
                    }
                    if (!string.IsNullOrEmpty(nameFilter))
                    {
                        List<string> nameFilters = new List<string>();
                        foreach (var name in nameFilter.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                        {
                            nameFilters.Add($"name like '%{name}%'");
                        }
                        actualFilters.Add("(" + string.Join(" or ", nameFilters) + ")");
                    }
                    view.RowFilter = string.Join(" and ", actualFilters);
                }
                catch (Exception exception)
                {
                    errorLogAction(exception.Message);
                    view.RowFilter = "";
                }
            }).AddTo(Disposable);
        }

        public string TableName { get => _dataTable.TableName; }

        public ObservableCollection<ColumnItemViewModel> ColumnItems { get; } = new ObservableCollection<ColumnItemViewModel>();

        public ReactiveProperty<string?> RowFilter { get; } = new();
        public ReactiveProperty<string?> RowNameFilter { get; } = new();

        public DataTable DataTable
        {
            get => _dataTable;
        }
        public DataTable Schema
        {
            get => _schema;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public void UpdateDirtySource()
        {
            _dirtySourceTable = _dataTable.Copy();
            IsDirty = false;
        }

        public void UpdateDirty()
        {
            IsDirty = !DataTableUtility.AreTablesEqual(_dataTable, _dirtySourceTable);
        }
    }
}
