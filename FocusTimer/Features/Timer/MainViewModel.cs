using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Features.License;
using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

internal partial class MainViewModel : BaseModel
{
    private readonly LicenseService _licenseService;

    public MainViewModel(LicenseService licenseService)
    {
        _licenseService = licenseService;
            
        UserActivityMonitor.Instance.OnActivated += () =>
        {
            RenderAll();
        };

        UserActivityMonitor.Instance.OnDeactivated += () =>
        {
            RenderAll();
        };

        foreach (var slot in TimerSlots)
        {
            slot.OnRegisterApplication += () => StartRegisteringApplication(slot);
            slot.OnClearApplication += () => ClearApplication(slot);
        }

        WindowWatcher watcher = new();

        watcher.OnFocused += (prev, current) =>
        {
            if (CurrentRegisteringTimerSlot != null && !APIWrapper.IsThisProcessForeground())
            {
                FinishRegisteringApp(current);
            }

            RestoreFocusIfNeeded(prev, current);

            RenderAll();
        };

        watcher.StartListening();
    }

    public void Loaded()
    {
        RestoreAppSlots();
        RestoreStatesFromLastUsage();

        InitGlobalTimer();
        InitFocusLock();

        TickAll();
        RenderAll();

        this.GetLogger().Info("ViewModel 시작!");
    }

    public void TickAll()
    {
        Tick();

        foreach (var slot in TimerSlots)
        {
            slot.Tick();
        }
    }

    public void Tick()
    {
        if (IsAnyAppActive)
        {
            ActiveStopwatch.Start();
        }
        else
        {
            ActiveStopwatch.Stop();
        }

        UpdateUsage();
    }

    public void RenderAll()
    {
        Render();

        foreach (var slot in TimerSlots)
        {
            slot.Render();
        }
    }

    public void Render()
    {
        NotifyPropertyChanged(nameof(ActiveElapsedTime));
        NotifyPropertyChanged(nameof(IsAnyAppActive));
        NotifyPropertyChanged(nameof(IsWarningBorderVisible));

        NotifyPropertyChanged(nameof(IsFocusLocked));
        NotifyPropertyChanged(nameof(IsFocusLockHold));
        NotifyPropertyChanged(nameof(LockImage));
        NotifyPropertyChanged(nameof(LockButtonToolTip));

        NotifyPropertyChanged(nameof(Concentration));
        NotifyPropertyChanged(nameof(IsOnConcentraion));
    }
}