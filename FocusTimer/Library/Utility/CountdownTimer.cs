using System;
using System.Windows.Threading;

namespace FocusTimer.Library.Utility;

/// <summary>
/// 정해진 기간 동안 일정 간격으로 Tick을 발생시킵니다.
/// </summary>
public class CountdownTimer
{
    public event Signal? OnFinish;

    public TimeSpan Duration { get; set; }

    public bool IsEnabled => _timer.IsEnabled;

    private readonly DispatcherTimer _timer = new();

    public TimeSpan TimeLeft { get; private set; }

    public CountdownTimer()
    {
        Prepare();
    }

    private void Prepare()
    {
        _timer.Tick += (_, _) =>
        {
            TimeLeft = TimeLeft.Subtract(TimeSpan.FromSeconds(1));

            if (TimeLeft == TimeSpan.Zero || TimeLeft.TotalSeconds <= 0)
            {
                _timer.Stop();
                OnFinish?.Invoke();
            }
        };
        _timer.Interval = TimeSpan.FromSeconds(1);
    }

    public void Start()
    {
        TimeLeft = new TimeSpan(Duration.Ticks);
        _timer.Start();
    }

    public void Stop()
    {
        _timer.Stop();
    }
}