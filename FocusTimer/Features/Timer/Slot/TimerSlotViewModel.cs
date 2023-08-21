using System.IO;
using System.Windows;
using FocusTimer.Domain.Entities;
using FocusTimer.Lib;
using FocusTimer.Lib.Component;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Features.Timer.Slot;

public class TimerSlotViewModel : BaseViewModel
{
    #region 이벤트 핸들러 정의와 구현
    
    public event Signal? OnRegisterApplication;
    public event Signal? OnClearApplication;

    public void FireAppRegisterEvent() => OnRegisterApplication?.Invoke();
    public void FireAppClearEvent() => OnClearApplication?.Invoke();

    #endregion

    #region 속성들

    public int SlotNumber { get; set; }

    public TimerApp? CurrentApp { get; private set; }
    public bool IsWaitingForApp { get; private set; }

    public Visibility IsAppVisible => !IsWaitingForApp && CurrentApp != null ? Visibility.Visible : Visibility.Hidden;

    public Visibility IsSetButtonVisible => !IsWaitingForApp && CurrentApp == null ? Visibility.Visible : Visibility.Hidden;

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

    public void RestoreApp(string? executablePath)
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            return;
        }
        if (!File.Exists(executablePath))
        {
            this.GetLogger().Warn($"Executable not found: {executablePath}");
            return;
        }

        StopWaitingAndRegisterApp(new TimerApp(executablePath));
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
        CurrentApp = app;
        IsWaitingForApp = false;

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
        CurrentApp = null;

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