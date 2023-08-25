using FocusTimer.Lib;

namespace FocusTimer.Features.Timer;

public partial class MainViewModel
{
    #region 동작 감지

    public int ActivityTimeout
    {
        get => Settings.GetActivityTimeout();
        set
        {
            Settings.SetActivityTimeout(value);
            _activityMonitor.Timeout = value;

            // 양방향 바인딩되는 속성으로, UI에 의해 변경시 여기에서 NotifyPropertyChanged를 트리거해요.
            NotifyPropertyChanged(nameof(ActivityTimeout));
        }
    }

    #endregion
}