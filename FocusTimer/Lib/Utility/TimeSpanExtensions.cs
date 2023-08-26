using System;

namespace FocusTimer.Lib.Utility;

public static class TimeSpanExtensions
{
    public static string ToSixDigits(this TimeSpan ts)
    {
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}