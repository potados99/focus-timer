using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            Debug.WriteLine(API.GetClassName(windowHandle));

            return Result.ALLOW;

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
        }
    }
}
