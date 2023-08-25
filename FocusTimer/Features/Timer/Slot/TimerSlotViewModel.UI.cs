using System.Windows;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerSlotViewModel : BaseViewModel
{
    public bool IsWaitingForApp { get; private set; }

    public Visibility IsAppVisible =>
        !IsWaitingForApp && CurrentAppItem != null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsSetButtonVisible =>
        !IsWaitingForApp && CurrentAppItem == null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsWaitLabelVisible => IsWaitingForApp ? Visibility.Visible : Visibility.Hidden;

    public bool IsAppActive => CurrentAppItem is {IsActive: true};

    public bool IsAppCountedOnConcentrationCalculation => CurrentAppItem is {IsCountedOnConcentrationCalculation: true};

    public string WindowSelectPrompt { get; set; } = "창을 클릭해주세요";

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

        Render();
    }

    public void UnableToHandleRegistering(string reason)
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 앱을 등록할 수 없음을 표시합니다. 사유: {reason}");

        WindowSelectPrompt = reason;

        Render();
    }

    public void CancelRegisteringApp()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)의 앱 등록을 취소합니다.");

        IsWaitingForApp = false;

        Render();
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

        Render();
    }

    public void Render()
    {
        NotifyPropertyChanged(nameof(CurrentAppItem));

        NotifyPropertyChanged(nameof(IsAppVisible));
        NotifyPropertyChanged(nameof(IsSetButtonVisible));
        NotifyPropertyChanged(nameof(IsWaitLabelVisible));

        NotifyPropertyChanged(nameof(IsAppActive));

        NotifyPropertyChanged(nameof(WindowSelectPrompt));
    }
}