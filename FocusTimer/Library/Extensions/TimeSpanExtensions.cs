using System;

namespace FocusTimer.Library.Extensions;

public static class TimeSpanExtensions
{
    public static string ToSixDigits(this TimeSpan ts)
    {
        return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
    }
}