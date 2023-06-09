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
        private static FocusTimerDatabaseContext context = new();

        public static IEnumerable<AppUsage> GetAppUsages()
        {
            return context.AppUsages;
        }

        public static IEnumerable<TimerUsage> GetTimerUsages()
        {
            return context.TimerUsages;
        }

        public static AppUsage CreateAppUsage(string appPath, bool IsConcentrated = true)
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
            return usage;
        }

        public static TimerUsage CreateTimerUsage()
        {
            var usage = new TimerUsage
            {
                StartedAt = DateTime.Now,
                Usage = 0
            };
            context.TimerUsages.Add(usage);
            return usage;
        }

        public static async Task SaveChanges()
        {
            await context.SaveChangesAsync();
        }
    }
}
