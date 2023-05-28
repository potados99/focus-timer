using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting.Entity
{
    public class TimerUsage
    {
        public long Id { get; set; }    

        public DateTime StartedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public long Usage { get; set; } 
    }
}
