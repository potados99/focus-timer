using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머의 사용 현황을 나타내는 엔티티입니다.
/// </summary>
public class TimerUsage
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 타이머가 시작된 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 이 엔티티가 업데이트된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 타이머가 실제로 실행중인 상태에서 흐른 시간(tick)입니다.
    /// </summary>
    public long ElapsedTicks { get; set; }

    public ICollection<TimerRunningUsage> RunningUsages { get; } = new List<TimerRunningUsage>();
    
    [NotMapped] public TimerRunningUsage RunningUsage => GetLastRunningUsage() ?? OpenNewRunningUsage();
    
    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);
    [NotMapped] public TimeSpan RunningElapsed => new(RunningUsages.Sum(u => u.Elapsed.Ticks));
    [NotMapped] public TimeSpan ActiveElapsed => new(RunningUsages.Sum(u => u.ActiveElapsed.Ticks));
    
    public void TouchUsage()
    {
        this.GetLogger().Debug("TimerUsage를 갱신합니다.");

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }
    
    private TimerRunningUsage? GetLastRunningUsage()
    {
        var usage = RunningUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 TimerRunningUsage를 가져왔습니다: {usage}");
        }

        return usage;
    }

    public TimerRunningUsage OpenNewRunningUsage()
    {
        this.GetLogger().Debug("새로운 TimerRunningUsage를 생성합니다.");

        var usage = new TimerRunningUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            TimerUsage = this
        };

        RunningUsages.Add(usage);

        return usage;
    }

    public override string ToString()
    {
        return
            $"TimerUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()}, RunningElapsed={RunningElapsed.ToSixDigits()}, ActiveElapsed={ActiveElapsed.ToSixDigits()})";
    }
}