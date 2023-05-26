using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace FocusTimer
{
    /// <summary>
    /// Interaction logic for ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public ChartWindow()
        {
            InitializeComponent();

            DataContext = ViewModel;
        }

        private readonly ChartViewModel ViewModel = new();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Loaded();

            LowerChart.Tooltip = new CustomTooltip();
        }

        private void LowerChart_MouseDown(object sender, MouseButtonEventArgs e)
        {

            var chart = sender as CartesianChart;
            Debug.WriteLine("!!");

            var tooltip = chart.Tooltip as CustomTooltip;
            var points = tooltip.Points;

            foreach (var p in points)
            {
                p.TertiaryValue = 99;
            }
        }
    }
}
