using FocusTimer.Lib;
using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;
using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region 타이머 슬롯의 등록, 초기화 및 상태

    public List<TimerSlotViewModel> TimerSlots { get; } = new List<TimerSlotViewModel>() {
        new TimerSlotViewModel() { SlotNumber = 0 },
        new TimerSlotViewModel() { SlotNumber = 1 },
        new TimerSlotViewModel() { SlotNumber = 2 },
        new TimerSlotViewModel() { SlotNumber = 3 },
        new TimerSlotViewModel() { SlotNumber = 4 },
    };

    private TimerSlotViewModel? CurrentRegisteringTimerSlot
    {
        get
        {
            return TimerSlots.FirstOrDefault(s => s.IsWaitingForApp);
        }
    }

    public bool IsAnyAppActive
    {
        get
        {
            return TimerSlots.Any(s => s.IsAppActive);
        }
    }

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
        TimerApp app;

        try
        {
            app = new TimerApp(windowHandle);
        } catch (Exception e)
        {
            Crashes.TrackError(e);
            e.GetLogger().Error("Window handle로부터 TimerApp을 만드는 데에 실패하였습니다. 앱 등록을 건너뜁니다.");
            e.GetLogger().Error(e);
            return;
        }
            
        if (TimerSlots.Select(s => s.CurrentApp?.ProcessExecutablePath).Contains(app.ProcessExecutablePath))
        {
            UnableToRegisterApp("이미 등록된 프로그램이에요.");
            return;
        }

        CurrentRegisteringTimerSlot?.StopWaitingAndRegisterApp(app);

        SaveApps();
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

        SaveApps();
        Render();

        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
    }

    #endregion

    #region 타이머 슬롯의 저장 및 복구

    public void SaveApps()
    {
        Settings.SetApps(TimerSlots.Select(s => s.GetAppExecutablePath()).ToList());
    }

    public void RestoreAppSlots()
    {
        foreach (var (app, index) in Settings.GetApps().WithIndex())
        {
            TimerSlots[index].RestoreApp(app);
        }
    }

    #endregion

}