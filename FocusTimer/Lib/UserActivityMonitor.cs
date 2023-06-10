using System;
using System.Windows.Threading;

namespace FocusTimer.Lib
{
    public class UserActivityMonitor
    {
        public static UserActivityMonitor Instance = new();

        public delegate void StateChangeHandler();
        public event StateChangeHandler? OnActivated;
        public event StateChangeHandler? OnDeactivated;

        private readonly DispatcherTimer Timer = new();

        public int Timeout { get; set; }

        public UserActivityMonitor()
        {
            Timeout = Settings.GetActivityTimeout();

            Timer.Interval = TimeSpan.FromMilliseconds(500);
            Timer.Tick += (_, _) =>
            {
                if (IsActive && !WasActive)
                {
                    OnActivated?.Invoke();
                }

                if (!IsActive && WasActive)
                {
                    OnDeactivated?.Invoke();
                }

                WasActive = IsActive;
            };

            Timer.Start();
        }

        private bool WasActive = true;

        public bool IsActive
        {
            get
            {
                return APIWrapper.GetElapsedFromLastInput() < (Timeout * 1000);
            }
        }
    }
}
