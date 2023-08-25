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

public class ChartDataProcessor
{
    private readonly FocusRepository _repository;
    
    public ChartDataProcessor(FocusRepository repository)
    {
        _repository = repository;
    }
    
    internal ObservableCollection<ISeries> GetUpperChartSeries(
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
            AppUsages = appUsages.Where(u => u.StartedAt.Date == d),
            TimerUsages = timerUsages.Where(u => u.StartedAt.Date == d)
        });

        var values = usagesPerDay.Select(u => new DataPoint
        {
            DateTime = u.Date,
            Value = Percentage(
                u.AppUsages.Where(au => au.IsConcentrated).Sum(au => au.Elapsed.Ticks),
                u.TimerUsages.Sum(tu => tu.Elapsed.Ticks)
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

    internal ObservableCollection<ISeries> GetLowerChartSeries(
        IEnumerable<AppUsage> appUsages,
        IEnumerable<TimerUsage> timerUsages
    )
    {
        var series = new ObservableCollection<ISeries>();

        // 존재하는 모든 종류의 앱의 경로를 중복 없이 가져옵니다.
        var appPaths = appUsages.Select(u => u.App.ExecutablePath).Distinct();

        // 앱별로 AppUsage를 분류합니다.
        var usagesPerApp = appPaths.Select(path =>
        {
            // 전 기간에 걸쳐 현재 앱의 사용량을 모두 가져옵니다.
            var thisAppUsageOnAllPeriod = appUsages.Where(au => au.App.ExecutablePath == path);

            // 앱별로 경로, 색상, 날짜별 사용량을 구합니다.
            return new
            {
                AppPath = path,
                FillColor = APIWrapper.ExtractAssociatedIcon(path).ToSKColor(),
                AppUsagesPerDay = Dates.Select(d => new
                {
                    Date = d,
                    Usages = thisAppUsageOnAllPeriod.Where(au => au.StartedAt.Date == d)
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
                    Value = u.Usages.Sum(uu => uu.Elapsed.Ticks)
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
            IdleTicks = timerUsages.Where(u => u.StartedAt.Date == d).Sum(tu => tu.Elapsed.Ticks) - appUsages.Where(au => au.StartedAt.Date == d).Sum(au => au.Elapsed.Ticks)
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
        if (selectedDate == DateTime.MinValue)
        {
            var usages = _repository.GetAppUsages();

            return new PrimaryMetricItem[]
            {
                new PrimaryMetricItem {
                    Name = "Avg. 타이머 가동",
                    Value = TickToMinutes((long)_repository.GetTimerUsages().GroupBy(u => u.StartedAt.Date).Average(g => g.Sum(u => u.Elapsed.Ticks)))
                },
                new PrimaryMetricItem
                {
                    Name = "Avg. 집중도",
                    Value = Percentage(
                        _repository.GetAppUsages().Where(u => u.IsConcentrated).Sum(u => u.Elapsed.Ticks),
                        _repository.GetTimerUsages().Sum(u => u.Elapsed.Ticks)
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
                    Value = TickToMinutes(_repository.GetTimerUsages().Where(u => u.StartedAt.Date == selectedDate.Date).Sum(u => u.Elapsed.Ticks))
                },
                new PrimaryMetricItem {
                    Name = "실제 사용",
                    Value = TickToMinutes(_repository.GetAppUsages().Where(u => u.StartedAt.Date == selectedDate.Date).Sum(u => u.Elapsed.Ticks))
                },
                new PrimaryMetricItem
                {
                    Name = "집중도",
                    Value = Percentage(
                        _repository.GetAppUsages().Where(u => u.StartedAt.Date == selectedDate.Date).Where(u => u.IsConcentrated).Sum(u => u.Elapsed.Ticks),
                        _repository.GetTimerUsages().Where(u => u.StartedAt.Date == selectedDate.Date).Sum(u => u.Elapsed.Ticks)
                    ) + "%"
                }
            };
        }
    }

    internal IEnumerable<AppUsageItem> GetAppUsagesAtDate(DateTime selectedDate)
    {
        if (selectedDate == DateTime.MinValue)
        {
            var usages = _repository.GetAppUsages();

            return usages.GroupBy(u => u.App.ExecutablePath).Select(thisAppGroup => new AppUsageItem
            {
                AppPath = thisAppGroup.Key,
                UsageString = TickToMinutes(thisAppGroup.Sum(g => g.Elapsed.Ticks)),
                UsagesByTime = new UsageByTimeItem[]
                {
                    new UsageByTimeItem
                    {
                        TimeString = "Avg. 사용 시간",
                        UsageString = TickToMinutes((long)thisAppGroup.GroupBy(u => u.StartedAt.Date).Average(g => g.Sum(u => u.Elapsed.Ticks)))
                    },
                    new UsageByTimeItem
                    {
                        TimeString = "Avg. 비활성 시간",
                        UsageString = TickToMinutes((long)thisAppGroup.GroupBy(u => u.StartedAt.Date).Average(g => g.Sum(u => (u.UpdatedAt - u.StartedAt).Ticks - u.Elapsed.Ticks)))
                    },
                }
            });
        }
        else
        {
            var usages = _repository.GetAppUsages().Where(u => u.StartedAt.Date == selectedDate.Date);

            return usages.GroupBy(u => u.App.ExecutablePath).Select(thisAppGroup => new AppUsageItem
            {
                Date = thisAppGroup.First().StartedAt.Date,
                AppPath = thisAppGroup.Key,
                UsageString = TickToMinutes(thisAppGroup.Sum(au => au.Elapsed.Ticks)),
                UsagesByTime = thisAppGroup.Select(au => new UsageByTimeItem
                {
                    TimeString = $"{au.StartedAt:HH:mm} ~ {au.UpdatedAt:HH:mm}",
                    UsageString = TickToMinutes(au.Elapsed.Ticks)
                }),
            });
        }
    }

    private int Percentage(long one, long that)
    {
        int percentage = 0;

        if (that > 0)
        {
            percentage = (int)Math.Round(100f * one / that);
        }

        return percentage;
    }

    private string TickToMinutes(long ticks)
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