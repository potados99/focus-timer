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

        const int WM_SIZING = 0x0214;
        const int WM_MOVING = 0x0216;

        int InitialHeight = -1;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(HwndMessageHook);
        }

        public MainWindow()
        {
            InitializeComponent();

            FocusMonitor monitor = new FocusMonitor();

            monitor.OnFocused += (focusedProcessName) =>
            {
                Debug.WriteLine("Yeah! " + focusedProcessName);
            };

            monitor.StartListening();
        }

        private IntPtr HwndMessageHook(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            switch (msg)
            {
                case WM_MOVING:
                    {
                        WIN32Rectangle rectangle = (WIN32Rectangle)Marshal.PtrToStructure(lParam, typeof(WIN32Rectangle));

                        if (InitialHeight == -1)
                        {
                            InitialHeight = rectangle.Bottom - rectangle.Top;
                        }

                        rectangle.Top = 0;
                        rectangle.Bottom = InitialHeight;

                        bHandled = true;

                        Marshal.StructureToPtr(rectangle, lParam, true);                     
                    }
                    break;

            }
            return IntPtr.Zero;
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
