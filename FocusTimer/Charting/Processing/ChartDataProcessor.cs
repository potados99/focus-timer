using FocusTimer.Charting.Entity;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FocusTimer.Charting.Processing
{
    public static class ChartDataProcessor
    {
        internal static LineSeries<DataPoint>[] GetUpperChartSeries(IEnumerable<Concentration> contentrations)
        {
            return new LineSeries<DataPoint>[] {
                new LineSeries<DataPoint>
                {
                    Name = "집중도",
                    Values = contentrations.Select(c => new DataPoint() { DateTime = c.Date, Value = c.Value }).ToList(),
                    TooltipLabelFormatter = (d) => $"집중도 {d.PrimaryValue}%",
                }
            };
        }

        internal static StackedColumnSeries<DataPoint>[] GetLowerChartValeus(IEnumerable<AppUsage> appUsages)
        {
            var series = new List<StackedColumnSeries<DataPoint>>();

            var appNames = appUsages.Select(u => u.AppName).Distinct();

            foreach (var appName in appNames)
            {
                var thisAppUsages = appUsages.Where(u => u.AppName == appName);

                var values = 1;
                // 날짜별로 그룹화한 다음 같은 날짜에서 같은 앱인 것들은 묶어서 Duration을 값으로 추출


            }

            return null;
        }

    }
}
