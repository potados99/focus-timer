using System;
using System.Windows.Threading;

namespace FocusTimer.Lib.Utility;

/// <summary>
/// 사용자의 입력을 감지하는 모니터입니다.
/// 사용자가 오랫동안 키보드와 마우스를 사용하지 않으면 비활성화 이벤트를 발생시키며,
/// 이후 입력이 감지되면 활성화 이벤트를 발생시킵니다.
/// </summary>
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