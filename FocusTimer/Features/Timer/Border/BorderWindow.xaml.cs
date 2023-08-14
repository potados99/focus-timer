using System.Windows;
using System.Windows.Input;

namespace FocusTimer.Features.Timer;

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
        this.Owner = w; // Okey, this will result to disappear icon for main window.
        w.Hide(); // Hide helper window just in case
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.System && e.SystemKey == Key.F4)
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