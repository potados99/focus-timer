using System;
using System.Collections.Generic;
using System.Linq;

namespace FocusTimer.Domain.Entities;

public interface IUsage<TRunningUsage> : IElapsable
    where TRunningUsage : IElapsable
{
    public ICollection<TRunningUsage> RunningUsages { get; }
    public TRunningUsage RunningUsage { get; }
    
    public TimeSpan RunningElapsed { get; }
    public TimeSpan ActiveElapsed { get; }
}