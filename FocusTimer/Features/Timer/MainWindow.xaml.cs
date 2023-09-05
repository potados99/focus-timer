// MainWindow.xaml.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using FocusTimer.Features.Charting;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FocusTimer.Features.License;
using FocusTimer.Features.Timer.Border;
using FocusTimer.Library.Control.Base;

namespace FocusTimer.Features.Timer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    protected override void OnInitialize()
    {
        if (ViewModel.ShouldAskForLicense())
        {
            new LicenseWindow().Show();
            Close();
        }

        InitializeComponent();

        DataContext = ViewModel;

        new BorderWindow
        {
            DataContext = ViewModel
        }.Show();
    }

    private ChartWindow? _openedChartWindow;

    #region 이벤트 핸들러

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            ViewModel.CancelRegisteringApp();
        }

        if (e.Key == Key.System && e.SystemKey == Key.F4)
        {
            // ALT + F4가 떨어지면 앱을 종료합니다.
            Application.Current.Shutdown();
        }
    }

    private void Window_Closed(object sender, EventArgs e)
    {
        Application.Current.Shutdown();
    }

    private void ExitItem_Click(object sender, RoutedEventArgs e)
    {
        App.Current.Shutdown();
    }

    private void ResetItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.ResetTimer();
    }

    private void ChartItem_Click(object sender, RoutedEventArgs e)
    {
        if (_openedChartWindow == null)
        {
            _openedChartWindow = new ChartWindow();
            _openedChartWindow.Closed += (_, _) => { _openedChartWindow = null; };
        }

        _openedChartWindow.Show();
        _openedChartWindow.Activate();
    }

    private void InfoItem_Click(object sender, RoutedEventArgs e)
    {
        var ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

        MessageBox.Show(
            $"Focus Timer\n버전 {ver}",
            "프로그램 정보",
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }

    private void StartFocusLockItem_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.StartFocusLockWithHold();
    }

    private void Lock_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button {ToolTip: ToolTip toolTip})
        {
            toolTip.IsOpen = true;
        }

        ViewModel.ToggleFocusLock();
    }

    protected override void OnClick()
    {
        ViewModel.ToggleExpanded();
    }

    #endregion
}

public abstract class MainViewModelWindow : BaseDraggableWindow<MainViewModel>
{
}