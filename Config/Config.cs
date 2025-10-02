using Microsoft.Azure.Devices;

namespace Greenhouse.Config;

public class Config
{
    public string? AppName { get; set; }
    public string? AppVersion { get; set; }
    public string? DatabaseEndpoint { get; set; }
    public string? DatabaseName { get; set; }
    public string? AzureBlobContainerEndpoint { get; set; }
    public string? AzureBlobContainerName { get; set; }
    public string? AzureStorageAccountName { get; set; }
    public string? AzureStorageAccountKey { get; set; }
    public string? OidcLogoutEndpoint { get; set; }
    public string? OidcLogoutRedirectUri { get; set; }
    public string? IotHubDeviceConnectionString{ get; set; }
    public string? IotHubEventHubConnectionString{ get; set; }
    public string? IotHubDeviceId { get; set; }
    public string? IotHubEventHubName { get; set; }
    public TransportType IotHubTransportType { get; set; }
    
}