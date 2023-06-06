using FocusTimer.Features.Charting;
using FocusTimer.Lib;
using SQLitePCL;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FocusTimer.Features.Timer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            DataContext = ViewModel;

            new BorderWindow
            {
                DataContext = ViewModel
            }.Show();
        }

        private readonly MainViewModel ViewModel = new();

        private ChartWindow? OpenedChartWindow = null;

        #region 이벤트 핸들러

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Loaded();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                ViewModel.CancelRegisteringApp();
            }
            if (e.Key == Key.System && e.SystemKey == Key.F4)
            {
                // ALT + F4가 떨어지면 앱을 종료합니다.
                Application.Current.Shutdown();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void ExitItem_Click(object sender, RoutedEventArgs e)
        {
            App.Current.Shutdown();
        }

        private void ChartItem_Click(object sender, RoutedEventArgs e)
        {
            if (OpenedChartWindow == null)
            {
                OpenedChartWindow = new ChartWindow();
                OpenedChartWindow.Closed += (_, _) => { OpenedChartWindow = null; };
            }

            OpenedChartWindow.Show();
            OpenedChartWindow.Activate();
        }

        private void InfoItem_Click(object sender, RoutedEventArgs e)
        {
            string? ver = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            MessageBox.Show(
                "Focus Timer\n" +
                "버전 " + ver.ToString() + "\n\n"
                );
        }

        private void StartFocusLockItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.StartFocusLockWithHold();
        }

        private void Lock_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button { ToolTip: ToolTip toolTip })
            {
                toolTip.IsOpen = true;
            }

            ViewModel.ToggleFocusLock();
        }

        #endregion

        #region 드래그 오버라이드

        Point prepoint;
        bool isDragInProgress;
        bool isMouseCaptured;
        bool isDraggedFar;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            isDragInProgress = true;
            prepoint = Mouse.GetPosition(this);

            CaptureMouse();
            isMouseCaptured = true;

            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!isDragInProgress || !isMouseCaptured)
            {
                return;
            }

            Point newmouse = Mouse.GetPosition(this);
            double top = (double)newmouse.Y - (double)prepoint.Y;
            double left = (double)newmouse.X - (double)prepoint.X;

            if (Math.Abs(top) > 0 || Math.Abs(left) > 0)
            {
                isDraggedFar = true;
            }

            SetValue(MainWindow.TopProperty, (Application.Current.MainWindow.Top + top));
            SetValue(MainWindow.LeftProperty, (Application.Current.MainWindow.Left + left));

            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            bool wasDraggedFar = isDraggedFar;

            isDragInProgress = false;
            isDraggedFar = false;
            ReleaseMouseCapture();
            isMouseCaptured = false;

            if (!wasDraggedFar)
            {
                base.OnMouseLeftButtonUp(e);

                ViewModel.ToggleExpanded();
            }
        }

        #endregion
    }
}
