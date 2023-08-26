using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using FocusTimer.Lib.Utility;

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
    /// 이 엔티티가 업데이트된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
    
    /// <summary>
    /// 타이머가 실제로 실행중인 상태에서 흐른 시간(tick)입니다.
    /// </summary>
    public long ElapsedTicks { get; set; }

    public bool IsConcentrated { get; set; }

    public ICollection<AppRunningUsage> RunningUsages { get; } = new List<AppRunningUsage>();
    
    [NotMapped] public AppRunningUsage RunningUsage => GetLastRunningUsage() ?? OpenNewRunningUsage();

    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);
    [NotMapped] public TimeSpan RunningElapsed => new(RunningUsages.Sum(u => u.Elapsed.Ticks));
    [NotMapped] public TimeSpan ActiveElapsed => new(RunningUsages.Sum(u => u.ActiveElapsed.Ticks));

    public void TouchUsage(bool isConcentrated)
    {
        this.GetLogger().Debug("AppUsage를 갱신합니다.");

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
        IsConcentrated = isConcentrated;
    }

    private AppRunningUsage? GetLastRunningUsage()
    {
        var usage = RunningUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 AppRunningUsage를 가져왔습니다: {usage}");
        }

        return usage;
    }
    
    public AppRunningUsage OpenNewRunningUsage()
    {
        this.GetLogger().Debug("새로운 AppRunningUsage를 생성합니다.");
        
        var usage = new AppRunningUsage
        {
            StartedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            AppUsage = this
        };
        
        RunningUsages.Add(usage);

        return usage;
    }

    public override string ToString()
    {
        return $"AppUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()}, ActiveElapsed={ActiveElapsed.ToSixDigits()})";
    }
}