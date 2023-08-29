using System;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib.Extensions;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Domain.Services;

public class AppUsageService
{
    private readonly FocusRepository _repository;
    
    public AppUsageService(FocusRepository repository)
    {
        _repository = repository;
    }

    public AppUsage? GetLastUsage(Entities.App app)
    {
        var usage = _repository.FindLastAppUsageByApp(app);
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 AppUsage를 가져왔습니다: {usage}");
        }
        
        return usage;
    }

    public AppUsage CreateNewUsage(Entities.App app)
    {
        this.GetLogger().Debug("새로운 AppUsage를 생성합니다.");

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
    
    public void AddResetHistory()
    {
        var history = ResetHistory.OfNow();
        
        _repository.StartTracking(history);
    }

    public void SaveRepository()
    {
        _repository.Save();
    }
}