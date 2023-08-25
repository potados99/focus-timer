using System;
using System.Windows.Media;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

/// <summary>
/// 타이머 슬롯에 등록되는 앱을 나타냅니다.
/// 타이머 슬롯에는 앱이 등록되어 있을 수도, 그렇지 않을 수도 있습니다.
/// </summary>
public class AppItem : StopwatchRunner
{
    private readonly AppService _appService = Modules.Get<AppService>();
    private readonly AppUsageService _appUsageService = Modules.Get<AppUsageService>();
    private readonly UserActivityMonitor _activityMonitor = Modules.Get<UserActivityMonitor>();
    private readonly WindowWatcher _watcher = Modules.Get<WindowWatcher>();

    public AppItem(IntPtr windowHandle) : this(APIWrapper.GetProcessByWindowHandle(windowHandle)?.ExecutablePath())
    {
    }

    private AppItem(string? executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            throw new Exception("TimerApp의 생성자에 executablePath가 null로 들어왔습니다. 이래서는 안 됩니다!");
        }

        App = _appService.GetOrCreateApp(executablePath);
        _watcher.OnFocused += OnFocusChanged;

        // 사용자가 직접 등록한 경우에 호출되는 생성자입니다.
        // 만약 이 경우에 이전 시간을 복원하지 않도록 하려면
        // 아래 줄을 주석 처리 해주세요
        LoadUsage();
    }

    public AppItem(Domain.Entities.App app)
    {
        App = app;
        _watcher.OnFocused += OnFocusChanged;

        LoadUsage();
    }

    ~AppItem()
    {
        _watcher.OnFocused -= OnFocusChanged;
    }

    public Domain.Entities.App App { get; }
    
    public string ProcessExecutablePath => App.ExecutablePath;

    public string AppName => App.Title;

    public ImageSource Image => App.Icon.ToImageSource();

    public bool IsCountedOnConcentrationCalculation { get; set; } = true;

    public bool IsActive => _watcher.ForegroundAppPath == ProcessExecutablePath &&
                            _activityMonitor.IsActive;
    
    private AppUsage? _usage;

    public void Tick()
    {
        StartOrStopActiveTimer(IsActive);
        UpdateUsage();
    }

  /*  public void ResetTimerAndUsage()
    {
        _usage = _appUsageService.CreateNewUsage(App);
        
        Restart(); 
    }*/

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