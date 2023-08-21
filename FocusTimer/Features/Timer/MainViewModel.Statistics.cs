using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Features.License;
using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

internal partial class MainViewModel : BaseModel
{
    #region 사용량 집계

    public long ElapsedTicks
    {
        get
        {
            return TicksStartOffset + AlwaysOnStopwatch.ElapsedTicks;
        }
    }

    public long ActiveElapsedTicks
    {
        get
        {
            return ActiveTicksStartOffset + ActiveStopwatch.ElapsedTicks;
        }
    }

    public string ActiveElapsedTime
    {
        get
        {
            return ActiveStopwatch.ElapsedString(ActiveTicksStartOffset);
        }
    }

    private TimerUsage? Usage;

    private long TicksStartOffset = 0;
    private long TicksElapsedOffset = 0;

    private long ActiveTicksStartOffset = 0;
    private long ActiveTicksElapsedOffset = 0;

    private void UpdateUsage()
    {
        if (Usage != null && Usage.StartedAt.Date < DateTime.Today)
        {
            // 날짜가 지났습니다!
            TicksElapsedOffset += Usage.Usage;
            ActiveTicksElapsedOffset += Usage.ActiveUsage;
            Usage = UsageRepository.CreateTimerUsage(closeOthers: false/*이전 것과 이어집니다.*/);
        }

        Usage ??= UsageRepository.CreateTimerUsage(closeOthers: true/*이전 것과 분리됩니다.*/);

        Usage.Usage = ElapsedTicks - TicksElapsedOffset;
        Usage.ActiveUsage = ActiveElapsedTicks - ActiveTicksElapsedOffset;
        Usage.UpdatedAt = DateTime.Now;

        UsageRepository.Save();
    }

    public void RestoreStatesFromLastUsage()
    {
        var openUsages = UsageRepository.GetLastTimerUsages();
        if (openUsages.Count() == 0)
        {
            return;
        }

        Usage = openUsages.Last();

        AlwaysOnStopwatch.Restart();
        ActiveStopwatch.Restart();

        TicksStartOffset += openUsages.Sum(u => u.Usage);
        ActiveTicksStartOffset += openUsages.Sum(u => u.ActiveUsage);

        foreach (var slot in TimerSlots)
        {
            slot.CurrentApp?.RestoreFromLastUsage();
        }
    }

    #endregion
}