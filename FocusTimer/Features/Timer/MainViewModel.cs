using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using FocusTimer.Domain.Services;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel : BaseViewModel
{
    private readonly LicenseService _licenseService;
    private readonly UserActivityMonitor _activityMonitor;

    public MainViewModel(LicenseService licenseService, UserActivityMonitor activityMonitor)
    {
        _licenseService = licenseService;
        _activityMonitor = activityMonitor;
    }

    public override void OnInitialize()
    {
        SetupActivityMonitor();
        SetupTimerSlots();
        SetupWatcher();
    }

    private void SetupActivityMonitor()
    {
        _activityMonitor.OnActivated += RenderAll;
        _activityMonitor.OnDeactivated += RenderAll;
    }

    private void SetupTimerSlots()
    {
        foreach (var slot in TimerSlots)
        {
            slot.OnRegisterApplication += () => StartRegisteringApplication(slot);
            slot.OnClearApplication += () => ClearApplication(slot);
        }
    }

    private void SetupWatcher()
    {
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

    public override void OnLoaded()
    {
        RestoreAppSlots();
        RestoreStatesFromLastUsage();

        InitGlobalTimer();
        InitFocusLock();

        TickAll();
        RenderAll();

        this.GetLogger().Info("ViewModel 시작!");
    }

    private void TickAll()
    {
        Tick();

        foreach (var slot in TimerSlots)
        {
            slot.Tick();
        }
    }

    private void Tick()
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

    private void RenderAll()
    {
        Render();

        foreach (var slot in TimerSlots)
        {
            slot.Render();
        }
    }

    private void Render()
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