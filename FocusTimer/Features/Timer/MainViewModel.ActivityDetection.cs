using FocusTimer.Lib;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    private void SaveActivityTimeout(int timeout)
    {
        Settings.SetActivityTimeout(timeout);
        _eventService.SetActivityTimeout(timeout);

        // 양방향 바인딩되는 속성으로, UI에 의해 변경시 여기에서 NotifyPropertyChanged를 트리거해요.
        NotifyPropertyChanged(nameof(ActivityTimeout));
    }
}