using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;

namespace Greenhouse.Data.Services;

public class MetricChartDataService(
    IGreenhouseMetricExtension greenhouseMetricExtension,
    IGreenhouseMetricService greenhouseMetricService)
    : IMetricChartDataService
{
    public async Task<List<WeeklyMetricChartData>> GetHiLowDailyMetricChartData(Func<GreenhouseMetric, double> valueSelector)
    {
        var lastWeekMetrics = await greenhouseMetricService.GetLastDaysMetrics(7);
        return greenhouseMetricExtension.ToWeeklyDataCharts(lastWeekMetrics, valueSelector);
    }

    public async Task<List<WeeklyMetricChartData>> GetDailyMetricChartData(Func<GreenhouseMetric, double> valueSelector)
    {
        var lastDayMetrics = await greenhouseMetricService.GetLastDaysMetrics(1);
        return greenhouseMetricExtension.ToWeeklyDataCharts(lastDayMetrics, valueSelector);
    }
}