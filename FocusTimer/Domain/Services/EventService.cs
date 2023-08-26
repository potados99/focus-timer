using FocusTimer.Lib;

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
            OnTick?.Invoke();
            OnRender?.Invoke();
        };

        _activityMonitor = activityMonitor;
        _activityMonitor.OnActivated += OnRender;
        _activityMonitor.OnDeactivated += OnRender;
        
        _windowWatcher = windowWatcher;
        _windowWatcher.OnFocused += (p, n) =>
        {
            OnFocusChanged?.Invoke(p, n);
            OnRender?.Invoke();
        };
    }

    public event Signal? OnTick;
    public event Signal? OnRender;
    public event Signal? OnReload;
    public event WindowWatcher.FocusedEventHandler? OnFocusChanged;
    
    public void EmitRender()
    {
        OnRender?.Invoke();
    }

    public void EmitReload()
    {
        OnReload?.Invoke();
    }

    public void SetActivityTimeout(int timeout)
    {
        _activityMonitor.Timeout = timeout;
    }
}