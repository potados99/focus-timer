using focus.common.component;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace focus.models
{
    public class TimerSlotViewModel : BaseModel
    {
        #region 이벤트 핸들러 정의와 구현

        public delegate void RegisterHandler();
        public delegate void ClearHandler();

        public event RegisterHandler? OnRegisterApplication;
        public event ClearHandler? OnClearApplication;

        public void FireAppRegisterEvent() => OnRegisterApplication?.Invoke();
        public void FireAppClearEvent() => OnClearApplication?.Invoke();

        #endregion

        #region 속성들

        public int SlotNumber { get; set; }

        public TimerApp? CurrentApp { get; private set; } = null;
        public bool IsWaitingForApp { get; private set; } = false;

        public Visibility IsAppVisible
        {
            get
            {
                return !IsWaitingForApp && CurrentApp != null ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility IsSetButtonVisible
        {
            get
            {
                return !IsWaitingForApp && CurrentApp == null ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public Visibility IsWaitLabelVisible
        {
            get
            {
                return IsWaitingForApp ? Visibility.Visible : Visibility.Hidden;
            }
        }

        #endregion

        #region 외부에 노출하는 제어용 메소드

        public void StartWaitingForApp()
        {
            CurrentApp = null;
            IsWaitingForApp = true;

            Render();
        }

        public void StopWaitingAndRegisterApp(TimerApp app)
        {
            CurrentApp = app;
            IsWaitingForApp = false;
            stopWatch.Reset();
            stopWatch.Start();

            Render();
        }

        public void ClearRegisteredApp()
        {
            CurrentApp = null;

            Render();
        }

        private void Render()
        {

            NotifyPropertyChanged(nameof(CurrentApp));

            NotifyPropertyChanged(nameof(IsAppVisible));
            NotifyPropertyChanged(nameof(IsSetButtonVisible));
            NotifyPropertyChanged(nameof(IsWaitLabelVisible));
        }

        public bool IsInSameProcess(IntPtr windowHandle)
        {
            return CurrentApp != null && CurrentApp.IsInSameProcess(windowHandle);
        }

        public void PauseStopwatch()
        {
            stopWatch.Stop();
        }

        public void ResumeStopwatch()
        {
            stopWatch.Start();
        }

        #endregion

        #region 스탑워치

        private Stopwatch stopWatch = new Stopwatch();

        public void RenderElapsedTime()
        {
            if (CurrentApp != null)
            {
                TimeSpan ts = stopWatch.Elapsed;
                string currentTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

                CurrentApp.Elapsed = currentTime;

                NotifyPropertyChanged(nameof(CurrentApp));
            }
        }

        #endregion
    }
}
