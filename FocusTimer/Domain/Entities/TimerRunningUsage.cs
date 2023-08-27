using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머가 켜져 있는 동안의 타이머 사용 정보를 나타내는 엔티티입니다.
/// 타이머가 켜지면 새로운 엔티티가 생깁니다.
/// </summary>
public class TimerRunningUsage : IRunningUsage<TimerUsage, TimerActiveUsage>
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 타이머가 켜진 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 마지막 업데이트 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 타이머가 켜져 있는 동안 흐른 시간입니다(tick).
    /// </summary>
    public long ElapsedTicks { get; set; }

    public ICollection<TimerActiveUsage> ActiveUsages { get; } = new List<TimerActiveUsage>();

    [NotMapped] public TimerActiveUsage ActiveUsage => GetLastActiveUsage() ?? OpenNewActiveUsage();

    public TimerUsage ParentUsage { get; set; }

    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);
    [NotMapped] public TimeSpan ActiveElapsed => new(ActiveUsages.Sum(u => u.Elapsed.Ticks));

    public void TouchUsage()
    {
        this.GetLogger().Debug("TimerRunningUsage를 갱신합니다.");

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }
    
    private TimerActiveUsage? GetLastActiveUsage()
    {
        var usage = ActiveUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 TimerActiveUsage를 가져왔습니다: {usage}");
        }

        return usage;
    }

    public TimerActiveUsage OpenNewActiveUsage()
    {
        this.GetLogger().Debug("새로운 TimerActiveUsage를 생성합니다.");

        var usage = new TimerActiveUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            ParentRunningUsage = this
        };

        ActiveUsages.Add(usage);

        return usage;
    }
    
    public override string ToString()
    {
        return $"TimerRunningUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()})";
    }
}