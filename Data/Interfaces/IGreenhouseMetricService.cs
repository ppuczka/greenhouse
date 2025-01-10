namespace Greenhouse.Data.Interfaces;

public interface IGreenhouseMetricService
{
    public Task<List<GreenhouseMetric>> GetGreenhouseMetrics();
    public void DeleteMetric(Guid id);
    public void UpdateMetric(GreenhouseMetric greenhouseMetric);
}