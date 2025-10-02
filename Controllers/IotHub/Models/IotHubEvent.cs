namespace Greenhouse.Controllers.IotHub.Models;

public class IotHubEvent(
    string message,
    Dictionary<string, object> dataProperties,
    Dictionary<string, object> systemProperties)
{
    private readonly string _message = message;
    private readonly IDictionary<string, object> _dataProperties = dataProperties;
    private readonly IDictionary<string, object> _systemProperties = systemProperties;
    
    public string Stringify()
    {
        var dataProps = string.Join(", ", _dataProperties.Select(kv => $"{kv.Key}: {kv.Value}"));
        var systemProps = string.Join(", ", _systemProperties.Select(kv => $"{kv.Key}: {kv.Value}"));

        return $"Message: {_message}, DataProperties: {{{dataProps}}}, SystemProperties: {{{systemProps}}}";
    }

}