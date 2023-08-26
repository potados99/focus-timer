using System.Linq;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerItem
{
    public bool IsAnyAppActive => TimerSlots.Any(s => s.IsAppActive);
}