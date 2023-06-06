using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FocusTimer.Lib.Component;
using System.Windows.Media.TextFormatting;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.ConditionalDraw;
using System.Windows.Media;
using System.Diagnostics;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using LiveChartsCore.Kernel;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using FocusTimer.Features.Charting.Processing;
using FocusTimer.Features.Charting.Repository;
using FocusTimer.Features.Charting.Entity;

namespace FocusTimer.Features.Charting
{
    internal class ChartViewModel : BaseModel
    {
        public delegate void ChartNeedsUpdateEventHandler();
        public event ChartNeedsUpdateEventHandler? OnChartNeedsUpdate;

        public void Loaded()
        {
            var context = new FocusTimerDatabaseContext();

            LiveCharts.Configure(config =>
                config
                    // registers SkiaSharp as the library backend
                    // REQUIRED unless you build your own
                    .AddSkiaSharp()

                    // select a theme, default is Light
                    // OPTIONAL
                    .AddDarkTheme()
                    );

            SeriesCollection1 = ChartDataProcessor.GetUpperChartSeries(
                UsageRepository.GetAppUsages(),
                UsageRepository.GetTimerUsages()
            );
            SeriesCollection2 = ChartDataProcessor.GetLowerChartSeries(
                UsageRepository.GetAppUsages(),
                UsageRepository.GetTimerUsages()
            );

            (SeriesCollection1.Last() as ColumnSeries<DataPoint>).ChartPointPointerDown += (a, b) =>
            {
                DateSelected(b.Model.DateTime);
            };

            (SeriesCollection2.Last() as StackedColumnSeries<DataPoint>).ChartPointPointerDown += (a, b) =>
            {
                DateSelected(b.Model.DateTime);
            };

            SharedXAxis = new Axis[] {
                new Axis() {
                    LabelsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        FontFamily = "맑은 고딕"
                    },
                    Labeler = (value) =>
                    {
                        var date = new DateTime((long)value);

                        return date.ToString(date.Day == 1 ? "M월 d" : "dd");
                    },
                    UnitWidth = TimeSpan.FromDays(1).Ticks,
                    MinStep = TimeSpan.FromDays(1).Ticks,
                    ForceStepToMin = true,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 1,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    },
                }
            };

            YAxis = new Axis[] {
                new Axis() {
                    IsVisible = false,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                }
            };

            NotifyPropertyChanged(nameof(SeriesCollection1));
            NotifyPropertyChanged(nameof(SeriesCollection2));
            NotifyPropertyChanged(nameof(SharedXAxis));
            NotifyPropertyChanged(nameof(YAxis));

            
            context.Initialize();
        }

        public ObservableCollection<ISeries> SeriesCollection1 { get; set; }
        public ObservableCollection<ISeries> SeriesCollection2 { get; set; }
        public Axis[] SharedXAxis { get; set; }

        public Axis[] YAxis { get; set; }

        public IPaint<SkiaSharpDrawingContext> TooltipPaint { get; set; } = new SolidColorPaint(new SKColor(28, 49, 58))
        {
            FontFamily = "맑은 고딕"
        };

        public DateTime SelectedDate { get; set; }
        public string SelectedDateString
        {
            get
            {
                if (SelectedDate == DateTime.MinValue)
                {
                    return "지난 21일";
                }
                return SelectedDate.ToString("yyyy. MM. dd");
            }
        }

        public void DateSelected(DateTime date)
        {
            if (SelectedDate == date)
            {
                foreach (ColumnSeries<DataPoint> s in SeriesCollection1)
                {
                    s.ClearSelection();
                }
                foreach (StackedColumnSeries<DataPoint> s in SeriesCollection2)
                {
                    s.ClearSelection();
                }
                SelectedDate = DateTime.MinValue;
            }
            else
            {
                foreach (ColumnSeries<DataPoint> s in SeriesCollection1)
                {
                    s.SelectPoints(s.ActivePoints.Where(p => (p.Context.DataSource as DataPoint).DateTime == date));
                }
                foreach (StackedColumnSeries<DataPoint> s in SeriesCollection2)
                {
                    s.SelectPoints(s.ActivePoints.Where(p => (p.Context.DataSource as DataPoint).DateTime == date));
                }
                SelectedDate = date;
            }

            OnChartNeedsUpdate?.Invoke();

            NotifyPropertyChanged(nameof(SelectedDateString));
            NotifyPropertyChanged(nameof(SelectedDateUsages));
            NotifyPropertyChanged(nameof(PrimaryMetrics));
        }

        public IEnumerable<PrimaryMetricItem> PrimaryMetrics
        {
            get
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
                            Value = $"{100 * UsageRepository.GetAppUsages().Where(u => u.IsConcentrated).Sum(u => u.Usage) / UsageRepository.GetTimerUsages().Sum(u => u.Usage):00}%"
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
                            Value = $"{100 * UsageRepository.GetAppUsages().Where(u => u.RegisteredAt.Date == SelectedDate.Date).Where(u => u.IsConcentrated).Sum(u => u.Usage) / UsageRepository.GetTimerUsages().Where(u => u.StartedAt.Date == SelectedDate.Date).Sum(u => u.Usage):00}%"
                        }
                    };
                }
            }
        }

        public IEnumerable<AppUsageItem> SelectedDateUsages
        {
            get
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

                    return usages.Select(u => new AppUsageItem
                    {
                        Date = u.RegisteredAt.Date,
                        AppPath = u.AppPath,
                        UsageString = TickToMinutes(usages.Where(au => au.AppPath == u.AppPath).Sum(au => au.Usage)),
                        UsagesByTime = usages.Where(au => au.AppPath == u.AppPath).Select(au => new UsageByTimeItem
                        {
                            TimeString = $"{au.RegisteredAt:HH:mm} ~ {au.UpdatedAt:HH:mm}",
                            UsageString = TickToMinutes(u.Usage)
                        }),
                    });
                }
            }
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
}
