using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting.Entity
{
    public class AppUsage
    {
        public long Id { get; set; }
        public string AppName { get; set; }

        public DateTime StartedAt { get; set; }
        public DateTime FinishedAt { get; set; }

        public TimeSpan Duration
        {
            get
            {
                return FinishedAt - StartedAt;
            }
        }
    }
}
