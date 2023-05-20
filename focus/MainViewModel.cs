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
                slot.OnSet += () => SetTimerSlot(slot);
                slot.OnClear += () => ClearTimerSlot(slot);
            }

            var watcher = new Watcher();

            watcher.OnFocused += (prev, current) =>
            {
               if (currentSettingTimerSlot != null)
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

                    // TODO 하나의 메소드 호출로
                    currentSettingTimerSlot.CurrentApp = app;
                    currentSettingTimerSlot.WaitingForApp = false;

                    currentSettingTimerSlot = null;
                }
            };

            watcher.StartListening();
        }

        private TimerSlotItem? currentSettingTimerSlot = null;

        private void SetTimerSlot(TimerSlotItem slot)
        {
            slot.WaitingForApp = true;
            currentSettingTimerSlot = slot;

            Debug.WriteLine($"Set slot number {slot.SlotNumber}!");
        }

        private void ClearTimerSlot(TimerSlotItem slot)
        {
            slot.CurrentApp = null;

            Debug.WriteLine($"Clear slot number {slot.SlotNumber}!");
        }

        public IEnumerable<TimerSlotItem> TimerSlots { get; } = new List<TimerSlotItem>() {
            new TimerSlotItem() { SlotNumber = 0, CurrentApp = null },
            new TimerSlotItem() { SlotNumber = 1, CurrentApp = null },
            new TimerSlotItem() { SlotNumber = 2, CurrentApp = null },
        };


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

        private readonly int windowHeight = 130;
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
                    double contentGridRowLengthStar = 1;
                    double expandableGridRowLengthStar = 3;
                    double expandableGridRowLength = (double)windowHeight / (contentGridRowLengthStar + expandableGridRowLengthStar) * contentGridRowLengthStar;
                    return (int)expandableGridRowLength + borderThickness + separatorThickness;
                }
            }
            set
            {

            }
        }

        public GridLength ExpandablePartLength
        {
            get
            {
                return Expanded ? new GridLength(3, GridUnitType.Star) : new GridLength(0);
            }
            set
            {

            }
        }
    }
}
