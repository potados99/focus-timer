using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 슬롯에 등록된 <see cref="App"/>의 실제 사용 현황을 나타내는 엔티티입니다.
/// </summary>
public class AppActiveUsage
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 사용 현황을 기록하는 앱입니다.
    /// </summary>
    public App App { get; set; }

    /// <summary>
    /// 앱의 실제 사용이 시작된 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 앱이 실제로 사용되는 상태로 유지된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 부모 <see cref="AppUsage"/>입니다.
    /// </summary>
    public AppUsage AppUsage { get; set; }
    
    [NotMapped] public TimeSpan Elapsed => UpdatedAt - StartedAt;
}