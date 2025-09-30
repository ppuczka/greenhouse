using System.Text;
using Greenhouse.Controllers.IotHub.Interfaces;
using Microsoft.Azure.Cosmos.Linq;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using static System.Threading.CancellationToken;

namespace Greenhouse.Controllers.IotHub;

public class IotHubService : IIotHubService
{

    private static readonly TimeSpan SleepDuration = TimeSpan.FromSeconds(15);

    private readonly CancellationTokenSource _appCancellation = new CancellationTokenSource();
    private readonly IIotHubConnector _iotHubConnector;
    private readonly ILogger<IotHubService> _logger;

    public IotHubService(IIotHubConnector iotHubConnector, ILogger<IotHubService> logger)
    {
        _iotHubConnector = iotHubConnector;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogInformation("Initializing IoT Hub Service...");

        try
        {
            await _iotHubConnector.ConnectAsync(_appCancellation.Token);
        }
        catch (OperationCanceledException) { } // User canceled the operation

        catch (Exception ex)
        {
            _logger.LogError("Unrecoverable exception caught, user action is required, so exiting: \n{ex}", ex);
            await _appCancellation.CancelAsync();
        }
    }

    public async Task SendMessageAsync(string messageText)
    {
        var messageId = Guid.NewGuid();

        while (!_appCancellation.IsCancellationRequested)
        {
            if (_iotHubConnector.IsDeviceConnected())
            {
                _logger.LogInformation("Device sending message {MessageId} to IoT hub.", messageId);
                using var message = PrepareMessage(messageId, messageText);
                await _iotHubConnector.SendMessageAsync(message, _appCancellation.Token);
                _logger.LogInformation("Device sent message {MessageId} to IoT hub.", messageId);
            }

            await Task.Delay(SleepDuration, _appCancellation.Token);
        }
    }

    public async Task<string> ReceiveMessageAsync()
    {
        while (!_appCancellation.IsCancellationRequested)
        {
            if (_iotHubConnector.IsDeviceConnected())
            {
                await Task.Delay(SleepDuration, _appCancellation.Token);
                continue;
            }
            else if (_transportType == TransportType.Http1)
            {
                // The call to ReceiveAsync over HTTP completes immediately, rather than waiting up to the specified
                // time or when a cancellation token is signaled, so if we want it to poll at the same rate, we need
                // to add an explicit delay here.
                await Task.Delay(s_sleepDuration, cancellationToken);
            }

            _logger.LogInformation($"Device waiting for C2D messages from the hub for {s_sleepDuration}." +
                                   $"\nUse the IoT Hub Azure Portal or Azure IoT Explorer to send a message to this device.");

            await ReceiveMessageAndCompleteAsync(cancellationToken);
        }
    }

    private async Task ReceiveMessageAndCompleteAsync(CancellationToken cancellationToken)
    {
        Message receivedMessage = null;
        try
        {
            receivedMessage = await s_deviceClient.ReceiveAsync(cancellationToken);
        }
        catch (IotHubCommunicationException ex) when (ex.InnerException is OperationCanceledException)
        {
            _logger.LogInformation("Timed out waiting to receive a message.");
        }

        if (receivedMessage == null)
        {
            _logger.LogInformation("No message received.");
            return;
        }

        using (receivedMessage)
        {
            string messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
            var formattedMessage = new StringBuilder($"Received message '{receivedMessage.MessageId}': [{messageData}]");

            foreach (KeyValuePair<string, string> prop in receivedMessage.Properties)
            {
                formattedMessage.AppendLine($"\n\tProperty: key={prop.Key}, value={prop.Value}");
            }
            _logger.LogInformation(formattedMessage.ToString());

            try
            {
                await s_deviceClient.CompleteAsync(receivedMessage, cancellationToken);
                _logger.LogInformation($"Completed message '{receivedMessage.MessageId}'.");
            }
            catch (DeviceMessageLockLostException)
            {
                _logger.LogWarning($"Took too long to process and complete a C2D message; it will be redelivered.");
            }
        }
    }


    private async Task CloseConnectionAsync()
    {
        _appCancellation.Dispose();
        await _iotHubConnector.CloseConnectionAsync(None);
    }
    
    private static Message PrepareMessage(Guid messageId, string messageText)
    {
        var messagePayload = $"Sample message {messageText}";

        var eventMessage = new Message(Encoding.UTF8.GetBytes(messagePayload))
        {
            MessageId = messageId.ToString(),
            ContentEncoding = Encoding.UTF8.ToString(),
            ContentType = "application/json",
        };
        eventMessage.Properties.Add("Command", "Test");

        return eventMessage;
    }
}