using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
using FocusTimer.Features.Charting;
using FocusTimer.Features.Charting.LiveCharts;
using FocusTimer.Features.Charting.Metric;
using FocusTimer.Features.Charting.Usages;
using FocusTimer.Features.Charting.Usages.Detail;
using FocusTimer.Lib;
using FocusTimer.Lib.Utility;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace FocusTimer.Domain.Services
{
    public static class ChartDataProcessor
    {
        internal static ObservableCollection<ISeries> GetUpperChartSeries(
            IEnumerable<AppUsage> appUsages,
            IEnumerable<TimerUsage> timerUsages
            )
        {
            // 차트에 표시할 영역의 날짜들을 가져옵니다.
            var dates = Enumerable.Range(0, 1 + EndDate.Subtract(StartDate).Days)
                .Select(offset => StartDate.AddDays(offset))
                .ToArray();

            var usagesPerDay = dates.Select(d => new
            {
                Date = d,
                AppUsages = appUsages.Where(u => u.RegisteredAt.Date == d),
                TimerUsages = timerUsages.Where(u => u.StartedAt.Date == d)
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

        private static DateTime StartDate
        {
            get
            {
                return DateTime.Now.Date.AddDays(-21);
            }
        }

        private static DateTime EndDate
        {
            get
            {
                return DateTime.Now.Date;
            }
        }

        private static IEnumerable<DateTime> Dates
        {
            get
            {
                return Enumerable.Range(0, 1 + EndDate.Subtract(StartDate).Days)
                .Select(offset => StartDate.AddDays(offset))
                .ToArray();
            }
        }

        internal static ObservableCollection<ISeries> GetLowerChartSeries(
            IEnumerable<AppUsage> appUsages,
            IEnumerable<TimerUsage> timerUsages
            )
        {
            var series = new ObservableCollection<ISeries>();

            // 존재하는 모든 종류의 앱의 경로를 중복 없이 가져옵니다.
            var appPaths = appUsages.Select(u => u.AppPath).Distinct();

            // 앱별로 AppUsage를 분류합니다.
            var usagesPerApp = appPaths.Select(path =>
            {
                // 전 기간에 걸쳐 현재 앱의 사용량을 모두 가져옵니다.
                var thisAppUsageOnAllPeriod = appUsages.Where(au => au.AppPath == path);

                // 앱별로 경로, 색상, 날짜별 사용량을 구합니다.
                return new
                {
                    AppPath = path,
                    FillColor = APIWrapper.ExtractAssociatedIcon(path).ToSKColor(),
                    AppUsagesPerDay = Dates.Select(d => new
                    {
                        Date = d,
                        Usages = thisAppUsageOnAllPeriod.Where(au => au.RegisteredAt.Date == d)
                    })
                };
            });

            // 앱마다 날짜별 사용량이 구해졌으니
            foreach (var thisAppUsage in usagesPerApp)
            {
                // 그것을 기반으로 series를 만들어 더해줍니다.
                series.Add(new StackedColumnSeries<DataPoint>
                {
                    Name = thisAppUsage.AppPath,
                    Values = thisAppUsage.AppUsagesPerDay.Select(u => new DataPoint
                    {
                        DateTime = u.Date,
                        Value = u.Usages.Sum(uu => uu.Usage)
                    }).ToArray(),
                    Fill = new SolidColorPaint(thisAppUsage.FillColor),
                    MaxBarWidth = 16,
                    Rx = 4,
                    Ry = 4,
                });
            }

            var idle = Dates.Select(d => new
            {
                Date = d,
                IdleTicks = timerUsages.Where(u => u.StartedAt.Date == d).Sum(tu => tu.Usage) - appUsages.Where(au => au.RegisteredAt.Date == d).Sum(au => au.Usage)
            });

            series.Add(new StackedColumnSeries<DataPoint>
            {
                Name = "미등록 프로그램",
                Values = idle.Select(u => new DataPoint
                {
                    DateTime = u.Date,
                    Value = u.IdleTicks
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
