using focus.lib;
using focus.utils;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;

namespace focus.models
{
    public class TimerApp
    {
        public TimerApp(IntPtr windowHandle)
        {
            this.WindowHandle = windowHandle;

            API.GetWindowThreadProcessId(windowHandle, out var processId);

            this.ProcessId = (int)processId;

            Process proc = Process.GetProcessById((int)processId);
            string filePath = proc.MainModule.FileName;
            string process_description = proc.MainModule.FileVersionInfo.FileDescription;

            this.Image = Icon.ExtractAssociatedIcon(filePath).ToImageSource();
            this.AppName = process_description;
            this.Elapsed = "00:00:00";
        }

        public IntPtr WindowHandle { get; set; }
        public int ProcessId { get; set; }

        public ImageSource Image { get; set; }
        public string AppName { get; set; }
        public string Elapsed { get; set; }

        public bool IsInSameProcess(IntPtr windowHandle)
        {
            API.GetWindowThreadProcessId(windowHandle, out var processId);

            return processId == this.ProcessId;
        }
    }
}
