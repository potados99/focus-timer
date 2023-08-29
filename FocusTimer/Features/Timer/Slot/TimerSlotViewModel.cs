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
    
    private void LoadSlot()
    {
        this.GetLogger().Info($"현재 슬롯({SlotNumber}번)의 앱 정보를 불러옵니다.");

        _slot = _slotService.GetOrCreateStatus(SlotNumber);

        if (_slot.App != null)
        {
            this.GetLogger().Debug($"현재 슬롯({SlotNumber}번)에 등록된 앱({_slot.App.Title})을 복구합니다.");

            StopWaitingAndRegisterApp(new AppItem(_slot.App));
        }
        else
        {
            this.GetLogger().Debug($"현재 슬롯({SlotNumber}번)에 등록된 앱이 없습니다.");
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