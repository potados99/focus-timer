using FocusTimer.Lib;
using FocusTimer.Lib.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Media;

namespace FocusTimer.Features.Charting
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
        public ImageSource? AppIcon
        {
            get
            {
                return APIWrapper.ExtractAssociatedIcon(AppPath)?.ToImageSource();
            }
        }
        public string AppName
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(AppPath).FileDescription;
            }
        }

        public System.Windows.Media.Brush AppColor
        {
            get
            {
                return new SolidColorBrush(APIWrapper.ExtractAssociatedIcon(AppPath).ToColor());
            }
        }

        public string UsageString { get; set; }

        public IEnumerable<UsageByTimeItem> UsagesByTime { get; set; }
    }
}
