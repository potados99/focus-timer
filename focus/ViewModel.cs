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

namespace focus
{
    internal class ViewModel : INotifyPropertyChanged
    {

        public ViewModel()
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<TimerSlotItem> TimerSlots { get; } = new List<TimerSlotItem>() {
            new TimerSlotItem() { SlotNumber = 0, CurrentApp = null },
            new TimerSlotItem() { SlotNumber = 1, CurrentApp = null },
            new TimerSlotItem() { SlotNumber = 2, CurrentApp = null },
        };

        private bool controlVisible = true;
        public bool ControlVisible
        {
            get
            {
                return controlVisible;
            }

            set
            {
                controlVisible = value;
                NotifyPropertyChanged("ControlVisible");
                NotifyPropertyChanged("WindowHeight");
            }
        }

        private readonly int windowHeight = 130;
        public int WindowHeight
        {
            get
            {
                if (ControlVisible)
                {
                    return windowHeight;
                }
                else
                {
                    int borderThickness = 2;
                    double contentGridRowLengthStar = 1;
                    double collapableGridRowLengthStar = BoolToGridRowHeightConverter.CollapsableGridRowLengthStar;
                    double collapableGridRowLength = (double)windowHeight / (contentGridRowLengthStar + collapableGridRowLengthStar) * contentGridRowLengthStar;
                    return (int)collapableGridRowLength + borderThickness;
                }
            }

            set
            {

            }
        }

    }
}
