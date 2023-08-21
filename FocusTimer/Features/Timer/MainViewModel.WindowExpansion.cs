using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Features.License;
using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
       #region Main Window의 확장 및 축소

    private bool expanded = true;
    public bool Expanded
    {
        get
        {
            return expanded;
        }
        set
        {
            expanded = value;
            NotifyPropertyChanged(nameof(Expanded));
            NotifyPropertyChanged(nameof(ExpandablePartLength)); // 얘가 WindowHeight보다 먼저 바뀌어야 탈이 없습니다.
            NotifyPropertyChanged(nameof(WindowHeight));
        }
    }

    public void ToggleExpanded()
    {
        if (TimerSlots.Any(s => s.IsWaitingForApp))
        {
            return;
        }

        Expanded = !Expanded;
    }

    private readonly int windowHeight = 40 * (1 + 5);
    public int WindowHeight
    {
        get
        {
            if (Expanded)
            {
                return windowHeight;
            }
            else
            {
                int borderThickness = 1;
                int separatorThickness = 1;
                double contentGridRowLengthStar = fixedPartLength.Value;
                double expandableGridRowLengthStar = expadedLength.Value;
                double expandableGridRowLength = windowHeight / (contentGridRowLengthStar + expandableGridRowLengthStar) * contentGridRowLengthStar;
                return (int)expandableGridRowLength + borderThickness + separatorThickness;
            }
        }
        set
        {
            // 왜인지는 모르겠는데 이 WindowHeight은 양방향 바인딩으로 넣어 주어야 잘 돌아갑니다...
        }
    }

    private GridLength fixedPartLength = new GridLength(1.4, GridUnitType.Star);
    private GridLength expadedLength = new GridLength(1 + 5, GridUnitType.Star);
    private GridLength collapsedLength = new GridLength(0);

    public GridLength FixedPartLength
    {
        get
        {
            return fixedPartLength;
        }
        set
        {

        }
    }

    public GridLength ExpandablePartLength
    {
        get
        {
            return Expanded ? expadedLength : collapsedLength;
        }
        set
        {

        }
    }

    #endregion
}