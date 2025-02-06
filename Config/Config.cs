namespace Greenhouse.Config;

public class Config
{
    public string? AppName { get; set; }
    public string? AppVersion { get; set; }
    public string? DatabaseEndpoint { get; set; }
    public string? DatabaseName { get; }
    
    public string? OidcLogoutEndpoint { get; set; }
    public string? OidcLogoutRedirectUri { get; set; }
}