// EventService.cs
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

using FocusTimer.Library;
using FocusTimer.Library.Extensions;
using FocusTimer.Library.Utility;

namespace FocusTimer.Domain.Services;

public class EventService
{
    private readonly ClockGenerator _clockGenerator;
    private readonly UserActivityMonitor _activityMonitor;
    private readonly WindowWatcher _windowWatcher;

    public EventService(
        ClockGenerator clockGenerator,
        UserActivityMonitor activityMonitor,
        WindowWatcher windowWatcher
    )
    {
        _clockGenerator = clockGenerator;
        _clockGenerator.OnTick += () =>
        {
            this.GetLogger().Info("[이벤트 발생] Tick 이벤트가 발생하였습니다.");
            OnTick?.Invoke();
            OnRender?.Invoke();
        };

        _activityMonitor = activityMonitor;
        _activityMonitor.OnActivated += () =>
        {
            this.GetLogger().Info("[이벤트 발생] Activated 이벤트가 발생하였습니다.");
            
            OnRender?.Invoke();
            OnActivated?.Invoke();
        };
        _activityMonitor.OnDeactivated += () =>
        {
            this.GetLogger().Info("[이벤트 발생] Deactivated 이벤트가 발생하였습니다.");

            OnRender?.Invoke();
            OnDeactivated?.Invoke();
        };
        
        _windowWatcher = windowWatcher;
        _windowWatcher.OnFocused += (p, n) =>
        {
            this.GetLogger().Info("[이벤트 발생] Focused 이벤트가 발생하였습니다.");

            OnFocusChanged?.Invoke(p, n);
            OnRender?.Invoke();
        };
    }

    public event Signal? OnTick;
    public event Signal? OnRender;
    public event Signal? OnReload;
    public event Signal? OnActivated;
    public event Signal? OnDeactivated;

    public event WindowWatcher.FocusedEventHandler? OnFocusChanged;
    
    public void EmitRender()
    {
        this.GetLogger().Debug("즉시 전체 렌더링을 유발합니다.");

        OnRender?.Invoke();
    }

    public void EmitReload()
    {
        this.GetLogger().Debug("타임 리셋 및 슬롯 다시 불러오기를 유발합니다.");

        OnReload?.Invoke();
    }

    public void SetActivityTimeout(int timeout)
    {
        _activityMonitor.Timeout = timeout;
    }
}