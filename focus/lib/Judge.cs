using System;
using System.Diagnostics;
using System.Linq;

namespace focus.lib
{
    internal class Judge
    {
        internal enum Result
        {
            ALLOW, BLOCK
        }

        private readonly Settings settings;

        public Judge(Settings settings)
        {
            this.settings = settings;
        }

        public Result Decide(IntPtr windowHandle)
        {
            API.GetWindowThreadProcessId(windowHandle, out var processId);
            string processName = Process.GetProcessById((int)processId).ProcessName;

            Debug.WriteLine($"[{API.GetClassName(windowHandle)}] of [{processName}]");

            if (settings.GetWindowClassAllowList().Contains(API.GetClassName(windowHandle)))
            {
                Debug.WriteLine($"OK, [{API.GetClassName(windowHandle)}] is allowed.");

                return Result.ALLOW;
            }
            else
            {
                Debug.WriteLine($"No, [{API.GetClassName(windowHandle)}] is not allowed!");

                return Result.BLOCK;
            }

            return Result.ALLOW;
            /*
            API.GetWindowThreadProcessId(windowHandle, out var processId);
            string processName = Process.GetProcessById((int)processId).ProcessName;

            if (settings.GetProcessAllowList().Contains(processName))
            {
                Debug.WriteLine($"OK, [{processName}] is allowed.");

                return Result.ALLOW;
            } else
            {
                Debug.WriteLine($"No, [{processName}] is not allowed!");

                return Result.BLOCK;
            }
            */
        }
    }
}
