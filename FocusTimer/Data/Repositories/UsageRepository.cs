using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Data.DataContext;
using FocusTimer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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

    public AppUsage? FindLastAppUsageByApp(Domain.Entities.App app)
    {
        return ReadingContext.AppUsages
            .Include(u => u.App)
            .Include(u => u.ActiveUsages)
            .OrderBy(u => u.StartedAt)
            .LastOrDefault(u => u.App == app);
    }

    public TimerUsage? FindLastTimerUsage()
    {
        return ReadingContext.TimerUsages
            .Include(u => u.ActiveUsages)
            .OrderBy(u => u.StartedAt)
            .LastOrDefault();
    }

    public SlotStatus? FindSlotStatusBySlotNumber(long slotNumber)
    {
        return ReadingContext.SlotStatuses
            .Include(s => s.App)
            .FirstOrDefault(s => s.SlotNumber == slotNumber);
    }

    public void StartTracking(AppUsage usage)
    {
        WritingContext.AddAppUsage(usage);
    }

    public void StartTracking(TimerUsage usage)
    {
        WritingContext.AddTimerUsage(usage);
    }

    public void StartTracking(SlotStatus status)
    {
        WritingContext.AddSlotStatus(status);
    }

    public IEnumerable<AppUsage> GetAppUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

        return ReadingContext.AppUsages
            .Include(u => u.App)
            .Include(u => u.ActiveUsages)
            .Where(u => u.StartedAt >= then)
            .OrderBy(u => u.StartedAt);
    }

    public IEnumerable<TimerUsage> GetTimerUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

        return ReadingContext.TimerUsages
            .Include(u => u.ActiveUsages)
            .Where(u => u.StartedAt >= then)
            .OrderBy(u => u.StartedAt);
    }

    public IEnumerable<SlotStatus> GetSlotStatuses()
    {
        return ReadingContext.SlotStatuses
            .Include(s => s.App)
            .AsEnumerable();
    }

    public void Save()
    {
        WritingContext.Save();
    }
}