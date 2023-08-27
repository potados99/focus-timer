using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머가 켜져 있는 동안의 앱 사용 정보를 나타내는 엔티티입니다.
/// 앱을 슬롯에 (재)등록하면 새로운 엔티티가 생깁니다.
/// </summary>
public class AppRunningUsage : IRunningUsage<AppUsage, AppActiveUsage>
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 앱이 슬롯에 (재)등록 또는 복구된 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 마지막 업데이트 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 타이머가 켜져 있고 앱이 슬롯에 등록되어 있는 동안 흐른 시간입니다(tick).
    /// </summary>
    public long ElapsedTicks { get; set; }
    
    public ICollection<AppActiveUsage> ActiveUsages { get; } = new List<AppActiveUsage>();

    [NotMapped] public AppActiveUsage ActiveUsage => GetLastActiveUsage() ?? OpenNewActiveUsage();

    public AppUsage ParentUsage { get; set; }

    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);
    [NotMapped] public TimeSpan ActiveElapsed => new(ActiveUsages.Sum(u => u.Elapsed.Ticks));

    public void TouchUsage()
    {
        this.GetLogger().Debug("AppRunningUsage를 갱신합니다.");

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }
    
    private AppActiveUsage? GetLastActiveUsage()
    {
        var usage = ActiveUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 AppActiveUsage를 가져왔습니다: {usage}");
        }

        return usage;
    }

    public AppActiveUsage OpenNewActiveUsage()
    {
        this.GetLogger().Debug("새로운 AppActiveUsage를 생성합니다.");

        var usage = new AppActiveUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            ParentRunningUsage = this
        };

        ActiveUsages.Add(usage);

        return usage;
    }
    
    public override string ToString()
    {
        return $"AppRunningUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()})";
    }
}