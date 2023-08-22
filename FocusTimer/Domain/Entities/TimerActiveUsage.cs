using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머의 실제 사용 현황을 나타내는 엔티티입니다.
/// </summary>
public class TimerActiveUsage
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 타이머에 등록된 앱이 포커스를 얻은 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 타이머에 등록된 앱이 포커스를 얻은 상태로 유지된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 부모 <see cref="TimerUsage"/>입니다.
    /// </summary>
    public TimerUsage TimerUsage { get; set; }
    
    [NotMapped] public TimeSpan Elapsed => UpdatedAt - StartedAt;
}