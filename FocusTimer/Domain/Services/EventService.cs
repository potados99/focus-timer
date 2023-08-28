using FocusTimer.Lib;
using FocusTimer.Lib.Utility;

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
            this.GetLogger().Debug("Tick 이벤트가 발생하였습니다.");
            OnTick?.Invoke();
            OnRender?.Invoke();
        };

        _activityMonitor = activityMonitor;
        _activityMonitor.OnActivated += () =>
        {
            this.GetLogger().Debug("Activated 이벤트가 발생하였습니다.");
            
            OnRender?.Invoke();
            OnActivated?.Invoke();
        };
        _activityMonitor.OnDeactivated += () =>
        {
            this.GetLogger().Debug("Deactivated 이벤트가 발생하였습니다.");

            OnRender?.Invoke();
            OnDeactivated?.Invoke();
        };
        
        _windowWatcher = windowWatcher;
        _windowWatcher.OnFocused += (p, n) =>
        {
            this.GetLogger().Debug("Focused 이벤트가 발생하였습니다.");

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