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

    public async Task DeleteMetric(string metricId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        try
        {
            var toDeleteMetric = await context.GreenhouseMetrics.FindAsync(metricId);
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

    public async Task AddComment(string metricId, string commentText)
    {
        var comment = new MetricComment(commentText);
        await using var context = await dbContextFactory.CreateDbContextAsync();

        try
        {
            var metric = await context.GreenhouseMetrics.FindAsync(metricId);
            if (metric != null)
            {
                metric.Comments.Add(comment);
                await context.SaveChangesAsync();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }
}