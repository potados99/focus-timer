﻿// App.xaml.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.Windows;
using FocusTimer.Data.DataContext;
using FocusTimer.Features.Splash;
using FocusTimer.Library;
using FocusTimer.Library.Extensions;
using FocusTimer.Library.Utility;

namespace FocusTimer;

public partial class App
{
    private log4net.ILog Logger => this.GetLogger();
    
    [STAThread]
    public static void Main()
    {
        SplashWindow.ShowAutoClosed();
        
        var application = new App();
        application.InitializeComponent();
        application.Run();
    }
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Logger.Info("앱을 시작합니다.");
        
        Settings.UpgradeIfNeeded();
        Logger.Info("설정을 이전하였습니다.");
        
        Modules.Initialize();
        Logger.Info("의존성 주입기를 초기화하였습니다.");

        Modules.Get<FocusTimerDatabaseContext>().Initialize();
        Logger.Info("DB Context를 초기화하였습니다.");
        
        Culture.Initialize();
        Logger.Info("언어를 설정하였습니다.");

        Strings.Initialize();
        Logger.Info("스트링 리소스를 초기화하였습니다.");

        new AppCenterCrashes().SetupExceptionHandler();
    }
}