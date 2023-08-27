using System.Collections.Generic;

namespace FocusTimer.Domain.Entities;

public interface IRunningUsage<TUsage, TActiveUsages> : IElapsable
    where TUsage : IElapsable
    where TActiveUsages : IElapsable
{
    public ICollection<TActiveUsages> ActiveUsages { get; }

    public TUsage ParentUsage { get; set; }
}