using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using System;
using System.Linq;
using FocusTimer.Features.Timer.Slot;
using FocusTimer.Lib.Extensions;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private void StartRegisteringApplication(TimerSlotViewModel slot)
    {
        if (CurrentRegisteringTimerSlot != null)
        {
            return;
        }
            
        if (IsFocusLocked)
        {
            StartAnimation("ShakeHorizontalAnimation");
            return;
        }
        
        slot.StartWaitingForApp();

        OnRender();
    }

    private void FinishRegisteringApp(IntPtr windowHandle)
    {
        AppItem appItem;

        try
        {
            appItem = new AppItem(windowHandle);
        } catch (Exception e)
        {
            Crashes.TrackError(e);
            e.GetLogger().Error("Window handle로부터 TimerApp을 만드는 데에 실패하였습니다. 앱 등록을 건너뜁니다.");
            e.GetLogger().Error(e);
            return;
        }
            
        if (TimerSlots.Select(s => s.CurrentAppItem?.ProcessExecutablePath).Contains(appItem.ProcessExecutablePath))
        {
            UnableToRegisterApp("이미 등록된 프로그램이에요.");
            return;
        }

        CurrentRegisteringTimerSlot?.StopWaitingAndRegisterApp(appItem);

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
            return;
        }

        if (IsFocusLocked)
        {
            StartAnimation("ShakeHorizontalAnimation");
            return;
        }

        slot.ClearRegisteredApp();

        OnRender();

        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
    }
}