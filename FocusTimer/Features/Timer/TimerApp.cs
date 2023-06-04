using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;

namespace FocusTimer.Features.Timer
{
    /// <summary>
    /// 타이머 슬롯에 등록되는 앱을 나타냅니다.
    /// 타이머 슬롯에는 앱이 등록되어 있을 수도, 그렇지 않을 수도 있습니다.
    /// </summary>
    public class TimerApp
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

        public bool IsCountedOnConcentrationCalculation { get; set; } = true;

        public string Elapsed
        {
            get
            {
                return ActiveStopwatch.ElapsedString();
            }
        }

        public long ElapsedMilliseconds
        {
            get
            {
                return ActiveStopwatch.ElapsedMilliseconds;
            }
        }

        public bool IsActive
        {
            get
            {
                return APIWrapper.GetForegroundProcess().ExecutablePath() == ProcessExecutablePath && UserActivityMonitor.Instance.IsActive;
            }
        }

        public void Render()
        {
            if (IsActive)
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
