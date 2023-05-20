using focus.models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using focus.utils;

namespace focus
{
    internal class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public TimerApp? TimerSlot1
        {
            get
            {
                return new TimerApp { Image = Icon.ExtractAssociatedIcon("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe").ToImageSource(), AppName = "Google Chrome", Elapsed = "01:26:31" };
            }
        }

        public TimerApp? TimerSlot2
        {
            get
            {
                return new TimerApp { Image = Icon.ExtractAssociatedIcon("C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\Common7\\IDE\\devenv.exe").ToImageSource(), AppName = "Microsoft Visual Studio", Elapsed = "05:42:32" };
            }
        }

        public TimerApp? TimerSlot3
        {
            get
            {
                return null;
            }
        }

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
