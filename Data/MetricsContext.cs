using System.Diagnostics;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Data;

public class MetricsContext : DbContext
{
    public required DbSet<GreenhouseMetric> GreenhouseMetrics { get; set; }

    public MetricsContext(DbContextOptions<MetricsContext> options) : base(options) =>
        Debug.WriteLine($"context created.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("greenhouse");
        modelBuilder.Entity<GreenhouseMetric>().HasNoDiscriminator();

        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.Humidity).ToJsonProperty("humidity");
        modelBuilder
            .Entity<GreenhouseMetric>()
            .Property(g => g.HumidityLevel)
            .HasConversion(
                v => v.ToString(),
                v => (HumidityLevel)Enum.Parse(typeof(HumidityLevel), v))
            .ToJsonProperty("humidity_level");  
        
        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.SoilMoisture).ToJsonProperty("soil_moisture");
        modelBuilder
            .Entity<GreenhouseMetric>()
            .Property(g => g.SoilMoistureLevel)
            .HasConversion(
                v => v.ToString(),
                v => (SoilMoistureLevel)Enum.Parse(typeof(SoilMoistureLevel), v))
            .ToJsonProperty("soil_moisture_level");
            
        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.Temperature).ToJsonProperty("temperature");
        modelBuilder
            .Entity<GreenhouseMetric>()
            .Property(g => g.TemperatureLevel)
            .HasConversion(
                v => v.ToString(),
                v => (TemperatureLevel)Enum.Parse(typeof(TemperatureLevel), v))
            .ToJsonProperty("temperature_level");
        
        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.DateTime).ToJsonProperty("date_time");

    }
}