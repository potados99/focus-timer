// BorderWindow.xaml.cs
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

using System.Windows;
using System.Windows.Input;

namespace FocusTimer.Features.Timer.Border;

/// <summary>
/// Interaction logic for BorderWindow.xaml
/// </summary>
public partial class BorderWindow : Window
{
    public BorderWindow()
    {
        InitializeComponent();

        HideFromAppSwitcher();
    }

    private void HideFromAppSwitcher()
    {
        Window w = new()
        {
            Top = -100, // Location of new window is outside of visible part of screen
            Left = -100,
            Width = 1, // size of window is enough small to avoid its appearance at the beginning
            Height = 1,

            WindowStyle = WindowStyle.ToolWindow, // Set window style as ToolWindow to avoid its icon in AltTab 
            ShowInTaskbar = false
        }; // Create helper window
        w.Show(); // We need to show window before set is as owner to our main window
        Owner = w; // Okay, this will result to disappear icon for main window.
        w.Hide(); // Hide helper window just in case
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e is {Key: Key.System, SystemKey: Key.F4})
        {
            // ALT + F4가 떨어지면 앱을 종료합니다.
            Application.Current.Shutdown();
        }
    }

    private void Window_Closed(object sender, System.EventArgs e)
    {
        Application.Current.Shutdown();
    }
}