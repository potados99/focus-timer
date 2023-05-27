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
using LiveChartsCore.SkiaSharpView.WPF;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.SKCharts;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.ConditionalDraw;
using System.Windows.Media;
using System.Diagnostics;
using FocusTimer.Charting;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;

namespace FocusTimer
{
    internal class ChartViewModel : BaseModel
    {
        public void Loaded()
        {
            LiveCharts.Configure(config =>
                config
                    // registers SkiaSharp as the library backend
                    // REQUIRED unless you build your own
                    .AddSkiaSharp()

                    // select a theme, default is Light
                    // OPTIONAL
                    .AddDarkTheme()
                    );

            var values1 = new DataPoint[50];
            var values2 = new DataPoint[50];
            var values3 = new DataPoint[50];
            var values4 = new DataPoint[50];
            var values5 = new DataPoint[50];
            var r = new Random();

            for (var i = 0; i < 50; i++)
            {
                var date = new DateTime(2023, 04, 27).Add(new TimeSpan(i, 0, 0, 0));

                values2[i] = new DataPoint { DateTime = date, Value = r.Next(20, 60) };
                values1[i] = new DataPoint { DateTime = date, Value = r.Next(50, 80) };
                values3[i] = new DataPoint { DateTime = date, Value = r.Next(10, 20) };
                values4[i] = new DataPoint { DateTime = date, Value = r.Next(5, 10) };
                values5[i] = new DataPoint { DateTime = date, Value = r.Next(10, 25) };
            }

            SeriesCollection1 = new ObservableCollection<ISeries> { 
                new LineSeries<DataPoint> { 
                    Name = "집중도",
                    Values = values1,
                    TooltipLabelFormatter = (d) => $"집중도 {d.PrimaryValue}%",
                }
            };
            SeriesCollection2 = new ObservableCollection<ISeries> { 
                new StackedColumnSeries<DataPoint> {
                    Name = "Microsoft Visual Studio",
                    Values = values2,
                },
                new StackedColumnSeries<DataPoint> {
                    Name = "Google Chrome",
                    Values = values3,
                },
                new StackedColumnSeries<DataPoint> {
                    Name = "Windows Explorer",
                    Values = values4,
                },
                new StackedColumnSeries<DataPoint> {
                    Name = "미등록 프로그램",
                    Values = values5,
                    Fill = new SolidColorPaint(SKColors.Gray),
                }
            };

            foreach (LineSeries<DataPoint> s in SeriesCollection1)
            {
                s.WithConditionalPaint(new SolidColorPaint(SKColors.Black))
                .When(P => P.Model?.IsSelected ?? false);
            }

            foreach (StackedColumnSeries<DataPoint> s in SeriesCollection2)
            {
                s.WithConditionalPaint(new SolidColorPaint(SKColors.Black))
                .When(P => P.Model?.IsSelected ?? false);

                s.MaxBarWidth = 16;
                s.Rx = 5;
                s.Ry = 5;
            }

            (SeriesCollection1.Last() as LineSeries<DataPoint>).ChartPointPointerDown += (a, b) =>
            {
                DateSelected(b.Model.DateTime);
            };

            (SeriesCollection2.Last() as StackedColumnSeries<DataPoint>).ChartPointPointerDown += (a, b) =>
            {
                DateSelected(b.Model.DateTime);
            };

            // sharing the same instance for both charts will keep the zooming and panning synced 
            SharedXAxis = new Axis[] {
                new Axis() {
                    LabelsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        FontFamily = "맑은 고딕"
                    },

                    Labeler = (value) =>
                    {
                        var date = new DateTime((long)value);

                        return date.ToString(date.Day == 1 ? "M월 d일" : "d일");
                    },
                    UnitWidth = TimeSpan.FromDays(1).Ticks,
                    MinStep = TimeSpan.FromDays(1).Ticks,
                    MinLimit = values1[36].DateTime.Ticks,
                    MaxLimit = values1[49].DateTime.Ticks,
                    ForceStepToMin = true
                }
            };

            // Force the chart to use 70px margin on the left, this way we can align both charts. 
            DrawMargin = new Margin(20, Margin.Auto, 20, Margin.Auto);
            // and thats it!

            // Advanced alternative:
            // you can also ask an axis its posible dimensions to determine the margin you need.

            // First you need to get a chart from the UI
            // in this sample we use the in-memory chart provided by the library.

            // var cartesianChart = new SKCartesianChart();
            // var axis = cartesianChart.YAxes.First() as Axis;
            // var size = axis.GetPossibleSize(cartesianChart.Core);

            // finally instead of using the static 70px, we can use the actual width of the axis.

            // DrawMargin = new Margin(size.Width, Margin.Auto, Margin.Auto, Margin.Auto);

            // normally you would need measure all the axes involved, and use the greater width to
            // calculate the required margin.

            NotifyPropertyChanged(nameof(SeriesCollection1));
            NotifyPropertyChanged(nameof(SeriesCollection2));
            NotifyPropertyChanged(nameof(SharedXAxis));
            NotifyPropertyChanged(nameof(DrawMargin));
        }

        public ObservableCollection<ISeries> SeriesCollection1 { get; set; }
        public ObservableCollection<ISeries> SeriesCollection2 { get; set; }
        public Axis[] SharedXAxis { get; set; }
        public Margin DrawMargin { get; set; }

        public IPaint<SkiaSharpDrawingContext> TooltipPaint { get; set; } = new SolidColorPaint(new SKColor(28, 49, 58))
        {
            FontFamily = "맑은 고딕"
        };

        public string SelectedDate { get; set; }

        public void DateSelected(DateTime date)
        {
            foreach (LineSeries<DataPoint> s in SeriesCollection1)
            {
                foreach (var p in s.ActivePoints)
                {
                    var point = p.Context.DataSource as DataPoint;
                    var visual = p.Context.Visual as BezierPoint<LiveChartsCore.SkiaSharpView.Drawing.Geometries.CircleGeometry>;

                    if (point.DateTime == date)
                    {
                        visual.Bezier.Yi -= 5;
                        visual.Bezier.Yj -= 5;
                        visual.Bezier.Ym -= 5;

                        visual.Geometry.Y -= 5;
                    }
                }

                SelectedDate = date.ToString("M월 d일");
                NotifyPropertyChanged(nameof(SelectedDate));
            }

            foreach (StackedColumnSeries<DataPoint> s in SeriesCollection2)
            {
                foreach (var p in s.ActivePoints)
                {
                    var point = p.Context.DataSource as DataPoint;
                    var visual = p.Context.Visual as LiveChartsCore.SkiaSharpView.Drawing.Geometries.RoundedRectangleGeometry;

                    if (point.DateTime == date)
                    {
                        visual.X -= 5;
                        visual.Width += 10;
                    }
                    // point.IsSelected = point.DateTime == date;
                }
            }
        }
    }
}
