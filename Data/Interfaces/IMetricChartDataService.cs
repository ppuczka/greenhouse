using Greenhouse.Data.Models;

namespace Greenhouse.Data.Interfaces;

public interface IMetricChartDataService
{
    Task<List<MetricChartData>> GetHiLowDailyMetricChartData(Func<GreenhouseMetric, double> valueSelector);
}