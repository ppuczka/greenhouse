namespace Greenhouse.Controllers.IotHub.Interfaces;

public interface IIotHubConnector
{
    Task ConnectAsync(CancellationToken cancellationToken);
}