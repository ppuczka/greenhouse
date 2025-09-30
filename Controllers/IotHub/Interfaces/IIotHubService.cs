namespace Greenhouse.Controllers.IotHub.Interfaces;

public interface IIotHubService
{
    Task SendMessageAsync(string message);
    Task<string> ReceiveMessageAsync();
}