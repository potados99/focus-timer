// AppItem.cs
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
using System.Windows.Controls.Primitives;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;
using FocusTimer.Library.Utility;

namespace FocusTimer.Features.Timer.Slot;

/// <summary>
/// 타이머 슬롯에 등록되는 앱을 나타냅니다.
/// 타이머 슬롯에는 앱이 등록되어 있을 수도, 그렇지 않을 수도 있습니다.
/// </summary>
public partial class AppItem : UsageContainer<AppUsage, AppRunningUsage, AppActiveUsage>, IDisposable
{
    private static int _instanceCounter = 0;
    private static int _disposedCounter = 0;
    private readonly int _instanceId;

    private readonly AppService _appService = Modules.Get<AppService>();
    private readonly AppUsageService _appUsageService = Modules.Get<AppUsageService>();
    private readonly UserActivityMonitor _activityMonitor = Modules.Get<UserActivityMonitor>();
    private readonly EventService _eventService = Modules.Get<EventService>();

    public AppItem(string executablePath)
    {
        _instanceId = ++_instanceCounter;
        this.GetLogger().Warn($"[AppItem #{_instanceId}] 생성됨! (총 생성: {_instanceCounter}, 총 Dispose: {_disposedCounter}, 살아있음: {_instanceCounter - _disposedCounter})");

        if (string.IsNullOrEmpty(executablePath))
        {
            throw new Exception("AppItem의 생성자에 executablePath가 null로 들어왔습니다. 이래서는 안 됩니다!");
        }

        App = _appService.GetOrCreateApp(executablePath);

        RegisterEvents();
        LoadUsage();
    }

    private volatile bool _disposed = false;

    public void Dispose()
    {
        if (_disposed)
        {
            this.GetLogger().Error($"[AppItem #{_instanceId}] 이미 Dispose되었는데 또 호출됨!");
            return;
        }

        _disposedCounter++;
        this.GetLogger().Warn($"[AppItem #{_instanceId}] Dispose됨! (총 생성: {_instanceCounter}, 총 Dispose: {_disposedCounter}, 살아있음: {_instanceCounter - _disposedCounter})");

        _disposed = true;
        UnregisterEvents();

        // Usage를 null로 설정하여 Dispose 후 이벤트가 와도 다른 AppItem의 Usage를 건드리지 않도록 함
        Usage = null;
    }

    ~AppItem()
    {
        if (!_disposed)
        {
            this.GetLogger().Error($"[AppItem #{_instanceId}] Dispose 없이 finalizer 호출됨! 메모리 누수 확인!");
        }
    }

    public Domain.Entities.App App { get; }

    public void ForceStartNewUsage()
    {
        this.GetLogger().Debug("강제로 새로운 AppUsage를 만들어줍니다.");

        var now = DateTime.Now;
        Usage = _appUsageService.CreateNewUsage(App);
        Usage.IsConcentrated = IsCountedOnConcentrationCalculation;
        Usage.OpenNewRunningUsage();
        Usage.RunningUsage.OpenNewActiveUsage(now); // 기존의 것을 또 건드리는 일을 막기 위해 일단 무지성으로 새로 만들어줍니다.

        IsCountedOnConcentrationCalculation = Usage.IsConcentrated;

        UpdateUsage(now);
    }

    private void RegisterEvents()
    {
        _eventService.OnTick += OnTick;
        _eventService.OnActivated += OnActivated;
        _eventService.OnFocusChanged += OnFocusChanged;
    }

    private void UnregisterEvents()
    {
        _eventService.OnTick -= OnTick;
        _eventService.OnActivated -= OnActivated;
        _eventService.OnFocusChanged -= OnFocusChanged;
    }

    private void OnTick(DateTime now)
    {
        if (_disposed)
        {
            this.GetLogger().Error($"[AppItem #{_instanceId}] Disposed 상태에서 OnTick 호출됨!");
            return;
        }

        this.GetLogger().Info($"[AppItem #{_instanceId}] OnTick: IsActive={IsActive}, App={App?.Title}");
        UpdateUsage(now);
    }

    private void OnActivated(DateTime now)
    {
        if (_disposed)
        {
            this.GetLogger().Error($"[AppItem #{_instanceId}] Disposed 상태에서 OnActivated 호출됨!");
            return;
        }

        if (_WasActive)
        {
            this.GetLogger().Info($"[AppItem #{_instanceId}] Activated 이벤트로 인해 새로운 AppActiveUsage를 생성합니다.");

            Usage?.RunningUsage.OpenNewActiveUsage(now);
        }
    }

    private bool _WasActive = false;

