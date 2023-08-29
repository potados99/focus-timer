namespace FocusTimer.Domain.Entities;

/// <summary>
/// 타이머에 등록된 앱이 포커스를 얻은 동안에 유지되는 엔티티의 필수 속성을 정의합니다.
/// </summary>
/// <typeparam name="TRunningUsage"></typeparam>
public interface IActiveUsage<TRunningUsage> : IElapsable
    where TRunningUsage : IElapsable
{
    /// <summary>
    /// 이 엔티티를 가지고 있는 부모 격의 RunningUsage입니다.
    /// </summary>
    public TRunningUsage ParentRunningUsage { get; set; }
}