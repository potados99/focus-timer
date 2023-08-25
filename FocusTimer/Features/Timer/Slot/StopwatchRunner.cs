using System;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public class StopwatchRunner
{
    private readonly OffsetStopwatch _activeStopwatch = new();
    private readonly OffsetStopwatch _alwaysOnStopwatch = new();

    public long ElapsedTicks => _alwaysOnStopwatch.ElapsedWithOffset.Ticks;
    public long ActiveElapsedTicks => _activeStopwatch.ElapsedWithOffset.Ticks;

    public string ElapsedString => _alwaysOnStopwatch.ElapsedString();
    public string ActiveElapsedString => _activeStopwatch.ElapsedString();

    public void AddOffset(TimeSpan activeOffset, TimeSpan alwaysOnOffset)
    {
        _activeStopwatch.AddOffset(activeOffset);
        _alwaysOnStopwatch.AddOffset(alwaysOnOffset);
    }
    
    public void StartOrStopActiveTimer(bool active)
    {
        if (active)
        {
            _activeStopwatch.Start();
        }
        else
        {
            _activeStopwatch.Stop();
        }
    }

    public void Restart()
    {
        _activeStopwatch.ClearOffset();
        _activeStopwatch.Restart();

        _alwaysOnStopwatch.ClearOffset();
        _alwaysOnStopwatch.Restart();
    }
}