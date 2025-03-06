using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Greenhouse.Data.Models;

public class MetricsContext : DbContext
{
    public required DbSet<GreenhouseMetric> GreenhouseMetrics { get; set; }

    public MetricsContext(DbContextOptions<MetricsContext> options) : base(options) =>
        Debug.WriteLine($"context created.");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer("greenhouse");
        modelBuilder.Entity<GreenhouseMetric>().HasNoDiscriminator();
        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.Id).ToJsonProperty("id");
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

        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.LightIntensity).ToJsonProperty("light_intensity");
        
        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.DateTime).ToJsonProperty("date_time");

        modelBuilder.Entity<GreenhouseMetric>().Property(g => g.Tags).ToJsonProperty("tags");
        
        modelBuilder.Entity<GreenhouseMetric>().OwnsMany(
            o => o.Comments,
            comment =>
            {
                comment.ToJsonProperty("comments");
                comment.Property(p => p.Comment).ToJsonProperty("comment");
                comment.Property(p => p.Created).ToJsonProperty("created");
            });

        modelBuilder.Entity<GreenhouseMetric>().OwnsMany(
            o => o.Attachments,
            attachment =>
            {
                attachment.ToJsonProperty("attachments");
                attachment.Property(p => p.FileName).ToJsonProperty("filename");
                attachment.Property(p => p.FileSize).ToJsonProperty("size");
                attachment.Property(p => p.BlobUri).ToJsonProperty("blob_uri");
                attachment.Property(p => p.UploadedOn).ToJsonProperty("uploaded_on");
            });
    }
}