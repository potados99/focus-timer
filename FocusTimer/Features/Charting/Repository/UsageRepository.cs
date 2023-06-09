﻿using FocusTimer.Features.Charting.Entity;
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

        public static AppUsage CreateAppUsage()
        {
            var usage = new AppUsage
            {
                RegisteredAt = DateTime.Now
            };

            WritingContext.AddAppUsage(usage);

            return usage;
        }

        public static TimerUsage CreateTimerUsage()
        {
            var usage = new TimerUsage
            {
                StartedAt = DateTime.Now
            };

            WritingContext.AddTimerUsage(usage);

            return usage;
        }

        public static void Save()
        {
            WritingContext.Save();
        }
    }
}
