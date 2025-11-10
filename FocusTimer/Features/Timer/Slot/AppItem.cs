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
    private readonly WindowWatcher _watcher = Modules.Get<WindowWatcher>();
    private readonly EventService _eventService = Modules.Get<EventService>();

    public AppItem(string executablePath)
    {
        _instanceId = ++_instanceCounter;
        this.GetLogger().Debug($"AppItem 생성: 인스턴스 #{_instanceId}, 앱={executablePath} (총 생성={_instanceCounter}, Dispose됨={_disposedCounter}, 살아있음={_instanceCounter - _disposedCounter})");

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
            this.GetLogger().Warn($"AppItem 중복 Dispose 시도: 인스턴스 #{_instanceId}");
            return;
        }

        _disposedCounter++;
        this.GetLogger().Debug($"AppItem Dispose: 인스턴스 #{_instanceId} (총 생성={_instanceCounter}, Dispose됨={_disposedCounter}, 살아있음={_instanceCounter - _disposedCounter})");

        _disposed = true;
        UnregisterEvents();

        // Usage를 null로 설정하여 Dispose 후 이벤트가 와도 다른 AppItem의 Usage를 건드리지 않도록 함
        Usage = null;
    }

    ~AppItem()
    {
        if (!_disposed)
        {
            this.GetLogger().Error($"AppItem 메모리 누수 감지: 인스턴스 #{_instanceId}가 Dispose 없이 GC됨");
        }
    }

    public Domain.Entities.App App { get; }

    public void ForceStartNewUsage()
    {
        this.GetLogger().Debug($"새 AppUsage 강제 생성: 인스턴스 #{_instanceId}");

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
            this.GetLogger().Error($"Dispose된 AppItem에서 OnTick 호출됨: 인스턴스 #{_instanceId}");
            return;
        }

        this.GetLogger().Debug($"OnTick: 인스턴스 #{_instanceId}, IsActive={IsActive}, 앱={App?.Title}");
        UpdateUsage(now);
    }

    private void OnActivated(DateTime now)
    {
        if (_disposed)
        {
            this.GetLogger().Error($"Dispose된 AppItem에서 OnActivated 호출됨: 인스턴스 #{_instanceId}");
            return;
        }

        if (IsActive)
        {
            this.GetLogger().Debug($"사용자 활성화로 새 AppActiveUsage 생성: 인스턴스 #{_instanceId}");

            Usage?.RunningUsage.OpenNewActiveUsage(now);
        }
    }

    private bool _WasActive = false;

    private void OnFocusChanged(IntPtr prev, IntPtr current, DateTime now)
    {
        if (_disposed)
        {
            this.GetLogger().Error($"Dispose된 AppItem에서 OnFocusChanged 호출됨: 인스턴스 #{_instanceId}, 앱={App?.Title}");
            return;
        }

        this.GetLogger().Debug($"OnFocusChanged: 인스턴스 #{_instanceId}, IsActive={IsActive}, 앱={App?.Title}");

        if (!_WasActive && IsActive)
        {
            this.GetLogger().Debug($"포커스 획득으로 새 AppActiveUsage 생성: 인스턴스 #{_instanceId}");
                
            Usage?.RunningUsage.OpenNewActiveUsage(now);
        }
        else if (_WasActive && !IsActive)
        {
            this.GetLogger().Debug($"포커스 상실로 ActiveUsage 종료: 인스턴스 #{_instanceId}");

            // Lost focus
            if (Usage != null)
            {
                var timeSinceLastUpdate = (now - Usage.RunningUsage.ActiveUsage.UpdatedAt).TotalSeconds;
                this.GetLogger().Debug($"마지막 갱신 이후 경과 시간: {timeSinceLastUpdate:F2}초");

                if (timeSinceLastUpdate > 2)
                {
                    this.GetLogger().Error($"AppActiveUsage 갱신 시각 이상: 인스턴스 #{_instanceId}, UpdatedAt={Usage.RunningUsage.ActiveUsage.UpdatedAt:HH:mm:ss}, Now={now:HH:mm:ss}, 경과={timeSinceLastUpdate:F2}초");
                    throw new Exception($"AppActiveUsage의 마지막 갱신 시각이 너무 오래되었습니다! 뭔가 잘못된 것 같습니다.");
                }

                Usage.RunningUsage.ActiveUsage.TouchUsage(now);
            }
        }
    }

    private void LoadUsage()
    {
        this.GetLogger().Debug($"AppUsage 로드: 인스턴스 #{_instanceId}");

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
            this.GetLogger().Warn($"UpdateUsage 호출됐으나 Usage가 null: 인스턴스 #{_instanceId}");
            return;
        }

        this.GetLogger().Debug($"UpdateUsage: 인스턴스 #{_instanceId}, IsActive={IsActive}, ActiveUsage.Id={Usage.RunningUsage.ActiveUsage.Id}");

        Usage.TouchUsage(now, IsCountedOnConcentrationCalculation);
        Usage.RunningUsage.TouchUsage(now);

        if (IsActive)
        {
            Usage.RunningUsage.ActiveUsage.TouchUsage(now);
            this.GetLogger().Debug($"ActiveUsage 갱신: 인스턴스 #{_instanceId}, Elapsed={Usage.RunningUsage.ActiveUsage.Elapsed.TotalSeconds:F2}초");
        }

        _appUsageService.SaveRepository();
    }
}