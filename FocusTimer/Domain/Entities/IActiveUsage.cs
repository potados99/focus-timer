namespace FocusTimer.Domain.Entities;

public interface IActiveUsage<TRunningUsage> : IElapsable
    where TRunningUsage : IElapsable
{
    public TRunningUsage ParentRunningUsage { get; set; }
}