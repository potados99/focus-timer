using System;
using FocusTimer.Lib.Component;

namespace FocusTimer.Lib.Utility;

public static class StopwatchExtensions
{
    public static string ElapsedString(this OffsetStopwatch watch)
    {
        return watch.ElapsedWithOffset.ToSixDigits();
    }
}