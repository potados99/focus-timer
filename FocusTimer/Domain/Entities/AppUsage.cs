// AppUsage.cs
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
/// 리셋과 리셋 사이의 앱 사용 정보를 나타내는 엔티티입니다.
/// 타이머가 리셋되면 새로운 엔티티가 생깁니다.
/// </summary>
public class AppUsage : IUsage<AppRunningUsage>
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
    /// 앱이 슬롯에 리셋 이후 처음으로 등록된 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 이 엔티티가 업데이트된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// [DEPRECATED] 이 필드는 더 이상 사용되지 않습니다.
    /// 하위 호환성을 위해 DB 컬럼으로 남아있지만 항상 0입니다.
    /// 실제 경과 시간은 <see cref="Elapsed"/> 프로퍼티를 사용하세요.
    /// </summary>
    [Obsolete("ElapsedTicks는 더 이상 사용되지 않습니다. Elapsed 프로퍼티를 사용하세요.")]
    public long ElapsedTicks { get; set; } = 0;

    /// <summary>
    /// 이 앱을 집중도 계산에 포함할지 여부를 나타냅니다.
    /// </summary>
    public bool IsConcentrated { get; set; }

    public ICollection<AppRunningUsage> RunningUsages { get; } = new List<AppRunningUsage>();

    [NotMapped] public AppRunningUsage RunningUsage => GetLastRunningUsage() ?? OpenNewRunningUsage();

    /// <summary>
    /// 리셋 이후 앱이 등록되어 있는 동안 흐른 시간입니다.
    /// 실제로 타이머가 켜져 있는 동안에만 증가하기 때문에,
    /// <see cref="RunningElapsed"/>와 사실상 같습니다.
    /// </summary>
    [NotMapped]
    public TimeSpan Elapsed => RunningElapsed;

    [NotMapped] public TimeSpan RunningElapsed => TimeSpan.FromTicks(RunningUsages.Sum(u => u.Elapsed.Ticks));
    [NotMapped] public TimeSpan ActiveElapsed => TimeSpan.FromTicks(RunningUsages.Sum(u => u.ActiveElapsed.Ticks));

    public void TouchUsage(DateTime now, bool isConcentrated)
    {
        this.GetLogger().Debug($"AppUsage 갱신: UpdatedAt={now:HH:mm:ss}, 집중도포함={isConcentrated}");

        UpdatedAt = now;
        IsConcentrated = isConcentrated;
    }

    private AppRunningUsage? GetLastRunningUsage()
    {
        var usage = RunningUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존 AppRunningUsage 조회: {usage}");
        }

        return usage;
    }

    public AppRunningUsage OpenNewRunningUsage()
    {
        this.GetLogger().Debug($"새 AppRunningUsage 생성: StartedAt={DateTime.Now:HH:mm:ss}");

        var usage = new AppRunningUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            ParentUsage = this
        };

        RunningUsages.Add(usage);

        return usage;
    }

    public override string ToString()
    {
        return $"AppUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()}, RunningElapsed={RunningElapsed.ToSixDigits()}, ActiveElapsed={ActiveElapsed.ToSixDigits()})";
    }
}