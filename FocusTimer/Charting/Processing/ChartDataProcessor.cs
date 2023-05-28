using FocusTimer.Charting.Entity;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting.Processing
{
    public static class ChartDataProcessor
    {
        internal static ColumnSeries<DataPoint>[] GetUpperChartSeries(
            IEnumerable<AppUsage> appUsages,
            IEnumerable<TimerUsage> timerUsages
            )
        {

            var usagesPerDay = timerUsages.GroupBy(u => u.StartedAt.Date).Select(g => new
            {
                Date = g.Key,
                AppUsages = appUsages.Where(u => u.RegisteredAt.Date == g.Key),
                TimerUsages = g
            });

            var values = usagesPerDay.Select(u => new DataPoint { 
                DateTime = u.Date,
                Value = 100 * u.AppUsages.Where(au => au.IsConcentrated).Sum(au => au.Usage) / u.TimerUsages.Sum(tu => tu.Usage)
            });

            return new ColumnSeries<DataPoint>[] {
                new ColumnSeries<DataPoint>
                {
                    Name = "집중도",
                    Values = values,
                    TooltipLabelFormatter = (d) => $"집중도 {d.PrimaryValue}%",
                    Fill = new SolidColorPaint(SKColors.Orange),
                    MaxBarWidth = 16,
                    Rx = 5,
                    Ry = 5
                }
            };
        }

        internal static StackedColumnSeries<DataPoint>[] GetLowerChartValeus(
            IEnumerable<AppUsage> appUsages,
            IEnumerable<TimerUsage> timerUsages
            )
        {
            var series = new List<StackedColumnSeries<DataPoint>>();

            var usagesPerApp = appUsages.Select(u => u.AppName).Distinct().Select(name => new {
                AppName = name,
                AppUsagesPerDay = timerUsages.Select(tu => tu.StartedAt.Date).Select(d => new
                {
                    Date = d,
                    Usages = appUsages.Where(au => au.RegisteredAt.Date == d && au.AppName == name)
                })
            });

            foreach (var thisAppUsage in usagesPerApp)
            {
                series.Add(new StackedColumnSeries<DataPoint>
                {
                    Name = thisAppUsage.AppName,
                    Values = thisAppUsage.AppUsagesPerDay.Select(u => new DataPoint
                    {
                        DateTime = u.Date,
                        Value = Math.Ceiling(new TimeSpan(u.Usages.Sum(uu => uu.Usage)).TotalMinutes)
                    }),
                    TooltipLabelFormatter = (d) => $"{thisAppUsage.AppName} {d.PrimaryValue}%",
                    MaxBarWidth = 16,
                    Rx = 5,
                    Ry = 5
                });
            }

            var idle = timerUsages.GroupBy(u => u.StartedAt.Date).Select(g => new
            {
                Date = g.Key,
                IdleMinutes = Math.Ceiling(
                    new TimeSpan(g.Sum(tu => tu.Usage) - appUsages.Where(au => au.RegisteredAt.Date == g.Key).Sum(au => au.Usage))
                    .TotalMinutes
                    )
            });

            series.Add(new StackedColumnSeries<DataPoint>
            {
                Name = "미등록 프로그램",
                Values = idle.Select(u => new DataPoint
                {
                    DateTime = u.Date,
                    Value = u.IdleMinutes
                }),
                TooltipLabelFormatter = (d) => $"미등록 프로그램 {d.PrimaryValue}%",
                MaxBarWidth = 16,
                Rx = 5,
                Ry = 5,
                Fill = new SolidColorPaint(SKColors.Gray)
            });

            return series.ToArray();
        }

    }
}
