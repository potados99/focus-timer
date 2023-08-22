using System.Linq;
using System.Windows;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region Main Window의 확장 및 축소

    private bool _expanded = true;
    public bool Expanded
    {
        get
        {
            return _expanded;
        }
        set
        {
            _expanded = value;
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

    private const int WINDOW_HEIGHT = 40 * (1 + 5);

    public int WindowHeight
    {
        get
        {
            if (Expanded)
            {
                return WINDOW_HEIGHT;
            }

            const int borderThickness = 1;
            const int separatorThickness = 1;
            var contentGridRowLengthStar = FixedPartLength.Value;
            var expandableGridRowLengthStar = _expandedLength.Value;
            var expandableGridRowLength = WINDOW_HEIGHT / (contentGridRowLengthStar + expandableGridRowLengthStar) * contentGridRowLengthStar;
            return (int)expandableGridRowLength + borderThickness + separatorThickness;
        }
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // 왜인지는 모르겠는데 이 WindowHeight은 양방향 바인딩으로 넣어 주어야 잘 돌아갑니다...
            // 그래서 setter를 둡니다,,,
        }
    }

    private readonly GridLength _expandedLength = new(1 + 5, GridUnitType.Star);
    private readonly GridLength _collapsedLength = new(0);

    public GridLength FixedPartLength { get; } = new(1.4, GridUnitType.Star);
    public GridLength ExpandablePartLength => Expanded ? _expandedLength : _collapsedLength;

    #endregion
}