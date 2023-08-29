﻿using System.Windows.Media;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.Timer.Slot;

public partial class AppItem 
{
    public string ProcessExecutablePath => App.ExecutablePath;

    public string AppName => App.Title;

    public ImageSource Image => App.Icon.ToImageSource();

    public bool IsCountedOnConcentrationCalculation { get; set; } = true;

    public bool IsActive => APIWrapper.GetForegroundProcess()?.ExecutablePath() == ProcessExecutablePath &&
                            _activityMonitor.IsActive;
    
}