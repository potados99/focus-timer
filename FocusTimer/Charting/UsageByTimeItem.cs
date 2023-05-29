using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting
{
    public class UsageByTimeItem
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public string TimeString
        {
            get
            {
                return Start.ToString("H시 m분") + " ~ " + End.ToString("H시 m분");
            }
        }

        public int UsageMinutes { get; set; }
        public string UsageMinutesString
        {
            get
            {
                return UsageMinutes.ToString() + "분";
            }
        }
    }
}
