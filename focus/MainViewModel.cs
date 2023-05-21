using focus.common.component;
using focus.lib;
using focus.models;
using focus.utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace focus
{
    internal class MainViewModel : BaseModel
    {

        public MainViewModel()
        {
            foreach (var slot in TimerSlots)
            {
                slot.OnRegisterApplication += () => StartRegisteringApplication(slot);
                slot.OnClearApplication += () => ClearApplication(slot);
            }

            var watcher = new Watcher();

            watcher.OnFocused += (prev, current) =>
            {
                NotifyPropertyChanged(nameof(IsInAllowedWindow));
                NotifyPropertyChanged(nameof(BackgroundColor));

                if (currentRegisteringTimerSlot != null)
                {
                    FinishRegisteringApp(new TimerApp(current));
                }

                RenderTimerStatus();
            };

            watcher.StartListening();
        }

        public void Loaded()
        {
            StartTimer();

            RenderTimerStatus();
        }

        public void RenderTimerStatus()
        {
            IntPtr current = API.GetForegroundWindow();

            foreach (var slot in TimerSlots)
            {
                if (slot.IsInSameProcess(current))
                {
                    slot.ResumeStopwatch();
                }
                else
                {
                    slot.PauseStopwatch();
                }
            }

            if (IsInAllowedWindow)
            {
                this.ResumeStopwatch();
            }
            else
            {
                this.PauseStopwatch();
            }
        }

        #region 속성들

        public string ElapsedTime { get; private set; } = "00:00:00";

        public bool IsInAllowedWindow
        {
            get
            {
                IntPtr current = API.GetForegroundWindow();

                return TimerSlots.Any(s => s.IsInSameProcess(current));
            }
            set
            {

            }
        }

        public SolidColorBrush BackgroundColor { 
            get {
                Color color = IsInAllowedWindow ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 128, 0);
                return new SolidColorBrush(color);
            } 
            set {

            }
        }

        #endregion

        #region 타이머 슬롯의 등록 및 초기화

        public IEnumerable<TimerSlotViewModel> TimerSlots { get; } = new List<TimerSlotViewModel>() {
            new TimerSlotViewModel() { SlotNumber = 0 },
            new TimerSlotViewModel() { SlotNumber = 1 },
            new TimerSlotViewModel() { SlotNumber = 2 },
        };

        private TimerSlotViewModel? currentRegisteringTimerSlot = null;

        private void StartRegisteringApplication(TimerSlotViewModel slot)
        {
            if (TimerSlots.Any(s => s.IsWaitingForApp))
            {
                return;
            }

            currentRegisteringTimerSlot = slot;
            currentRegisteringTimerSlot.StartWaitingForApp();

            Debug.WriteLine($"Set slot number {slot.SlotNumber}!");

            NotifyPropertyChanged(nameof(IsInAllowedWindow));
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        private void FinishRegisteringApp(TimerApp app)
        {
            if (currentRegisteringTimerSlot != null)
            {
                currentRegisteringTimerSlot.StopWaitingAndRegisterApp(app);
                currentRegisteringTimerSlot = null;
            }

            NotifyPropertyChanged(nameof(IsInAllowedWindow));
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        private void ClearApplication(TimerSlotViewModel slot)
        {
            if (TimerSlots.Any(s => s.IsWaitingForApp))
            {
                return;
            }

            slot.ClearRegisteredApp();

            NotifyPropertyChanged(nameof(IsInAllowedWindow));
            NotifyPropertyChanged(nameof(BackgroundColor));

            Debug.WriteLine($"Clear slot number {slot.SlotNumber}!");
        }

        #endregion

        #region Main Window의 확장 및 축소

        private bool expanded = true;
        public bool Expanded
        {
            get
            {
                return expanded;
            }
            set
            {
                expanded = value;
                NotifyPropertyChanged(nameof(Expanded));
                NotifyPropertyChanged(nameof(ExpandablePartLength)); // 얘가 WindowHeight보다 먼저 바뀌어야 탈이 없습니다.
                NotifyPropertyChanged(nameof(WindowHeight));
            }
        }

        public void ToggleExpanded()
        {
            if (TimerSlots.Any(s => s.IsWaitingForApp))
            {
                return;
            }

            Expanded = !Expanded;
        }

        private readonly int windowHeight = 160;
        public int WindowHeight
        {
            get
            {
                if (Expanded)
                {
                    return windowHeight;
                }
                else
                {
                    int borderThickness = 1;
                    int separatorThickness = 1;
                    double contentGridRowLengthStar = fixedPartLength.Value;
                    double expandableGridRowLengthStar = expadedLength.Value;
                    double expandableGridRowLength = (double)windowHeight / (contentGridRowLengthStar + expandableGridRowLengthStar) * contentGridRowLengthStar;
                    return (int)expandableGridRowLength + borderThickness + separatorThickness;
                }
            }
            set
            {

            }
        }

        private GridLength fixedPartLength = new GridLength(1.4, GridUnitType.Star);
        private GridLength expadedLength = new GridLength(4, GridUnitType.Star);
        private GridLength collapsedLength = new GridLength(0);

        public GridLength FixedPartLength
        {
            get
            {
                return fixedPartLength;
            }
            set
            {

            }
        }

        public GridLength ExpandablePartLength
        {
            get
            {
                return Expanded ? expadedLength : collapsedLength;
            }
            set
            {

            }
        }

        #endregion

        #region 타이머와 UI 업데이트

        private Stopwatch stopWatch = new Stopwatch();
        private DispatcherTimer dispatcherTimer = new DispatcherTimer();

        public void StartTimer()
        {
            dispatcherTimer.Tick += new EventHandler(Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            dispatcherTimer.Start();

            stopWatch.Start();
        }

        private void Tick(object? sender, EventArgs e)
        {
            UpdateTimeLabels();
        }

        public void UpdateTimeLabels()
        {
            TimeSpan ts = stopWatch.Elapsed;
            string currentTime = String.Format("{0:00}:{1:00}:{2:00}", ts.Hours, ts.Minutes, ts.Seconds);

            ElapsedTime = currentTime;

            NotifyPropertyChanged(nameof(ElapsedTime));

            foreach (var slot in TimerSlots)
            {
                slot.RenderElapsedTime();
            }
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
    }
}
