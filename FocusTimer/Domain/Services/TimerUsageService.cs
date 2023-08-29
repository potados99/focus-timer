using System;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib.Extensions;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Domain.Services;

public class TimerUsageService
{
    private readonly FocusRepository _repository;
    
    public TimerUsageService(FocusRepository repository)
    {
        _repository = repository;
    }

    public TimerUsage? GetLastUsage()
    {
        var usage = _repository.FindLastTimerUsage();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 TimerUsage를 가져왔습니다: {usage}");
        }

        return usage;
    }
    
    public TimerUsage CreateNewUsage()
    {
        this.GetLogger().Debug("새로운 TimerUsage를 생성합니다.");

        var usage = new TimerUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        _repository.StartTracking(usage);
        
        return usage;
    }

    public void SaveRepository()
    {
        _repository.Save();
    }
}