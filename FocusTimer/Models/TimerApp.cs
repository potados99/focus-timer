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
        public TimerApp(IntPtr windowHandle) : this(APIWrapper.GetProcessByWindowHandle(windowHandle).ExecutablePath())
        {

        }

        public TimerApp(string? executablePath)
        {
            if (string.IsNullOrEmpty(executablePath))
            {
                throw new Exception("TimerApp의 생성자에 executablePath가 null로 들어왔습니다. 이래서는 안 됩니다!");
            }

            ProcessExecutablePath = executablePath;

            Image = Icon.ExtractAssociatedIcon(executablePath)?.ToImageSource();
            AppName = FileVersionInfo.GetVersionInfo(executablePath).FileDescription;
        }

        ~TimerApp()
        {
            ActiveStopwatch.Reset();
        }

        private readonly Stopwatch ActiveStopwatch = new();

        public string ProcessExecutablePath { get; init; }

        public ImageSource? Image { get; init; }
        public string? AppName { get; init; }
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
