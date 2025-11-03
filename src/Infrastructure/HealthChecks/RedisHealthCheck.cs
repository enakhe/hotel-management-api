using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace HotelManagement.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Redis cache connectivity
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer? _redis;

    public RedisHealthCheck(IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_redis == null)
            {
                return HealthCheckResult.Degraded(
                    "Redis is not configured",
                    data: new Dictionary<string, object>
                    {
                        { "status", "not_configured" }
                    });
            }

            if (!_redis.IsConnected)
            {
                return HealthCheckResult.Unhealthy(
                    "Redis is not connected",
                    data: new Dictionary<string, object>
                    {
                        { "status", "disconnected" }
                    });
            }

            // Test a ping to verify connectivity
            var database = _redis.GetDatabase();
            var pingTime = await database.PingAsync();

            var endpoints = _redis.GetEndPoints();
            
            var data = new Dictionary<string, object>
            {
                { "status", "connected" },
                { "ping_ms", pingTime.TotalMilliseconds },
                { "endpoints", endpoints.Length }
            };

            if (pingTime.TotalMilliseconds > 100)
            {
                return HealthCheckResult.Degraded(
                    $"Redis response time is slow ({pingTime.TotalMilliseconds:F2}ms)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                "Redis is healthy",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Redis health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                });
        }
    }
}

