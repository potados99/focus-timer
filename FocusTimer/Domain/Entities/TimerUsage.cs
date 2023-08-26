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
    /// 타이머가 켜진 상태로 유지된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 타이머의 실제 사용 기록들입니다.
    /// </summary>
    public ICollection<TimerActiveUsage> ActiveUsages { get; } = new List<TimerActiveUsage>();
    
    [NotMapped] public TimeSpan Elapsed => UpdatedAt - StartedAt;

    [NotMapped] public TimeSpan ActiveElapsed => new(ActiveUsages.Sum(u => u.Elapsed.Ticks));
    
    private TimerActiveUsage? GetLastActiveUsage()
    {
        var usage = ActiveUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug("기존의 TimerActiveUsage를 가져왔습니다.");
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

    public void TouchActiveUsage()
    {
        var usage = GetLastActiveUsage() ?? OpenNewActiveUsage();

        usage.UpdatedAt = DateTime.Now;
    }
}