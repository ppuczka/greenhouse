using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;
using Syncfusion.Blazor.Charts.Chart.Internal;
using Syncfusion.Blazor.Data;

namespace Greenhouse.Data.Extensions;

public class GreenhouseMetricExtensions : IGreenhouseMetricExtension
{
    public List<MetricChartData> ToDataCharts(List<GreenhouseMetric> metrics, Func<GreenhouseMetric, double> selector)
    {
        return metrics
            .GroupBy(m => m.DateTime.Date)
            .Select(v => new MetricChartData
            {
                WeekDay = v.Key.DayOfWeek.ToString(),
                Low = v.Min(selector),
                High = v.Max(selector),
            })
            .ToList();
    }
}
