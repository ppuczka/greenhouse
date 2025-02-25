using Greenhouse.Data.Models;

namespace Greenhouse.Data.Interfaces;

public interface IGreenhouseMetricExtension
{
    List<WeeklyMetricChartData> ToWeeklyDataCharts(List<GreenhouseMetric> metrics, Func<GreenhouseMetric, double> selector);
    List<DailyMetricChartData> ToDailyDataCharts(List<GreenhouseMetric> metrics, Func<GreenhouseMetric, double> selector);
}