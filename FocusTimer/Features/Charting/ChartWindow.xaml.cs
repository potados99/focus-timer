using System.Windows;
using FocusTimer.Features.Charting.LiveCharts;
using FocusTimer.Lib.Component;

namespace FocusTimer.Features.Charting;

/// <summary>
/// Interaction logic for ChartWindow.xaml
/// </summary>
public partial class ChartWindow : ChartViewModelWindow
{
    protected override void OnInitialize()
    {
        InitializeComponent();
    }

    protected override void OnLoaded()
    {
        LowerChart.Tooltip = new CustomTooltip();

        ViewModel.OnChartNeedsUpdate += () =>
        {
            UpperChart.CoreChart.Update();
            LowerChart.CoreChart.Update();
        };
    }

    private void OnMinimizeButtonClick(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    private void OnCloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}

public abstract class ChartViewModelWindow : BaseWindow<ChartViewModel>
{
}