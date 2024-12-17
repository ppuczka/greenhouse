using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Greenhouse.Data;

public class GreenhouseMetric
{
    public Guid Id { get; set; }
    public int Temperature { get; set; }
    public int Humidity { get; set; }
    public int SoilMoisture { get; set; }
    public HumidityLevel HumidityLevel { get; set; }
    public TemperatureLevel TemperatureLevel { get; set; }
    public SoilMoistureLevel SoilMoistureLevel { get; set; }
    
    public DateTime DateTime { get; set; }
}

public enum HumidityLevel
{
    high = 0,
    optimal = 1,
    low = 2,
}

public enum TemperatureLevel
{
    high = 0,
    optimal = 1,
    low = 2,
}

public enum SoilMoistureLevel
{
    dry = 0,
    moist = 1,
    wet = 2,
}