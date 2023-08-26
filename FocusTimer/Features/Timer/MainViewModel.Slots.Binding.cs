using System.Collections.Generic;
using System.Linq;
using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public List<TimerSlotViewModel> TimerSlots => TimerItem.TimerSlots;

    private TimerSlotViewModel? CurrentRegisteringTimerSlot => TimerSlots.FirstOrDefault(s => s.IsWaitingForApp);
}