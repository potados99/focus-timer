using System;
using System.Diagnostics;

namespace focus.lib
{
    internal class Actuator
    {
        public void SetFocus(IntPtr windowHandle)
        {
            string processName = APIWrapper.GetProcessByWindowHandle(windowHandle).ProcessName;

            Debug.WriteLine($"Get back to [{processName}]!");

            APIWrapper.SetForegroundWindow(windowHandle);
        }

        public void Minimize(IntPtr windowHandle)
        {
            APIWrapper.MinimizeWindow(windowHandle);
        }
    }
}
