using FocusTimer.Lib.Utility;
using System.Linq;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public string Concentration => CalculateConcentration() + "%";

    public bool IsOnConcentration =>
        TimerSlots.Any(s => s is {IsAppActive: true, IsAppCountedOnConcentrationCalculation: true});

    public BindableMenuItem[] ConcentrationContextMenu => GenerateConcentrationSelectionMenu();
}