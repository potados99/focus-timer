// TimerSlotViewModel.cs
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

using System.Diagnostics.CodeAnalysis;
using FocusTimer.Domain.Services;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Features.Timer.Slot;

public partial class TimerSlotViewModel
{
    // 특이하게, 이 ViewModel은 외부에서 직접 instantiate 합니다.
    // 밖에서 생성자로 이것저것 넣어줄 여유가 없기 때문에,
    // 필요한 것들은 안에서 직접 받아옵니다.
    private readonly SlotService _slotService = Modules.Get<SlotService>();
    private readonly EventService _eventService = Modules.Get<EventService>();

    public override void OnLoaded()
    {
        RegisterEvents();
        LoadSlot();
    }

    public event Signal? OnRegisterApplication;
    public event Signal? OnClearApplication;

    public void FireAppRegisterEvent() => OnRegisterApplication?.Invoke();
    public void FireAppClearEvent() => OnClearApplication?.Invoke();
    
    public int SlotNumber { get; init; }
    
    public AppItem? CurrentAppItem { get; private set; }

    private Domain.Entities.Slot? _slot;

    public string? GetAppExecutablePath()
    {
        return CurrentAppItem?.ProcessExecutablePath;
    }

    private void RegisterEvents()
    {
        _eventService.OnReload += LoadSlot;
        _eventService.OnRender += OnRender;
    }
    
    [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
    private void LoadSlot()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)의 앱 정보를 불러옵니다.");

        if (CurrentAppItem != null)
        {
            this.GetLogger().Warn($"[LoadSlot] 기존 AppItem이 있습니다. Dispose될 예정입니다. App={CurrentAppItem.App?.Title}");
        }

        _slot = _slotService.GetOrCreateStatus(SlotNumber);

        if (_slot.App != null)
        {
            this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 등록된 앱({_slot.App.Title})을 복구합니다.");

            var newAppItem = new AppItem(_slot.App.ExecutablePath);
            this.GetLogger().Info($"[LoadSlot] 새 AppItem 생성 완료. 이제 StopWaitingAndRegisterApp 호출합니다.");

            StopWaitingAndRegisterApp(newAppItem);

            this.GetLogger().Info($"[LoadSlot] StopWaitingAndRegisterApp 완료.");
        }
        else
        {
            this.GetLogger().Info($"현재 슬롯({SlotNumber}번)에 등록된 앱이 없습니다.");
        }
    }
    
    private void OnRender()
    {
        NotifyPropertyChanged(nameof(CurrentAppItem));

        NotifyPropertyChanged(nameof(IsAppVisible));
        NotifyPropertyChanged(nameof(IsSetButtonVisible));
        NotifyPropertyChanged(nameof(IsWaitLabelVisible));

        NotifyPropertyChanged(nameof(IsAppActive));

        NotifyPropertyChanged(nameof(WindowSelectPrompt));
    }
}