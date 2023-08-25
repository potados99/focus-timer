﻿using System;
using System.Windows.Media;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace FocusTimer.Features.Timer.Slot;

/// <summary>
/// 타이머 슬롯에 등록되는 앱을 나타냅니다.
/// 타이머 슬롯에는 앱이 등록되어 있을 수도, 그렇지 않을 수도 있습니다.
/// </summary>
public class TimerApp : StopwatchRunner
{
    private readonly AppService _appService = App.Provider.GetService<AppService>()!;
    private readonly AppUsageService _appUsageService = App.Provider.GetService<AppUsageService>()!;
    private readonly UserActivityMonitor _activityMonitor = App.Provider.GetService<UserActivityMonitor>()!;
    private readonly WindowWatcher _watcher = App.Provider.GetService<WindowWatcher>()!;

    public TimerApp(IntPtr windowHandle) : this(APIWrapper.GetProcessByWindowHandle(windowHandle)?.ExecutablePath())
    {
    }

    public TimerApp(string? executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            throw new Exception("TimerApp의 생성자에 executablePath가 null로 들어왔습니다. 이래서는 안 됩니다!");
        }

        _app = _appService.GetOrCreateApp(executablePath);
        _watcher.OnFocused += OnFocusChanged;
        
        LoadUsage();
    }

    public TimerApp(Domain.Entities.App app)
    {
        _app = app;
        _watcher.OnFocused += OnFocusChanged;
        
        LoadUsage();        
    }

    ~TimerApp()
    {
        _watcher.OnFocused -= OnFocusChanged;
    }
    
    private readonly Domain.Entities.App _app;
    private AppUsage? _usage;

    public Domain.Entities.App AppEntity => _app;
    
    public string ProcessExecutablePath => _app.ExecutablePath;

    public string AppName => _app.Title;

    public ImageSource Image => _app.Icon.ToImageSource();

    public bool IsCountedOnConcentrationCalculation { get; set; } = true;

    public bool IsActive => _watcher.ForegroundAppPath == ProcessExecutablePath &&
                            _activityMonitor.IsActive;

    public void Tick()
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
        _usage = _appUsageService.GetLastUsage(_app) ?? _appUsageService.CreateNewUsage(_app);

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