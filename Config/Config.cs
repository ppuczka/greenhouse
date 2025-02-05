namespace Greenhouse.Config;

public class Config
{
    public string? AppName { get; }
    public string? AppVersion { get; }
    public string? DatabaseEndpoint { get; }
    public string? DatabaseName { get; }
    
    public string? OidcLogoutEndpoint { get; }
    public string? OidcLogoutRedirectUri { get; }
}