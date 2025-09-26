using Greenhouse.Controllers.IotHub.Interfaces;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using Microsoft.Azure.Devices.Shared;
using Microsoft.Extensions.Options;

namespace Greenhouse.Controllers.IotHub;

public class IotHubConnector : IIotHubConnector

{
    private static readonly Random Random = new();
    private static readonly TimeSpan RandomDelay = TimeSpan.FromSeconds(15);

    private static readonly SemaphoreSlim InitSemaphore = new(1, 1);
    private static readonly ClientOptions ClientOptions = new() { SdkAssignsMessageId = SdkAssignsMessageId.WhenUnset };

    private static readonly HashSet<Type> ExceptionsToBeRetried = new()
    {
        typeof(TimeoutException),
        typeof(UnauthorizedAccessException)
    };

    private static volatile DeviceClient DeviceClient;
    private static volatile ConnectionStatus ConnectionStatus;

    // private static CancellationTokenSource AppCancellation;

    private static long _localDesiredPropertyVersion = 1;

    private readonly TransportType _transportType;
    private readonly IRetryPolicy _retryPolicy;

    private readonly Config.Config _config;

    private readonly List<string> _deviceConnectionStrings;

    private readonly ILogger<IotHubConnector> _logger;


    public IotHubConnector(
        IOptions<Config.Config> appConfig,
        TransportType transportType,
        ILogger<IotHubConnector> logger
    )
    {
        _config = appConfig.Value;
        _logger = logger;
        _retryPolicy = new CustomRetryPolicy(ExceptionsToBeRetried, _logger);

        if (_config.IotHubDeviceConnectionStrings == null || _config.IotHubDeviceConnectionStrings.Count == 0)
        {
            throw new ArgumentException("At least one connection string must be provided.",
                nameof(_config.IotHubDeviceConnectionStrings));
        }

        _deviceConnectionStrings = _config.IotHubDeviceConnectionStrings;
        _logger.LogInformation("Supplied with {s} connection string(s).", _deviceConnectionStrings.Count);

        _transportType = transportType;
        _logger.LogInformation("Using {s} transport.", _transportType);
    }


