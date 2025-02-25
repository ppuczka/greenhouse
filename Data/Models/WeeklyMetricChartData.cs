namespace Greenhouse.Data.Models;

public class WeeklyMetricChartData
{
    public string? WeekDay { get; set; }
    public double Low { get; set; }
    public double High { get; set; }
}
public class DailyMetricChartData
{
    public string? Hour { get; set; }
    public double Value { get; set; }
}