using System;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 지나간 시간을 표현할 수 있는 속성들을 정의합니다.
/// </summary>
public interface IElapsable
{
    public DateTime StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public long ElapsedTicks { get; set; }
}