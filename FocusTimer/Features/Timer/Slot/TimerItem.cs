// TimerItem.cs
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
        new TimerSlotViewModel { SlotNumber = 0 },
        new TimerSlotViewModel { SlotNumber = 1 },
        new TimerSlotViewModel { SlotNumber = 2 },
        new TimerSlotViewModel { SlotNumber = 3 },
        new TimerSlotViewModel { SlotNumber = 4 },
    };

    private void RegisterEvents()
    {
        _eventService.OnTick += OnTick;
        _eventService.OnReload += OnReload;
        _eventService.OnActivated += OnActivated;
        _eventService.OnFocusChanged += OnFocusChanged;
    }

    private void OnTick(DateTime now)
    {
        UpdateUsage(now);
    }

    private void OnReload()
    {
        Usage = _timerUsageService.CreateNewUsage();
    }

    private void OnActivated(DateTime now)
    {
        if (IsAnyAppActive)
        {
            this.GetLogger().Info("Activated 이벤트로 인해 새로운 TimerActiveUsage를 생성합니다.");

            Usage?.RunningUsage.OpenNewActiveUsage(now);
        }
    }

    private bool _WasAnyAppActive = false;

    private void OnFocusChanged(IntPtr prev, IntPtr current, DateTime now)
    {
        if (!_WasAnyAppActive && IsAnyAppActive)
        {
            this.GetLogger().Info("Focused 이벤트로 인해 새로운 TimerActiveUsage를 생성합니다.");

            Usage?.RunningUsage.OpenNewActiveUsage(now);
        }
        else if (_WasAnyAppActive && !IsAnyAppActive)
        {
            // Lost focus
            Usage?.RunningUsage.ActiveUsage.TouchUsage(now);
        }

        _WasAnyAppActive = IsAnyAppActive;
    }

    private void LoadUsage()
    {
        this.GetLogger().Debug("TimerUsage를 불러옵니다.");

        Usage = _timerUsageService.GetLastUsage() ?? _timerUsageService.CreateNewUsage();
        Usage.OpenNewRunningUsage();
    }

    private void UpdateUsage(DateTime now)
    {
        if (Usage == null)
        {
            return;
        }

        Usage.TouchUsage(now);
        Usage.RunningUsage.TouchUsage(now);

        if (IsAnyAppActive)
        {
            Usage.RunningUsage.ActiveUsage.TouchUsage(now);
        }

        _timerUsageService.SaveRepository();

        // [DEBUG] 시간 차이 분석
        DebugTimeDiscrepancy();
    }

    private void DebugTimeDiscrepancy()
    {
        if (Usage == null) return;

        // 타이머 전체 ActiveElapsed (메인 위에 표시되는 시간)
        var timerActiveElapsedTicks = ActiveElapsedTicks;
        var timerActiveElapsed = new TimeSpan(timerActiveElapsedTicks);

        // 각 슬롯의 AppItem들의 ActiveElapsed 합산
        long appsActiveElapsedTicksSum = 0;
        foreach (var slot in TimerSlots)
        {
            if (slot.CurrentAppItem != null)
            {
                appsActiveElapsedTicksSum += slot.CurrentAppItem.ActiveElapsedTicks;
            }
        }

        var appsActiveElapsedSum = new TimeSpan(appsActiveElapsedTicksSum);

        // 차이 계산
        var discrepancy = timerActiveElapsed - appsActiveElapsedSum;
        
        this.GetLogger().Info($"[시간 비교] 타이머 Active: {timerActiveElapsed.ToSixDigits()}, " +
                              $"앱들 합계: {appsActiveElapsedSum.ToSixDigits()}, " +
                              $"차이: {discrepancy.TotalSeconds:F2}초");

        // 상세 분석: 각 TimerActiveUsage와 AppActiveUsage 출력
        this.GetLogger().Debug("=== TimerActiveUsages ===");
        foreach (var ru in Usage.RunningUsages)
        {
            foreach (var au in ru.ActiveUsages)
            {
                this.GetLogger().Debug($"  TimerActiveUsage: StartedAt={au.StartedAt:HH:mm:ss.fff}, " +
                                      $"UpdatedAt={au.UpdatedAt:HH:mm:ss.fff}, " +
                                      $"Elapsed={au.Elapsed.ToSixDigits()}");
            }
        }

        this.GetLogger().Debug("=== AppActiveUsages (슬롯별) ===");
        foreach (var slot in TimerSlots)
        {
            if (slot.CurrentAppItem != null)
            {
                this.GetLogger().Debug($"  슬롯 #{slot.SlotNumber}:");

                foreach (var ru in slot.CurrentAppItem.Usage?.RunningUsages ?? Enumerable.Empty<AppRunningUsage>())
                {
                    foreach (var au in ru.ActiveUsages)
                    {
                        this.GetLogger().Debug($"    AppActiveUsage: StartedAt={au.StartedAt:HH:mm:ss.fff}, " +
                                              $"UpdatedAt={au.UpdatedAt:HH:mm:ss.fff}, " +
                                              $"Elapsed={au.Elapsed.ToSixDigits()}");
                    }
                }
            }
        }
    }
}