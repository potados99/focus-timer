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

            var values1 = new int[50];
            var values2 = new int[50];
            var values3 = new int[50];
            var values4 = new int[50];
            var values5 = new int[50];
            var r = new Random();

            for (var i = 0; i < 50; i++)
            {
                values1[i] = r.Next(50, 80);
                values2[i] = r.Next(20, 60);
                values3[i] = r.Next(10, 20);
                values4[i] = r.Next(5, 10);
                values5[i] = r.Next(10, 25);
            }

            SeriesCollection1 = new ISeries[] { 
                new LineSeries<int> { 
                    Name = "집중도",
                    Values = values1,
                    TooltipLabelFormatter = (d) => $"집중도 {d.PrimaryValue}%",
                }
            };
            SeriesCollection2 = new ISeries[] { 
                new StackedColumnSeries<int> {
                    Name = "Microsoft Visual Studio",
                    Values = values2,
                    TooltipLabelFormatter = (d) => $"Microsoft Visual Studio {d.PrimaryValue}분",
                },
                new StackedColumnSeries<int> {
                    Name = "Google Chrome",
                    Values = values3,
                    TooltipLabelFormatter = (d) => $"Google Chrome {d.PrimaryValue}분"
                },
                new StackedColumnSeries<int> {
                    Name = "Windows Explorer",
                    Values = values4,
                    TooltipLabelFormatter = (d) => $"Windows Explorer {d.PrimaryValue}분"
                },
                new StackedColumnSeries<int> {
                    Name = "미등록 프로그램",
                    Values = values5,
                    TooltipLabelFormatter = (d) => $"미등록 프로그램 {d.PrimaryValue}분",
                    Fill = new SolidColorPaint(SKColors.Gray),
                }
            };

            foreach (StackedColumnSeries<int> s in SeriesCollection2)
            {
                s.MaxBarWidth = 16;
                s.Rx = 5;
                s.Ry = 5;
            }

            var d = new string[] { "일", "월", "화", "수", "목", "금", "토" };
            // sharing the same instance for both charts will keep the zooming and panning synced 
            SharedXAxis = new Axis[] { 
                new Axis() {                 
                    LabelsPaint = new SolidColorPaint(SKColors.LightGray)
                    {
                        FontFamily = "맑은 고딕"
                    },
                    ForceStepToMin = true,
                    MinStep = 1,
                    MinLimit = 30,
                    MaxLimit = 49
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

        public ISeries[] SeriesCollection1 { get; set; }
        public ISeries[] SeriesCollection2 { get; set; }
        public Axis[] SharedXAxis { get; set; }
        public Margin DrawMargin { get; set; }

        public IPaint<SkiaSharpDrawingContext> TooltipPaint { get; set; } = new SolidColorPaint(new SKColor(28, 49, 58))
        {
            FontFamily = "맑은 고딕"
        };
    }
}
