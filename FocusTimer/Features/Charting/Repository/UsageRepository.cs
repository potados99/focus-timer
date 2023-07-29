using FocusTimer.Features.Charting.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FocusTimer.Features.Charting.Repository
{
    public static class UsageRepository
    {
        private static readonly FocusTimerDatabaseContext ReadingContext = new(true);
        private static readonly FocusTimerDatabaseContext WritingContext = new(false);

        private static readonly int LastHowManyDays = 21;

        public static IEnumerable<AppUsage> GetAppUsages()
        {
            var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

            return ReadingContext.AppUsages.Where(u => u.RegisteredAt >= then).OrderBy(u => u.RegisteredAt);
        } 

        public static IEnumerable<TimerUsage> GetTimerUsages()
        {
            var then = DateTime.Now.Date.Subtract(TimeSpan.FromDays(LastHowManyDays - 1));

            return ReadingContext.TimerUsages.Where(u => u.StartedAt >= then).OrderBy(u => u.StartedAt);
        }

        public static AppUsage CreateAppUsage(string appPath, bool closeOthers = true)
        {
            if (closeOthers)
            {
                WritingContext.CloseAppUsages(appPath);
            }

            var usage = new AppUsage
            {
                AppPath = appPath,
                RegisteredAt = DateTime.Now,
                IsOpen = true
            };

            WritingContext.AddAppUsage(usage);

            return usage;
        }

        public static IEnumerable<AppUsage> GetLastAppUsages(string appPath)
        {
            return WritingContext.AppUsages.Where(u => u.AppPath == appPath && u.IsOpen).AsEnumerable();
        }

        public static TimerUsage CreateTimerUsage(bool closeOthers = true)
        {
            if (closeOthers)
            {
                WritingContext.CloseTimerUsages();
            }

            var usage = new TimerUsage
            {
                StartedAt = DateTime.Now,
                IsOpen = true
            };

            WritingContext.AddTimerUsage(usage);

            return usage;
        }

        public static IEnumerable<TimerUsage> GetLastTimerUsages()
        {
            return WritingContext.TimerUsages.Where(u => u.IsOpen).AsEnumerable();
        }

        public static void Save()
        {
            WritingContext.Save();
        }
    }
}
