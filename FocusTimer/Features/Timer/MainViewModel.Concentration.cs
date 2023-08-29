using System;
using FocusTimer.Lib.Utility;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using FocusTimer.Lib.Control;
using FocusTimer.Lib.Extensions;

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

        this.GetLogger().Info($"집중도 계산에 포함하는 앱들의 활성 사용 시간 합: {new TimeSpan(elapsedTotal).ToSixDigits()}, 타이머가 구동중인 시간의 합: {TimerItem.ElapsedString}");
            
        return concentration;
    }

    private readonly BindableMenuItem _whichAppToIncludeMenuItem = new()
    {
        IsCheckable = false,
        IsChecked = false,
        Icon = new System.Windows.Controls.Image()
            {Source = Application.Current.FindResource("ic_calculator_variant_outline") as DrawingImage},
        Header = "집중도 계산에 포함할 프로그램  ",
    };
    
    private BindableMenuItem[] GenerateConcentrationSelectionMenu()
    {
        _whichAppToIncludeMenuItem.Children = TimerSlots
            .Where(s => s.CurrentAppItem != null)
            .Select(s =>
            {
                var app = s.CurrentAppItem!;
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

        return new[] {_whichAppToIncludeMenuItem};
    }
}