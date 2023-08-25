using FocusTimer.Lib.Component;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Services;
using FocusTimer.Features.Charting.LiveCharts;
using FocusTimer.Features.Charting.Metric;
using FocusTimer.Features.Charting.Usages;
using FocusTimer.Lib;

namespace FocusTimer.Features.Charting;

public class ChartViewModel : BaseViewModel
{
    private readonly ChartDataProcessor _processor;
    private readonly FocusRepository _repository;
    
    public ChartViewModel(ChartDataProcessor processor, FocusRepository repository)
    {
        _processor = processor;
        _repository = repository;
    }

    public event Signal? OnChartNeedsUpdate;

    public void Loaded()
    {
        LiveChartsCore.LiveCharts.Configure(config =>
            config
                // registers SkiaSharp as the library backend
                // REQUIRED unless you build your own
                .AddSkiaSharp()

                // select a theme, default is Light
                // OPTIONAL
                .AddDarkTheme()
        );

        SeriesCollection1 = _processor.GetUpperChartSeries(
            _repository.GetAppUsages(),
            _repository.GetTimerUsages()
        );
        SeriesCollection2 = _processor.GetLowerChartSeries(
            _repository.GetAppUsages(),
            _repository.GetTimerUsages()
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
            return _processor.GetPrimaryMetrics(SelectedDate);
        }
    }

    public IEnumerable<AppUsageItem> SelectedDateUsages
    {
        get
        {
            return _processor.GetAppUsagesAtDate(SelectedDate);
        }
    }
}