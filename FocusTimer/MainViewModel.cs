﻿using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Models;
using FocusTimer.Lib.Utility;
using FocusTimer.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace FocusTimer
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

            Watcher watcher = new();

            watcher.OnFocused += (prev, current) =>
            {
                if (CurrentRegisteringTimerSlot != null)
                {
                    FinishRegisteringApp(new TimerApp(current));
                }

                RestoreFocusIfNeeded(prev, current);

                RenderAll();
            };

            watcher.StartListening();
        }

        public void Loaded()
        {
            RestoreApps();
            StartTimer();
            RenderAll();
        }

        #region UI 속성들

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

        public SolidColorBrush? TimerLabelForegroundColor
        {
            get
            {
                string resourceName = IsAnyAppActive ? "TextWhite" : "DimmedGray";

                return Application.Current.FindResource(resourceName) as SolidColorBrush;
            }
        }

        public Visibility IsWarningBorderVisible
        {
            get
            {
                return !IsFocusLocked || IsAnyAppActive ? Visibility.Hidden : Visibility.Visible;
            }
        }

        public bool IsFocusLocked { get; set; } = false;

        public DrawingImage? LockImage
        {
            get
            {
                string resourceName = IsFocusLocked ? "ic_lock" : "ic_lock_open_outline";

                return Application.Current.FindResource(resourceName) as DrawingImage;
            }
        }

        public string Concentration
        {
            get
            {
                if (AlwaysOnStopwatch.ElapsedMilliseconds == 0)
                {
                    return "0%";
                }

                double concentration = 100 * ActiveStopwatch.ElapsedMilliseconds / AlwaysOnStopwatch.ElapsedMilliseconds;

                return concentration + "%";
            }
        }

        public int FocusLockHoldDuration
        {
            get
            {
                return Settings.GetFocusLockHoldDuration();
            }
            set
            {
                Settings.SetFocusLockHoldDuration(value);

                // 양방향 바인딩되는 속성으로, UI에 의해 변경시 여기에서 NotifyPropertyChanged를 트리거해요.
                NotifyPropertyChanged(nameof(FocusLockHoldDuration));
            }
        }

        public bool IsFocusLockHold
        {
            get
            {
                return FocusLockTimer.IsEnabled;
            }
        }

        private readonly ToolTip _LockButtonToolTip = new();
        public ToolTip? LockButtonToolTip
        {
            get
            {
                _LockButtonToolTip.Content = $"{(int)Math.Ceiling(FocusLockTimeSpan.TotalMinutes)}분 남았습니다.";

                return IsFocusLockHold ? _LockButtonToolTip : null;     
            }
        }

        #endregion

        #region 타이머 슬롯의 등록 및 초기화

        public List<TimerSlotViewModel> TimerSlots { get; } = new List<TimerSlotViewModel>() {
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

            SaveApps();
            Render();
        }

        private void ClearApplication(TimerSlotViewModel slot)
        {
            if (CurrentRegisteringTimerSlot != null)
            {
                return;
            }

            slot.ClearRegisteredApp();

            SaveApps();
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
                // 왜인지는 모르겠는데 이 WindowHeight은 양방향 바인딩으로 넣어 주어야 잘 돌아갑니다...
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
        private Stopwatch AlwaysOnStopwatch = new Stopwatch();

        private DispatcherTimer DispatchTimer = new DispatcherTimer();

        public void StartTimer()
        {
            DispatchTimer.Tick += (_, _) => RenderAll();
            DispatchTimer.Interval = new TimeSpan(0, 0, 0, 1);
            DispatchTimer.Start();

            AlwaysOnStopwatch.Start();
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
            NotifyPropertyChanged(nameof(TimerLabelForegroundColor));
            NotifyPropertyChanged(nameof(IsWarningBorderVisible));

            NotifyPropertyChanged(nameof(IsFocusLocked));
            NotifyPropertyChanged(nameof(IsFocusLockHold));
            NotifyPropertyChanged(nameof(LockImage));
            NotifyPropertyChanged(nameof(LockButtonToolTip));

            NotifyPropertyChanged(nameof(Concentration));
        }

        #endregion

        #region 포커스 잠금 

        private DispatcherTimer FocusLockTimer = new();
        private TimeSpan FocusLockTimeSpan = new();

        public void ToggleFocusLock()
        {
            if (IsFocusLocked)
            {
                UnlockFocus();
            } else
            {
                LockFocus();
            }

            Render();
        }

        private void LockFocus()
        {
            //FocusLockTimeSpan = new TimeSpan(0, FocusLockHoldDuration, 0);
            FocusLockTimeSpan = new TimeSpan(0, 0, 2);
            FocusLockTimer.Interval = TimeSpan.FromSeconds(1);
            FocusLockTimer.Tick += (_, _) => {
                if (FocusLockTimeSpan == TimeSpan.Zero || FocusLockTimeSpan.TotalSeconds <= 0)
                {
                    FocusLockTimer.Stop();
                    UnlockFocus();
                } else
                {
                    FocusLockTimeSpan = FocusLockTimeSpan.Add(TimeSpan.FromSeconds(-1));
                }

                Render();
            };
            FocusLockTimer.Start();

            IsFocusLocked = true;
            StartAnimation("LockingAnimation");
        }

        private void UnlockFocus()
        {
            if (IsFocusLockHold)
            {
                _LockButtonToolTip.IsOpen = true;
                StartAnimation("ShakeHorizontalAnimation");

                return;
            }

            IsFocusLocked = false;
            StartAnimation("UnlockingAnimation");
        }

        private void StartAnimation(string name)
        {
            Storyboard? sb = Application.Current.MainWindow.Resources[name] as Storyboard;
            sb?.Begin();
        }

        private void RestoreFocusIfNeeded(IntPtr prev, IntPtr current)
        {
            if (!IsFocusLocked)
            {
                // 포커스 잠금이 없으면 아무 것도 하지 않습니다.
                return;
            }

            if (IsAnyAppActive)
            {
                // 등록된 앱 중 활성화된 앱이 있으면 정상적인 상태입니다.
                return;
            }

            if (Watcher.SkipList.Contains(APIWrapper.GetForegroundWindowClass()))
            {
                // 현재 포커스가 시스템 UI로 가 있다면 넘어갑니다.
                return;
            }

            if (APIWrapper.IsThisProcessForeground())
            {
                // 현재 포커스가 이 프로세스라면 넘어갑니다.
                return;
            }

            // 위 아무 조건에도 해당하지 않았다면
            // 포커스를 빼앗아야 할 때입니다.
            // 포커스를 이전 앱에 주고 현재 앱을 줄입니다.
            Task.Delay(200).ContinueWith(_ =>
            {
                APIWrapper.MinimizeWindow(current);
                APIWrapper.SetForegroundWindow(prev);
            });
        }

        #endregion

        #region 저장과 복구

        public void SaveApps()
        {
            Settings.SetApps(TimerSlots.Select(s => s.GetAppExecutablePath()).ToList());
        }

        public void RestoreApps()
        {
            foreach (var (app, index) in Settings.GetApps().WithIndex())
            {
                TimerSlots[index].RestoreApp(app);
            }
        }

        #endregion
    }
}
