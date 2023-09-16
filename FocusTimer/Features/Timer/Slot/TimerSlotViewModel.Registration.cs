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

        CurrentAppItem?.Dispose();
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