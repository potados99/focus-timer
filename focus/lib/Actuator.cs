using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace focus.lib
{
    internal class Actuator
    {
        public void SetFocus(IntPtr windowHandle)
        {
            API.GetWindowThreadProcessId(windowHandle, out var processId);
            string processName = Process.GetProcessById((int)processId).ProcessName;

            Debug.WriteLine($"Get back to [{processName}]!");

            API.SetForegroundWindow(windowHandle);
        }

        public void Minimize(IntPtr windowHandle)
        {
            API.ShowWindow(windowHandle, API.SW_MINIMIZE);
        }
    }
}
