using System.Collections.Generic;

namespace FocusTimer.Domain.Entities;

public interface IUsage<TRunningUsage> : IElapsable
    where TRunningUsage : IElapsable
{
    public ICollection<TRunningUsage> RunningUsages { get; }
}