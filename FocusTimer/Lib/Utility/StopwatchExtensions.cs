using System;
using System.Diagnostics;
using FocusTimer.Lib.Component;

namespace FocusTimer.Lib.Utility;

public static class StopwatchExtensions
{
    public static string ElapsedString(this OffsetStopwatch watch)
    {
        TimeSpan ts = watch.ElapsedWithOffset;
        string currentTime = string.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

        return currentTime;
    }
}