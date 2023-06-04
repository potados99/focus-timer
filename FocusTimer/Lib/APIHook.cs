using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Lib
{
    public class APIHook
    {
        public delegate void Callback(int code, IntPtr wParam, IntPtr lParam);
        public event Callback? HookProc;

        private readonly int IdHook;
        private API.HookProc? Hook; // 멤버 참조로 계속 데리고 있어야 GC 안 당합니다.

        private long CallCount = 0;

        public APIHook(int idHook)
        {
            this.IdHook = idHook;
        }

        public void Install()
        {
            Hook = (int code, IntPtr lParam, IntPtr wParam) =>
            {
                HookProc?.Invoke(code, lParam, wParam);

                return API.CallNextHookEx(IntPtr.Zero, code, lParam, wParam);
            };

            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;

            API.SetWindowsHookEx(IdHook, Hook, API.GetModuleHandle(curModule.ModuleName), 0);
        }
    }
}
