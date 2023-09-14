// SplashWindow.xaml.cs
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

using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace FocusTimer.Features.Splash;

/// <summary>
/// 시작할 때에 보여줄 스플래시 화면입니다.
/// 새 스레드에서 보여주고, 자동으로 닫힙니다.
/// </summary>
public partial class SplashWindow
{
    private static SplashWindow? _splash;
    
    public SplashWindow()
    {
        InitializeComponent();
    }

    public static void ShowAutoClosed()
    {
        var t = new Thread(() =>
        {
            _splash = new SplashWindow();
            _splash.Show();
            Dispatcher.Run();
        });
        t.SetApartmentState(ApartmentState.STA);
        t.IsBackground = true;
        t.Start();
        
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Loaded,
            (DispatcherOperationCallback)(arg =>
            {
                CloseWindowSafe(_splash);
                return null;
            }),
            _splash);
    }
    
    private static void CloseWindowSafe(Window? w)
    {
        if (w == null)
        {
            return;
        }
        
        if (w.Dispatcher.CheckAccess())
        {
            w.Close();
        }
        else
        {
            w.Dispatcher.Invoke(DispatcherPriority.Normal, new ThreadStart(w.Close));
        }
    }
}