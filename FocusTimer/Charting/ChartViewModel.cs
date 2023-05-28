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
using FocusTimer.Charting;
using LiveChartsCore.Defaults;
using System.Collections.ObjectModel;
using LiveChartsCore.Kernel;

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
                new ColumnSeries<DataPoint> { 
                    Name = "집중도",
                    Values = values1,
                    TooltipLabelFormatter = (d) => $"집중도 {d.PrimaryValue}%",
                    Fill = new SolidColorPaint(SKColors.Orange),
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

            foreach (ColumnSeries<DataPoint> s in SeriesCollection1)
            {
                s.MaxBarWidth = 20;
                s.Rx = 4;
                s.Ry = 4;
            }

            foreach (StackedColumnSeries<DataPoint> s in SeriesCollection2)
            {
                s.MaxBarWidth = 20;
                s.Rx = 4;
                s.Ry = 4;
            }

            (SeriesCollection1.Last() as ColumnSeries<DataPoint>).ChartPointPointerDown += (a, b) =>
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

                        return date.ToString(date.Day == 1 ? "M월 d" : "dd");
                    },
                    UnitWidth = TimeSpan.FromDays(1).Ticks,
                    MinStep = TimeSpan.FromDays(1).Ticks,
                    MinLimit = values1[36].DateTime.Ticks,
                    MaxLimit = values1[49].DateTime.Ticks,
                    ForceStepToMin = true
                }
            };

            NotifyPropertyChanged(nameof(SeriesCollection1));
            NotifyPropertyChanged(nameof(SeriesCollection2));
            NotifyPropertyChanged(nameof(SharedXAxis));
        }

        public ObservableCollection<ISeries> SeriesCollection1 { get; set; }
        public ObservableCollection<ISeries> SeriesCollection2 { get; set; }
        public Axis[] SharedXAxis { get; set; }

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
                    return "지난 14일";
                }
                return SelectedDate.ToString("yyyy.MM.dd");
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
            
            NotifyPropertyChanged(nameof(SelectedDateString));
        }
    }
}
