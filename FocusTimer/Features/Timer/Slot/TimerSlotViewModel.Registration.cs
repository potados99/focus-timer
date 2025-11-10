// TimerSlotViewModel.Registration.cs
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

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerSlotViewModel
{
    public bool IsWaitingForApp { get; private set; }

    public void StartWaitingForApp()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에서 사용자의 창 선택을 기다립니다.");

        CurrentAppItem?.Dispose();
        CurrentAppItem = null;
        IsWaitingForApp = true;
        WindowSelectPrompt = Strings.Get("click_window");

        OnRender();
    }

    public void StopWaitingAndRegisterApp(AppItem appItem)
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 사용자가 선택한 앱({appItem.ProcessExecutablePath})을 등록합니다.");

        if (CurrentAppItem?.App == appItem.App)
        {
            appItem.IsCountedOnConcentrationCalculation = CurrentAppItem?.IsCountedOnConcentrationCalculation ?? true;
            this.GetLogger().Info($"아, 이거 같은 앱으로 교체되는거네요? 집중도 포함 여부를 승계해줍니다. 프로그램 '{appItem.App.Title}'은(는) 집중도 계산에 " +
                                  (appItem.IsCountedOnConcentrationCalculation ? "포함됩니다." : "포함되지 않습니다."));
        }

        if (CurrentAppItem != null)
        {
            this.GetLogger().Warn($"[StopWaitingAndRegisterApp] 기존 AppItem을 Dispose합니다. App={CurrentAppItem.App?.Title}");
            CurrentAppItem.Dispose();
            this.GetLogger().Warn($"[StopWaitingAndRegisterApp] 기존 AppItem Dispose 완료.");
        }

        this.GetLogger().Info($"[StopWaitingAndRegisterApp] 새 AppItem을 CurrentAppItem에 할당합니다.");
        CurrentAppItem = appItem;
        IsWaitingForApp = false;

        if (_slot != null)
        {
            this.GetLogger().Info($"현재 슬롯({SlotNumber}번)의 변동을 기록합니다.");

            _slot.App = appItem.App;
            _slotService.SaveRepository();
        }

        OnRender();
    }

    public void UnableToHandleRegistering(string reason)
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 앱을 등록할 수 없음을 표시합니다. 사유: {reason}");

        WindowSelectPrompt = reason;

        OnRender();
    }

    public void CancelRegisteringApp()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)의 앱 등록을 취소합니다.");

        IsWaitingForApp = false;

        OnRender();
    }

    public void ClearRegisteredApp()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 등록된 앱을 제거합니다.");

        CurrentAppItem?.ForceStartNewUsage(); // 새 AppUsage를 시작합니다. 이렇게 하면 앱을 다시 불러왔을 때에 기록이 0인 상태로 시작하니, 리셋의 효과가 있습니다.
        CurrentAppItem?.Dispose();
        CurrentAppItem = null;

        if (_slot != null)
        {
            this.GetLogger().Info($"현재 슬롯({SlotNumber}번)의 변동을 기록합니다.");

            _slot.App = null;
            _slotService.SaveRepository();
        }

        OnRender();
    }
}