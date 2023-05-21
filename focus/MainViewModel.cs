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
                if (CurrentRegisteringTimerSlot != null)
                {
                    FinishRegisteringApp(new TimerApp(current));
                }

                RenderAll();
            };

            watcher.StartListening();
        }

        public void Loaded()
        {
            StartTimer();

            RenderAll();
        }

        #region 속성들

        public string ElapsedTime
        {
            get
            {
                return ActiveStopwatch.ElapsedString();
            }
        }

        public bool IsAnyAppActive
        {
            get
            {
                return TimerSlots.Any(s => s.IsAppActive);
            }
        }

        public SolidColorBrush BackgroundColor { 
            get {
                Color color = IsAnyAppActive ? Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 128, 0);
                return new SolidColorBrush(color);
            } 
        }

        #endregion

        #region 타이머 슬롯의 등록 및 초기화

        public IEnumerable<TimerSlotViewModel> TimerSlots { get; } = new List<TimerSlotViewModel>() {
            new TimerSlotViewModel() { SlotNumber = 0 },
            new TimerSlotViewModel() { SlotNumber = 1 },
            new TimerSlotViewModel() { SlotNumber = 2 },
        };

        private TimerSlotViewModel? CurrentRegisteringTimerSlot
        {
            get
            {
                return TimerSlots.FirstOrDefault(s => s.IsWaitingForApp);
            }
        }

        private void StartRegisteringApplication(TimerSlotViewModel slot)
        {
            if (CurrentRegisteringTimerSlot != null)
            {
                return;
            }

            slot.StartWaitingForApp();

            Render();
        }

        private void FinishRegisteringApp(TimerApp app)
        {
            CurrentRegisteringTimerSlot?.StopWaitingAndRegisterApp(app);

            Render();
        }

        private void ClearApplication(TimerSlotViewModel slot)
        {
            if (CurrentRegisteringTimerSlot != null)
            {
                return;
            }

            slot.ClearRegisteredApp();

            Render();
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

        private Stopwatch ActiveStopwatch = new Stopwatch();
        private DispatcherTimer DispatchTimer = new DispatcherTimer();

        public void StartTimer()
        {
            DispatchTimer.Tick += (_, _) => RenderAll();
            DispatchTimer.Interval = new TimeSpan(0, 0, 0, 1);
            DispatchTimer.Start();

            ActiveStopwatch.Start();
        }

        public void RenderAll()
        {
            Render();

            foreach (var slot in TimerSlots)
            {
                slot.Render();
            }
        }

        public void Render()
        {
            if (IsAnyAppActive)
            {
                ActiveStopwatch.Start();
            }
            else
            {
                ActiveStopwatch.Stop();
            }

            NotifyPropertyChanged(nameof(ElapsedTime));
            NotifyPropertyChanged(nameof(IsAnyAppActive));
            NotifyPropertyChanged(nameof(BackgroundColor));
        }

        #endregion
    }
}
