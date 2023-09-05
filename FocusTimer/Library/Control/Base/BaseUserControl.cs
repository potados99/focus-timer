// BaseUserControl.cs
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

using System;
using System.Windows.Controls;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Library.Control.Base;

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