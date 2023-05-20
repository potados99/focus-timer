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
using System.Drawing;
using System.Security.Cryptography;
using System.Windows;

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
               if (currentRegisteringTimerSlot != null)
                {

                    API.GetWindowThreadProcessId(current, out var processId);

                    Process proc = Process.GetProcessById((int)processId);
                    string filePath = proc.MainModule.FileName;
                    string process_description = proc.MainModule.FileVersionInfo.FileDescription;

                    var app = new TimerApp { 
                        Image = Icon.ExtractAssociatedIcon(filePath).ToImageSource(),
                        AppName = process_description, 
                        Elapsed = "01:26:31" 
                    };

                    FinishRegisteringApp(app);
                }
            };

            watcher.StartListening();
        }

        #region 타이머 슬롯의 등록 및 초기화

        public IEnumerable<TimerSlotModel> TimerSlots { get; } = new List<TimerSlotModel>() {
            new TimerSlotModel() { SlotNumber = 0 },
            new TimerSlotModel() { SlotNumber = 1 },
            new TimerSlotModel() { SlotNumber = 2 },
        };

        private TimerSlotModel? currentRegisteringTimerSlot = null;

        private void StartRegisteringApplication(TimerSlotModel slot)
        {
            currentRegisteringTimerSlot = slot;
            currentRegisteringTimerSlot.StartWaitingForApp();

            Debug.WriteLine($"Set slot number {slot.SlotNumber}!");
        }

        private void FinishRegisteringApp(TimerApp app)
        {
            if (currentRegisteringTimerSlot != null)
            {
                currentRegisteringTimerSlot.StopWaitingAndRegisterApp(app);
                currentRegisteringTimerSlot = null;
            }
        }

        private void ClearApplication(TimerSlotModel slot)
        {
            slot.ClearRegisteredApp();

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
    }
}
