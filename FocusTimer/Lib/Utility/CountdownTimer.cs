using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FocusTimer.Lib.Utility
{
    public class CountdownTimer
    {
        public delegate void TickHandler();
        public event TickHandler? OnFinish;

        public TimeSpan Duration { get; set; }

        public bool IsEnabled
        {
            get
            {
                return this.Timer.IsEnabled;
            }
        }

        private readonly DispatcherTimer Timer = new();

        public TimeSpan TimeLeft { get; private set; }

        public CountdownTimer() {
            Prepare();
        }

        private void Prepare()
        {
            Timer.Tick += (_, _) =>
            {
                this.TimeLeft = this.TimeLeft.Subtract(TimeSpan.FromSeconds(1));

                if (this.TimeLeft == TimeSpan.Zero || this.TimeLeft.TotalSeconds <= 0)
                {
                    this.Timer.Stop();
                    this.OnFinish?.Invoke();
                    return;
                }
            };
            Timer.Interval = TimeSpan.FromSeconds(1);
        }

        public void Start()
        {
            this.TimeLeft = new TimeSpan(Duration.Ticks);
            this.Timer.Start();
        }

        public void Stop()
        {
            this.Timer.Stop();
        }
    }
}
