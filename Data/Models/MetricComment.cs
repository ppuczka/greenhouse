using Newtonsoft.Json;

namespace Greenhouse.Data.Models;

public class MetricComment
{
    [JsonProperty("comment")]
    public string Comment { get; set; } = string.Empty;
    
    [JsonProperty("created")]
    public DateTime Created { get; set; }
}


