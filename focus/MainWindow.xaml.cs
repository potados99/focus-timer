using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

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
            this.DataContext = vm;
        }

        private MainViewModel vm = new MainViewModel();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // new Worker().StartWorking();

            // WindowStickHelper stickHelper = new WindowStickHelper(this);
            // stickHelper.StickWindowToTopOnMove();
        }

        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            //vm.ControlVisible = true;
        }

        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            Debug.WriteLine("Mouse left!");
            //vm.ControlVisible = false;
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void InfoItem_Click(object sender, RoutedEventArgs e)
        {
            var ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            MessageBox.Show(
                "Focus Timer\n" +
                "버전 " + ver.ToString() + "\n\n"
                );
        }








        int xx = 0;
        Point prepoint;
        private bool _IsDragInProgress { get; set; }
        private bool _IsDraggedFar { get; set; }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            this._IsDragInProgress = true;
            this.CaptureMouse();
            this.prepoint = Mouse.GetPosition(this);
            xx = 1;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!this._IsDragInProgress)
                return;
            if (xx == 0)
                return;

            Point newmouse = Mouse.GetPosition(this);
            double top = (double)newmouse.Y - (double)this.prepoint.Y;
            double left = (double)newmouse.X - (double)this.prepoint.X;

            if (Math.Abs(top) > 0 || Math.Abs(left) > 0)
            {
                Debug.WriteLine("Dragged so far");

                _IsDraggedFar = true;
            }

            this.SetValue(MainWindow.TopProperty, (Application.Current.MainWindow.Top + top));
            this.SetValue(MainWindow.LeftProperty, (Application.Current.MainWindow.Left + left));
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (_IsDraggedFar)
            {
                this._IsDragInProgress = false;
                this._IsDraggedFar = false;
                this.ReleaseMouseCapture();
                xx = 0;
                Debug.WriteLine("Dragged so far, so not firing button up.");
            }
            else
            {
                this._IsDragInProgress = false;
                this._IsDraggedFar = false;
                this.ReleaseMouseCapture();
                xx = 0;
                base.OnMouseLeftButtonUp(e);

                vm.ToggleExpanded();
            }

        }


    }
}
