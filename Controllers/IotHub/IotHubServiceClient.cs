using System.Text;
using Azure.Messaging.EventHubs.Consumer;
using Greenhouse.Controllers.IotHub.Interfaces;
using Greenhouse.Controllers.IotHub.Models;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;

namespace Greenhouse.Controllers.IotHub;

public class IotHubServiceClient : IIotHubServiceClient
{
    private static readonly TimeSpan SleepDuration = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan OperationTimeout = TimeSpan.FromSeconds(10);

    private static readonly SemaphoreSlim InitSemaphore = new(1, 1);

    private static ServiceClient? _serviceClient;
    private static EventHubConsumerClient? _eventHubConsumerClient;

    private readonly TransportType _transportType;

    private readonly string _hubConnectionString;
    private readonly string _eventHubConnectionString;
    private readonly string _eventHubName;

    private readonly string _deviceId;

    private readonly ILogger<IotHubServiceClient> _logger;

    public IotHubServiceClient(IOptions<Config.Config> appConfig, ILogger<IotHubServiceClient> logger)
    {
        _hubConnectionString = appConfig.Value.IotHubDeviceConnectionString ??
                               throw new ArgumentNullException(nameof(appConfig.Value.IotHubDeviceConnectionString));

        _eventHubConnectionString = appConfig.Value.IotHubEventHubConnectionString ??
                                    throw new ArgumentNullException(
                                        nameof(appConfig.Value.IotHubEventHubConnectionString));

        _transportType = appConfig.Value.IotHubTransportType;

        _deviceId = appConfig.Value.IotHubDeviceId ??
                    throw new ArgumentNullException(nameof(appConfig.Value.IotHubDeviceId));
        _eventHubName = appConfig.Value.IotHubEventHubName ??
                        throw new ArgumentNullException(nameof(appConfig.Value.IotHubEventHubName));

        _logger = logger;
    }

    public async Task SendCloud2DeviceMessageWithFeedbackAsync(string messageText, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureServiceClientInitializedAsync();
            var sendTask = SendC2DMessageAsync(messageText, cancellationToken);
            var feedbackTask = ReceiveMessageFeedbacksAsync(cancellationToken);

            await Task.WhenAll(sendTask, feedbackTask);
        }
        catch (Exception ex)
        {
            _logger.LogError("Unrecoverable exception caught, user action is required, so exiting...: \n{Exception}",
                ex);
            throw;
        }
    }

    public async IAsyncEnumerable<IotHubEvent> ReceiveMessageFromDeviceAsync()
    {
        await EnsureEventHubConsumerClientInitializedAsync();
        await foreach (var partitionEvent in _eventHubConsumerClient!.ReadEventsAsync())
        {
            IotHubEvent? iotHubEvent = null;
            try
            {
                var eventBody = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());
                _logger.LogInformation("Received event: {EventBody}", eventBody);

                var systemProperties = partitionEvent.Data.SystemProperties;
                var dataProperties = partitionEvent.Data.Properties;

                iotHubEvent = new IotHubEvent(
                    eventBody,
                    new Dictionary<string, object>(dataProperties),
                    new Dictionary<string, object>(systemProperties)
                );
            }
            catch (TaskCanceledException ex)
            {
                  // This is expected when the token is signaled; it should not be considered an error in this scenario.
            }

            if (iotHubEvent != null)
            {
                yield return iotHubEvent;
            }
        }
    }

    private async Task SendC2DMessageAsync(string messageText, CancellationToken cancellationToken)
    {
        {
            await EnsureServiceClientInitializedAsync();

            while (!cancellationToken.IsCancellationRequested)
            {
                using var message = new Message(Encoding.ASCII.GetBytes(messageText));
                message.Ack = DeliveryAcknowledgement.Full;

                _logger.LogInformation("Sending C2D message with Id {MessageMessageId} to {DeviceId}.",
                    message.MessageId, _deviceId);

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        await _serviceClient!.SendAsync(_deviceId, message, OperationTimeout);
                        _logger.LogInformation(
                            "Sent message with Id {MessageMessageId} to {DeviceId}.", message.MessageId, _deviceId);
                        break;
                    }
                    catch (Exception e) when (ExceptionHelper.IsNetwork(e))
                    {
                        _logger.LogError("Transient Exception occurred, will retry: {Exception}", e);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError("Unexpected error, will need to reinitialize the client: {Exception}", e);
                        await EnsureServiceClientInitializedAsync();
                    }

                    await Task.Delay(SleepDuration, cancellationToken);
                }

                await Task.Delay(SleepDuration, cancellationToken);
            }
        }
    }

    private async Task ReceiveMessageFeedbacksAsync(CancellationToken token)
    {
        _logger.LogInformation("Starting to listen to feedback messages");

        var feedbackReceiver = _serviceClient!.GetFeedbackReceiver();

        while (!token.IsCancellationRequested)
        {
            try
            {
                var feedbackMessages = await feedbackReceiver.ReceiveAsync(token);
                if (feedbackMessages != null)
                {
                    _logger.LogInformation("New Feedback received:");
                    _logger.LogInformation("Enqueue Time: {FeedbackMessagesEnqueuedTime}",
                        feedbackMessages.EnqueuedTime);
                    _logger.LogInformation("Number of messages in the batch: {Count}",
                        feedbackMessages.Records.Count());
                    foreach (var feedbackRecord in feedbackMessages.Records)
                    {
                        _logger.LogInformation(
                            "Device {FeedbackRecordDeviceId} acted on message: {FeedbackRecordOriginalMessageId} with status: {FeedbackRecordStatusCode}",
                            feedbackRecord.DeviceId, feedbackRecord.OriginalMessageId, feedbackRecord.StatusCode);
                    }

                    await feedbackReceiver.CompleteAsync(feedbackMessages, token);
                }

                await Task.Delay(SleepDuration, token);
            }
            catch (Exception e) when (ExceptionHelper.IsNetwork(e))
            {
                _logger.LogError($"Transient Exception occurred; will retry: {e}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Unexpected error, will need to reinitialize the client: {e}");
                await EnsureServiceClientInitializedAsync();
                feedbackReceiver = _serviceClient.GetFeedbackReceiver();
            }
        }
    }

    private async Task EnsureServiceClientInitializedAsync()
    {
        if (_serviceClient == null)
        {
            await InitSemaphore.WaitAsync();
            try
            {
                if (_serviceClient == null)
                {
                    var options = new ServiceClientOptions
                    {
                        SdkAssignsMessageId = SdkAssignsMessageId.WhenUnset
                    };
                    _serviceClient =
                        ServiceClient.CreateFromConnectionString(_hubConnectionString, _transportType, options);
                    _logger.LogInformation("Initialized a new service client instance.");
                }
            }
            finally
            {
                InitSemaphore.Release();
            }
        }
    }

    private async Task EnsureEventHubConsumerClientInitializedAsync()
    {
        if (_eventHubConsumerClient == null)
        {
            await InitSemaphore.WaitAsync();
            try
            {
                if (_eventHubConsumerClient == null)
                {
                    _eventHubConsumerClient = new EventHubConsumerClient(
                        EventHubConsumerClient.DefaultConsumerGroupName,
                        _eventHubConnectionString,
                        _eventHubName
                    );

                    _logger.LogInformation("Initialized a new Event Hub consumer client instance.");
                }
            }
            finally
            {
                InitSemaphore.Release();
            }
        }
    }
}