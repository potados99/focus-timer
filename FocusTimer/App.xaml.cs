// App.xaml.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using FocusTimer.Library.Extensions;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace FocusTimer;

public partial class App
{
    private log4net.ILog Logger => this.GetLogger();

    [STAThread]
    public static void Main()
    {
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

#if !DEBUG
        HandleUnhandledExceptions();
#endif
    }

    private void HandleUnhandledExceptions()
    {
        AppDomain.CurrentDomain.UnhandledException += UnhandledExceptionHandler;

        Crashes.ShouldProcessErrorReport = _ => true;

        AppCenter.Start("66ce761f-2892-41cd-b8f7-95ba75670719", typeof(Analytics), typeof(Crashes));
    }

    private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args)
    {
        Exception e = (Exception) args.ExceptionObject;
        Logger.Fatal("처리되지 않은 예외가 발생하여 런타임을 종료합니다.", e);

        // 이 곳에서 modal dialog로 시간을 오래 끌게 되면 아래 코드가 실행되지 않습니다.
        // 따라서 새 스레드로 빠르게 넘깁니다.
        new Thread(() =>
            MessageBox.Show("미처 예상하지 못한 문제가 발생하여 프로그램을 종료하게 되었습니다. 프로그램을 다시 실행하면 이 문제에 대한 자세한 정보가 전송됩니다.",
                "죄송합니다.")).Start();

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

            Task.Delay(30 * 1000).ContinueWith(t =>
            {
                Logger.Info("크래시를 보내는 작업이 30초를 초과하였습니다. 이제 프로그램을 종료합니다.");
                tcs.SetResult(false);
            });

            tcs.Task.GetAwaiter().GetResult();
            Application.Current.Shutdown();
        }
    }
}