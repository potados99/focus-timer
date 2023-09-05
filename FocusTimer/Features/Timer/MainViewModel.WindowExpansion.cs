// MainViewModel.WindowExpansion.cs
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