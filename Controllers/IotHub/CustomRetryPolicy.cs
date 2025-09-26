using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;

namespace Greenhouse.Controllers;

public class CustomRetryPolicy : IRetryPolicy
{
    private const int MaxRetryCount = int.MaxValue;

    // Avoid integer overlow (max of 30) and clamp max wait to just over 1 hour (2^22 = 1.16 hours).
    private const int MaxExponent = 22; 

    private readonly Random _random = new();
    private readonly object _randLock = new();

    private readonly HashSet<Type> _exceptionsToBeRetried;

    private readonly ILogger _logger;

    internal CustomRetryPolicy(HashSet<Type> exceptionsToBeRetried, ILogger logger)
    {
        _exceptionsToBeRetried = exceptionsToBeRetried;
        _logger = logger;
    }

    public bool ShouldRetry(int currentRetryCount, Exception lastException, out TimeSpan retryInterval)
    {
        retryInterval = TimeSpan.Zero;
        _logger.LogInformation(
            "Retry requested #{CurrentRetryCount} exception [{Type}: {LastExceptionMessage}].", currentRetryCount,
            lastException.GetType(), lastException.Message);

        if (currentRetryCount > MaxRetryCount)
        {
            _logger.LogError(
                "Retry requested #{CurrentRetryCount} of {I} so giving up.", currentRetryCount, MaxRetryCount);
            return false;
        }

        if (lastException is IotHubException iotHubException
            && iotHubException.IsTransient || ExceptionHelper.IsNetworkExceptionChain(lastException) ||
            _exceptionsToBeRetried.Contains(lastException.GetType()))
        {
            double jitterMs;
            // Because Random is not threadsafe
            lock (_randLock)
            {
                int plusOrMinus = _random.Next(0, 2) * 2 - 1;

                // a random double from 0 to 999, positive or negative
                jitterMs = plusOrMinus * _random.NextDouble() * 1000;
            }

            // Avoid integer overlow and clamp max wait.
            int exponent = Math.Min(MaxExponent, currentRetryCount);

            // 2 to the power of the retry count gives us exponential back-off.
            // Because jitter could be negative, protect the result with absolute value.
            double exponentialIntervalMs = Math.Abs(Math.Pow(2.0, exponent) + jitterMs);

            retryInterval = TimeSpan.FromMilliseconds(exponentialIntervalMs);
            _logger.LogInformation(
                "Retry requested #{CurrentRetryCount} with calculated delay of {RetryInterval}.", currentRetryCount,
                retryInterval);
            return true;
        }

        _logger.LogError(
            "Retry requested #{CurrentRetryCount} but failed criteria for retry [with {Type}: {LastExceptionMessage}], so giving up."
            , currentRetryCount, lastException.GetType(), lastException.Message);
        return false;
    }
}