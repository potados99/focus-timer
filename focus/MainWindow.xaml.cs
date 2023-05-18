using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace focus
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WIN32Rectangle
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        public const int GWL_EXSTYLE = -20;
        public const int WS_EX_LAYERED = 0x80000;
        public const int LWA_ALPHA = 0x2;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FocusMonitor monitor = new FocusMonitor();

            monitor.OnFocused += (prev, current) =>
            {
                Debug.WriteLine("Yeah! " + prev + " -> " + current);

                SetWindowLong(prev, GWL_EXSTYLE, GetWindowLong(prev, GWL_EXSTYLE) | WS_EX_LAYERED);
                SetLayeredWindowAttributes(prev, 0, 255, LWA_ALPHA);

                SetWindowLong(current, GWL_EXSTYLE, GetWindowLong(current, GWL_EXSTYLE) | WS_EX_LAYERED);
                SetLayeredWindowAttributes(current, 0, 128, LWA_ALPHA);
            };

            monitor.StartListening();

            WindowStickHelper stickHelper = new WindowStickHelper(this);

            stickHelper.StickWindowToTopOnMove();
        }


        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }
    }
}
