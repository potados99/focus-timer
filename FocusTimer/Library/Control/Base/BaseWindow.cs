// BaseWindow.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System.Windows;
using FocusTimer.Library.Extensions;

namespace FocusTimer.Library.Control.Base;

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