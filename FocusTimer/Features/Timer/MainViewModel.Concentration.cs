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
    #region 집중도

    public string Concentration
    {
        get
        {
            var elapsedTotal = TimerSlots
                .Where(s => s.CurrentApp?.IsCountedOnConcentrationCalculation ?? false)
                .Sum(s => s.CurrentApp?.ElapsedTicks ?? 0);

            if (elapsedTotal == 0)
            {
                return "0%";
            }

            double concentration = 100 * elapsedTotal / (ElapsedTicks + 1);

            return concentration + "%";
        }
    }

    public bool IsOnConcentraion
    {
        get
        {
            return TimerSlots.Any(s => s.IsAppActive && s.IsAppCountedOnConcentrationCalculation);
        }
    }

    public BindableMenuItem WhichAppToIncludeMenuItem = new()
    {
        IsCheckable = false,
        IsChecked = false,
        Icon = new System.Windows.Controls.Image() { Source = Application.Current.FindResource("ic_calculator_variant_outline") as DrawingImage },
        Header = "집중도 계산에 포함할 프로그램  ",
    };

    public BindableMenuItem[] ConcentrationContextMenu
    {
        get
        {
            WhichAppToIncludeMenuItem.Children = TimerSlots
                .Where(s => s.CurrentApp != null)
                .Select(s =>
                {
                    var app = s.CurrentApp;
                    var item = new BindableMenuItem()
                    {
                        IsCheckable = true,
                        IsChecked = app.IsCountedOnConcentrationCalculation,
                        Header = app.AppName
                    };

                    item.OnCheck += (isChecked) =>
                    {
                        app.IsCountedOnConcentrationCalculation = isChecked;
                        NotifyPropertyChanged(nameof(ConcentrationContextMenu));
                    };

                    return item;
                })
                .ToArray();

            return new BindableMenuItem[] { WhichAppToIncludeMenuItem };
        }
    }

    #endregion
}