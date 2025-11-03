using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace HotelManagement.Infrastructure.Resilience;

/// <summary>
/// Factory for creating HTTP clients with resilience policies
/// </summary>
public class ResilientHttpClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ResilientHttpClientFactory> _logger;

    public ResilientHttpClientFactory(
        IHttpClientFactory httpClientFactory,
        ILogger<ResilientHttpClientFactory> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Creates an HTTP client with retry, circuit breaker, and timeout policies
    /// </summary>
    public HttpClient CreateResilientClient(string clientName = "default")
    {
        return _httpClientFactory.CreateClient(clientName);
    }

    /// <summary>
    /// Gets a retry policy for HTTP requests
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError() // 408, 5xx errors
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    logger.LogWarning(
                        "Request failed with {StatusCode}. Retry {Retry} after {Delay}s",
                        outcome.Result?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError,
                        retryCount,
                        timespan.TotalSeconds);
                });
    }

    /// <summary>
    /// Gets a circuit breaker policy for HTTP requests
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(ILogger logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, duration) =>
                {
                    logger.LogError(
                        "Circuit breaker opened for {Duration}s. Status: {StatusCode}",
                        duration.TotalSeconds,
                        outcome.Result?.StatusCode ?? System.Net.HttpStatusCode.InternalServerError);
                },
                onReset: () =>
                {
                    logger.LogInformation("Circuit breaker closed - service recovered");
                },
                onHalfOpen: () =>
                {
                    logger.LogInformation("Circuit breaker half-open - testing service");
                });
    }

    /// <summary>
    /// Gets a timeout policy for HTTP requests
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(ILogger logger)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            timeout: TimeSpan.FromSeconds(30),
            onTimeoutAsync: async (context, timeSpan, task) =>
            {
                logger.LogWarning("HTTP request timed out after {Timeout}s", timeSpan.TotalSeconds);
                await Task.CompletedTask;
            });
    }
}

