using FocusTimer.Lib.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace FocusTimer.Charting
{
    public class AppUsageItem
    {
        public DateTime Date { get; set; }
        public string DateString
        {
            get
            {
                return Date.ToString();
            }
        }

        public string AppPath { get; set; }
        public ImageSource? AppIcon { 
            get
            {
                return Icon.ExtractAssociatedIcon(AppPath)?.ToImageSource();
            }
        }
        public string AppName
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(AppPath).FileDescription;
            }
        }

        public string UsageString { get; set; }

        public IEnumerable<UsageByTimeItem> UsagesByTime { get; set; }
    }
}
