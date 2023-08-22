using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;

namespace FocusTimer.Data.Repositories;

public class UsageRepository
{
    private readonly FocusTimerDatabaseContext ReadingContext = new(true);
    private readonly FocusTimerDatabaseContext WritingContext = new(false);

    private readonly int LastHowManyDays = 21;


    public Domain.Entities.App? FindAppByPath(string path)
    {
        return ReadingContext.Apps.FirstOrDefault(a => a.ExecutablePath == path);
    }

    public AppUsage? FindAppUsageById(int id)
    {
        return ReadingContext.AppUsages.Find(id);
    }

    public AppUsage? FindLastAppUsageByApp(Domain.Entities.App app)
    {
        return ReadingContext.AppUsages.LastOrDefault(u => u.App == app);
    }
    
    public TimerUsage? FindLastTimerUsage()
    {
        return ReadingContext.TimerUsages.LastOrDefault();
    }

    public void StartTracking(AppUsage usage)
    {
        WritingContext.AddAppUsage(usage);
    }

    public void StartTracking(TimerUsage usage)
    {
        WritingContext.AddTimerUsage(usage);
    }

    public IEnumerable<AppUsage> GetAppUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

        return ReadingContext.AppUsages.Where(u => u.StartedAt >= then).OrderBy(u => u.StartedAt);
    }

    public IEnumerable<TimerUsage> GetTimerUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

        return ReadingContext.TimerUsages.Where(u => u.StartedAt >= then).OrderBy(u => u.StartedAt);
    }
    

    public void Save()
    {
        WritingContext.Save();
    }
}