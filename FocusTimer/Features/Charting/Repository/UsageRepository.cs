using FocusTimer.Features.Charting.Entity;
using FocusTimer.Features.Charting.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Features.Charting.Repository
{
    public static class UsageRepository
    {
        private static Context c = ChartDataProcessor.GenerateDummy();

        private static FocusTimerDatabaseContext context = new();

        public static IEnumerable<AppUsage> GetAppUsages()
        {
            return c.AppUsages;
        }

        public static IEnumerable<TimerUsage> GetTimerUsages()
        {
            return c.TimerUsages;
        }

        public static async Task<AppUsage> CreateAppUsage(string appPath, bool IsConcentrated = true)
        {
            var usage = new AppUsage
            {
                AppPath = appPath,
                RegisteredAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                Usage = 0,
                IsConcentrated = IsConcentrated
            };
            context.AppUsages.Add(usage);
            await context.SaveChangesAsync();
            return usage;
        }

        public static async Task<TimerUsage> CreateTimerUsage()
        {
            var usage = new TimerUsage();
            context.TimerUsages.Add(usage);
            await context.SaveChangesAsync();
            return usage;
        }

        public static async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }
    }
}
