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

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            FocusMonitor monitor = new FocusMonitor();

            monitor.OnFocused += (prev, current) =>
            {
                //SetWindowLong(current, GWL_EXSTYLE, GetWindowLong(current, GWL_EXSTYLE) | WS_EX_LAYERED);
                //SetLayeredWindowAttributes(current, 0, 255, LWA_ALPHA);

                //SetWindowLong(prev, GWL_EXSTYLE, GetWindowLong(prev, GWL_EXSTYLE) | WS_EX_LAYERED);
                //SetLayeredWindowAttributes(prev, 0, 128, LWA_ALPHA);

                var allowList = new List<string>() { "focus", "devenv" };

                API.GetWindowThreadProcessId(current, out var processId);

                string processName = Process.GetProcessById((int)processId).ProcessName;
                
                Debug.WriteLine("Yeah! " + prev + " -> " + current + ". The [" + processName + "] got focus!");

                if (allowList.IndexOf(processName) < 0)
                {
                    Debug.WriteLine($"No, [{processName}] is not allowd!");
                    MessageBox.Show($"{processName}은(는) 지금 사용하실 수 없어요!", "안 돼요!");

                    API.SetForegroundWindow(prev);
                }
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
