using SqliteEditor.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
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

        public TableViewModel(DataTable data, DataTable schema)
        {
            _dataTable = data;
            _schema = schema;
            _dirtySourceTable = data.Copy();
        }

        public string TableName { get => _dataTable.TableName; }

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
