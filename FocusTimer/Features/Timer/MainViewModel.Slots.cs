using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region 타이머 슬롯의 등록, 초기화 및 상태

    public List<TimerSlotViewModel> TimerSlots => TimerItem.TimerSlots;

    private TimerSlotViewModel? CurrentRegisteringTimerSlot => TimerSlots.FirstOrDefault(s => s.IsWaitingForApp);

    public bool IsAnyAppActive => TimerItem.IsAnyAppActive;

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

        Render();
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

        Render();

        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
    }

    private void UnableToRegisterApp(string reason)
    {
        CurrentRegisteringTimerSlot?.UnableToHandleRegistering(reason);

        Render();
    }

    public void CancelRegisteringApp()
    {
        CurrentRegisteringTimerSlot?.CancelRegisteringApp();

        Render();
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

        Render();

        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
    }

    #endregion

    #region 타이머 슬롯의 저장 및 복구

    private void RestoreAppSlots()
    {
        foreach (var slot in TimerSlots)
        {
            slot.LoadSlot();
        }
    }

    #endregion
}