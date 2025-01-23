using Newtonsoft.Json;

namespace Greenhouse.Data.Models;

public class MetricComment
{
    public string Comment { get; set; } = string.Empty;
    public DateTime Created { get; set; }
}


