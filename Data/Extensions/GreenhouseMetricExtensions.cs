using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;
using Syncfusion.Blazor.Charts.Chart.Internal;

namespace Greenhouse.Data.Extensions;

public class GreenhouseMetricExtensions : IGreenhouseMetricExtension
{
    public List<MetricChartData> ToDataCharts(List<GreenhouseMetric> metrics)
    {
         return metrics
            .GroupBy(m => m.DateTime.Date)
            .Select(v => new MetricChartData
            {
                WeekDay = v.Key.DayOfWeek.ToString(),
                Low = v.Min(m => m.Temperature),
                High = v.Max(m => m.Temperature),
            })
            .ToList();
    }
}
