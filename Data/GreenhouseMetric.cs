using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Greenhouse.Data;

public class GreenhouseMetric
{
    public int Id { get; set; }
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
    High = 0,
    Optimal = 1,
    Low = 2,
}

public enum TemperatureLevel
{
    High = 0,
    Optimal = 1,
    Low = 2,
}

public enum SoilMoistureLevel
{
    High = 0,
    Optimal = 1,
    Low = 2,
}