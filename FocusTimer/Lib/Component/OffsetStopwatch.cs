using System;
using System.Diagnostics;

namespace FocusTimer.Lib.Component;

/// <summary>
/// 시작 오프셋을 추가한 <see cref="Stopwatch"/>입니다.
/// </summary>
public class OffsetStopwatch : Stopwatch
{
    private TimeSpan _offset = TimeSpan.Zero;

    public OffsetStopwatch()
    {
    }

    public OffsetStopwatch(TimeSpan offset)
    {
        _offset = offset;
    }

    public void AddOffset(TimeSpan offset)
    {
        _offset += offset;
    }

    public void ClearOffset()
    {
        _offset = TimeSpan.Zero;
    }

    public TimeSpan ElapsedWithOffset => Elapsed + _offset;
}