    private void OnFocusChanged(IntPtr prev, IntPtr current, DateTime now)
    {
        if (_disposed)
        {
            this.GetLogger().Error($"[AppItem #{_instanceId}] Disposed 상태에서 OnFocusChanged 호출됨! App={App?.Title}");
            return;
        }

        try
        {
            this.GetLogger().Info($"[AppItem #{_instanceId}] OnFocusChanged 시작: WasActive={_WasActive}, IsActive={IsActive}, App={App?.Title}");

            if (!_WasActive && IsActive)
            {
                this.GetLogger().Info($"[AppItem #{_instanceId}] Focused 이벤트로 인해 새로운 AppActiveUsage를 생성합니다.");

                if (Usage == null)
                {
                    this.GetLogger().Error($"[AppItem #{_instanceId}] Usage가 null입니다!");
                    return;
                }

                Usage.RunningUsage.OpenNewActiveUsage(now);
            }
            else if (_WasActive && !IsActive)
            {
                this.GetLogger().Info($"[AppItem #{_instanceId}] Lost focus - ActiveUsage를 터치합니다.");

                // Lost focus
                if (Usage != null)
                {
                    var timeSinceLastUpdate = (now - Usage.RunningUsage.ActiveUsage.UpdatedAt).TotalSeconds;
                    this.GetLogger().Info($"[AppItem #{_instanceId}] 마지막 갱신 이후 경과 시간: {timeSinceLastUpdate:F2}초");

                    if (timeSinceLastUpdate > 2)
                    {
                        this.GetLogger().Error($"[AppItem #{_instanceId}] AppActiveUsage의 마지막 갱신 시각이 너무 오래되었습니다! " +
                                              $"UpdatedAt={Usage.RunningUsage.ActiveUsage.UpdatedAt}, Now={now}, Diff={timeSinceLastUpdate:F2}초");
                        throw new Exception($"[AppItem #{_instanceId}] AppActiveUsage의 마지막 갱신 시각이 너무 오래되었습니다! 뭔가 잘못된 것 같습니다.");
                    }

                    Usage.RunningUsage.ActiveUsage.TouchUsage(now);
                }
            }

            _WasActive = IsActive;
            this.GetLogger().Info($"[AppItem #{_instanceId}] OnFocusChanged 완료");
        }
        catch (Exception ex)
        {
            this.GetLogger().Error($"[AppItem #{_instanceId}] OnFocusChanged에서 예외 발생: {ex.Message}\n{ex.StackTrace}");
            throw;
        }
    }

    private void LoadUsage()
    {
        this.GetLogger().Debug("AppUsage를 불러옵니다.");

        var now = DateTime.Now;
        Usage = _appUsageService.GetLastUsage(App) ?? _appUsageService.CreateNewUsage(App);
        Usage.OpenNewRunningUsage();
        Usage.RunningUsage.OpenNewActiveUsage(now); // 기존의 것을 또 건드리는 일을 막기 위해 일단 무지성으로 새로 만들어줍니다.

        IsCountedOnConcentrationCalculation = Usage.IsConcentrated;
    }

    private void UpdateUsage(DateTime now)
    {
        if (Usage == null)
        {
            this.GetLogger().Warn($"[AppItem #{_instanceId}] UpdateUsage: Usage가 null입니다!");
            return;
        }

        this.GetLogger().Info($"[AppItem #{_instanceId}] UpdateUsage 시작: _WasActive={_WasActive}, IsActive={IsActive}, ActiveUsage.Id={Usage.RunningUsage.ActiveUsage.Id}, UpdatedAt={Usage.RunningUsage.ActiveUsage.UpdatedAt:HH:mm:ss}");

        Usage.TouchUsage(now, IsCountedOnConcentrationCalculation);
        Usage.RunningUsage.TouchUsage(now);

        // OnTick에서는 실시간 IsActive가 아닌 _WasActive를 사용해야 함
        // IsActive는 실시간으로 GetForegroundProcess()를 호출하므로, Focused 이벤트보다 먼저 처리되면 타이밍 문제 발생
        //
        // 언제 문제가 되느냐? 틱에 의해 업데이트가 트리거되는 시점에서 포커스가 막 바뀌는 경우!
        // 아직 OnFocusChanged로 새로운 ActiveUsage가 만들어지지도 않은 상황이고, IsActive는 false여야 하는 "아직은 조용한" 상황.
        // 그런데 사실 사용자는 이미 앱 윈도우를 클릭해서 포커스를 옮긴 상태.
        // 이렇게 되면 IsActive가 true를 띄움. 그러면 얘는 애초에 포커스 받아오고 있는 중이던 것을 터치하듯이 마지막 ActiveUsage를 주워다가 터치하게 됨.
        // 그런데 터치해야 할 ActiveUsage는 당연히 그게 아님. 이 포커스 이벤트를 제대로 처리할 OnFocusChanged에서 새로 만들어져야 할 ActiveUsage임.
        // 즉슨 틱마다 업데이트할 때 IsActive에만 의존하면 가장 마지막 ActiveUsage가 지금까지 잘 써오고 있던 "진짜" 살아있는 usage인지
        // 아니면 한참동안 방치되어 있다가 이제 막 곧 다가올 OnFocusChanged에 의해 덮어씌워질 usage인지 구분할 방법이 없음.
        // 그치만 _WasActive를 활용하면 확실히 "지난 OnFocusChanged에서 이미 포커스를 받고 있었던가?"를 알 수 있으므로,
        // 현재 틱 시점에서 확실히 포커스를 받아 살아있는 usage임을 보장할 수 있음.
        if (_WasActive)
        {
            var beforeUpdate = Usage.RunningUsage.ActiveUsage.UpdatedAt;
            Usage.RunningUsage.ActiveUsage.TouchUsage(now);
            var afterUpdate = Usage.RunningUsage.ActiveUsage.UpdatedAt;

            this.GetLogger().Info($"[AppItem #{_instanceId}] ActiveUsage를 터치했습니다. Before={beforeUpdate:HH:mm:ss}, After={afterUpdate:HH:mm:ss}, Elapsed={Usage.RunningUsage.ActiveUsage.Elapsed.TotalSeconds:F2}초");
        }
        else
        {
            this.GetLogger().Info($"[AppItem #{_instanceId}] _WasActive=false이므로 ActiveUsage 터치하지 않음");
            if (IsActive)
            {
                this.GetLogger().Warn($"[AppItem #{_instanceId}] 타이밍 불일치 감지! IsActive=true이지만 _WasActive=false → OnFocusChanged가 아직 안 왔음");
            }
        }

        _appUsageService.SaveRepository();

        this.GetLogger().Info($"[AppItem #{_instanceId}] UpdateUsage 완료");
    }
}