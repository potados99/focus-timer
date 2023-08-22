using System;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;

namespace FocusTimer.Domain.Services;

public class TimerUsageService
{
    private readonly UsageRepository _repository;
    
    public TimerUsageService(UsageRepository repository)
    {
        _repository = repository;
    }

    public TimerUsage CreateNewUsage()
    {
        var usage = new TimerUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
        };

        _repository.StartTracking(usage);
        
        return usage;
    }

    public TimerUsage? GetLastUsage()
    {
        return _repository.FindLastTimerUsage();
    }

    public void SaveRepository()
    {
        _repository.Save();
    }
}