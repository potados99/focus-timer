using System;
using System.ComponentModel.DataAnnotations.Schema;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 등록된 앱이 focus를 가지고 있을 때의 사용 정보를 나타내는 엔티티입니다.
/// 등록된 앱이 foreground로 (재)진입하면 새로운 엔티티가 생깁니다.
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
    /// 마지막 업데이트 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 타이머에 등록된 앱이 focus를 가지고 있는 동안 흐른 시간입니다(tick).
    /// </summary>
    public long ElapsedTicks { get; set; }

    public TimerRunningUsage TimerRunningUsage { get; set; }

    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);

    public void TouchUsage()
    {
        this.GetLogger().Debug("TimerActiveUsage를 갱신합니다.");

        if (UpdatedAt - StartedAt > Elapsed + TimeSpan.FromSeconds(5))
        {
            throw new InvalidOperationException(
                "이 TimerActiveUsage에는 중간에 5초 이상 downtime이 있었던 것으로 보입니다. 시작 이후 흐른 시간이 실제 유효 시간보다 1분 넘게 큽니다.");
        }

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }
 
    public override string ToString()
    {
        return $"TimerActiveUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()})";
    }
}