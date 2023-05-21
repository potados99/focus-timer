using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace focus.utils
{
    public static class StopwatchExtensions
    {
        public static string ElapsedString(this Stopwatch watch)
        {
            TimeSpan ts = watch.Elapsed;
            string currentTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

            return currentTime;
        }
    }
}
