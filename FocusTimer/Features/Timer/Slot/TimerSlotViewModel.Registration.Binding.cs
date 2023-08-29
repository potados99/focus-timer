using System.Windows;
using FocusTimer.Lib.Control.Base;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerSlotViewModel : BaseViewModel
{
    public Visibility IsAppVisible =>
        !IsWaitingForApp && CurrentAppItem != null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsSetButtonVisible =>
        !IsWaitingForApp && CurrentAppItem == null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsWaitLabelVisible => IsWaitingForApp ? Visibility.Visible : Visibility.Hidden;

    public bool IsAppActive => CurrentAppItem is {IsActive: true};

    public bool IsAppCountedOnConcentrationCalculation => CurrentAppItem is {IsCountedOnConcentrationCalculation: true};

    public string WindowSelectPrompt { get; set; } = "창을 클릭해주세요";
}