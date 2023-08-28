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
        _activeStopwatch.Reset();

        _alwaysOnStopwatch.ClearOffset();
        _alwaysOnStopwatch.Reset();
        
        // 이 친구는 상태와 무관하게 계속 흘러갑니다.
        // 따라서 이곳에서 시작되며 다시 멈추지 않습니다.
        _alwaysOnStopwatch.Start();
    }
}