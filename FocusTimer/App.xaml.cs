// App.xaml.cs
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        // 오래된 로그 파일을 백그라운드에서 비동기로 정리 (UI 블로킹 방지)
        _ = Task.Run(() => CleanupOldLogFiles());

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

    /// <summary>
    /// log4net은 앱 시작 시 기존 로그 파일을 정리하지 않으므로,
    /// 수동으로 maxSizeRollBackups 설정에 맞춰 오래된 로그 파일을 삭제합니다.
    /// </summary>
    private void CleanupOldLogFiles()
    {
        try
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "World Moment", "Focus Timer", "logs"
            );

            if (!Directory.Exists(logDir))
            {
                return;
            }

            // 이 기간 만큼의 로그만 남겨놔요
            const int maxDaysToKeep = 7;

            // log4net.config 설정과 일치하는 패턴으로 정리
            // <file value="...\logs\app" />
            // <datePattern value="'-'yyyyMMdd'.log'" />
            // 결과: app-20260201.log
            DeleteOldLogFilesByPattern(logDir, "app", "-yyyyMMdd.log", maxDaysToKeep);
            DeleteOldLogFilesByPattern(logDir, "error", "-yyyyMMdd.log", maxDaysToKeep);

            // 구버전 패턴도 정리 (app.log-20260201)
            DeleteOldLogFilesByPattern(logDir, "app.log", "-yyyyMMdd", maxDaysToKeep);
            DeleteOldLogFilesByPattern(logDir, "error.log", "-yyyyMMdd", maxDaysToKeep);
        }
        catch
        {
            // 로그 정리 실패해도 앱 시작은 계속
        }
    }

    /// <summary>
    /// 특정 파일명 패턴에 맞는 오래된 로그 파일을 삭제합니다.
    /// </summary>
    /// <param name="logDir">로그 디렉토리 경로</param>
    /// <param name="baseFileName">기본 파일명 (예: "app", "error", "app.log")</param>
    /// <param name="datePattern">날짜 패턴 (예: "-yyyyMMdd.log", "-yyyyMMdd")</param>
    /// <param name="daysToKeep">보관할 일수</param>
    private void DeleteOldLogFilesByPattern(string logDir, string baseFileName, string datePattern, int daysToKeep)
    {
        try
        {
            var cutoffDate = DateTime.Now.Date.AddDays(-daysToKeep);

            // datePattern을 정규식으로 변환
            // 예: "-yyyyMMdd.log" → "-(\d{8})\.log"
            // 예: "-yyyyMMdd" → "-(\d{8})"
            var regexDatePattern = datePattern.Replace("yyyyMMdd", @"(\d{8})");

            // 특수문자 이스케이프 후 날짜 부분만 그룹으로
            var escapedBase = Regex.Escape(baseFileName);
            var escapedPattern = Regex.Escape(datePattern).Replace("yyyyMMdd", @"(\d{8})");
            var fullPattern = $"^{escapedBase}{escapedPattern}$";

            var regex = new Regex(fullPattern);
            var searchPattern = $"{baseFileName}*";

            // EnumerateFiles로 스트리밍 방식 처리 (메모리 효율적)
            foreach (var filePath in Directory.EnumerateFiles(logDir, searchPattern))
            {
                var fileName = Path.GetFileName(filePath);
                var match = regex.Match(fileName);

                // 정규식 매칭 실패 시 스킵
                if (!match.Success || match.Groups.Count <= 1)
                {
                    continue;
                }

                var dateString = match.Groups[1].Value;

                // 날짜 파싱 및 기한 체크
                if (DateTime.TryParseExact(dateString, "yyyyMMdd", null,
                    System.Globalization.DateTimeStyles.None, out var date) && date < cutoffDate)
                {
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch
                    {
                        // 삭제 실패해도 무시 (파일이 사용 중일 수 있음)
                    }
                }
            }
        }
        catch
        {
            // 패턴 매칭 실패해도 무시
        }
    }
}