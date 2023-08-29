using System;
using System.Windows.Threading;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Library.Utility;

/// <summary>
/// 정해진 시간 간격마다 이벤트를 발생시킵니다.
/// 이 앱에서는 1초 간격을 사용합니다.
/// </summary>
public class ClockGenerator
{
    public Signal? OnTick;

    public ClockGenerator()
    {
        StartClock(1);
    }

    private readonly DispatcherTimer _oneSecTickTimer = new();
    
    private void StartClock(int intervalSec)
    {
        _oneSecTickTimer.Stop();
        _oneSecTickTimer.RemoveHandlers();
        _oneSecTickTimer.Tick += (_, _) =>
        {
            OnTick?.Invoke();
        };
        _oneSecTickTimer.Interval = TimeSpan.FromSeconds(intervalSec);
        _oneSecTickTimer.Start();       
    }
}