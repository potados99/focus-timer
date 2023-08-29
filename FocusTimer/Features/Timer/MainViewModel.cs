using System;
using FocusTimer.Lib;
using FocusTimer.Domain.Services;
using FocusTimer.Features.Timer.Slot;
using FocusTimer.Lib.Control.Base;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel : BaseViewModel
{
    private readonly LicenseService _licenseService;
    private readonly WindowWatcher _watcher;
    private readonly AppUsageService _appUsageService;
    private readonly EventService _eventService;

    public MainViewModel(
        LicenseService licenseService,
        WindowWatcher watcher,
        AppUsageService appUsageService,
        EventService eventService)
    {
        _licenseService = licenseService;
        _watcher = watcher;
        _appUsageService = appUsageService;
        _eventService = eventService;
    }
    
    public TimerItem TimerItem { get; } = new();

    public override void OnInitialize()
    {
        RegisterEvents();
    }

    private void RegisterEvents()
    {
        _focusLockTimer.OnFinish += UnlockFocus;
        
        _eventService.OnRender += OnRender;
        _eventService.OnFocusChanged += OnFocusChanged;

        foreach (var slot in TimerSlots)
        {
            slot.OnRegisterApplication += () => StartRegisteringApplication(slot);
            slot.OnClearApplication += () => ClearApplication(slot);
        }
    }
    
    private void OnRender()
    {
        NotifyPropertyChanged(nameof(TimerItem));
        NotifyPropertyChanged(nameof(IsWarningBorderVisible));

        NotifyPropertyChanged(nameof(IsFocusLocked));
        NotifyPropertyChanged(nameof(IsFocusLockHold));
        NotifyPropertyChanged(nameof(LockImage));
        NotifyPropertyChanged(nameof(LockButtonToolTip));

        NotifyPropertyChanged(nameof(Concentration));
        NotifyPropertyChanged(nameof(IsOnConcentration));
    }

    private void OnFocusChanged(IntPtr prev, IntPtr current)
    {
        if (CurrentRegisteringTimerSlot != null && !APIWrapper.IsThisProcessForeground())
        {
            FinishRegisteringApp(current);
        }

        RestoreFocusIfNeeded(prev, current);
    }
}