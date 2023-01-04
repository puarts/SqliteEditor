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

        public TableViewModel(DataTable source)
        {
            _dataTable = source;
            _dirtySourceTable = source.Copy();
        }

        public string TableName { get => _dataTable.TableName; }

        public DataTable DataTable
        {
            get => _dataTable;
        }

        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        public void UpdateDirty()
        {
            IsDirty = !DataTableUtility.AreTablesEqual(_dataTable, _dirtySourceTable);
        }
    }
}
