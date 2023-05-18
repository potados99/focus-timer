using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;

namespace focus
{
    class FocusMonitor
    {
        public FocusMonitor() {
            FocusedWindow = API.GetForegroundWindow();
        }  

        public delegate void FocusedEventHandler(IntPtr prev, IntPtr current);

        public event FocusedEventHandler? OnFocused;

        public IntPtr FocusedWindow { get; private set; }

        public void StartListening()
        {
            IntPtr m_hhook = API.SetWinEventHook(API.EVENT_SYSTEM_FOREGROUND, API.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, WinEventProc, 0, 0, API.WINEVENT_OUTOFCONTEXT);
        }

        public void StopListening()
        {

        }

        private void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime) //STATIC
        {
            OnFocused?.Invoke(FocusedWindow, hwnd);

            FocusedWindow = hwnd;
        }
    }
}