    public async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (ShouldClientBeInitialized(ConnectionStatus))
        {
            // Allow a single thread to dispose and initialize the client instance.
            await InitSemaphore.WaitAsync(cancellationToken);
            try
            {
                if (ShouldClientBeInitialized(ConnectionStatus))
                {
                    _logger.LogDebug("Attempting to initialize the client instance, current status={s}",
                        ConnectionStatus);

                    // If the device client instance has been previously initialized, close and dispose it.
                    if (DeviceClient != null)
                    {
                        try
                        {
                            await DeviceClient.CloseAsync(cancellationToken);
                        }
                        catch (UnauthorizedException)
                        {
                        } // If the previous token is now invalid, this call may fail

                        DeviceClient.Dispose();
                    }

                    DeviceClient = DeviceClient.CreateFromConnectionString(_deviceConnectionStrings.First(),
                        _transportType, ClientOptions);
                    DeviceClient.SetConnectionStatusChangesHandler(ConnectionStatusChangeHandlerAsync);
                    DeviceClient.SetRetryPolicy(_retryPolicy);
                    _logger.LogDebug("Initialized the client instance.");

                    // Force connection now.
                    // OpenAsync() is an idempotent call, it has the same effect if called once or multiple times on the same client.
                    await DeviceClient.OpenAsync(cancellationToken);
                    _logger.LogDebug($"The client instance has been opened.");
                }
            }
            finally
            {
                InitSemaphore.Release();
            }

            // You will need to subscribe to the client callbacks any time the client is initialized.
            await DeviceClient?.SetDesiredPropertyUpdateCallbackAsync(HandleTwinUpdateNotificationsAsync, null,
                cancellationToken);
            _logger.LogDebug("The client has subscribed to desired property update notifications.");
        }
    }


    // It is not generally a good practice to have async void methods, however, DeviceClient.ConnectionStatusChangeHandlerAsync() event handler signature
    // has a void return type. As a result, any operation within this block will be executed unmonitored on another thread.
    // To prevent multi-threaded synchronization issues, the async method InitializeClientAsync being called in here first grabs a lock before attempting to
    // initialize or dispose the device client instance; the async method GetTwinAndDetectChangesAsync is implemented similarly for the same purpose
    private async void ConnectionStatusChangeHandlerAsync(ConnectionStatus status, ConnectionStatusChangeReason reason)
    {
        _logger.LogDebug("Connection status changed: status={Status}, reason={ConnectionStatusChangeReason}", status,
            reason);
        ConnectionStatus = status;

        switch (status)
        {
            case ConnectionStatus.Connected:
                _logger.LogDebug("### The DeviceClient is CONNECTED; all operations will be carried out as normal.");

                // Call GetTwinAndDetectChangesAsync() to retrieve twin values from the server once the connection status changes into Connected.
                // This can get back "lost" twin updates in a device reconnection from status like Disconnected_Retrying or Disconnected.
                //
                // However, considering how a fleet of devices connected to a hub may behave together, one must consider the implication of performing
                // work on a device (e.g., get twin) when it comes online. If all the devices go offline and then come online at the same time (for example,
                // during a servicing event) it could introduce increased latency or even throttling responses.
                // For more information, see https://docs.microsoft.com/azure/iot-hub/iot-hub-devguide-quotas-throttling#traffic-shaping.
                await GetTwinAndDetectChangesAsync(AppCancellation.Token);
                _logger.LogDebug(
                    "The client has retrieved twin values after the connection status changes into CONNECTED.");
                break;

            case ConnectionStatus.Disconnected_Retrying:
                _logger.LogDebug(
                    "### The DeviceClient is retrying based on the retry policy. Do NOT close or open the DeviceClient instance.");
                break;

            case ConnectionStatus.Disabled:
                _logger.LogDebug("### The DeviceClient has been closed gracefully." +
                                 "\nIf you want to perform more operations on the device client, you should dispose (DisposeAsync()) and then open (OpenAsync()) the client.");
                break;

            case ConnectionStatus.Disconnected:
                switch (reason)
                {
                    case ConnectionStatusChangeReason.Bad_Credential:
                        // When getting this reason, the current connection string being used is not valid.
                        // If we had a backup, we can try using that.
                        _deviceConnectionStrings.RemoveAt(0);
                        if (_deviceConnectionStrings.Any())
                        {
                            _logger.LogWarning($"The current connection string is invalid. Trying another.");

                            try
                            {
                                await ConnectAsync(AppCancellation.Token);
                            }
                            catch (OperationCanceledException)
                            {
                            } // User canceled

                            break;
                        }

                        _logger.LogWarning(
                            "### The supplied credentials are invalid. Update the parameters and run again.");
                        AppCancellation.Cancel();
                        break;

                    case ConnectionStatusChangeReason.Device_Disabled:
                        _logger.LogWarning(
                            "### The device has been deleted or marked as disabled (on your hub instance)." +
                            "\nFix the device status in Azure and then create a new device client instance.");
                        AppCancellation.Cancel();
                        break;

                    case ConnectionStatusChangeReason.Retry_Expired:
                        _logger.LogWarning(
                            "### The DeviceClient has been disconnected because the retry policy expired." +
                            "\nIf you want to perform more operations on the device client, you should dispose (DisposeAsync()) and then open (OpenAsync()) the client.");

                        try
                        {
                            await ConnectAsync(AppCancellation.Token);
                        }
                        catch (OperationCanceledException)
                        {
                        } // User canceled

                        break;

                    case ConnectionStatusChangeReason.Communication_Error:
                        _logger.LogWarning(
                            "### The DeviceClient has been disconnected due to a non-retry-able exception. Inspect the exception for details." +
                            "\nIf you want to perform more operations on the device client, you should dispose (DisposeAsync()) and then open (OpenAsync()) the client.");

                        try
                        {
                            await ConnectAsync(AppCancellation.Token);
                        }
                        catch (OperationCanceledException)
                        {
                        } // User canceled

                        break;

                    default:
                        _logger.LogError(
                            "### This combination of ConnectionStatus and ConnectionStatusChangeReason is not expected, contact the client library team with logs.");
                        break;
                }

                break;

            default:
                _logger.LogError(
                    "### This combination of ConnectionStatus and ConnectionStatusChangeReason is not expected, contact the client library team with logs.");
                break;
        }
    }


    private async Task GetTwinAndDetectChangesAsync(CancellationToken cancellationToken)
    {
        // For the following call, we execute with a retry strategy with incrementally increasing delays between retry.
        var twin = await DeviceClient.GetTwinAsync(cancellationToken);

        _logger.LogInformation("Device retrieving twin values: {ToJson}", twin.ToJson());

        var twinCollection = twin.Properties.Desired;
        long serverDesiredPropertyVersion = twinCollection.Version;

        // Check if the desired property version is outdated on the local side.
        if (serverDesiredPropertyVersion > _localDesiredPropertyVersion)
        {
            _logger.LogDebug(
                $"The desired property version cached on local is changing from {_localDesiredPropertyVersion} to {serverDesiredPropertyVersion}.");
            await HandleTwinUpdateNotificationsAsync(twinCollection, cancellationToken);
        }
    }

    private async Task HandleTwinUpdateNotificationsAsync(TwinCollection twinUpdateRequest, object userContext)
    {
        var reportedProperties = new TwinCollection();

        _logger.LogInformation("Twin property update requested: \n{ToJson}", twinUpdateRequest.ToJson());

        // For the purpose of this sample, we'll blindly accept all twin property write requests.
        // In a real-world scenario, you would want to validate the incoming values before applying them.
        // Also, you would want to make sure that the version of the desired properties being sent
        // is more recent than the version you have locally.
        //Todo: Add validation of incoming values
        foreach (KeyValuePair<string, object> desiredProperty in twinUpdateRequest)
        {
            _logger.LogInformation($"Setting property {desiredProperty.Key} to {desiredProperty.Value}.");
            reportedProperties[desiredProperty.Key] = desiredProperty.Value;
        }

        _localDesiredPropertyVersion = twinUpdateRequest.Version;
        _logger.LogDebug("The desired property version on local is currently {LocalDesiredPropertyVersion}.", _localDesiredPropertyVersion);

        try
        {
            // For the purpose of this sample, we'll blindly accept all twin property write requests.
            await DeviceClient.UpdateReportedPropertiesAsync(reportedProperties, AppCancellation.Token);
        }
        catch (OperationCanceledException)
        {
            // Fail gracefully on sample exit.
        }
    }
    
    private bool ShouldClientBeInitialized(ConnectionStatus connectionStatus)
    {
        return (connectionStatus == ConnectionStatus.Disconnected || connectionStatus == ConnectionStatus.Disabled)
               && _deviceConnectionStrings.Any();
    }
}