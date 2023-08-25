using FocusTimer.Features.Timer.Slot;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel 
{
    #region 사용량 집계와 복구

    public TimerItem TimerItem { get; } = new();

    #endregion
}