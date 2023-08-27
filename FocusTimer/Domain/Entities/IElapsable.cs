using System;

namespace FocusTimer.Domain.Entities;

public interface IElapsable
{
    public DateTime StartedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public long ElapsedTicks { get; set; }
}