using System;
using System.Collections.Generic;
using System.Linq;

namespace FocusTimer.Domain.Entities;

public interface IRunningUsage<TUsage, TActiveUsage> : IElapsable
    where TUsage : IElapsable
    where TActiveUsage : IElapsable
{
    public ICollection<TActiveUsage> ActiveUsages { get; }
    public TActiveUsage ActiveUsage { get; }

    public TimeSpan ActiveElapsed { get; }

    public TUsage ParentUsage { get; set; }
}