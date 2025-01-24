using Greenhouse.Data.Models;

namespace Greenhouse.Data.Interfaces;

public interface IGreenhouseMetricService
{
    public Task<List<GreenhouseMetric>> GetGreenhouseMetrics();
    public Task<GreenhouseMetric> GetLatestMetric();
    public Task DeleteMetric(string metricId);
    public Task UpdateMetric(GreenhouseMetric greenhouseMetric);
    public Task AddComment(string metricId, string comment);
}