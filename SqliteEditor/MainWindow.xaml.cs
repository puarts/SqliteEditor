using SqliteEditor.Utilities;
using SqliteEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SqliteEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _isManualEditCommit = false;

        public MainWindow()
        {
            InitializeComponent();
        }

        private MainViewModel ViewModel { get => (MainViewModel)DataContext; }

        private void Window_Closed(object sender, EventArgs e)
        {
            ViewModel.SaveApplicationSettings();
        }

        private void DataTableGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (!_isManualEditCommit)
            {
                _isManualEditCommit = true;
                DataGrid grid = (DataGrid)sender;
                grid.CommitEdit(DataGridEditingUnit.Row, true);
                _isManualEditCommit = false;

                //ViewModel.UpdateSelectedTableDirty();
                var tableViewModel = ViewModel.GetSelectedTableViewModel();
                var row = ViewModel.SelectedRow.Value;
                if (tableViewModel is not null && row is not null)
                {
                    SqliteUtility.UpdateRecordById(
                        tableViewModel.DatabasePath,
                        tableViewModel.TableName,
                        row.Row,
                        tableViewModel.Schema);
                }
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            bool success = ViewModel.ShowConfirmSavingDialog(this);
            if (!success)
            {
                e.Cancel = true;
            }
        }

        private void RowFilterBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.SelectedTable.Value.RowFilter.Value = ((TextBox)sender).Text;
            }
        }

        private void RowFilterBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.SelectedTable.Value.RowNameFilter.Value = ((TextBox)sender).Text;
            }
        }

        private void DataGrid_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true; // デフォルトのショートカットを無効化

                if (sender is DataGrid dataGrid && dataGrid.CurrentCell != null)
                {
                    var cell = GetCell(dataGrid.CurrentCell);
                    if (cell != null)
                    {
                        if (!cell.IsEditing)
                        {
                            dataGrid.BeginEdit();
                        }
                        else
                        {
                            dataGrid.CommitEdit();
                            MoveFocusToNextCell(dataGrid);
                        }
                    }
                }
            }
        }

        private static DataGridCell? GetCell(DataGridCellInfo cellInfo)
        {
            var cellContent = cellInfo.Column.GetCellContent(cellInfo.Item);
            if (cellContent != null)
            {
                return (DataGridCell)cellContent.Parent;
            }
            return null;
        }

        private static void MoveFocusToNextCell(DataGrid dataGrid)
        {
            if (dataGrid.CurrentCell != null)
            {
                DataGridCellInfo currentCell = dataGrid.CurrentCell;
                int columnIndex = dataGrid.CurrentCell.Column.DisplayIndex;
                int rowIndex = dataGrid.Items.IndexOf(dataGrid.CurrentItem);

                if (columnIndex < dataGrid.Columns.Count - 1)
                {
                    DataGridColumn nextColumn = dataGrid.Columns[columnIndex + 1];
                    DataGridCellInfo nextCell = new DataGridCellInfo(dataGrid.Items[rowIndex], nextColumn);
                    dataGrid.CurrentCell = nextCell;
                    dataGrid.ScrollIntoView(dataGrid.Items[rowIndex], nextColumn);
                }
                else
                {
                    // 右端のセルの場合は次の行の最初のセルにフォーカスを移動する
                    if (rowIndex < dataGrid.Items.Count - 1)
                    {
                        DataGridCellInfo nextCell = new DataGridCellInfo(dataGrid.Items[rowIndex + 1], dataGrid.Columns[0]);
                        dataGrid.CurrentCell = nextCell;
                        dataGrid.ScrollIntoView(dataGrid.Items[rowIndex + 1], dataGrid.Columns[0]);
                    }
                }
            }
        }
    }
}
