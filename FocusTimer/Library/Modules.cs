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
    private static readonly ServiceProvider s_provider =
        ConfigureServices(new ServiceCollection()).BuildServiceProvider();

    public static T Get<T>()
    {
        return s_provider.GetService<T>()!;
    }

    public static IServiceCollection ConfigureServices(IServiceCollection services)
    {
        // ViewModels
        services
            .AddTransient<MainViewModel>()
            .AddTransient<ChartViewModel>()
            .AddTransient<LicenseViewModel>();

        // Services
        services
            .AddSingleton<EventService>()
            .AddSingleton<LicenseService>()
            .AddSingleton<MigrationService>()
            .AddSingleton<AppService>()
            .AddSingleton<AppUsageService>()
            .AddSingleton<TimerUsageService>()
            .AddSingleton<SlotService>();

        // Repositories
        services
            .AddSingleton<FocusRepository>();

        // Others
        services
            .AddSingleton<ClockGenerator>()
            .AddSingleton<FocusTimerDatabaseContext>()
            .AddSingleton<UserActivityMonitor>()
            .AddSingleton<ChartDataProcessingService>()
            .AddSingleton<WindowWatcher>();

        return services;
    }
}