using System.Diagnostics;
using System.Windows;
using FocusTimer.Library.Control.Base;

namespace FocusTimer.Features.Timer.Slot;

/// <summary>
/// Interaction logic for TimerSlot.xaml
/// </summary>
public partial class TimerSlotControl : TimerSlotViewModelControl
{
    protected override void OnInitialize()
    {
        InitializeComponent();
    }
    
    #region 이벤트 핸들러

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.FireAppClearEvent();
    }

    private void Register_Click(object sender, RoutedEventArgs e)
    {
        Debug.WriteLine(ViewModel);
        ViewModel.FireAppRegisterEvent();
    }

    #endregion
}

public abstract class TimerSlotViewModelControl : BaseUserControl<TimerSlotViewModel>
{
}