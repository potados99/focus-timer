using System;
using System.Diagnostics;

namespace FocusTimer.Lib.Utility
{
    public static class StopwatchExtensions
    {
        public static string ElapsedString(this Stopwatch watch)
        {
            TimeSpan ts = watch.Elapsed;
            string currentTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

            return currentTime;
        }
    }
}
