// IRunningUsage.cs
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

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머가 켜져 있는 동안의 사용 정보를 나타내는 엔티티의 필수 속성을 정의합니다.
/// </summary>
/// <typeparam name="TUsage"></typeparam>
/// <typeparam name="TActiveUsage"></typeparam>
public interface IRunningUsage<TUsage, TActiveUsage> : IElapsable
    where TUsage : IElapsable
    where TActiveUsage : IElapsable
{
    /// <summary>
    /// 이 엔티티에 속한 ActiveUsages들입니다.
    /// </summary>
    public ICollection<TActiveUsage> ActiveUsages { get; }
    
    /// <summary>
    /// 이 엔티티에 속한 ActiveUsage 중,
    /// 현재 포커스 세션 동안에만 유지되는 가장 최신의 ActiveUsage입니다.
    /// </summary>
    public TActiveUsage ActiveUsage { get; }

    /// <summary>
    /// 이 엔티티에 속한 모든 ActiveUsage들에 대한 Elapsed의 합입니다.
    /// </summary>
    /// <remarks>
    /// 주의할 점은, 이는 현재 <see cref="TActiveUsage"/>에 대한 Elapsed의 값이 아니라는 점입니다.
    /// 지금 진행중인 <see cref="TActiveUsage"/>의 Elapsed를 구하고자 한다면,
    /// <see cref="ActiveUsage"/>의 Elapsed 속성을 참조해주세요.
    /// </remarks>
    public TimeSpan ActiveElapsed { get; }

    /// <summary>
    /// 이 엔티티를 가지고 있는 부모 격의 Usage입니다.
    /// </summary>
    public TUsage ParentUsage { get; set; }
}