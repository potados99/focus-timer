using System;
using System.Diagnostics;

namespace FocusTimer.Lib.Utility
{
    public static class StopwatchExtensions
    {
        public static string ElapsedString(this Stopwatch watch, long offsetTicks = 0)
        {
            TimeSpan ts = watch.Elapsed + TimeSpan.FromTicks(offsetTicks);
            string currentTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

            return currentTime;
        }
    }
}
