// TimerItem.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerItem : UsageContainer<TimerUsage, TimerRunningUsage, TimerActiveUsage>
{
    private readonly TimerUsageService _timerUsageService = Modules.Get<TimerUsageService>();
    private readonly EventService _eventService = Modules.Get<EventService>();

    public TimerItem()
    {
        this.GetLogger().Info("TimerItem을 초기화합니다.");
        
        RegisterEvents();
        LoadUsage();
    }

    public List<TimerSlotViewModel> TimerSlots { get; } = new()
    {
        new TimerSlotViewModel {SlotNumber = 0},
        new TimerSlotViewModel {SlotNumber = 1},
        new TimerSlotViewModel {SlotNumber = 2},
        new TimerSlotViewModel {SlotNumber = 3},
        new TimerSlotViewModel {SlotNumber = 4},
    };
    
    private void RegisterEvents()
    {
        _eventService.OnTick += OnTick;
        _eventService.OnReload += OnReload;
        _eventService.OnActivated += OnActivated;
        _eventService.OnFocusChanged += OnFocusChanged;
    }

    private void OnTick()
    {
        UpdateUsage();
    }

    private void OnReload()
    {
        Usage = _timerUsageService.CreateNewUsage();
    }
    
    private void OnActivated()
    {
        if (IsAnyAppActive)
        {
            this.GetLogger().Info("Activated 이벤트로 인해 새로운 TimerActiveUsage를 생성합니다.");
            
            Usage?.RunningUsage.OpenNewActiveUsage();
        }
    }

    private void OnFocusChanged(IntPtr prev, IntPtr current)
    {
        var prevProcess = APIWrapper.GetProcessByWindowHandle(prev);
        var currentAppPaths = TimerSlots.Select(s => s.GetAppExecutablePath());
        var wasInactive = prevProcess == null || !currentAppPaths.Contains(prevProcess.ExecutablePath());

        if (wasInactive && IsAnyAppActive)
        {
            this.GetLogger().Info("Focused 이벤트로 인해 새로운 TimerActiveUsage를 생성합니다.");

            Usage?.RunningUsage.OpenNewActiveUsage();
        }
    }

    private void LoadUsage()
    {
        this.GetLogger().Debug("TimerUsage를 불러옵니다.");

        Usage = _timerUsageService.GetLastUsage() ?? _timerUsageService.CreateNewUsage();
        Usage.OpenNewRunningUsage();
    }

    private void UpdateUsage()
    {
        if (Usage == null)
        {
            return;
        }
        
        Usage.TouchUsage();
        Usage.RunningUsage.TouchUsage();

        if (IsAnyAppActive)
        {
            Usage.RunningUsage.ActiveUsage.TouchUsage();
        }

        _timerUsageService.SaveRepository();
    }
}