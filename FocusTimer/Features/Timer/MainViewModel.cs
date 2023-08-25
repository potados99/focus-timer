using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using FocusTimer.Domain.Services;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel : BaseViewModel
{
    private readonly LicenseService _licenseService;
    private readonly UserActivityMonitor _activityMonitor;
    private readonly WindowWatcher _watcher;
    private readonly SlotService _slotService;

    public MainViewModel(
        LicenseService licenseService,
        UserActivityMonitor activityMonitor,
        WindowWatcher watcher,
        SlotService slotService
    )
    {
        _licenseService = licenseService;
        _activityMonitor = activityMonitor;
        _watcher = watcher;
        _slotService = slotService;
    }

    #region 생성자 시점 초기화

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
        _watcher.OnFocused += (prev, current) =>
        {
            if (CurrentRegisteringTimerSlot != null && !APIWrapper.IsThisProcessForeground())
            {
                FinishRegisteringApp(current);
            }

            RestoreFocusIfNeeded(prev, current);

            RenderAll();
        };

        _watcher.StartListening();
    }

    #endregion

    #region 컨트롤 로드 시점 초기화

    public override void OnLoaded()
    {
        RestoreAppSlots();

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
        Timer.Tick(IsAnyAppActive);
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
        NotifyPropertyChanged(nameof(Timer));
        NotifyPropertyChanged(nameof(IsAnyAppActive));
        NotifyPropertyChanged(nameof(IsWarningBorderVisible));

        NotifyPropertyChanged(nameof(IsFocusLocked));
        NotifyPropertyChanged(nameof(IsFocusLockHold));
        NotifyPropertyChanged(nameof(LockImage));
        NotifyPropertyChanged(nameof(LockButtonToolTip));

        NotifyPropertyChanged(nameof(Concentration));
        NotifyPropertyChanged(nameof(IsOnConcentration));
    }

    #endregion
}