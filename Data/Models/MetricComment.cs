using Newtonsoft.Json;

namespace Greenhouse.Data.Models;

public class MetricComment
{
    public string? Comment { get; set; }
    public DateTime? Created { get; set; } = DateTime.UtcNow;
    
    public MetricComment() { }
    
    public MetricComment(string comment)
    {
        Comment = comment;
    }

}


