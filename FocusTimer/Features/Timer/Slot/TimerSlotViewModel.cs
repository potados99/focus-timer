using System.IO;
using System.Windows;
using FocusTimer.Domain.Entities;
using FocusTimer.Domain.Services;
using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.DependencyInjection;

namespace FocusTimer.Features.Timer.Slot;

public class TimerSlotViewModel : BaseViewModel
{
    private readonly SlotService _slotService = App.Provider.GetService<SlotService>()!;

    #region 이벤트 핸들러 정의와 구현

    public event Signal? OnRegisterApplication;
    public event Signal? OnClearApplication;

    public void FireAppRegisterEvent() => OnRegisterApplication?.Invoke();
    public void FireAppClearEvent() => OnClearApplication?.Invoke();

    #endregion

    #region 속성들

    public int SlotNumber { get; init; }

    private SlotStatus? _status;

    public TimerApp? CurrentApp { get; private set; }
    public bool IsWaitingForApp { get; private set; }

    public Visibility IsAppVisible => !IsWaitingForApp && CurrentApp != null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsSetButtonVisible =>
        !IsWaitingForApp && CurrentApp == null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsWaitLabelVisible => IsWaitingForApp ? Visibility.Visible : Visibility.Hidden;

    public bool IsAppActive => CurrentApp is {IsActive: true};

    public bool IsAppCountedOnConcentrationCalculation => CurrentApp is {IsCountedOnConcentrationCalculation: true};

    public string WindowSelectPrompt { get; set; } = "창을 클릭해주세요";

    #endregion

    #region 외부에 노출하는 제어용 메소드

    public string? GetAppExecutablePath()
    {
        return CurrentApp?.ProcessExecutablePath;
    }

    public Domain.Entities.App? GetAppEntity()
    {
        return CurrentApp?.AppEntity;
    }

    public void RestoreApp()
    {
        _status = _slotService.GetOrCreateStatus(SlotNumber);

        if (_status.App != null)
        {
            StopWaitingAndRegisterApp(new TimerApp(_status.App));
        }
    }

    public void StartWaitingForApp()
    {
        CurrentApp = null;
        IsWaitingForApp = true;
        WindowSelectPrompt = "창을 클릭해주세요";

        Render();
    }

    public void StopWaitingAndRegisterApp(TimerApp app)
    {
        this.GetLogger().Info($"{SlotNumber}번 슬롯에 앱({app.ProcessExecutablePath})을 등록합니다.");

        CurrentApp = app;
        IsWaitingForApp = false;

        if (_status != null)
        {
            _status.App = app.AppEntity;
            _slotService.SaveRepository();
        }

        Render();
    }

    public void UnableToHandleRegistering(string reason)
    {
        WindowSelectPrompt = reason;

        Render();
    }

    public void CancelRegisteringApp()
    {
        IsWaitingForApp = false;

        Render();
    }

    public void ClearRegisteredApp()
    {
        this.GetLogger().Info($"{SlotNumber}번 슬롯에서 앱을 제거합니다.");

        CurrentApp = null;

        if (_status != null)
        {
            _status.App = null;
            _slotService.SaveRepository();
        }

        Render();
    }

    public void Tick()
    {
        CurrentApp?.Tick();
    }

    public void Render()
    {
        NotifyPropertyChanged(nameof(CurrentApp));

        NotifyPropertyChanged(nameof(IsAppVisible));
        NotifyPropertyChanged(nameof(IsSetButtonVisible));
        NotifyPropertyChanged(nameof(IsWaitLabelVisible));

        NotifyPropertyChanged(nameof(IsAppActive));

        NotifyPropertyChanged(nameof(WindowSelectPrompt));
    }

    #endregion
}