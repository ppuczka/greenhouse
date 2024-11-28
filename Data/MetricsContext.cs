using Azure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Data;

public class MetricsContext : DbContext
{
    private readonly Config.Config _config;
    public required DbSet<GreenhouseMetric> GreenhouseMetrics { get; set; }

    protected MetricsContext(Config.Config config, DbSet<GreenhouseMetric> greenhouseMetrics)
    {
        this._config = config;
        GreenhouseMetrics = greenhouseMetrics;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseCosmos(_config.DatabaseEndpoint, new DefaultAzureCredential(), _config.DatabaseName);
}