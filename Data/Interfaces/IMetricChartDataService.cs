using Greenhouse.Data.Models;

namespace Greenhouse.Data.Interfaces;

public interface IMetricChartDataService
{
    Task<List<WeeklyMetricChartData>> GetHiLowDailyMetricChartData(Func<GreenhouseMetric, double> valueSelector);
    
    Task<List<DailyMetricChartData>> GetDailyMetricChartData(Func<GreenhouseMetric, double> valueSelector);
}