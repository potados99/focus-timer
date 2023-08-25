using System.Windows;
using FocusTimer.Lib.Utility;

namespace FocusTimer.Lib.Component;

public abstract class BaseWindow<T> : Window where T : BaseViewModel
{
    protected readonly T ViewModel = Modules.Get<T>();

    protected BaseWindow()
    {
        this.GetLogger().Info("Window를 초기화합니다.");
        
        DataContext = ViewModel;

        Loaded += (_, _) =>
        {
            this.GetLogger().Info("Window가 로드되었습니다.");

            OnLoaded();
            ViewModel.OnLoaded();
        };
        
        OnInitialize();
        ViewModel.OnInitialize();
        
        this.GetLogger().Info("Window가 초기화되었습니다.");
    }

    protected abstract void OnInitialize();

    protected virtual void OnLoaded() {}
}