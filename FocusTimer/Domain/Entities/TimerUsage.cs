// TimerUsage.cs
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
/// 리셋과 리셋 사이의 타이머 사용 정보를 나타내는 엔티티입니다.
/// 타이머가 리셋되면 새로운 엔티티가 생깁니다.
/// </summary>
public class TimerUsage : IUsage<TimerRunningUsage>
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 타이머가 리셋된 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 이 엔티티가 업데이트된 마지막 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 리셋 이후 흐른 시간입니다(tick).
    /// 실제로 타이머가 켜져 있는 동안에만 증가하기 때문에,
    /// <see cref="RunningUsage"/>의 <see cref="ElapsedTicks"/>와 사실상 같습니다.
    /// </summary>
    public long ElapsedTicks { get; set; }

    public ICollection<TimerRunningUsage> RunningUsages { get; } = new List<TimerRunningUsage>();
    
    [NotMapped] public TimerRunningUsage RunningUsage => GetLastRunningUsage() ?? OpenNewRunningUsage();
    
    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);
    [NotMapped] public TimeSpan RunningElapsed => new(RunningUsages.Sum(u => u.Elapsed.Ticks));
    [NotMapped] public TimeSpan ActiveElapsed => new(RunningUsages.Sum(u => u.ActiveElapsed.Ticks));
    
    public void TouchUsage()
    {
        this.GetLogger().Debug("TimerUsage를 갱신합니다.");

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }
    
    private TimerRunningUsage? GetLastRunningUsage()
    {
        var usage = RunningUsages.LastOrDefault();
        if (usage != null)
        {
            this.GetLogger().Debug($"기존의 TimerRunningUsage를 가져왔습니다: {usage}");
        }

        return usage;
    }

    public TimerRunningUsage OpenNewRunningUsage()
    {
        this.GetLogger().Debug("새로운 TimerRunningUsage를 생성합니다.");

        var usage = new TimerRunningUsage
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
        return
            $"TimerUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()}, RunningElapsed={RunningElapsed.ToSixDigits()}, ActiveElapsed={ActiveElapsed.ToSixDigits()})";
    }
}