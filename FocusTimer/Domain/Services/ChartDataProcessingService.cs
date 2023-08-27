using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Entities;
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

namespace FocusTimer.Domain.Services;

public class ChartDataProcessingService
{
    private readonly FocusRepository _repository;

    public ChartDataProcessingService(FocusRepository repository)
    {
        _repository = repository;
    }

    private IEnumerable<AppRunningUsage> GetAppRunningUsages()
    {
        return _repository
            .GetAppUsages()
            .SelectMany(UsageSplitter<AppUsage>
                .GetRunningUsagesSplitByDate<AppUsage, AppRunningUsage, AppActiveUsage>);
    }

    private IEnumerable<TimerRunningUsage> GetTimerRunningUsages()
    {
        return _repository
            .GetTimerUsages()
            .SelectMany(UsageSplitter<TimerUsage>
                .GetRunningUsagesSplitByDate<TimerUsage, TimerRunningUsage, TimerActiveUsage>);
    }

    public ObservableCollection<ISeries> GetUpperChartSeries()
    {
        var appRunningUsages = GetAppRunningUsages();
        var timerRunningUsages = GetTimerRunningUsages();

        // 차트에 표시할 영역의 날짜들을 가져옵니다.
        var dates = Enumerable.Range(0, 1 + EndDate.Subtract(StartDate).Days)
            .Select(offset => StartDate.AddDays(offset))
            .ToArray();

        // 날짜마다 RunningUsage들을 묶습니다.
        var runningUsagesPerDay = dates.Select(d => new
        {
            Date = d,
            AppRunningUsages = appRunningUsages.Where(u => u.StartedAt.Date == d),
            TimerRunningUsages = timerRunningUsages.Where(u => u.StartedAt.Date == d)
        });

        // 날짜마다 집중도를 꺼내옵니다.
        var values = runningUsagesPerDay.Select(usagesOfDay => new DataPoint
        {
            DateTime = usagesOfDay.Date,
            Value = Percentage(
                usagesOfDay.AppRunningUsages.Where(u => u.ParentUsage.IsConcentrated)
                    .Sum(au => au.ActiveElapsed.Ticks), // 집중에 참여한 앱들이 포커스를 얻은 동안 흐른 시간
                usagesOfDay.TimerRunningUsages.Sum(u => u.Elapsed.Ticks) // 타이머가 켜져 있는 동안 흐른 시간
            )
        });

        return new ObservableCollection<ISeries>
        {
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

    private DateTime StartDate => DateTime.Now.Date.AddDays(-21);

    private DateTime EndDate => DateTime.Now.Date;

    private IEnumerable<DateTime> Dates
    {
        get
        {
            return Enumerable.Range(0, 1 + EndDate.Subtract(StartDate).Days)
                .Select(offset => StartDate.AddDays(offset))
                .ToArray();
        }
    }

    internal ObservableCollection<ISeries> GetLowerChartSeries()
    {
        var appRunningUsages = GetAppRunningUsages();
        var timerRunningUsages = GetTimerRunningUsages();

        var series = new ObservableCollection<ISeries>();

        // 존재하는 모든 종류의 앱의 경로를 중복 없이 가져옵니다.
        var appPaths = appRunningUsages.Select(u => u.ParentUsage.App.ExecutablePath).Distinct();

        // 앱별로 AppUsage를 분류합니다.
        var runningUsagesPerApp = appPaths.Select(path =>
        {
            // 전 기간에 걸쳐 현재 앱의 사용량을 모두 가져옵니다.
            var thisAppRunningUsageOnAllPeriod = appRunningUsages
                .Where(au => au.ParentUsage.App.ExecutablePath == path);

            // 앱별로 경로, 색상, 날짜별 사용량을 구합니다.
            return new
            {
                AppPath = path,
                FillColor = APIWrapper.ExtractAssociatedIcon(path).ToSKColor(),
                AppRunningUsagesPerDay = Dates.Select(d => new
                {
                    Date = d,
                    RunningUsages = thisAppRunningUsageOnAllPeriod.Where(au => au.StartedAt.Date == d)
                })
            };
        });

        // 앱마다 날짜별 사용량이 구해졌으니
        foreach (var thisRunningAppUsage in runningUsagesPerApp)
        {
            // 그것을 기반으로 series를 만들어 더해줍니다.
            series.Add(new StackedColumnSeries<DataPoint>
            {
                Name = thisRunningAppUsage.AppPath,
                Values = thisRunningAppUsage.AppRunningUsagesPerDay.Select(u => new DataPoint
                {
                    DateTime = u.Date,
                    Value = u.RunningUsages.Sum(uu => uu.ActiveElapsed.Ticks)
                }).ToArray(),
                Fill = new SolidColorPaint(thisRunningAppUsage.FillColor),
                MaxBarWidth = 16,
                Rx = 4,
                Ry = 4,
            });
        }

        var idle = Dates.Select(d => new
        {
            Date = d,
            IdleTicks = timerRunningUsages.Where(u => u.StartedAt.Date == d).Sum(tu => tu.Elapsed.Ticks) -
                        appRunningUsages.Where(au => au.StartedAt.Date == d).Sum(au => au.ActiveElapsed.Ticks)
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

    internal IEnumerable<PrimaryMetricItem> GetPrimaryMetrics(DateTime selectedDate)
    {
        var appRunningUsages = GetAppRunningUsages();
        var timerRunningUsages = GetTimerRunningUsages();

        if (selectedDate == DateTime.MinValue)
        {
            return new PrimaryMetricItem[]
            {
                new()
                {
                    Name = "Avg. 타이머 가동",
                    Value = TickToMinutes((long) timerRunningUsages.GroupBy(u => u.StartedAt.Date)
                        .Average(g => g.Sum(u => u.ActiveElapsed.Ticks)))
                },
                new()
                {
                    Name = "Avg. 집중도",
                    Value = Percentage(
                        appRunningUsages.Where(u => u.ParentUsage.IsConcentrated).Sum(u => u.ActiveElapsed.Ticks),
                        timerRunningUsages.Sum(u => u.Elapsed.Ticks)
                    ) + "%"
                }
            };
        }
        else
        {
            return new PrimaryMetricItem[]
            {
                new()
                {
                    Name = "타이머 가동",
                    Value = TickToMinutes(timerRunningUsages.Where(u => u.StartedAt.Date == selectedDate.Date)
                        .Sum(u => u.Elapsed.Ticks))
                },
                new()
                {
                    Name = "실제 사용",
                    Value = TickToMinutes(appRunningUsages.Where(u => u.StartedAt.Date == selectedDate.Date)
                        .Sum(u => u.ActiveElapsed.Ticks))
                },
                new()
                {
                    Name = "집중도",
                    Value = Percentage(
                        appRunningUsages.Where(u => u.StartedAt.Date == selectedDate.Date)
                            .Where(u => u.ParentUsage.IsConcentrated).Sum(u => u.ActiveElapsed.Ticks),
                        timerRunningUsages.Where(u => u.StartedAt.Date == selectedDate.Date)
                            .Sum(u => u.Elapsed.Ticks)
                    ) + "%"
                }
            };
        }
    }

    internal IEnumerable<AppUsageItem> GetAppUsagesAtDate(DateTime selectedDate)
    {
        if (selectedDate == DateTime.MinValue)
        {
            var appRunningUsages = GetAppRunningUsages();
            
            return appRunningUsages.GroupBy(u => u.ParentUsage.App.ExecutablePath).Select(thisAppGroup => new AppUsageItem
            {
                AppPath = thisAppGroup.Key,
                UsageString = TickToMinutes(thisAppGroup.Sum(g => g.ActiveElapsed.Ticks)),
                UsagesByTime = new UsageByTimeItem[]
                {
                    new()
                    {
                        TimeString = "Avg. 사용 시간",
                        UsageString = TickToMinutes((long) thisAppGroup.GroupBy(u => u.StartedAt.Date)
                            .Average(g => g.Sum(u => u.ActiveElapsed.Ticks)))
                    },
                    new()
                    {
                        TimeString = "Avg. 비활성 시간",
                        UsageString = TickToMinutes((long) thisAppGroup.GroupBy(u => u.StartedAt.Date)
                            .Average(g => g.Sum(u => u.Elapsed.Ticks - u.ActiveElapsed.Ticks)))
                    },
                }
            });
        }
        else
        {
            var appRunningUsages = GetAppRunningUsages().Where(u => u.StartedAt.Date == selectedDate.Date);
            
            return appRunningUsages.GroupBy(u => u.ParentUsage.App.ExecutablePath).Select(thisAppGroup => new AppUsageItem
            {
                Date = thisAppGroup.First().StartedAt.Date,
                AppPath = thisAppGroup.Key,
                UsageString = TickToMinutes(thisAppGroup.Sum(au => au.ActiveElapsed.Ticks)),
                UsagesByTime = thisAppGroup.Select(au => new UsageByTimeItem
                {
                    TimeString = $"{au.StartedAt:HH:mm} ~ {au.UpdatedAt:HH:mm}",
                    UsageString = TickToMinutes(au.ActiveElapsed.Ticks)
                }),
            });
        }
    }

    private int Percentage(long one, long that)
    {
        int percentage = 0;

        if (that > 0)
        {
            percentage = (int) Math.Round(100f * one / that);
        }

        return percentage;
    }

    private string TickToMinutes(long ticks)
    {
        var minutes = (int) Math.Ceiling(new TimeSpan(ticks).TotalMinutes);

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