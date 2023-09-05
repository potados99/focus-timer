// MainViewModel.Timer.Binding.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FocusTimer.Library;

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