// CustomTooltip.cs
// 이 파일은 FocusTimer의 일부입니다.
// 
// © 2023 Potados <song@potados.com>
// 
// FocusTimer은(는) 자유 소프트웨어입니다.
// GNU General Public License v3.0을 준수하는 범위 내에서
// 누구든지 자유롭게 변경, 수정하거나 배포할 수 있습니다.
// 
// 이 프로그램을 공유함으로서 다른 누군가에게 도움이 되기를 바랍니다.
// 다만 프로그램 배포와 함께 아무 것도 보증하지 않습니다. 자세한 내용은
// GNU General Public License를 참고하세요.
// 
// 라이센스 전문은 이 프로그램과 함께 제공되었을 것입니다. 만약 아니라면,
// 다음 링크에서 받아볼 수 있습니다: <https://www.gnu.org/licenses/gpl-3.0.txt>

using System;
using System.Collections.Generic;
using System.Linq;
using FocusTimer.Library;
using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.Drawing.Geometries;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.ImageFilters;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.VisualElements;
using SkiaSharp;

namespace FocusTimer.Features.Charting.LiveCharts;

/// <inheritdoc cref="IChartTooltip{TDrawingContext}" />
public class CustomTooltip : IChartTooltip<SkiaSharpDrawingContext>, IImageControl
{
    private static readonly int s_zIndex = 10050;
    private Chart<SkiaSharpDrawingContext>? _chart;
    private IPaint<SkiaSharpDrawingContext>? _backgroundPaint;
    private StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>? _stackPanel;
    private readonly Dictionary<ISeries, SeriesVisual> _seriesVisualsMap = new();

