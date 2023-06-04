using FocusTimer.Lib.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Lib
{
    public class UserActivityMonitor
    {
        public static UserActivityMonitor Instance = new();

        public delegate void StateChangeHandler();
        public event StateChangeHandler? OnActivated;
        public event StateChangeHandler? OnDeactivated;

        private readonly CountdownTimer Timer = new();

        public int Timeout { get; set; }

        public UserActivityMonitor()
        {
            Timeout = Settings.GetActivityTimeout();

            APIWrapper.HookKeyboard(() => { StartTimer(); });
            APIWrapper.HookMouse(() => { StartTimer(); });

            SetupTimer();
            StartTimer();
        }

        private void SetupTimer()
        {
            Timer.OnFinish += () => { OnDeactivated?.Invoke(); };
        }

        private void StartTimer()
        {
            if (!Timer.IsEnabled)
            {
                OnActivated?.Invoke();
            }

            Timer.Stop();
            Timer.Duration = TimeSpan.FromSeconds(Timeout);
            Timer.Start();
        }

        public bool IsActive
        {
            get
            {
                return Timer.IsEnabled;
            }
        }
    }
}
