using focus.common.component;
using focus.lib;
using focus.utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;

namespace focus.models
{
    public class TimerApp : BaseModel
    {
        public TimerApp(IntPtr windowHandle)
        {
            Process process = API.GetProcessByWindowHandle(windowHandle);

            WindowHandle = windowHandle;
            ProcessId = process.Id;

            Image = Icon.ExtractAssociatedIcon(process.ExecutablePath())?.ToImageSource();
            AppName = process.ExecutableDescription();
        }

        ~TimerApp()
        {
            ActiveStopwatch.Reset();
        }

        private readonly Stopwatch ActiveStopwatch = new();

        public IntPtr WindowHandle { get; init; }
        public int ProcessId { get; init; }

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
                return API.GetForegroundProcess().Id == this.ProcessId;
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
