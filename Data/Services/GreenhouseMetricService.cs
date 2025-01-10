using Greenhouse.Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Data.Services;

public class GreenhouseMetricService(IDbContextFactory<MetricsContext> dbContextFactory) : IGreenhouseMetricService
{
    public async Task<List<GreenhouseMetric>> GetGreenhouseMetrics()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        return await context.GreenhouseMetrics.ToListAsync();
    }

    public void DeleteMetric(Guid id)
    {
        throw new NotImplementedException();
    }

    public void UpdateMetric(GreenhouseMetric greenhouseMetric)
    {
        throw new NotImplementedException();
    }
}