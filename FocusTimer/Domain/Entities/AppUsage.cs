using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 슬롯에 등록된 <see cref="App"/>의 사용 현황을 나타내는 엔티티입니다.
/// </summary>
public class AppUsage
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
    /// 앱이 슬롯에 등록된 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 앱이 슬롯에 등록된 상태로 유지된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 앱의 실제 사용 기록들입니다.
    /// </summary>
    public ICollection<AppActiveUsage> ActiveUsages { get; } = new List<AppActiveUsage>();

    /// <summary>
    /// 이 앱이 슬롯에서 "집중도 계산에 포함"하도록 설정되어 있는지 여부입니다.
    /// </summary>
    public bool IsConcentrated { get; set; }

    [NotMapped] public TimeSpan Elapsed => UpdatedAt - StartedAt;
    [NotMapped] public TimeSpan ActiveElapsed => new(ActiveUsages.Sum(u => u.Elapsed.Ticks));

}