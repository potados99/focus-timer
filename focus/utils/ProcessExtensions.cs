using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace focus.utils
{
    public static class ProcessExtensions
    {
        public static string? ExecutablePath(this Process process)
        {
            if (process.Id == 0)
            {
                return null;
            }

            return process.MainModule.FileName;
        }

        public static string ExecutableDescription(this Process process)
        {
            return process.MainModule.FileVersionInfo.FileDescription;
        }
    }
}
