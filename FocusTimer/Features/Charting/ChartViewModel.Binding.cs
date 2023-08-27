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

public partial class ChartViewModel
{
    public ObservableCollection<ISeries> SeriesCollection1 { get; set; }
    public ObservableCollection<ISeries> SeriesCollection2 { get; set; }
    public Axis[] SharedXAxis { get; set; }

    public Axis[] YAxis { get; set; }

    public IPaint<SkiaSharpDrawingContext> TooltipPaint { get; set; } = new SolidColorPaint(new SKColor(28, 49, 58))
    {
        FontFamily = "맑은 고딕"
    };

    public string SelectedDateString => _selectedDate == DateTime.MinValue ? "지난 21일" : _selectedDate.ToString("yyyy. MM. dd");

    public IEnumerable<PrimaryMetricItem> PrimaryMetrics => _processor.GetPrimaryMetrics(_selectedDate);

    public IEnumerable<AppUsageItem> SelectedDateUsages => _processor.GetAppUsagesAtDate(_selectedDate);
}