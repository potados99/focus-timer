using System;
using System.Windows.Threading;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Lib;

public class ClockGenerator
{
    public Signal? OnTick;

    public ClockGenerator()
    {
        StartClock();
    }

    private readonly DispatcherTimer _oneSecTickTimer = new();
    
    private void StartClock()
    {
        _oneSecTickTimer.Stop();
        _oneSecTickTimer.RemoveHandlers();
        _oneSecTickTimer.Tick += (_, _) =>
        {
            OnTick?.Invoke();
        };
        _oneSecTickTimer.Interval = TimeSpan.FromSeconds(1);
        _oneSecTickTimer.Start();       
    }
}