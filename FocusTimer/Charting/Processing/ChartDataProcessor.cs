using FocusTimer.Charting.Entity;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting.Processing
{
    public static class ChartDataProcessor
    {
        internal static ObservableCollection<ColumnSeries<DataPoint>> GetUpperChartSeries(
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

            return new ObservableCollection<ColumnSeries<DataPoint>> {
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

        internal static ObservableCollection<StackedColumnSeries<DataPoint>> GetLowerChartSeries(
            IEnumerable<AppUsage> appUsages,
            IEnumerable<TimerUsage> timerUsages
            )
        {
            var series = new ObservableCollection<StackedColumnSeries<DataPoint>>();

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
                    }).ToArray(),
                    TooltipLabelFormatter = (d) => $"{thisAppUsage.AppName} {d.PrimaryValue}%",
                    MaxBarWidth = 16,
                    Rx = 5,
                    Ry = 5,
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
                TooltipLabelFormatter = (d) => $"미등록 프로그램 {d.PrimaryValue}%",
                MaxBarWidth = 16,
                Rx = 5,
                Ry = 5,
                Fill = new SolidColorPaint(SKColors.Gray)
            });

            return series;
        }

        public static Context GenerateDummy()
        {
            var c = new Context();

            while (c.IsItNotDone)
            {
                if (c.OutOfWorkingHour)
                {
                    // 아무것도 안 해요
                }
                else if (c.IsFocusing)
                {
                    var app = c.GetOrCreateCurrentAppUsage();

                    app.Usage += new TimeSpan(0, 1, 0).Ticks;
                } 
                else if (c.IsFocusingFinished)
                {
                    if (c.WithHalfChance)
                    {
                        // 절반 확률로 쉬거나
                        c.StartResting();
                    } else
                    {
                        // 바로 다른거 하러 갑니다.
                        c.StartFocusing();
                    }
                }
                else if (c.IsResting)
                {
                    // 놀 때에는 아무 것도 안 해요
                } 
                else if (c.IsRestingFinished)
                {
                    // 다 쉬었으면 바로 다른거 하러 갑니다.
                    c.StartFocusing();
                }
                else
                {
                    // 집중하지도 놀지도 않고 있을 때에는
                    // 바로 집중 시작합니다.
                    c.StartFocusing();
                }

                // 타이머는 켜져 있고,
                c.GetOrCreateTimerUsage().Usage += new TimeSpan(0, 1, 0).Ticks;

                // 암튼 시간은 흐릅니다.
                c.TimeGoes();
            }

            return c;
        }

    }

    public class Context
    {
        private DateTime CurrentDateTime = DateTime.Now.Subtract(new TimeSpan(14, 0, 0, 0));
        private DateTime UntilNow = DateTime.Now;
        
        private string[] AppNames = new string[]
            {
                "AppOne",
                "AppTwo",
                "AppThree",
                "AppFour"
            };

        public DateTime? FocusOnUntil { get; set; }
        public DateTime? TakeARestUntil { get; set; }

        private int WorkingHourBegin = 7;
        private int WorkingHourEnd = 18;

        private Random r = new Random();
        public string? CurrentAppName { get; set; } = null;

        public List<AppUsage> AppUsages = new();
        public List<TimerUsage> TimerUsages = new();

        public bool OutOfWorkingHour
        {
            get
            {
                return CurrentDateTime.Hour > WorkingHourEnd || CurrentDateTime.Hour < WorkingHourBegin;
            }
        }

        public bool WithHalfChance
        {
            get
            {
                return r.Next(0, 1) > 0;
            }
        }

        public bool IsItNotDone
        {
            get
            {
                return CurrentDateTime < UntilNow;
            }
        }

        public bool IsItZeroOClock
        {
            get
            {
                return CurrentDateTime.Hour == 0 && CurrentDateTime.Minute == 0;
            }
        }

        public bool IsFocusing
        {
            get
            {
                return FocusOnUntil != null && CurrentDateTime < FocusOnUntil;
            }
        }

        public bool IsFocusingFinished
        {
            get
            {
                return FocusOnUntil != null && (FocusOnUntil - CurrentDateTime)?.TotalMinutes == 0;
            }
        }

        public bool IsResting
        {
            get
            {
                return TakeARestUntil != null && CurrentDateTime < TakeARestUntil;
            }
        }

        public bool IsRestingFinished
        {
            get
            {
                return TakeARestUntil != null && (TakeARestUntil - CurrentDateTime)?.TotalMinutes == 0;
            }
        }

        public void TimeGoes()
        {
            if (IsItZeroOClock)
            {
                var yesterday = CurrentDateTime.Date.Subtract(new TimeSpan(1, 0, 0, 0));
                var usagesOfYesterday = AppUsages.Where(u => u.RegisteredAt.Date == yesterday);

                AppUsages.AddRange(
                    usagesOfYesterday.Select(u => new AppUsage
                    {
                        AppName = u.AppName,
                        Usage = 0,
                        RegisteredAt = new DateTime(CurrentDateTime.Ticks),
                        UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                        IsConcentrated = true
                    }).ToList());

                TimerUsages.Add(new TimerUsage
                {
                    Usage = 0,
                    StartedAt = new DateTime(CurrentDateTime.Ticks),
                    UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                });

                WorkingHourBegin = r.Next(6, 12);
                WorkingHourEnd = r.Next(14, 22);
            }

            var appUsagesOfToday = AppUsages.Where(u => u.RegisteredAt.Date == CurrentDateTime.Date);

            foreach (var usage in appUsagesOfToday)
            {
                usage.UpdatedAt = new DateTime(CurrentDateTime.Ticks);
            }

            var timerUsagesOfToday = TimerUsages.Where(u => u.StartedAt.Date == CurrentDateTime.Date);

            foreach (var usage in timerUsagesOfToday)
            {
                usage.UpdatedAt = new DateTime(CurrentDateTime.Ticks);
            }
               
            CurrentDateTime = CurrentDateTime.AddMinutes(1);
        }

        public AppUsage GetOrCreateCurrentAppUsage()
        {
            var u = AppUsages.LastOrDefault(u => u.AppName == CurrentAppName && u.RegisteredAt.Date == CurrentDateTime.Date);

            if (u == null)
            {
                u = new AppUsage
                {
                    AppName = CurrentAppName,
                    Usage = 0,
                    RegisteredAt = new DateTime(CurrentDateTime.Ticks),
                    UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                    IsConcentrated = true
                };

                AppUsages.Add(u);
            }

            return u;
        }

        public TimerUsage GetOrCreateTimerUsage()
        {
            var u = TimerUsages.LastOrDefault(u => u.StartedAt.Date == CurrentDateTime.Date);

            if (u == null)
            {
                u = new TimerUsage
                {
                    Usage = 0,
                    StartedAt = new DateTime(CurrentDateTime.Ticks),
                    UpdatedAt = new DateTime(CurrentDateTime.Ticks),
                };

                TimerUsages.Add(u);
            }

            return u;
        }

        public void StartFocusing()
        {
            TakeARestUntil = null;
            FocusOnUntil = CurrentDateTime.AddMinutes(r.Next(1, 60));
            CurrentAppName = AppNames[r.Next(0, AppNames.Length - 1)];
        }

        public void StartResting()
        {
            FocusOnUntil = null;
            TakeARestUntil = CurrentDateTime.AddMinutes(r.Next(10, 100));
            CurrentAppName = null;
        }
    }
}
