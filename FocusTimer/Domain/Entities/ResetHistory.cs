using System;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머가 리셋된 기록입니다.
/// </summary>
public class ResetHistory
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }
    
    /// <summary>
    /// 타이머가 리셋된 시각입니다.
    /// </summary>
    public DateTime ResetAt { get; set; }

    public static ResetHistory OfNow()
    {
        return new ResetHistory()
        {
            ResetAt = DateTime.Now
        };
    }
}