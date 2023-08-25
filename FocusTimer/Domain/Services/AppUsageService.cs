using System;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;

namespace FocusTimer.Domain.Services;

public class AppUsageService
{
    private readonly UsageRepository _repository;
    
    public AppUsageService(UsageRepository repository)
    {
        _repository = repository;
    }

    public AppUsage CreateNewUsage(Entities.App app)
    {
        var usage = new AppUsage
        {
            App = app,
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            IsConcentrated = true
        };

        _repository.StartTracking(usage);
        
        return usage;
    }

    public AppUsage? GetLastUsage(Entities.App app)
    {
        return _repository.FindLastAppUsageByApp(app);
    }

    public void SaveRepository()
    {
        _repository.Save();
    }
}