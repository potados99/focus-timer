using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace FocusTimer.Lib
{
    public static class APIWrapper
    {
        #region 창

        public static string GetClassName(IntPtr windowHandle)
        {
            StringBuilder builder = new StringBuilder(256);

            API.GetClassName(windowHandle, builder, builder.Capacity);

            return builder.ToString();
        }

        public static IntPtr GetForegroundWindow()
        {
            return API.GetForegroundWindow();
        }

        public static string GetForegroundWindowClass()
        {
            return GetClassName(GetForegroundWindow());
        }

        public static void SetForegroundWindow(IntPtr windowHandle)
        {
            // ALT 키를 눌러주지 않으면, 창이 전면으로 오지는 않고
            // 작업 표시줄에서 반짝이기만 합니다.
            // 이 증상은 Visual Studio와 attach되었을 때에는
            // 나타나지 않습니다.
            // 해결책: https://stackoverflow.com/questions/10740346/setforegroundwindow-only-working-while-visual-studio-is-open

            // ALT를 눌러 줍니다.
            API.keybd_event((byte)API.ALT, 0x45, API.EXTENDEDKEY | 0, 0);

            // 누른 ALT를 다시 뗍니다.
            API.keybd_event((byte)API.ALT, 0x45, API.EXTENDEDKEY | API.KEYUP, 0);

            API.SetForegroundWindow(windowHandle);
        }

        public static void MinimizeWindow(IntPtr windowHandle)
        {
            API.ShowWindow(windowHandle, API.SW_MINIMIZE);
        }

        #endregion

        #region 프로세스

        public static Process GetProcessByWindowHandle(IntPtr windowHandle)
        {
            API.GetWindowThreadProcessId(windowHandle, out var processId);

            return Process.GetProcessById((int)processId);
        }

        public static Process GetForegroundProcess()
        {
            return GetProcessByWindowHandle(GetForegroundWindow());
        }

        public static bool IsThisProcessForeground()
        {
            return GetForegroundProcess().Id == Process.GetCurrentProcess().Id;
        }

        #endregion
    }
}
