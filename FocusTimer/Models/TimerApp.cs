using FocusTimer.Common.Component;
using FocusTimer.Lib;
using FocusTimer.Utility;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;

namespace FocusTimer.Models
{
    public class TimerApp : BaseModel
    {
        public TimerApp(IntPtr windowHandle)
        {
            Process process = APIWrapper.GetProcessByWindowHandle(windowHandle);

            WindowHandle = windowHandle;
            ProcessId = process.Id;
            ProcessExecutablePath = process.ExecutablePath();

            Image = Icon.ExtractAssociatedIcon(ProcessExecutablePath)?.ToImageSource();
            AppName = process.ExecutableDescription();
        }

        ~TimerApp()
        {
            ActiveStopwatch.Reset();
        }

        private readonly Stopwatch ActiveStopwatch = new();

        public IntPtr WindowHandle { get; init; }
        public int ProcessId { get; init; }
        public string ProcessExecutablePath { get; init; }

        public ImageSource? Image { get; init; }
        public string AppName { get; init; }
        public string Elapsed
        {
            get
            {
                return ActiveStopwatch.ElapsedString();
            }
        }

        public bool IsAppActive
        {
            get
            {
                return APIWrapper.GetForegroundProcess().ExecutablePath() == this.ProcessExecutablePath;
            }
        }

        public void Render()
        {
            if (IsAppActive)
            {
                ActiveStopwatch.Start();
            } else
            {
                ActiveStopwatch.Stop();
            }
        }
    }
}
