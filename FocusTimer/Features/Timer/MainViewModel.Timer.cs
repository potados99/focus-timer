using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region 스탑워치와 글로벌 틱 타이머

    private void InitGlobalTimer()
    {
        _oneSecTickTimer.Stop();
        _oneSecTickTimer.RemoveHandlers();
        _alwaysOnStopwatch.Reset();
        _activeStopwatch.Reset();

        _oneSecTickTimer.Tick += (_, _) =>
        {
            TickAll();
            RenderAll();
        };
        _oneSecTickTimer.Interval = TimeSpan.FromSeconds(1);
        _oneSecTickTimer.Start();

        _alwaysOnStopwatch.Start();
        _activeStopwatch.Start();
    }

    private readonly Stopwatch _activeStopwatch = new();
    private readonly Stopwatch _alwaysOnStopwatch = new();
    private readonly DispatcherTimer _oneSecTickTimer = new();

    #endregion

    #region 포커스 잠금과 홀드 타이머

    private void InitFocusLock()
    {
        _focusLockTimer.OnFinish += () =>
        {
            UnlockFocus();
        };
    }

    private readonly CountdownTimer _focusLockTimer = new();

    public bool IsFocusLocked { get; set; }

    public bool IsFocusLockHold
    {
        get
        {
            return _focusLockTimer.IsEnabled;
        }
    }

    public int FocusLockHoldDuration
    {
        get
        {
            return Settings.GetFocusLockHoldDuration();
        }
        set
        {
            Settings.SetFocusLockHoldDuration(value);

            // 양방향 바인딩되는 속성으로, UI에 의해 변경시 여기에서 NotifyPropertyChanged를 트리거해요.
            NotifyPropertyChanged(nameof(FocusLockHoldDuration));
            NotifyPropertyChanged(nameof(StartFocusLockItemLabel));
        }
    }

    public Visibility IsWarningBorderVisible
    {
        get
        {
            return !IsFocusLocked || IsAnyAppActive ? Visibility.Hidden : Visibility.Visible;
        }
    }

    public DrawingImage? LockImage
    {
        get
        {
            string resourceName = IsFocusLocked ? "ic_lock" : "ic_lock_open_outline";

            return Application.Current.FindResource(resourceName) as DrawingImage;
        }
    }

    private readonly ToolTip _lockButtonToolTip = new();
    public ToolTip? LockButtonToolTip
    {
        get
        {
            _lockButtonToolTip.Content = $"{(int)Math.Ceiling(_focusLockTimer.TimeLeft.TotalMinutes)}분 남았습니다.";

            return IsFocusLockHold ? _lockButtonToolTip : null;
        }
    }

    public string StartFocusLockItemLabel
    {
        get
        {
            return $"{FocusLockHoldDuration}분간 강제 잠금";
        }
    }

    public void StartFocusLockWithHold()
    {
        LockFocusWithHold();

        Render();
    }
    private void LockFocusWithHold()
    {
        _focusLockTimer.Duration = TimeSpan.FromMinutes(FocusLockHoldDuration);
        _focusLockTimer.Start();

        IsFocusLocked = true;
        StartAnimation("LockingAnimation");
    }

    public void ToggleFocusLock()
    {
        if (IsFocusLocked)
        {
            UnlockFocus();
        }
        else
        {
            LockFocus();
        }

        Render();
    }

    private void LockFocus()
    {
        _focusLockTimer.Stop();
        IsFocusLocked = true;

        StartAnimation("LockingAnimation");
    }

    private void UnlockFocus()
    {
        if (IsFocusLockHold)
        {
            _lockButtonToolTip.IsOpen = true;
            Task.Delay(700).ContinueWith(_ => Application.Current.Dispatcher.Invoke(() =>
            {
                _lockButtonToolTip.IsOpen = false;
            }));

            StartAnimation("ShakeHorizontalAnimation");

            return;
        }

        IsFocusLocked = false;
        StartAnimation("UnlockingAnimation");
    }

    private void RestoreFocusIfNeeded(IntPtr prev, IntPtr current)
    {
        if (!IsFocusLocked)
        {
            // 포커스 잠금이 없으면 아무 것도 하지 않습니다.
            return;
        }

        if (IsAnyAppActive)
        {
            // 등록된 앱 중 활성화된 앱이 있으면 정상적인 상태입니다.
            return;
        }

        if (WindowWatcher.SkipList.Contains(APIWrapper.GetForegroundWindowClass()))
        {
            // 현재 포커스가 시스템 UI로 가 있다면 넘어갑니다.
            return;
        }

        if (APIWrapper.IsThisProcessForeground())
        {
            // 현재 포커스가 이 프로세스라면 넘어갑니다.
            return;
        }

        // 위 아무 조건에도 해당하지 않았다면
        // 포커스를 빼앗아야 할 때입니다.
        // 포커스를 이전 앱에 주고 현재 앱을 줄입니다.
        Task.Delay(200).ContinueWith(_ =>
        {
            APIWrapper.MinimizeWindow(current);
            APIWrapper.SetForegroundWindow(prev);
        });
    }

    #endregion
    
    #region 타이머의 리셋

    public void ResetTimer()
    {
        InitGlobalTimer();

        _usage = null;
        _ticksStartOffset = 0;
        _ticksElapsedOffset = 0;
        _activeTicksStartOffset = 0;
        _activeTicksElapsedOffset = 0;

        RestoreAppSlots();

        TickAll();
        RenderAll();
    }

    #endregion
}