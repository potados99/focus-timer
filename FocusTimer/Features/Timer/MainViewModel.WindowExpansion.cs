using System.Linq;
using System.Windows;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private const int WINDOW_HEIGHT = 40 * (1 + 5);

    private bool _expanded = true;
    private readonly GridLength _expandedLength = new(1 + 5, GridUnitType.Star);
    private readonly GridLength _collapsedLength = new(0);

    private bool Expanded
    {
        get => _expanded;
        set
        {
            _expanded = value;
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

    private int CalculateWindowHeight()
    {
        if (Expanded)
        {
            return WINDOW_HEIGHT;
        }

        const int borderThickness = 1;
        const int separatorThickness = 1;
        var contentGridRowLengthStar = FixedPartLength.Value;
        var expandableGridRowLengthStar = _expandedLength.Value;
        var expandableGridRowLength = WINDOW_HEIGHT / (contentGridRowLengthStar + expandableGridRowLengthStar) *
                                      contentGridRowLengthStar;
        return (int) expandableGridRowLength + borderThickness + separatorThickness;
    }
}