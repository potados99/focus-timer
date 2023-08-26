using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerItem : StopwatchRunner
{
    private readonly TimerUsageService _timerUsageService = Modules.Get<TimerUsageService>();
    private readonly WindowWatcher _watcher = Modules.Get<WindowWatcher>();
    private readonly EventService _eventService = Modules.Get<EventService>();

    public TimerItem()
    {
        RegisterEvents();
        LoadUsage();
    }

    ~TimerItem()
    {
        UnregisterEvents();
    }

    public List<TimerSlotViewModel> TimerSlots { get; } = new()
    {
        new TimerSlotViewModel {SlotNumber = 0},
        new TimerSlotViewModel {SlotNumber = 1},
        new TimerSlotViewModel {SlotNumber = 2},
        new TimerSlotViewModel {SlotNumber = 3},
        new TimerSlotViewModel {SlotNumber = 4},
    };
    
    private TimerUsage? _usage;

    private void RegisterEvents()
    {
        _eventService.OnTick += OnTick;
        _watcher.OnFocused += OnFocusChanged;
    }

    private void UnregisterEvents()
    {
        _eventService.OnTick -= OnTick;
        _watcher.OnFocused -= OnFocusChanged;
    }

    private void OnTick()
    {
        StartOrStopActiveTimer(IsAnyAppActive);
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