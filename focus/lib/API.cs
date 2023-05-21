using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace focus.lib
{
    class API
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public const int WM_MOVING = 0x0216;

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;

        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_SYSTEM_FOREGROUND = 3;

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        public const int SW_MAXIMIZE = 3;
        public const int SW_MINIMIZE = 6;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        public static string GetClassName(IntPtr windowHandle)
        {
            StringBuilder builder = new StringBuilder(256);

            GetClassName(windowHandle, builder, builder.Capacity);

            return builder.ToString();
        }

        public static Process GetProcessByWindowHandle(IntPtr windowHandle)
        {
            GetWindowThreadProcessId(windowHandle, out var processId);

            return Process.GetProcessById((int)processId);
        }

        public static Process GetForegroundProcess()
        {
            return GetProcessByWindowHandle(GetForegroundWindow());
        }


    }
}
