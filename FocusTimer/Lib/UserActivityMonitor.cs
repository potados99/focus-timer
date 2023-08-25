using System;
using System.Windows.Threading;

namespace FocusTimer.Lib;

public class UserActivityMonitor
{
    public event Signal? OnActivated;
    public event Signal? OnDeactivated;

    private readonly DispatcherTimer _timer = new();

    public int Timeout { get; set; }

    public UserActivityMonitor()
    {
        Timeout = Settings.GetActivityTimeout();

        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += (_, _) =>
        {
            if (IsActive && !_wasActive)
            {
                OnActivated?.Invoke();
            }

            if (!IsActive && _wasActive)
            {
                OnDeactivated?.Invoke();
            }

            _wasActive = IsActive;
        };

        _timer.Start();
    }

    private bool _wasActive = true;

    public bool IsActive => APIWrapper.GetElapsedFromLastInput() < (Timeout * 1000);
}