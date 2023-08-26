using System.Windows;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public int WindowHeight
    {
        get => CalculateWindowHeight();
        // ReSharper disable once ValueParameterNotUsed
        set
        {
            // 왜인지는 모르겠는데 이 WindowHeight은 양방향 바인딩으로 넣어 주어야 잘 돌아갑니다...
            // 그래서 setter를 둡니다,,,
        }
    }

    public GridLength FixedPartLength { get; } = new(1.4, GridUnitType.Star);
    public GridLength ExpandablePartLength => Expanded ? _expandedLength : _collapsedLength;
}