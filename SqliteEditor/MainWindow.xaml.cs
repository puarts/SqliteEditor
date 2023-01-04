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
    }
}
