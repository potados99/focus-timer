// Modules.cs
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
using FocusTimer.Data.DataContext;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Services;
using FocusTimer.Features.Charting;
using FocusTimer.Features.License;
using FocusTimer.Features.Timer;
using FocusTimer.Library.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace FocusTimer.Library;

/// <summary>
/// 의존성으로 주입할 모듈들을 정의합니다.
/// </summary>
public static class Modules
{
    private static ServiceProvider? _provider;

    /// <summary>
    /// 이 객체를 사용하려면 이 메소드를 시작 시점에 호출해주어야 합니다.
    /// </summary>
    public static void Initialize()
    {
        _provider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();
    }

    private static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // ViewModels
        services
            .AddTransient<MainViewModel>()
            .AddTransient<ChartViewModel>()
            .AddTransient<LicenseViewModel>();

        // Services
        services
            .AddSingleton<EventService>()
            .AddSingleton<MigrationService>()
            .AddSingleton<AppService>()
            .AddSingleton<AppUsageService>()
            .AddSingleton<TimerUsageService>()
            .AddSingleton<SlotService>()
#if STEAM
            .AddSingleton<ILicenseService, LicenseDummyService>(); // 스팀 배포용이다? 라이센스 체크 생략합니다.
#else
            .AddSingleton<ILicenseService, LicenseCredentialService>(); // 아니라면 CredentialManager 구현을 사용합니다.
#endif

        // Repositories
        services
            .AddSingleton<FocusRepository>();

        // Others
        services
            .AddSingleton<ClockGenerator>()
            .AddSingleton<FocusTimerDatabaseContext>() // DB Context를 singleton으로 두는 것은 일반적으로 좋지 않지만, 여기서는 스레드가 하나뿐이니 넘어갑니다~
            .AddSingleton<UserActivityMonitor>()
            .AddSingleton<ChartDataProcessingService>()
            .AddSingleton<WindowWatcher>();

        return services;
    }

    /// <summary>
    /// 주어진 타입의 인스턴스를 가져옵니다.
    /// </summary>
    /// <remarks>
    /// <code>ServiceProvider.GetServices</code> 호출의 wrapper입니다.
    /// </remarks>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static T Get<T>()
    {
        if (_provider == null)
        {
            throw new Exception("의존성 주입기가 초기화되지 않았습니다. 아직 앱이 시작되지 않은 것 같습니다.");
        }

        return _provider.GetService<T>()!;
    }
}