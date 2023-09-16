// MainViewModel.Slots.cs
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

using Microsoft.AppCenter.Crashes;
using System;
using System.Linq;
using System.Windows;
using FocusTimer.Features.Timer.Slot;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private void StartRegisteringApplication(TimerSlotViewModel slot)
    {
        if (CurrentRegisteringTimerSlot != null)
        {
            this.GetLogger().Warn("앱 등록을 시작하려는데, 현재 타이머가 잠겨있기 때문에 무시합니다.");
            return;
        }

        if (IsFocusLocked)
        {
            this.GetLogger().Warn("앱 등록을 시작하려는데, 현재 타이머가 잠겨있기 때문에 무시합니다.");
            StartAnimation("ShakeHorizontalAnimation");
            return;
        }

        slot.StartWaitingForApp();

        OnRender();
    }

    private void FinishRegisteringApp(IntPtr windowHandle)
    {
        var process = APIWrapper.GetProcessByWindowHandle(windowHandle);
        if (process == null)
        {
            this.GetLogger().Warn($"앱 등록을 마치려는데, 주어진 윈도우 핸들({windowHandle})에 해당하는 프로세스가 없습니다. 따라서 아무 일도 하지 않고 중단합니다.");
            return;
        }

        var executablePath = process.ExecutablePath();
        if (executablePath == null)
        {
            this.GetLogger().Warn($"앱 등록을 마치려는데, 주어진 윈도우 핸들({windowHandle})로부터 불러온 프로세스({process.Id})의 실행 파일 경로를 가져올 수 없습니다. 따라서 아무 일도 하지 않고 중단합니다.");
            return;
        }

        if (TimerSlots.Select(s => s.CurrentAppItem?.ProcessExecutablePath).Contains(executablePath))
        {
            this.GetLogger().Warn("앱 등록을 마치려는데, 이미 등록된 프로그램입니다. 따라서 아무 일도 하지 않고 중단합니다.");
            UnableToRegisterApp(Strings.Get("program_already_added"));
            return;
        }

        CurrentRegisteringTimerSlot?.StopWaitingAndRegisterApp(new AppItem(executablePath));

        OnRender();

        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
    }

    private void UnableToRegisterApp(string reason)
    {
        CurrentRegisteringTimerSlot?.UnableToHandleRegistering(reason);

        OnRender();
    }

    public void CancelRegisteringApp()
    {
        CurrentRegisteringTimerSlot?.CancelRegisteringApp();

        OnRender();
    }

    private void ClearApplication(TimerSlotViewModel slot)
    {
        if (CurrentRegisteringTimerSlot != null)
        {
            this.GetLogger().Warn("앱을 슬롯에서 지우려는데, 현재 앱 등록을 위해 프로그램 포커스를 기다리는 중이기 때문에 무시합니다.");
            return;
        }

        if (IsFocusLocked)
        {
            this.GetLogger().Warn("앱을 슬롯에서 지우려는데, 현재 타이머가 잠겨있기 때문에 무시합니다.");
            StartAnimation("ShakeHorizontalAnimation");
            return;
        }

        slot.ClearRegisteredApp();

        OnRender();

        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
    }
}