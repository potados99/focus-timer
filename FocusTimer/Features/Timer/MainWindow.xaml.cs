using FocusTimer.Features.Charting;
using FocusTimer.Lib.Utility;
using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Threading;
using System.Threading.Tasks;

namespace FocusTimer.Features.Timer
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private log4net.ILog Logger { get { return this.GetLogger(); } }

        public MainWindow()
        {
#if !DEBUG
            HandleUnhandledExceptions();
#endif

            InitializeComponent();

            DataContext = ViewModel;

            new BorderWindow
            {
                DataContext = ViewModel
            }.Show();
        }

        private void HandleUnhandledExceptions()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            Crashes.ShouldProcessErrorReport = (ErrorReport report) => true;
         
            AppCenter.Start("66ce761f-2892-41cd-b8f7-95ba75670719", typeof(Analytics), typeof(Crashes));
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Logger.Fatal("처리되지 않은 예외가 발생하여 런타임을 종료합니다.", e);

            // 이 곳에서 modal dialog로 시간을 오래 끌게 되면 아래 코드가 실행되지 않습니다.
            // 따라서 새 스레드로 빠르게 넘깁니다.
            new Thread(() => MessageBox.Show("미처 예상하지 못한 문제가 발생하여 프로그램을 종료하게 되었습니다. 프로그램을 다시 실행하면 이 문제에 대한 자세한 정보가 전송됩니다.", "죄송합니다.")).Start();

            bool didAppCrash = Crashes.HasCrashedInLastSessionAsync().GetAwaiter().GetResult();
            if (didAppCrash)
            {
                Logger.Info("이전에 발생한 크래시가 있어, 프로그램 종료 전까지 기다립니다.");
                var tcs = new TaskCompletionSource<bool>();
                
                Crashes.SentErrorReport += (sender, e) =>
                {
                    Logger.Info("크래시를 보고하였습니다. 이제 프로그램을 종료합니다.");
                    tcs.SetResult(true);
                };

                Crashes.FailedToSendErrorReport += (sender, e) =>
                {
                    Logger.Info("크래시를 보내는 데에 실패하였습니다. 이제 프로그램을 종료합니다.");
                    tcs.SetResult(false);
                };
                
                Task.Delay(30 * 1000).ContinueWith(t => {
                    Logger.Info("크래시를 보내는 작업이 30초를 초과하였습니다. 이제 프로그램을 종료합니다.");
                    tcs.SetResult(false);
                });

                tcs.Task.GetAwaiter().GetResult();
                Application.Current.Shutdown();
            }
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

        private void ResetItem_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.ResetTimer();
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
