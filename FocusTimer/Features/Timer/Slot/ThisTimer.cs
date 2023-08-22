using System;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FocusTimer.Features.Timer.Slot;

public class ThisTimer : StopwatchRunner
{
    private readonly TimerUsageService _timerUsageService = App.Provider.GetService<TimerUsageService>()!;

    public ThisTimer()
    {
        LoadUsage();
    }
    
    private TimerUsage? _usage;

    public void Tick(bool active)
    {
        ActiveStatusChanged(active);
        UpdateUsage();
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

        _timerUsageService.SaveRepository();
    }
    
    public void Reset()
    {
        Restart();
        
        _usage = _timerUsageService.CreateNewUsage();
    }
}