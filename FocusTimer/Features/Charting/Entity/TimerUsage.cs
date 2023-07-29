using System;

namespace FocusTimer.Features.Charting.Entity
{
    public class TimerUsage
    {
        public long Id { get; set; }

        public DateTime StartedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public long Usage { get; set; }

        public long ActiveUsage { get; set; }

        public bool IsOpen { get; set; }
    }
}
