using FocusTimer.Features.Charting.Entity;
using FocusTimer.Features.Charting.Repository;
using FocusTimer.Lib;
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
        public TimerApp(IntPtr windowHandle) : this(APIWrapper.GetProcessByWindowHandle(windowHandle)?.ExecutablePath())
        { }

        public TimerApp(string? executablePath)
        {
            if (string.IsNullOrEmpty(executablePath))
            {
                throw new Exception("TimerApp의 생성자에 executablePath가 null로 들어왔습니다. 이래서는 안 됩니다!");
            }

            ProcessExecutablePath = executablePath!;

            Image = APIWrapper.ExtractAssociatedIcon(ProcessExecutablePath).ToImageSource();
            AppName = FileVersionInfo.GetVersionInfo(ProcessExecutablePath).FileDescription;
        }

        ~TimerApp()
        {
            ActiveStopwatch.Reset();
        }

        #region UI 속성들

        public string? AppName { get; set; }

        public ImageSource? Image { get; set; }

        public bool IsCountedOnConcentrationCalculation { get; set; } = true;

        public string ProcessExecutablePath { get; set; }

        public string Elapsed
        {
            get
            {
                return ActiveStopwatch.ElapsedString(TicksStartOffset);
            }
        }

        public bool IsActive
        {
            get
            {
                return APIWrapper.GetForegroundProcess()?.ExecutablePath() == ProcessExecutablePath && UserActivityMonitor.Instance.IsActive;
            }
        }

        #endregion

        public long ElapsedTicks
        {
            get
            {
                return TicksStartOffset + ActiveStopwatch.ElapsedTicks;
            }
        }

        private readonly Stopwatch ActiveStopwatch = new();

        public void Tick()
        {
            if (IsActive)
            {
                ActiveStopwatch.Start();
            }
            else
            {
                ActiveStopwatch.Stop();
            }

            UpdateUsage();
        }

        #region 사용량 집계

        private AppUsage? Usage;
        private long TicksStartOffset = 0;
        private long TicksElapsedOffset = 0;

        private void UpdateUsage()
        {
            if (Usage != null && Usage.RegisteredAt.Date < DateTime.Today)
            {
                // 날짜가 지났습니다!
                TicksElapsedOffset += Usage.Usage;
                Usage = null;
            }

            Usage ??= UsageRepository.CreateAppUsage();

            Usage.AppPath = ProcessExecutablePath;
            Usage.Usage = ElapsedTicks - TicksElapsedOffset;
            Usage.UpdatedAt = DateTime.Now;
            Usage.IsConcentrated = IsCountedOnConcentrationCalculation;

            UsageRepository.Save();
        }

        public void RestoreFromLastUsage()
        {
            Usage = UsageRepository.GetLastAppUsage(ProcessExecutablePath);

            if (Usage != null)
            {
                ActiveStopwatch.Restart();

                TicksStartOffset += Usage.Usage;
            }
        }

        #endregion
    }
}
