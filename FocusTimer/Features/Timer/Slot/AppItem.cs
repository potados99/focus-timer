using System;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

/// <summary>
/// 타이머 슬롯에 등록되는 앱을 나타냅니다.
/// 타이머 슬롯에는 앱이 등록되어 있을 수도, 그렇지 않을 수도 있습니다.
/// </summary>
public partial class AppItem : StopwatchRunner
{
    private readonly AppService _appService = Modules.Get<AppService>();
    private readonly AppUsageService _appUsageService = Modules.Get<AppUsageService>();
    private readonly UserActivityMonitor _activityMonitor = Modules.Get<UserActivityMonitor>();
    private readonly EventService _eventService = Modules.Get<EventService>();

    public AppItem(IntPtr windowHandle) : this(APIWrapper.GetProcessByWindowHandle(windowHandle)?.ExecutablePath())
    {
    }
    
    public AppItem(Domain.Entities.App app) : this(app.ExecutablePath)
    {
    }

    private AppItem(string? executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            throw new Exception("TimerApp의 생성자에 executablePath가 null로 들어왔습니다. 이래서는 안 됩니다!");
        }

        App = _appService.GetOrCreateApp(executablePath);

        RegisterEvents();
        LoadUsage();
    }
    
    ~AppItem()
    {
        UnregisterEvents();
    }

    public Domain.Entities.App App { get; }

    private AppUsage? _usage;

    private void RegisterEvents()
    {
        _eventService.OnTick += OnTick;
        _eventService.OnFocusChanged += OnFocusChanged;
    }

    private void UnregisterEvents()
    {
        _eventService.OnTick -= OnTick;
        _eventService.OnFocusChanged -= OnFocusChanged;
    }

    private void OnTick()
    {
        StartOrStopActiveTimer(IsActive);
        UpdateUsage();
    }

    private void OnFocusChanged(IntPtr prev, IntPtr current)
    {
        if (IsActive)
        {
            _usage?.OpenNewActiveUsage();
        }
    }

    private void LoadUsage()
    {
        _usage = _appUsageService.GetLastUsage(App) ?? _appUsageService.CreateNewUsage(App);

        IsCountedOnConcentrationCalculation = _usage.IsConcentrated;

        Restart();
        AddOffset(_usage.ActiveElapsed, TimeSpan.Zero);
    }

    private void UpdateUsage()
    {
        if (_usage == null)
        {
            return;
        }

        _usage.UpdatedAt = DateTime.Now;
        _usage.IsConcentrated = IsCountedOnConcentrationCalculation;

        if (IsActive)
        {
            _usage.TouchActiveUsage();
        }

        _appUsageService.SaveRepository();
    }
}