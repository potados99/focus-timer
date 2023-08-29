using System.Windows;
using FocusTimer.Lib.Extensions;

namespace FocusTimer.Lib.Control.Base;

/// <summary>
/// 뷰 모델과 라이프사이클 이벤트를 가지는 윈도우입니다.
/// </summary>
/// <typeparam name="T"></typeparam>
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