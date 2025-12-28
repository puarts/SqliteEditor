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
                    try
                    {
                        SqliteUtility.UpdateRecordById(
                            tableViewModel.DatabasePath,
                            tableViewModel.TableName,
                            row.Row,
                            tableViewModel.Schema);
                    }
                    catch (Exception exception)
                    {
                        ViewModel.WriteError($"レコードの更新に失敗しました。\n{exception.Message}\n{exception.StackTrace}");
                    }
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

                if (sender is DataGrid dataGrid && dataGrid?.CurrentCell != null)
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
            else if (e.Key == Key.Delete)
            {
                // If currently editing a cell, don't treat Delete as "delete row" — let the editing control handle it.
                if (sender is DataGrid editingGrid && editingGrid?.CurrentCell != null)
                {
                    var editingCell = GetCell(editingGrid.CurrentCell);
                    if (editingCell != null && editingCell.IsEditing)
                    {
                        // Do not mark handled; allow editing control (e.g. TextBox) to process Delete
                        return;
                    }

                    // Also if keyboard focus is inside an element within the cell (such as a TextBox), allow it to handle Delete
                    var focused = Keyboard.FocusedElement as DependencyObject;
                    if (focused != null)
                    {
                        // walk up visual tree to see if focus is inside the current cell
                        DependencyObject? current = focused;
                        while (current != null)
                        {
                            if (current == editingCell)
                            {
                                return;
                            }
                            current = VisualTreeHelper.GetParent(current);
                        }
                    }
                }

                // DataGrid の既定削除を抑制して、自前の削除ロジックを実行する例
                e.Handled = true;

                if (sender is not DataGrid dataGrid) return;

                var selectedRows = dataGrid.SelectedItems.Cast<System.Data.DataRowView>().ToArray();
                if (selectedRows.Length == 0) return;

                var tableVm = ViewModel.GetSelectedTableViewModel();
                if (tableVm is null) return;

                foreach (var drv in selectedRows)
                {
                    // DataTable から行を削除（後で SaveDirtyTables() で永続化される設計に合わせる）
                    tableVm.DataTable.Rows.Remove(drv.Row);
                }

                // 差分判定を更新して「変更あり」にする
                tableVm.UpdateDirty();
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
            if (dataGrid?.CurrentCell != null)
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
