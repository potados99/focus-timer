﻿using LiveChartsCore.SkiaSharpView;
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
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using FocusTimer.Charting.Processing;

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

            var dummy = ChartDataProcessor.GenerateDummy();

            SeriesCollection1 = ChartDataProcessor.GetUpperChartSeries(dummy.AppUsages, dummy.TimerUsages);
            SeriesCollection2 = ChartDataProcessor.GetLowerChartSeries(dummy.AppUsages, dummy.TimerUsages);

            SeriesCollection1.Last().ChartPointPointerDown += (a, b) =>
            {
                DateSelected(b.Model.DateTime);
            };

            SeriesCollection2.Last().ChartPointPointerDown += (a, b) =>
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
                    ForceStepToMin = true,
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        StrokeThickness = 2,
                        PathEffect = new DashEffect(new float[] { 3, 3 })
                    },
                }
            };

            YAxis = new Axis[] {
                new Axis() {
                    SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
                    {
                        
                    }
                }
            };

            NotifyPropertyChanged(nameof(SeriesCollection1));
            NotifyPropertyChanged(nameof(SeriesCollection2));
            NotifyPropertyChanged(nameof(SharedXAxis));
            NotifyPropertyChanged(nameof(YAxis));
        }

        public ObservableCollection<ColumnSeries<DataPoint>> SeriesCollection1 { get; set; }
        public ObservableCollection<StackedColumnSeries<DataPoint>> SeriesCollection2 { get; set; }
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
