using Greenhouse.Data.Models;

namespace Greenhouse.Data.Interfaces;

public interface IGreenhouseMetricExtension
{
    List<MetricChartData> ToDataCharts(List<GreenhouseMetric> metrics);
}