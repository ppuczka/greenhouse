using Microsoft.Azure.Devices.Client;

namespace Greenhouse.Controllers.IotHub.Interfaces;

public interface IIotHubConnector
{
    bool IsDeviceConnected();

    Task ConnectAsync(CancellationToken parentCancellationToken);
    Task CloseConnectionAsync(CancellationToken parentCancellationToken);
    Task SendMessageAsync(Message message, CancellationToken cancellationToken);
}