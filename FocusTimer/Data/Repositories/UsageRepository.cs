using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Data.DataContext;
using FocusTimer.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FocusTimer.Data.Repositories;

public class UsageRepository
{
    private readonly FocusTimerDatabaseContext WritingContext = new(false);

    private readonly int LastHowManyDays = 21;
    
    public Domain.Entities.App? FindAppByPath(string path)
    {
        return WritingContext.Apps.FirstOrDefault(a => a.ExecutablePath == path);
    }

    public AppUsage? FindLastAppUsageByApp(Domain.Entities.App app)
    {
        // 마지막으로 리셋이 진행된 시각을 알아내서
        var lastResetAt = WritingContext.ResetHistories
            .OrderBy(h => h.ResetAt)
            .LastOrDefault()
            ?.ResetAt ?? DateTime.MinValue;

        // 그보다 이후에 시작된 기록 중에서만 가져옵니다.
        return WritingContext.AppUsages
            .Include(u => u.App)
            .Include(u => u.ActiveUsages)
            .Where(u => u.StartedAt > lastResetAt)
            .OrderBy(u => u.StartedAt)
            .LastOrDefault(u => u.App == app);
    }

    public TimerUsage? FindLastTimerUsage()
    {
        return WritingContext.TimerUsages
            .Include(u => u.ActiveUsages)
            .OrderBy(u => u.StartedAt)
            .LastOrDefault();
    }

    public Slot? FindSlotStatusBySlotNumber(long slotNumber)
    {
        return WritingContext.SlotStatuses
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

    public void StartTracking(Slot status)
    {
        WritingContext.AddSlotStatus(status);
    }

    public void StartTracking(ResetHistory history)
    {
        WritingContext.AddResetHistory(history);
    }

    public IEnumerable<AppUsage> GetAppUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

        return WritingContext.AppUsages
            .Include(u => u.App)
            .Include(u => u.ActiveUsages)
            .Where(u => u.StartedAt >= then)
            .OrderBy(u => u.StartedAt);
    }

    public IEnumerable<TimerUsage> GetTimerUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

        return WritingContext.TimerUsages
            .Include(u => u.ActiveUsages)
            .Where(u => u.StartedAt >= then)
            .OrderBy(u => u.StartedAt);
    }

    public IEnumerable<Slot> GetSlotStatuses()
    {
        return WritingContext.SlotStatuses
            .Include(s => s.App)
            .AsEnumerable();
    }

    public void Save()
    {
        WritingContext.Save();
    }
}