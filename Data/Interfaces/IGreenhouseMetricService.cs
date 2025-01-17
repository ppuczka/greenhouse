using Greenhouse.Data.Models;

namespace Greenhouse.Data.Interfaces;

public interface IGreenhouseMetricService
{
    public Task<List<GreenhouseMetric>> GetGreenhouseMetrics();
    public Task DeleteMetric(string id);
    public Task UpdateMetric(GreenhouseMetric greenhouseMetric);
    public Task AddComment(Guid id, string comment);
}