using System;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public class StopwatchRunner<TUsage, TRunningUsage, TActiveUSage>
    where TUsage : IUsage<TRunningUsage>
    where TRunningUsage : IRunningUsage<TUsage, TActiveUSage>
    where TActiveUSage : IActiveUsage<TRunningUsage>
{
    private readonly OffsetStopwatch _activeStopwatch = new();
    private readonly OffsetStopwatch _alwaysOnStopwatch = new();

    public TUsage? Usage { get; set; }

    public long ElapsedTicks => Usage?.RunningUsage.ElapsedTicks ?? 0;
    public long ActiveElapsedTicks => Usage?.RunningUsage.ActiveElapsed.Ticks ?? 0;

    public string ElapsedString => new TimeSpan(ElapsedTicks).ToSixDigits();
    public string ActiveElapsedString => new TimeSpan(ActiveElapsedTicks).ToSixDigits();

    protected void AddOffset(TimeSpan activeOffset, TimeSpan alwaysOnOffset)
    {
        _activeStopwatch.AddOffset(activeOffset);
        _alwaysOnStopwatch.AddOffset(alwaysOnOffset);
    }

    protected void StartOrStopActiveTimer(bool active)
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

    protected void Restart()
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