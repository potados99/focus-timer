// ChartViewModel.cs
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

using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.Painting.Effects;
using SkiaSharp;
using System;
using System.Linq;
using FocusTimer.Data.Repositories;
using FocusTimer.Domain.Services;
using FocusTimer.Features.Charting.LiveCharts;
using FocusTimer.Library;
using FocusTimer.Library.Control.Base;

namespace FocusTimer.Features.Charting;

public partial class ChartViewModel : BaseViewModel
{
    private readonly ChartDataProcessingService _processingService;
    private readonly FocusRepository _repository;

    public ChartViewModel(ChartDataProcessingService processingService, FocusRepository repository)
    {
        _processingService = processingService;
        _repository = repository;
    }

    public event Signal? OnChartNeedsUpdate;
    
    private DateTime _selectedDate;

    public override void OnLoaded()
    {
        ConfigureChart();

        LoadCollections();

        RegisterEvents();
        
        SetAxis();

        RenderChart();
    }

    private void ConfigureChart()
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
    }
    
    private void LoadCollections()
    {
        SeriesCollection1 = _processingService.GetUpperChartSeries();
        SeriesCollection2 = _processingService.GetLowerChartSeries();
    }
    
    private void RegisterEvents() {
        ((ColumnSeries<DataPoint>) SeriesCollection1.Last()).ChartPointPointerDown += (a, b) =>
        {
            DateSelected(b.Model.DateTime);
        };

        ((StackedColumnSeries<DataPoint>) SeriesCollection2.Last()).ChartPointPointerDown += (a, b) =>
        {
            DateSelected(b.Model.DateTime);
        };
    }
    
    private void SetAxis()
    {
        SharedXAxis = new[]
        {
            BuildXAxis()
        };

        YAxis = new[]
        {
            BuildYAxis()
        };
    }

    private Axis BuildXAxis()
    {
        return new Axis
        {
            LabelsPaint = new SolidColorPaint(SKColors.LightGray)
            {
                FontFamily = "맑은 고딕"
            },
            Labeler = (value) =>
            {
                var date = new DateTime((long) value);

                return date.ToString(date.Day == 1 ? "M월 d" : "dd");
            },
            UnitWidth = TimeSpan.FromDays(1).Ticks,
            MinStep = TimeSpan.FromDays(1).Ticks,
            ForceStepToMin = true,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
            {
                StrokeThickness = 1,
                PathEffect = new DashEffect(new float[] {3, 3})
            },
        };
    }

    private Axis BuildYAxis()
    {
        return new Axis
        {
            IsVisible = false,
            SeparatorsPaint = new SolidColorPaint(SKColors.LightSlateGray)
        };
    }
    
    private void RenderChart()
    {
        NotifyPropertyChanged(nameof(SeriesCollection1));
        NotifyPropertyChanged(nameof(SeriesCollection2));
        NotifyPropertyChanged(nameof(SharedXAxis));
        NotifyPropertyChanged(nameof(YAxis));
    }
    
    private void DateSelected(DateTime date)
    {
        if (_selectedDate == date)
        {
            foreach (var s in SeriesCollection1)
            {
                s.ClearSelection();
            }

            foreach (var s in SeriesCollection2)
            {
                s.ClearSelection();
            }

            _selectedDate = DateTime.MinValue;
        }
        else
        {
            foreach (var s in SeriesCollection1)
            {
                s.SelectPoints(s.ActivePoints.Where(p => (p.Context.DataSource as DataPoint).DateTime == date));
            }

            foreach (var s in SeriesCollection2)
            {
                s.SelectPoints(s.ActivePoints.Where(p => (p.Context.DataSource as DataPoint).DateTime == date));
            }

            _selectedDate = date;
        }

        RenderDateSelection();
    }

    private void RenderDateSelection()
    {
        OnChartNeedsUpdate?.Invoke();

        NotifyPropertyChanged(nameof(SelectedDateString));
        NotifyPropertyChanged(nameof(SelectedDateUsages));
        NotifyPropertyChanged(nameof(PrimaryMetrics));
    }
    
}