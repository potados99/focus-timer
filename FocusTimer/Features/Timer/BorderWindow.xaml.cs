using System.Windows;
using System.Windows.Input;

namespace FocusTimer.Features.Timer
{
    /// <summary>
    /// Interaction logic for BorderWindow.xaml
    /// </summary>
    public partial class BorderWindow : Window
    {
        public BorderWindow()
        {
            InitializeComponent();
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
}
