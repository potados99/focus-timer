using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Data.DataContext;
using FocusTimer.Domain.Entities;
using FocusTimer.Library.Extensions;
using Microsoft.EntityFrameworkCore;

namespace FocusTimer.Data.Repositories;

public class FocusRepository
{
    private readonly FocusTimerDatabaseContext _context;

    public FocusRepository(FocusTimerDatabaseContext context)
    {
        _context = context;
    }

    private const int LAST_HOW_MANY_DAYS = 21;

    public Domain.Entities.App? FindAppByPath(string path)
    {
        return _context.Apps.FirstOrDefault(a => a.ExecutablePath == path);
    }

    public AppUsage? FindLastAppUsageByApp(Domain.Entities.App app)
    {
        // 마지막으로 리셋이 진행된 시각을 알아내서
        var lastResetAt = _context.ResetHistories
            .OrderBy(h => h.ResetAt)
            .LastOrDefault()
            ?.ResetAt ?? DateTime.MinValue;

        // 그보다 이후에 시작된 기록 중에서만 가져옵니다.
        return _context.AppUsages
            .Include(u => u.App)
            .Include(u => u.RunningUsages)
            .ThenInclude(u => u.ActiveUsages)
            .Where(u => u.StartedAt > lastResetAt)
            .OrderBy(u => u.StartedAt)
            .LastOrDefault(u => u.App == app);
    }

    public TimerUsage? FindLastTimerUsage()
    {
        // 마지막으로 리셋이 진행된 시각을 알아내서
        var lastResetAt = _context.ResetHistories
            .OrderBy(h => h.ResetAt)
            .LastOrDefault()
            ?.ResetAt ?? DateTime.MinValue;

        // 그보다 이후에 시작된 기록 중에서만 가져옵니다.
        return _context.TimerUsages
            .Include(u => u.RunningUsages)
            .ThenInclude(u => u.ActiveUsages)
            .Where(u => u.StartedAt > lastResetAt)
            .OrderBy(u => u.StartedAt)
            .LastOrDefault();
    }

    public Slot? FindSlotStatusBySlotNumber(long slotNumber)
    {
        return _context.SlotStatuses
            .Include(s => s.App)
            .FirstOrDefault(s => s.SlotNumber == slotNumber);
    }

    public IEnumerable<AppUsage> GetAppUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LAST_HOW_MANY_DAYS - 1));

        return _context.AppUsages
            .Include(u => u.App)
            .Include(u => u.RunningUsages)
            .ThenInclude(u => u.ActiveUsages)
            .Where(u => u.StartedAt >= then)
            .OrderBy(u => u.StartedAt);
    }

    public IEnumerable<TimerUsage> GetTimerUsages()
    {
        var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LAST_HOW_MANY_DAYS - 1));

        return _context.TimerUsages
            .Include(u => u.RunningUsages)
            .ThenInclude(u => u.ActiveUsages)
            .Where(u => u.StartedAt >= then)
            .OrderBy(u => u.StartedAt);
    }

    public IEnumerable<Slot> GetSlotStatuses()
    {
        return _context.SlotStatuses
            .Include(s => s.App)
            .AsEnumerable();
    }

    public void StartTracking(object entity)
    {
        _context.Add(entity);
    }

    public void Save()
    {
        this.GetLogger().Debug("데이터베이스에 변경 사항을 기록합니다.");

        _context.Save();
    }
}