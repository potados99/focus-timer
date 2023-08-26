using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerSlotViewModel
{
    public bool IsWaitingForApp { get; private set; }
    
    public void StartWaitingForApp()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에서 사용자의 창 선택을 기다립니다.");

        CurrentAppItem = null;
        IsWaitingForApp = true;
        WindowSelectPrompt = "창을 클릭해주세요";

        OnRender();
    }
    
    public void StopWaitingAndRegisterApp(AppItem appItem)
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 사용자가 선택한 앱({appItem.ProcessExecutablePath})을 등록합니다.");

        CurrentAppItem = appItem;
        IsWaitingForApp = false;

        if (_slot != null)
        {
            this.GetLogger().Debug($"현재 슬롯({SlotNumber}번)의 변동을 기록합니다.");

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

        CurrentAppItem = null;

        if (_slot != null)
        {
            this.GetLogger().Debug($"현재 슬롯({SlotNumber}번)의 변동을 기록합니다.");

            _slot.App = null;
            _slotService.SaveRepository();
        }

        OnRender();
    }
}