using System.Windows;
using FocusTimer.Features.Charting.LiveCharts;

namespace FocusTimer.Features.Charting;

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

        ViewModel.OnChartNeedsUpdate += () =>
        {
            UpperChart.CoreChart.Update();
            LowerChart.CoreChart.Update();
        };
    }

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
    {
        this.WindowState = WindowState.Minimized;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}