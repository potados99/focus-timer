using FocusTimer.Library;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    public int ActivityTimeout
    {
        get => Settings.GetActivityTimeout();
        set => SaveActivityTimeout(value);
    }
}