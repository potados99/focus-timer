// UserActivityMonitor.cs
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

using System;
using System.Windows.Threading;

namespace FocusTimer.Library.Utility;

/// <summary>
/// 사용자의 입력을 감지하는 모니터입니다.
/// 사용자가 오랫동안 키보드와 마우스를 사용하지 않으면 비활성화 이벤트를 발생시키며,
/// 이후 입력이 감지되면 활성화 이벤트를 발생시킵니다.
/// </summary>
public class UserActivityMonitor
{
    public event Signal? OnActivated;
    public event Signal? OnDeactivated;

    private readonly DispatcherTimer _timer = new();

    public int Timeout { get; set; }

    public UserActivityMonitor()
    {
        Timeout = Settings.GetActivityTimeout();

        _timer.Interval = TimeSpan.FromMilliseconds(500);
        _timer.Tick += (_, _) =>
        {
            if (IsActive && !_wasActive)
            {
                OnActivated?.Invoke();
            }

            if (!IsActive && _wasActive)
            {
                OnDeactivated?.Invoke();
            }

            _wasActive = IsActive;
        };

        _timer.Start();
    }

    private bool _wasActive = true;

    public bool IsActive => (Timeout == 0 /*타임아웃 비활성화된고임*/) || APIWrapper.GetElapsedFromLastInput() < (Timeout * 1000);
}