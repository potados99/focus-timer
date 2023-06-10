using FocusTimer.Features.Charting.Entity;
using FocusTimer.Features.Charting.Repository;
using FocusTimer.Lib.Utility;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;

namespace FocusTimer.Features.Charting.Processing
{
    public static class ChartDataProcessor
    {
        internal static ObservableCollection<ISeries> GetUpperChartSeries(
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

            var values = usagesPerDay.Select(u => new DataPoint
            {
                DateTime = u.Date,
                Value = Percentage(
                    u.AppUsages.Where(au => au.IsConcentrated).Sum(au => au.Usage),
                    u.TimerUsages.Sum(tu => tu.Usage)
                    )
            });

            return new ObservableCollection<ISeries> {
                new ColumnSeries<DataPoint>
                {
                    Name = "집중도",
                    Values = values.ToArray(),
                    TooltipLabelFormatter = (d) => $"집중도 {d.PrimaryValue}%",
                    Fill = new SolidColorPaint(SKColors.Orange),
                    MaxBarWidth = 16,
                    Rx = 5,
                    Ry = 5
                }
            };
        }

        internal static ObservableCollection<ISeries> GetLowerChartSeries(
            IEnumerable<AppUsage> appUsages,
            IEnumerable<TimerUsage> timerUsages
            )
        {
            var series = new ObservableCollection<ISeries>();

            var usagesPerApp = appUsages.Select(u => u.AppPath).Distinct().Select(path => new
            {
                AppPath = path,
                FillColor = Icon.ExtractAssociatedIcon(path).ToSKColor(),
                AppUsagesPerDay = timerUsages.Select(tu => tu.StartedAt.Date).Select(d => new
                {
                    Date = d,
                    Usages = appUsages.Where(au => au.RegisteredAt.Date == d && au.AppPath == path)
                })
            });

            foreach (var thisAppUsage in usagesPerApp)
            {
                series.Add(new StackedColumnSeries<DataPoint>
                {
                    Name = thisAppUsage.AppPath,
                    Values = thisAppUsage.AppUsagesPerDay.Select(u => new DataPoint
                    {
                        DateTime = u.Date,
                        Value = Math.Ceiling(new TimeSpan(u.Usages.Sum(uu => uu.Usage)).TotalMinutes)
                    }).ToArray(),
                    Fill = new SolidColorPaint(thisAppUsage.FillColor),
                    MaxBarWidth = 16,
                    Rx = 4,
                    Ry = 4,
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
                }).ToArray(),
                MaxBarWidth = 16,
                Rx = 4,
                Ry = 4,
                Fill = new SolidColorPaint(SKColors.Gray)
            });

            return series;
        }

        internal static IEnumerable<PrimaryMetricItem> GetPrimaryMetrics(DateTime SelectedDate)
        {
            if (SelectedDate == DateTime.MinValue)
            {
                var usages = UsageRepository.GetAppUsages();

                return new PrimaryMetricItem[]
                {
                        new PrimaryMetricItem {
                            Name = "Avg. 타이머 가동",
                            Value = TickToMinutes((long)UsageRepository.GetTimerUsages().GroupBy(u => u.StartedAt.Date).Average(g => g.Sum(u => u.Usage)))
                        },
                        new PrimaryMetricItem
                        {
                            Name = "Avg. 집중도",
                            Value = Percentage(
                                UsageRepository.GetAppUsages().Where(u => u.IsConcentrated).Sum(u => u.Usage),
                                UsageRepository.GetTimerUsages().Sum(u => u.Usage)
                                ) + "%"
                        }
                };
            }
            else
            {
                return new PrimaryMetricItem[]
                {
                        new PrimaryMetricItem {
                            Name = "타이머 가동",
                            Value = TickToMinutes(UsageRepository.GetTimerUsages().Where(u => u.StartedAt.Date == SelectedDate.Date).Sum(u => u.Usage))
                        },
                        new PrimaryMetricItem {
                            Name = "실제 사용",
                            Value = TickToMinutes(UsageRepository.GetAppUsages().Where(u => u.RegisteredAt.Date == SelectedDate.Date).Sum(u => u.Usage))
                        },
                        new PrimaryMetricItem
                        {
                            Name = "집중도",
                            Value = Percentage(
                                UsageRepository.GetAppUsages().Where(u => u.RegisteredAt.Date == SelectedDate.Date).Where(u => u.IsConcentrated).Sum(u => u.Usage),
                                UsageRepository.GetTimerUsages().Where(u => u.StartedAt.Date == SelectedDate.Date).Sum(u => u.Usage)
                                ) + "%"
                        }
                };
            }
        }

        internal static IEnumerable<AppUsageItem> GetAppUsagesAtDate(DateTime SelectedDate)
        {
            if (SelectedDate == DateTime.MinValue)
            {
                var usages = UsageRepository.GetAppUsages();

                return usages.GroupBy(u => u.AppPath).Select(thisAppGroup => new AppUsageItem
                {
                    AppPath = thisAppGroup.Key,
                    UsageString = TickToMinutes(thisAppGroup.Sum(g => g.Usage)),
                    UsagesByTime = new UsageByTimeItem[]
                    {
                            new UsageByTimeItem
                            {
                                TimeString = "Avg. 사용 시간",
                                UsageString = TickToMinutes((long)thisAppGroup.GroupBy(u => u.RegisteredAt.Date).Average(g => g.Sum(u => u.Usage)))
                            },
                            new UsageByTimeItem
                            {
                                TimeString = "Avg. 비활성 시간",
                                UsageString = TickToMinutes((long)thisAppGroup.GroupBy(u => u.RegisteredAt.Date).Average(g => g.Sum(u => (u.UpdatedAt - u.RegisteredAt).Ticks - u.Usage)))
                            },
                    }
                });
            }
            else
            {
                var usages = UsageRepository.GetAppUsages().Where(u => u.RegisteredAt.Date == SelectedDate.Date);

                return usages.GroupBy(u => u.AppPath).Select(thisAppGroup => new AppUsageItem
                {
                    Date = thisAppGroup.First().RegisteredAt.Date,
                    AppPath = thisAppGroup.Key,
                    UsageString = TickToMinutes(thisAppGroup.Sum(au => au.Usage)),
                    UsagesByTime = thisAppGroup.Select(au => new UsageByTimeItem
                    {
                        TimeString = $"{au.RegisteredAt:HH:mm} ~ {au.UpdatedAt:HH:mm}",
                        UsageString = TickToMinutes(au.Usage)
                    }),
                });
            }
        }

        private static int Percentage(long one, long that)
        {
            int percentage = 0;

            if (that > 0)
            {
                percentage = (int)Math.Round(100f * one / that);
            }

            return percentage;
        }

        private static string TickToMinutes(long ticks)
        {
            var minutes = (int)Math.Ceiling(new TimeSpan(ticks).TotalMinutes);

            if (minutes >= 60)
            {
                return $"{minutes / 60}시간 {minutes % 60}분";
            }
            else
            {
                return $"{minutes % 60}분";
            }
        }
    }
}