    public IEnumerable<ChartPoint> Points { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SKDefaultTooltip"/> class.
    /// </summary>
    public CustomTooltip()
    {
        FontPaint = new SolidColorPaint(new SKColor(28, 49, 58))
        {
            FontFamily = "맑은 고딕"
        };
        BackgroundPaint = new SolidColorPaint(new SKColor(240, 240, 240))
        {
            ImageFilter = new DropShadow(2, 2, 2, 2, new SKColor(30, 30, 30, 60))
        };
    }

    /// <inheritdoc cref="IImageControl.Size"/>
    public LvcSize Size { get; private set; }

    /// <summary>
    /// Gets or sets the legend font paint.
    /// </summary>
    public IPaint<SkiaSharpDrawingContext>? FontPaint { get; set; }

    /// <summary>
    /// Gets or sets the background paint.
    /// </summary>
    public IPaint<SkiaSharpDrawingContext>? BackgroundPaint
    {
        get => _backgroundPaint;
        set
        {
            _backgroundPaint = value;
            if (value is not null)
            {
                value.IsFill = true;
            }
        }
    }

    /// <summary>
    /// Gets or sets the fonts size.
    /// </summary>
    public double TextSize { get; set; } = 16;

    /// <inheritdoc cref="IChartTooltip{TDrawingContext}.Show(IEnumerable{ChartPoint}, Chart{TDrawingContext})"/>
    public void Show(IEnumerable<ChartPoint> foundPoints, Chart<SkiaSharpDrawingContext> chart)
    {
        Points = foundPoints;

        foundPoints = new List<ChartPoint>() {
            foundPoints.Last()
        };


        _chart = chart;

        if (chart.View.TooltipBackgroundPaint is not null) BackgroundPaint = chart.View.TooltipBackgroundPaint;
        if (chart.View.TooltipTextPaint is not null) FontPaint = chart.View.TooltipTextPaint;
        if (chart.View.TooltipTextSize is not null) TextSize = chart.View.TooltipTextSize.Value;

        if (BackgroundPaint is not null) BackgroundPaint.ZIndex = s_zIndex;
        if (FontPaint is not null) FontPaint.ZIndex = s_zIndex + 1;

        var sp = _stackPanel ??= new StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>
        {
            Padding = new Padding(12, 8),
            Orientation = ContainerOrientation.Vertical,
            HorizontalAlignment = Align.Start,
            VerticalAlignment = Align.Middle,
            BackgroundPaint = BackgroundPaint
        };

        var toRemoveSeries = new List<SeriesVisual>(_seriesVisualsMap.Values);
        foreach (var point in foundPoints)
        {
            var seriesMiniatureVisual = GetSeriesVisual(point);
            _ = toRemoveSeries.Remove(seriesMiniatureVisual);
        }

        Measure(chart);

        var location = foundPoints.GetTooltipLocation(Size, chart);

        _stackPanel.X = location.X;
        _stackPanel.Y = location.Y;

        foreach (var seriesVisual in toRemoveSeries)
        {
            _ = _stackPanel.Children.Remove(seriesVisual.Visual);
            chart.RemoveVisual(seriesVisual.Visual);
            _ = _seriesVisualsMap.Remove(seriesVisual.Series);
        }

        chart.AddVisual(sp);
    }

    /// <inheritdoc cref="IChartTooltip{TDrawingContext}.Hide"/>
    public void Hide()
    {
        if (_chart is null || _stackPanel is null) return;
        _chart.RemoveVisual(_stackPanel);
    }

    /// <inheritdoc cref="IImageControl.Measure(IChart)"/>
    public void Measure(IChart chart)
    {
        if (_stackPanel is null) return;
        Size = _stackPanel.Measure((Chart<SkiaSharpDrawingContext>)chart, null, null);
    }

    private SeriesVisual GetSeriesVisual(ChartPoint point)
    {
        if (_seriesVisualsMap.TryGetValue(point.Context.Series, out var visual))
        {
            if (_chart is null) return visual;
            visual.TitleLabelVisual.Text = Strings.Get("time_total_usage");
            visual.TitleLabelVisual.Invalidate(_chart);
            visual.ValueLabelVisual.Text = PointToValueString(point);
            visual.ValueLabelVisual.Invalidate(_chart);
            return visual;
        }

        var sketch = ((IChartSeries<SkiaSharpDrawingContext>)point.Context.Series).GetMiniatresSketch();
        var relativePanel = sketch.AsDrawnControl();

        var titleLabel = new LabelVisual
        {
            Text = Strings.Get("time_total_usage"),
            Paint = FontPaint,
            TextSize = 12,
            Padding = new Padding(8, 0, 0, 0),
            VerticalAlignment = Align.Start,
            HorizontalAlignment = Align.Start
        };

        var valueLabel = new LabelVisual
        {
            Text = PointToValueString(point),
            Paint = FontPaint,
            TextSize = TextSize,
            Padding = new Padding(8, 0, 0, 0),
            VerticalAlignment = Align.Start,
            HorizontalAlignment = Align.Start
        };

        var sp = new StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext>
        {
            Padding = new Padding(0, 4),
            Orientation = ContainerOrientation.Vertical,
            VerticalAlignment = Align.Middle,
            HorizontalAlignment = Align.Middle,
            Children =
            {
                titleLabel,
                valueLabel
            }
        };

        _ = _stackPanel?.Children.Add(sp);
        var seriesVisual = new SeriesVisual(point.Context.Series, sp, titleLabel, valueLabel);
        _seriesVisualsMap.Add(point.Context.Series, seriesVisual);

        return seriesVisual;
    }

    private string PointToValueString(ChartPoint point)
    {
        var minutes = Math.Ceiling(new TimeSpan((long)point.StackedValue.Total).TotalMinutes);
        var dt = new DateTime(TimeSpan.FromMinutes(minutes).Ticks);

        if (minutes >= 60)
        {
            return dt.ToString($"H\\{Strings.Get("hour_short")} m\\{Strings.Get("minute_short")}");
        }
        else
        {
            return dt.ToString($"m\\{Strings.Get("minute_short")}");
        }
    }

    private class SeriesVisual
    {
        public SeriesVisual(
            ISeries series,
            StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext> stackPanel,
            LabelVisual titleLabelVisual,
            LabelVisual valueLabelVisual)
        {
            Series = series;
            Visual = stackPanel;
            TitleLabelVisual = titleLabelVisual;
            ValueLabelVisual = valueLabelVisual;
        }

        public ISeries Series { get; }

        public StackPanel<RoundedRectangleGeometry, SkiaSharpDrawingContext> Visual { get; }

        public LabelVisual TitleLabelVisual { get; set; }

        public LabelVisual ValueLabelVisual { get; set; }
    }
}