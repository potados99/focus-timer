using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

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
}