// The MIT License(MIT)
//
// Copyright(c) 2021 Alberto Rodriguez Orozco & LiveCharts Contributors
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Linq;
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
            visual.TitleLabelVisual.Text = $"총 사용 시간";
            visual.TitleLabelVisual.Invalidate(_chart);
            visual.ValueLabelVisual.Text = PointToValueString(point);
            visual.ValueLabelVisual.Invalidate(_chart);
            return visual;
        }

        var sketch = ((IChartSeries<SkiaSharpDrawingContext>)point.Context.Series).GetMiniatresSketch();
        var relativePanel = sketch.AsDrawnControl();

        var titleLabel = new LabelVisual
        {
            Text = $"총 사용 시간",
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
            return dt.ToString("H시간 m분");
        }
        else
        {
            return dt.ToString("m분");
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