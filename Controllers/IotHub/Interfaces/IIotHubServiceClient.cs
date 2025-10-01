namespace Greenhouse.Controllers.IotHub.Interfaces;

public interface IIotHubServiceClient
{
    Task SendCloud2DeviceMessageWithFeedbackAsync(string messageText, CancellationToken cancellationToken);
    Task<string> ReceiveMessageAsync();
}