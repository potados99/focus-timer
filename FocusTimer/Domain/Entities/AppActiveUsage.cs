// AppActiveUsage.cs
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
using System.ComponentModel.DataAnnotations.Schema;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 앱이 focus를 가지고 있을 때의 사용 정보를 나타내는 엔티티입니다.
/// 앱이 foreground로 (재)진입하면 새로운 엔티티가 생깁니다.
/// </summary>
public class AppActiveUsage : IActiveUsage<AppRunningUsage>
{
    /// <summary>
    /// PK입니다.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 앱이 focus를 얻은 시각입니다.
    /// </summary>
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// 마지막 업데이트 시각입니다.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// 앱이 focus를 가지고 있는 동안 흐른 시간입니다(tick).
    /// </summary>
    public long ElapsedTicks { get; set; }

    public AppRunningUsage ParentRunningUsage { get; set; }

    [NotMapped] public TimeSpan Elapsed => new(ElapsedTicks);

    public void TouchUsage()
    {
        this.GetLogger().Debug("AppActiveUsage를 갱신합니다.");

        if (UpdatedAt - StartedAt > Elapsed + TimeSpan.FromSeconds(5))
        {
            this.GetLogger()
                .Error($"이 {ToString()}에는 중간에 5초 이상 downtime이 있었던 것으로 보입니다. 시작 이후 흐른 시간이 실제 유효 시간보다 5초 넘게 큽니다.");
        }

        UpdatedAt = DateTime.Now;
        ElapsedTicks += TimeSpan.TicksPerSecond;
    }

    public override string ToString()
    {
        return $"AppActiveUsage(Id={Id}, Elapsed={Elapsed.ToSixDigits()})";
    }
}