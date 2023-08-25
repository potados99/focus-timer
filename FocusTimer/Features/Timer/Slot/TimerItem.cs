using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public class TimerItem : StopwatchRunner
{
    private readonly TimerUsageService _timerUsageService = Modules.Get<TimerUsageService>();
    private readonly WindowWatcher _watcher = Modules.Get<WindowWatcher>();

    public TimerItem()
    {
        _watcher.OnFocused += OnFocusChanged;

        LoadUsage();
    }

    ~TimerItem()
    {
        _watcher.OnFocused -= OnFocusChanged;
    }

    public List<TimerSlotViewModel> TimerSlots { get; } = new()
    {
        new TimerSlotViewModel {SlotNumber = 0},
        new TimerSlotViewModel {SlotNumber = 1},
        new TimerSlotViewModel {SlotNumber = 2},
        new TimerSlotViewModel {SlotNumber = 3},
        new TimerSlotViewModel {SlotNumber = 4},
    };

    public bool IsAnyAppActive => TimerSlots.Any(s => s.IsAppActive);

    private TimerUsage? _usage;

    public void Tick(bool active)
    {
        StartOrStopActiveTimer(active);
        UpdateUsage();
    }

    private void OnFocusChanged(IntPtr prev, IntPtr current)
    {
        var prevProcess = APIWrapper.GetProcessByWindowHandle(prev);
        var currentAppPaths = TimerSlots.Select(s => s.GetAppExecutablePath());
        var wasInactive = prevProcess == null || !currentAppPaths.Contains(prevProcess.ExecutablePath());

        if (wasInactive && IsAnyAppActive)
        {
            _usage?.OpenNewActiveUsage();
        }
    }

    private void LoadUsage()
    {
        _usage = _timerUsageService.GetLastUsage() ?? _timerUsageService.CreateNewUsage();

        Restart();
        AddOffset(_usage.ActiveElapsed, _usage.Elapsed);
    }

    private void UpdateUsage()
    {
        if (_usage == null)
        {
            return;
        }

        _usage.UpdatedAt = DateTime.Now;

        if (IsAnyAppActive)
        {
            _usage.TouchActiveUsage();
        }

        _timerUsageService.SaveRepository();
    }

    public void Reset()
    {
        Restart();

        _usage = _timerUsageService.CreateNewUsage();
    }
}