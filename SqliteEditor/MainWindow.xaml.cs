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

                ViewModel.UpdateSelectedTableDirty();
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
    }
}
