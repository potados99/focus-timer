using System;
using System.Collections.Generic;

namespace FocusTimer.Domain.Entities;

/// <summary>
/// 리셋과 리셋 사이의 사용 정보를 나타내는 엔티티의 필수 속성을 정의합니다.
/// </summary>
/// <typeparam name="TRunningUsage"></typeparam>
public interface IUsage<TRunningUsage> : IElapsable
    where TRunningUsage : IElapsable
{
    /// <summary>
    /// 이 엔티티에 속한 RunningUsage들입니다.
    /// </summary>
    public ICollection<TRunningUsage> RunningUsages { get; }
    
    /// <summary>
    /// 이 엔티티에 속한 RunningUsage 중,
    /// 현재 실행 동안에만 유지되는 가장 최신의 RunningUsage입니다.
    /// </summary>
    public TRunningUsage RunningUsage { get; }
    
    /// <summary>
    /// 이 엔티티에 속한 모든 RunningUsage들에 대한 Elapsed의 합입니다.
    /// </summary>
    /// <remarks>
    /// 주의할 점은, 이는 현재 <see cref="TRunningUsage"/>에 대한 Elapsed의 값이 아니라는 점입니다.
    /// 지금 진행중인 <see cref="TRunningUsage"/>의 Elapsed를 구하고자 한다면,
    /// <see cref="RunningUsage"/>의 Elapsed 속성을 참조해주세요.
    /// </remarks>
    public TimeSpan RunningElapsed { get; }
    
    /// <summary>
    /// 이 엔티티에 속한 모든 ActiveUsage들에 대한 Elapsed의 합입니다.
    /// </summary>
    public TimeSpan ActiveElapsed { get; }
}