using System.ComponentModel;
using System.Windows;

namespace focus.models
{
    public class TimerSlotItem : INotifyPropertyChanged
    {
        public delegate void SetHandler();
        public delegate void ClearHandler();

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int SlotNumber { get; set; }

        private TimerApp? currentApp;
        public TimerApp? CurrentApp
        {
            get
            {
                return currentApp;
            }
            set
            {
                this.currentApp = value;

                NotifyPropertyChanged(nameof(CurrentApp));

                NotifyPropertyChanged(nameof(IsAppVisible));
                NotifyPropertyChanged(nameof(IsSetButtonVisible));
                NotifyPropertyChanged(nameof(IsWaitLabelVisible));
            }
        }

        private bool waitingForApp { get; set; } = false;

        public bool WaitingForApp
        {
            get
            {
                return waitingForApp;
            }
            set
            {
                this.waitingForApp = value;

                NotifyPropertyChanged(nameof(IsAppVisible));
                NotifyPropertyChanged(nameof(IsSetButtonVisible));
                NotifyPropertyChanged(nameof(IsWaitLabelVisible));
            }
        }

        public event SetHandler? OnSet;
        public event ClearHandler? OnClear;

        public void Set() => OnSet?.Invoke();
        public void Clear() => OnClear?.Invoke();



        public Visibility IsAppVisible
        {
            get
            {
                return !WaitingForApp && currentApp != null ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility IsSetButtonVisible
        {
            get
            {
                return !WaitingForApp && currentApp == null ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility IsWaitLabelVisible
        {
            get
            {
                return WaitingForApp ? Visibility.Visible : Visibility.Hidden;
            }
        }
    }
}
