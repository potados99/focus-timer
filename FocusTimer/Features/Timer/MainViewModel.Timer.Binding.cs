using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FocusTimer.Lib;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public bool IsFocusLocked { get; set; }

    public bool IsFocusLockHold => _focusLockTimer.IsEnabled;

    public int FocusLockHoldDuration
    {
        get => Settings.GetFocusLockHoldDuration();
        set
        {
            Settings.SetFocusLockHoldDuration(value);

            // 양방향 바인딩되는 속성으로, UI에 의해 변경시 여기에서 NotifyPropertyChanged를 트리거해요.
            NotifyPropertyChanged(nameof(FocusLockHoldDuration));
            NotifyPropertyChanged(nameof(StartFocusLockItemLabel));
        }
    }

    public Visibility IsWarningBorderVisible =>
        !IsFocusLocked || TimerItem.IsAnyAppActive ? Visibility.Hidden : Visibility.Visible;

    public DrawingImage? LockImage
    {
        get
        {
            var resourceName = IsFocusLocked ? "ic_lock" : "ic_lock_open_outline";

            return Application.Current.FindResource(resourceName) as DrawingImage;
        }
    }

    private readonly ToolTip _lockButtonToolTip = new();

    public ToolTip? LockButtonToolTip
    {
        get
        {
            _lockButtonToolTip.Content = $"{(int) Math.Ceiling(_focusLockTimer.TimeLeft.TotalMinutes)}분 남았습니다.";

            return IsFocusLockHold ? _lockButtonToolTip : null;
        }
    }

    public string StartFocusLockItemLabel => $"{FocusLockHoldDuration}분간 강제 잠금";
}