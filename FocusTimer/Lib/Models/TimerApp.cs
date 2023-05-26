using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;

namespace FocusTimer.Lib.Models
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

        public string ProcessExecutablePath { get; set; }

        public ImageSource? Image { get; set; }
        public string? AppName { get; set; }
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
                return APIWrapper.GetForegroundProcess().ExecutablePath() == ProcessExecutablePath;
            }
        }

        public void Render()
        {
            if (IsAppActive)
            {
                ActiveStopwatch.Start();
            }
            else
            {
                ActiveStopwatch.Stop();
            }
        }
    }
}
