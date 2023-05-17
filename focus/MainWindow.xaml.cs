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

        private Point InitialWindowLocation;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource.FromHwnd(helper.Handle).AddHook(HwndMessageHook);

            InitialWindowLocation = new Point(this.Left, this.Top);
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
                case WM_SIZING:
                case WM_MOVING:
                    {
                        WIN32Rectangle rectangle = (WIN32Rectangle)Marshal.PtrToStructure(lParam, typeof(WIN32Rectangle));

                        rectangle.Top = 0;
                        rectangle.Bottom = 250;

                        bHandled = true;

                        if (bHandled)
                        {
                            Marshal.StructureToPtr(rectangle, lParam, true);
                        }
                    }
                    break;

            }
            return IntPtr.Zero;
        }

    }
}
