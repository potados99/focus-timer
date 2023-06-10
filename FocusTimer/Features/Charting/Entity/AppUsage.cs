using System;

namespace FocusTimer.Features.Charting.Entity
{
    public class AppUsage
    {
        public long Id { get; set; }

        public string AppPath { get; set; }

        public DateTime RegisteredAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public long Usage { get; set; }

        public bool IsConcentrated { get; set; }
    }
}
