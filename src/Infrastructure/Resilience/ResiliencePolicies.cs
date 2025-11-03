using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace HotelManagement.Infrastructure.Resilience;

/// <summary>
/// Centralized resilience policies using Polly
/// </summary>
public static class ResiliencePolicies
{
    /// <summary>
    /// Standard retry policy for transient failures
    /// </summary>
    public static AsyncRetryPolicy CreateStandardRetryPolicy(ILogger logger, int retryCount = 3)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: retryCount,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Retry {Retry} after {Delay}s due to {ExceptionType}: {Message}",
                        retry,
                        timeSpan.TotalSeconds,
                        exception.GetType().Name,
                        exception.Message);
                });
    }

    /// <summary>
    /// Circuit breaker policy to prevent cascading failures
    /// </summary>
    public static AsyncCircuitBreakerPolicy CreateCircuitBreakerPolicy(
        ILogger logger,
        int exceptionsBeforeBreaking = 5,
        int durationOfBreakInSeconds = 30)
    {
        return Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: exceptionsBeforeBreaking,
                durationOfBreak: TimeSpan.FromSeconds(durationOfBreakInSeconds),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "Circuit breaker opened for {Duration}s due to {ExceptionType}: {Message}",
                        duration.TotalSeconds,
                        exception.GetType().Name,
                        exception.Message);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker reset - service recovered");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open - testing service");
                });
    }

    /// <summary>
    /// Timeout policy for preventing hung requests
    /// </summary>
    public static AsyncTimeoutPolicy CreateTimeoutPolicy(
        ILogger logger,
        int timeoutInSeconds = 30)
    {
        return Policy
            .TimeoutAsync(
                timeout: TimeSpan.FromSeconds(timeoutInSeconds),
                timeoutStrategy: TimeoutStrategy.Pessimistic,
                onTimeoutAsync: async (context, timeSpan, task) =>
                {
                    logger.LogWarning(
                        "Operation timed out after {Timeout}s",
                        timeSpan.TotalSeconds);
                    await Task.CompletedTask;
                });
    }

    /// <summary>
    /// Fallback policy for graceful degradation
    /// </summary>
    public static AsyncPolicy<T> CreateFallbackPolicy<T>(
        ILogger logger,
        T fallbackValue,
        string serviceName)
    {
        return Policy<T>
            .Handle<Exception>()
            .FallbackAsync(
                fallbackValue: fallbackValue,
                onFallbackAsync: async (result, context) =>
                {
                    logger.LogWarning(
                        result.Exception,
                        "{ServiceName} failed, using fallback value",
                        serviceName);
                    await Task.CompletedTask;
                });
    }

    /// <summary>
    /// Combined policy: Retry + Circuit Breaker + Timeout
    /// </summary>
    public static IAsyncPolicy CreateCombinedPolicy(ILogger logger)
    {
        var retry = CreateStandardRetryPolicy(logger, retryCount: 3);
        var circuitBreaker = CreateCircuitBreakerPolicy(logger, exceptionsBeforeBreaking: 5, durationOfBreakInSeconds: 30);
        var timeout = CreateTimeoutPolicy(logger, timeoutInSeconds: 30);

        // Wrap policies: Timeout → Retry → Circuit Breaker
        return Policy.WrapAsync(timeout, retry, circuitBreaker);
    }

    /// <summary>
    /// Policy for email service
    /// </summary>
    public static IAsyncPolicy CreateEmailPolicy(ILogger logger)
    {
        var retry = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 2,
                sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Email send retry {Retry} after {Delay}s",
                        retry,
                        timeSpan.TotalSeconds);
                });

        var timeout = Policy
            .TimeoutAsync(TimeSpan.FromSeconds(10));

        return Policy.WrapAsync(timeout, retry);
    }

    /// <summary>
    /// Policy for database operations
    /// </summary>
    public static IAsyncPolicy CreateDatabasePolicy(ILogger logger)
    {
        var retry = Policy
            .Handle<Exception>(ex => IsTransientError(ex))
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "Database operation retry {Retry} after {Delay}ms",
                        retry,
                        timeSpan.TotalMilliseconds);
                });

        return retry;
    }

    /// <summary>
    /// Policy for external API calls
    /// </summary>
    public static IAsyncPolicy CreateExternalApiPolicy(ILogger logger)
    {
        var retry = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timeSpan, retry, context) =>
                {
                    logger.LogWarning(
                        exception,
                        "External API retry {Retry} after {Delay}s",
                        retry,
                        timeSpan.TotalSeconds);
                });

        var circuitBreaker = Policy
            .Handle<HttpRequestException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    logger.LogError(
                        exception,
                        "External API circuit breaker opened for {Duration}",
                        duration);
                },
                onReset: () => logger.LogInformation("External API circuit breaker reset"));

        var timeout = Policy.TimeoutAsync(TimeSpan.FromSeconds(30));

        return Policy.WrapAsync(timeout, retry, circuitBreaker);
    }

    /// <summary>
    /// Determines if an exception is a transient error that should be retried
    /// </summary>
    private static bool IsTransientError(Exception ex)
    {
        // Check for known transient errors
        var message = ex.Message.ToLowerInvariant();
        
        return message.Contains("timeout") ||
               message.Contains("deadlock") ||
               message.Contains("network") ||
               message.Contains("connection") ||
               ex is TimeoutException ||
               ex is HttpRequestException;
    }
}

/// <summary>
/// Configuration options for resilience policies
/// </summary>
public class ResilienceOptions
{
    public int RetryCount { get; set; } = 3;
    public int CircuitBreakerThreshold { get; set; } = 5;
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableResilience { get; set; } = true;
}

