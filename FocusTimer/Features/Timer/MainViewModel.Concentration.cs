using FocusTimer.Lib.Utility;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region 집중도

    public string Concentration
    {
        get
        {
            var elapsedTotal = TimerSlots
                .Where(s => s.CurrentAppItem?.IsCountedOnConcentrationCalculation ?? false)
                .Sum(s => s.CurrentAppItem?.ActiveElapsedTicks ?? 0);

            if (elapsedTotal == 0)
            {
                return "0%";
            }

            double concentration = 100 * elapsedTotal / (TimerItem.ElapsedTicks + 1);

            return concentration + "%";
        }
    }

    public bool IsOnConcentration
    {
        get
        {
            return TimerSlots.Any(s => s.IsAppActive && s.IsAppCountedOnConcentrationCalculation);
        }
    }
    
    public BindableMenuItem[] ConcentrationContextMenu
    {
        get
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

            return new[] { _whichAppToIncludeMenuItem };
        }
    }

    private readonly BindableMenuItem _whichAppToIncludeMenuItem = new()
    {
        IsCheckable = false,
        IsChecked = false,
        Icon = new System.Windows.Controls.Image() { Source = Application.Current.FindResource("ic_calculator_variant_outline") as DrawingImage },
        Header = "집중도 계산에 포함할 프로그램  ",
    };

    #endregion
}