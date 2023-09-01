using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using FocusTimer.Features.Timer.Slot;
using FocusTimer.Library;
using FocusTimer.Library.Control;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private double CalculateConcentration()
    {
        var elapsedTotal = TimerSlots
            .Where(s => s.CurrentAppItem?.IsCountedOnConcentrationCalculation ?? false)
            .Sum(s => s.CurrentAppItem?.ActiveElapsedTicks ?? 0);

        if (elapsedTotal == 0)
        {
            return 0;
        }

        double concentration = 100 * elapsedTotal / (TimerItem.ElapsedTicks + 1);

        this.GetLogger()
            .Info(
                $"집중도 계산에 포함하는 앱들의 활성 사용 시간 합: {new TimeSpan(elapsedTotal).ToSixDigits()}, 타이머가 구동중인 시간의 합: {TimerItem.ElapsedString}");

        return concentration;
    }

    private readonly BindableMenuItem _concentrationOptionsItem = new()
    {
        IsCheckable = false,
        IsChecked = false,
        Icon = new System.Windows.Controls.Image
            {Source = Application.Current.FindResource("ic_magnify") as DrawingImage},
        Header = "집중도 표시 옵션  ",
    };

    private readonly BindableMenuItem _whichAppToIncludeMenuItem = new()
    {
        IsCheckable = false,
        IsChecked = false,
        Icon = new System.Windows.Controls.Image
            {Source = Application.Current.FindResource("ic_calculator_variant_outline") as DrawingImage},
        Header = "집중도 계산에 포함할 프로그램  ",
    };

    private BindableMenuItem[] GenerateConcentrationSelectionMenu()
    {
        _concentrationOptionsItem.Children = new[]
        {
            BuildShowConcentrationMenuItem()
        };

        _whichAppToIncludeMenuItem.Children = TimerSlots
            .Where(s => s.CurrentAppItem != null)
            .Select(BuildConcentrationCountMenuItem)
            .ToArray();

        return new[] {_concentrationOptionsItem, _whichAppToIncludeMenuItem};
    }

    private BindableMenuItem BuildShowConcentrationMenuItem()
    {
        var item = new BindableMenuItem
        {
            IsCheckable = true,
            IsChecked = Settings.GetShowConcentration(),
            Header = "집중도를 표시합니다"
        };

        item.OnCheck += (isChecked) =>
        {
            Settings.SetShowConcentration(isChecked);

            NotifyPropertyChanged(nameof(ShowConcentrationOpacity));
            NotifyPropertyChanged(nameof(ConcentrationContextMenu));
        };

        return item;
    }

    private BindableMenuItem BuildConcentrationCountMenuItem(TimerSlotViewModel timerSlotViewModel)
    {
        var app = timerSlotViewModel.CurrentAppItem!;
        var item = new BindableMenuItem
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
    }
}