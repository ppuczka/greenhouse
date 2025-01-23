using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Data.Services;

public class GreenhouseMetricService(IDbContextFactory<MetricsContext> dbContextFactory) : IGreenhouseMetricService
{
    public async Task<List<GreenhouseMetric>> GetGreenhouseMetrics()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var sortedMetrics = context.GreenhouseMetrics.OrderByDescending(
            item => item.DateTime);
        return await context.GreenhouseMetrics.ToListAsync();
    }

    public async Task<GreenhouseMetric> GetLatestMetric()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var sortedMetrics = context.GreenhouseMetrics.OrderByDescending(
            item => item.DateTime);
        return await context.GreenhouseMetrics.FirstAsync();
    }

    public async Task DeleteMetric(string id)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        try
        {
            var toDeleteMetric = await context.GreenhouseMetrics.FindAsync(id);
            if (toDeleteMetric != null)
            {
                context.GreenhouseMetrics.Remove(toDeleteMetric);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public Task UpdateMetric(GreenhouseMetric greenhouseMetric)
    {
        throw new NotImplementedException();
    }

    public Task AddComment(Guid id, string comment)
    {
        throw new NotImplementedException();
    }
}