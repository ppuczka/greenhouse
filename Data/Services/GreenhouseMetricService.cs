using System.Linq.Dynamic.Core;
using Greenhouse.Data.Extensions;
using Greenhouse.Data.Interfaces;
using Greenhouse.Data.Models;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Blazor.Data;

namespace Greenhouse.Data.Services;

public class GreenhouseMetricService(IDbContextFactory<MetricsContext> dbContextFactory) : IGreenhouseMetricService
{
    public async Task<List<GreenhouseMetric>> GetGreenhouseMetrics()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var sortedMetrics = context.GreenhouseMetrics.OrderByDescending(
            item => item.DateTime);
        return await sortedMetrics.ToListAsync();
    }

    public async Task<GreenhouseMetric> GetLatestMetric()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var sortedMetrics = context.GreenhouseMetrics.OrderByDescending(
            item => item.DateTime);
        return await sortedMetrics.FirstAsync();
    }

    public async Task<List<GreenhouseMetric>> GetLast7DaysMetrics()
    {
        var fromDate = DateTime.Now.AddDays(-7);
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var fromDateMetrics = context.GreenhouseMetrics
            .Where(item => item.DateTime >= fromDate);
        return await fromDateMetrics.ToListAsync();
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

    public async Task AddTag(string metricId, string tag)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var tags = tag.Split(" ").ToList();
        try
        {
            var metric = await context.GreenhouseMetrics.FindAsync(metricId);
            if (metric != null)
            {
                (metric.Tags as List<string>)?.AddRange(tags);
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