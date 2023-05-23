using System.Diagnostics;

namespace FocusTimer.Lib.Utility
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
