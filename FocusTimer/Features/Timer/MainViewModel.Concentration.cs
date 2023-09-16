// MainViewModel.Concentration.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

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
        Header = $"{Strings.Get("concentration_display_options")}  ",
    };

    private readonly BindableMenuItem _whichAppToIncludeMenuItem = new()
    {
        IsCheckable = false,
        IsChecked = false,
        Icon = new System.Windows.Controls.Image
            {Source = Application.Current.FindResource("ic_calculator_variant_outline") as DrawingImage},
        Header = $"{Strings.Get("programs_to_include_in_calculation")}  ",
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
            Header = Strings.Get("show_concentration")
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