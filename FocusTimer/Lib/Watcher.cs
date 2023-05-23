using System;
using System.Linq;
using System.Windows.Threading;

namespace FocusTimer.Lib
{
    class Watcher
    {
        public delegate void FocusedEventHandler(IntPtr prev, IntPtr current);

        public event FocusedEventHandler? OnFocused;

        private readonly DispatcherTimer Timer = new DispatcherTimer();

        public static readonly string[] SkipList = new string[] {
                "TaskListThumbnailWnd", // 윈도우가 여러 개일 때 작업표시줄에 표시되는 작은 썸네일
                "ForegroundStaging", // Alt + Tab으로 보이는 UI
                "MultitaskingViewFrame", // Alt + Tab으로 보이는 UI
                "Windows.UI.Core.CoreWindow", // 시작 버튼 누르면 보이는 UI
                "Shell_TrayWnd", // 작업표시줄 아이콘 우클릭하면 보이는 UI
                "ApplicationManager_DesktopShellWindow" // 새 창 띄울 때에 개입하는 UI
            };

        public IntPtr FocusedWindow { get; private set; } = IntPtr.Zero;

        public void StartListening()
        {
            Timer.Tick += (_, _) => Tick();
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 1);

            Timer.Start();
        }

        public void StopListening()
        {
            Timer.Stop();
        }

        private void Tick()
        {
            IntPtr nowFocused = APIWrapper.GetForegroundWindow();

            if (nowFocused == IntPtr.Zero)
            {
                // Null 포인터인 핸들은 취급하지 않습니다.
                return;
            }

            if (FocusedWindow == nowFocused)
            {
                // 변화가 없으면 취급하지 않습니다.
                return;
            }

            if (SkipList.Contains(APIWrapper.GetForegroundWindowClass()))
            {
                // 특정 시스템 UI는 다루지 않습니다.
                return;
            }

            OnFocused?.Invoke(FocusedWindow, nowFocused);

            FocusedWindow = nowFocused;
        }
    }
}
