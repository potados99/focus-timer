using System;
using System.Windows.Controls;
using FocusTimer.Lib.Extensions;

namespace FocusTimer.Lib.Control.Base;

/// <summary>
/// 뷰 모델과 라이프사이클 이벤트를 가지는 컨트롤입니다.
/// </summary>
/// <remarks>
/// 뷰 모델의 생성 주체는 외부에 있고, DataContext를 통해 뷰 모델의 인스턴스를 가져옵니다.
/// </remarks>
/// <typeparam name="T"></typeparam>
public abstract class BaseUserControl<T> : UserControl where T : BaseViewModel
{
    protected T ViewModel;

    protected BaseUserControl()
    {
        this.GetLogger().Info("Control을 초기화합니다.");

        Loaded += (_, _) =>
        {
            this.GetLogger().Info("Control이 로드되었습니다.");

            OnLoaded();

            ViewModel = DataContext as T ?? throw new InvalidOperationException("No ViewModel given as a DataContext");
            ViewModel.OnLoaded();
        };
        
        OnInitialize();
        
        // 여기에서는 ViewModel의 OnInitialize를 부를 수 없습니다.
        // 컨트롤의 생성자 시점에서 DataContext에 담긴 ViewModel이 있을 수 없기 때문입니다.
        
        this.GetLogger().Info("Control이 초기화되었습니다.");
    }

    protected abstract void OnInitialize();
    
    protected virtual void OnLoaded() {}
}