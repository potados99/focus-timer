// TimerRunningUsage.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FocusTimer.Library.Extensions;

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
    /// [DEPRECATED] 이 필드는 더 이상 사용되지 않습니다.
    /// 하위 호환성을 위해 DB 컬럼으로 남아있지만 항상 0입니다.
    /// 실제 경과 시간은 <see cref="Elapsed"/> 프로퍼티를 사용하세요.
    /// </summary>
    [Obsolete("ElapsedTicks는 더 이상 사용되지 않습니다. Elapsed 프로퍼티를 사용하세요.")]
    public long ElapsedTicks { get; set; } = 0;

    public ICollection<TimerActiveUsage> ActiveUsages { get; } = new List<TimerActiveUsage>();

    [NotMapped] public TimerActiveUsage ActiveUsage => GetLastActiveUsage() ?? OpenNewActiveUsage();

    public TimerUsage ParentUsage { get; set; }

    /// <summary>
    /// 타이머가 켜져 있는 동안 흐른 시간입니다.
    /// </summary>
    [NotMapped]
    public TimeSpan Elapsed => UpdatedAt - StartedAt;

    [NotMapped] public TimeSpan ActiveElapsed => TimeSpan.FromTicks(ActiveUsages.Sum(u => u.Elapsed.Ticks));

    public void TouchUsage(DateTime now)
    {
        this.GetLogger().Debug($"TimerRunningUsage 갱신: UpdatedAt={now:HH:mm:ss}");

        UpdatedAt = now;
    }

    private TimerActiveUsage? GetLastActiveUsage()
    {
        var usage = ActiveUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존 TimerActiveUsage 조회: {usage}");
        }

        return usage;
    }

    public TimerActiveUsage OpenNewActiveUsage()
    {
        return OpenNewActiveUsage(DateTime.Now);
    }

    public TimerActiveUsage OpenNewActiveUsage(DateTime now)
    {
        this.GetLogger().Debug($"새 TimerActiveUsage 생성: StartedAt={now:HH:mm:ss}");

        var usage = new TimerActiveUsage
        {
            StartedAt = now,
            UpdatedAt = now,
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