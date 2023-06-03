﻿using FocusTimer.Features.Charting.Entity;
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

        public static IEnumerable<AppUsage> GetAppUsages()
        {
            return c.AppUsages;
        }

        public static IEnumerable<TimerUsage> GetTimerUsages()
        {
            return c.TimerUsages;
        }
    }
}