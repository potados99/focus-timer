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

    /// <summary>
    /// 타이머의 실제 사용 기록들입니다.
    /// </summary>
    public ICollection<TimerActiveUsage> ActiveUsages { get; } = new List<TimerActiveUsage>();
    
    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);
    [NotMapped] public TimeSpan ActiveElapsed => new(ActiveUsages.Sum(u => u.Elapsed.Ticks));

    public void TouchUsage()
    {
        this.GetLogger().Debug("TimerUsage를 갱신합니다.");

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }

    public void TouchActiveUsage()
    {
        this.GetLogger().Debug("TimerActiveUsage를 갱신합니다.");

        var usage = GetLastActiveUsage() ?? OpenNewActiveUsage();

        usage.UpdatedAt = DateTime.Now;
        usage.ElapsedTicks += TimeSpan.TicksPerSecond;
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
            TimerUsage = this
        };
        
        ActiveUsages.Add(usage);

        return usage;
    }
    
    public override string ToString()
    {
        return $"TimerUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()}, ActiveElapsed={ActiveElapsed.ToSixDigits()})";
    }
}