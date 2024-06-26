﻿using System;
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
using System.Windows.Shapes;

namespace SqliteEditor.Plugins.SummonRowEditPlugins
{
    /// <summary>
    /// SummonRowEditWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class SummonRowEditWindow : Window
    {
        public SummonRowEditWindow()
        {
            InitializeComponent();
        }

        private void ComboBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox? comboBox = sender as ComboBox;
            var vm = comboBox?.DataContext as HeroIdViewModel;
            if (vm != null)
            {
                vm.Id = "";
                if (e.AddedItems is not null)
                {
                    HeroInfo? selected = null;
                    foreach (HeroInfo item in e.AddedItems)
                    {
                        selected = item;
                        break;
                    }
                    if (selected != null)
                    {
                        vm.Id = selected.Id.ToString();
                    }
                }
            }
        }
    }
}
