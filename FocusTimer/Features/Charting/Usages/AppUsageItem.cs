using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Media;
using FocusTimer.Features.Charting.Usages.Detail;
using FocusTimer.Lib;
using FocusTimer.Lib.Extensions;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Charting.Usages;

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