using System;
using System.Linq;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel 
{
    #region 사용량 집계와 복구

    private long ElapsedTicks => _ticksStartOffset + _alwaysOnStopwatch.ElapsedTicks;

    private long ActiveElapsedTicks => _activeTicksStartOffset + _activeStopwatch.ElapsedTicks;

    public string ActiveElapsedTime
    {
        get
        {
            return _activeStopwatch.ElapsedString(_activeTicksStartOffset);
        }
    }

    private TimerUsage? _usage;

    private long _ticksStartOffset;
    private long _ticksElapsedOffset;

    private long _activeTicksStartOffset;
    private long _activeTicksElapsedOffset;

    private void UpdateUsage()
    {
        if (_usage != null && _usage.StartedAt.Date < DateTime.Today)
        {
            // 날짜가 지났습니다!
            _ticksElapsedOffset += _usage.Usage;
            _activeTicksElapsedOffset += _usage.ActiveUsage;
            _usage = UsageRepository.CreateTimerUsage(closeOthers: false/*이전 것과 이어집니다.*/);
        }

        _usage ??= UsageRepository.CreateTimerUsage(closeOthers: true/*이전 것과 분리됩니다.*/);

        _usage.Usage = ElapsedTicks - _ticksElapsedOffset;
        _usage.ActiveUsage = ActiveElapsedTicks - _activeTicksElapsedOffset;
        _usage.UpdatedAt = DateTime.Now;

        UsageRepository.Save();
    }

    private void RestoreStatesFromLastUsage()
    {
        var openUsages = UsageRepository.GetLastTimerUsages();
        if (openUsages.Count() == 0)
        {
            return;
        }

        _usage = openUsages.Last();

        _alwaysOnStopwatch.Restart();
        _activeStopwatch.Restart();

        _ticksStartOffset += openUsages.Sum(u => u.Usage);
        _activeTicksStartOffset += openUsages.Sum(u => u.ActiveUsage);

        foreach (var slot in TimerSlots)
        {
            slot.CurrentApp?.RestoreFromLastUsage();
        }
    }

    #endregion
}