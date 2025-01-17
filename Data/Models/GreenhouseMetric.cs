namespace Greenhouse.Data.Models;

public class GreenhouseMetric
{
    public string Id { get; set; }
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