using System;
using FocusTimer.Domain.Entities;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.Timer.Slot;

/// <summary>
/// Usage를 보유하며 이로부터 주요 속성들을 제공하는 기반 클래스입니다.
/// </summary>
/// <typeparam name="TUsage">Usage의 타입입니다. <see cref="IUsage{TRunningUsage}"/>만 됩니다.</typeparam>
/// <typeparam name="TRunningUsage">RunningUsage의 타입입니다. <see cref="IRunningUsage{TUsage,TActiveUsage}"/>만 됩니다.</typeparam>
/// <typeparam name="TActiveUSage">ActiveUsage의 타입입니다. <see cref="IActiveUsage{TRunningUsage}"/>만 됩니다.</typeparam>
public abstract class UsageContainer<TUsage, TRunningUsage, TActiveUSage>
    where TUsage : IUsage<TRunningUsage>
    where TRunningUsage : IRunningUsage<TUsage, TActiveUSage>
    where TActiveUSage : IActiveUsage<TRunningUsage>
{
    /// <summary>
    /// 보유하고 있는 <see cref="IUsage{TRunningUsage}"/>입니다.
    /// </summary>
    /// <remarks>
    /// 외부로 노출될 일은 없기에 protected 접근 제한을 가집니다.
    /// </remarks>
    protected TUsage? Usage { get; set; }

    /// <summary>
    /// 현재 <see cref="Usage"/>에서 타이머가 실행되는 동안 흐른 총 시간을 나타냅니다.
    /// 이전 <see cref="IRunningUsage{TUsage,TActiveUsage}"/>에서 흐른 시간도 포함합니다.
    /// </summary>
    public long ElapsedTicks => Usage?.RunningElapsed.Ticks ?? 0;
    
    /// <summary>
    /// 현재 <see cref="Usage"/>에서 등록된 앱이 포커스를 얻은 채로(=타이머가 활성화된 채로) 흐른 총 시간을 나타냅니다.
    /// 이전 <see cref="IRunningUsage{TUsage,TActiveUsage}"/>에서 흐른 시간도 포함합니다.
    /// </summary>
    public long ActiveElapsedTicks => Usage?.ActiveElapsed.Ticks ?? 0;

    /// <summary>
    /// <see cref="ElapsedTicks"/>를 읽기 편한 스트링으로 제공합니다.
    /// </summary>
    public string ElapsedString => new TimeSpan(ElapsedTicks).ToSixDigits();
    
    /// <summary>
    /// <see cref="ActiveElapsedString"/>를 읽기 편한 스트링으로 제공합니다.
    /// </summary>
    public string ActiveElapsedString => new TimeSpan(ActiveElapsedTicks).ToSixDigits();
}