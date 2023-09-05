// MainViewModel.Timer.cs
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
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using FocusTimer.Library;
using FocusTimer.Library.Utility;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private readonly CountdownTimer _focusLockTimer = new();

    public void StartFocusLockWithHold()
    {
        LockFocusWithHold();

        OnRender();
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

        OnRender();
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

        if (TimerItem.IsAnyAppActive)
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
    
    public void ResetTimer()
    {
        _appUsageService.AddResetHistory();
        _appUsageService.SaveRepository();
        
        // 여기에서 새 TimerUsage가 만들어집니다.
        // 이는 반드시 ResetHistory가 만들어진 다음에
        // 실행되어야 합니다.
        _eventService.EmitReload();
        _eventService.EmitRender();
    }
}