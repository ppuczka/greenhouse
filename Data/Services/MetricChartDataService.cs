using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;

namespace Greenhouse.Data.Services;

public class MetricChartDataService(
    IGreenhouseMetricExtension greenhouseMetricExtension,
    IGreenhouseMetricService greenhouseMetricService)
    : IMetricChartDataService
{
    public async Task<List<MetricChartData>> GetHiLowDailyMetricChartData(Func<GreenhouseMetric, double> valueSelector)
    {
        var lastWeekMetrics = await greenhouseMetricService.GetLast7DaysMetrics();
        return greenhouseMetricExtension.ToDataCharts(lastWeekMetrics, valueSelector);
    }
}