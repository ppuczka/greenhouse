using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;
using Syncfusion.Blazor.Charts.Chart.Internal;
using Syncfusion.Blazor.Data;

namespace Greenhouse.Data.Extensions;

public class GreenhouseMetricExtensions : IGreenhouseMetricExtension
{
    public List<WeeklyMetricChartData> ToWeeklyDataCharts(List<GreenhouseMetric> metrics, Func<GreenhouseMetric, double> selector)
    {
        return metrics
            .GroupBy(m => m.DateTime.Date)
            .Select(v => new WeeklyMetricChartData
            {
                WeekDay = v.Key.DayOfWeek.ToString(),
                Low = v.Min(selector),
                High = v.Max(selector),
            })
            .ToList();
    }

    public List<DailyMetricChartData> ToDailyDataCharts(List<GreenhouseMetric> metrics, Func<GreenhouseMetric, double> selector)
    {
        return metrics
            .GroupBy(m => m.DateTime.Hour)
            .Select(v => new DailyMetricChartData
            {
                Hour = v.Key.ToString(),
                Value = v.Average(selector)
            }).ToList();
    }
}
