using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

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

        #region 입력

        public static long GetElapsedFromLastInput()
        {
            API.LASTINPUTINFO lastInputInfo = new API.LASTINPUTINFO();
            lastInputInfo.cbSize = Marshal.SizeOf(lastInputInfo);
            lastInputInfo.dwTime = 0;

            API.GetLastInputInfo(out lastInputInfo);

            int lastInputTick = lastInputInfo.dwTime;

            return Environment.TickCount - lastInputTick;
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

        public static String GetProcessFilename(Process p)
        {
            int capacity = 2000;
            StringBuilder builder = new(capacity);

            // Process.MainModule은 OpenProcess 호출에
            // access 인자로 0x0410을 넘깁니다.
            // 이는 PROCESS_QUERY_INFORMATION 과 PROCESS_VM_READ 이 합쳐진 것인데, (https://learn.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights)
            // 대상 프로세스가 상승된 권한으로 실행되고 있을 때에 문제를 일으킵니다. (https://stackoverflow.com/questions/9501771/how-to-avoid-a-win32-exception-when-accessing-process-mainmodule-filename-in-c#comment96065027_34991822)
            // 따라서 PROCESS_QUERY_LIMITED_INFORMATION access를 사용합니다.
            IntPtr ptr = API.OpenProcess(API.ProcessAccessFlags.QueryLimitedInformation, false, p.Id);
            if (!API.QueryFullProcessImageName(ptr, 0, builder, ref capacity))
            {
                return String.Empty;
            }

            return builder.ToString();
        }

        #endregion
    }
}
