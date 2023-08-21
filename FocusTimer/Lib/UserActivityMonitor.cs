using System;
using System.Windows.Threading;

namespace FocusTimer.Lib;

public class UserActivityMonitor
{
    public static UserActivityMonitor Instance = new();

    public delegate void StateChangeHandler();
    public event StateChangeHandler? OnActivated;
    public event StateChangeHandler? OnDeactivated;

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