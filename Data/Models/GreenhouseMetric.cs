using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Greenhouse.Data.Models;

public class GreenhouseMetric
{
    public required string Id { get; set; }
    public int Temperature { get; set; }
    public int Humidity { get; set; }
    public int SoilMoisture { get; set; }
    public HumidityLevel HumidityLevel { get; set; }
    public TemperatureLevel TemperatureLevel { get; set; }
    public SoilMoistureLevel SoilMoistureLevel { get; set; }
    
    [JsonProperty("comment")]
    public string Comment { get; set; }
    public DateTime DateTime { get; set; }
    
    [NotMapped]
    public bool IsExpanded { get; set; }
}

public enum HumidityLevel
{
    high = 0,
    optimal = 1,
    lo = 2,
}

public enum TemperatureLevel
{
    high = 0,
    optimal = 1,
    lo = 2,
}

public enum SoilMoistureLevel
{
    dry = 0,
    moist = 1,
    wet = 2,
}