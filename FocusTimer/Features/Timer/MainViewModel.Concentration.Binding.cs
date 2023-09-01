using System.Linq;
using System.Windows;
using FocusTimer.Library;
using FocusTimer.Library.Control;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public string Concentration => CalculateConcentration() + "%";
    
    public double ShowConcentrationOpacity => Settings.GetShowConcentration() ? 1 : 0;
    
    public bool IsOnConcentration =>
        TimerSlots.Any(s => s is {IsAppActive: true, IsAppCountedOnConcentrationCalculation: true});

    public BindableMenuItem[] ConcentrationContextMenu => GenerateConcentrationSelectionMenu();
}