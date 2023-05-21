using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace focus.lib
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

        public static void SetForegroundWindow(IntPtr windowHandle)
        {
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

        #endregion
    }
}
