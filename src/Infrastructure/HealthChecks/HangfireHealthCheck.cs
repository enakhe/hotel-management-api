using Hangfire;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HotelManagement.Infrastructure.HealthChecks;

/// <summary>
/// Health check for Hangfire background job processing
/// </summary>
public class HangfireHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get Hangfire monitoring API
            var monitoringApi = JobStorage.Current.GetMonitoringApi();

            // Get statistics
            var stats = monitoringApi.GetStatistics();
            
            var data = new Dictionary<string, object>
            {
                { "servers", stats.Servers },
                { "enqueued", stats.Enqueued },
                { "scheduled", stats.Scheduled },
                { "processing", stats.Processing },
                { "succeeded", stats.Succeeded },
                { "failed", stats.Failed },
                { "recurring", stats.Recurring },
                { "queues", stats.Queues }
            };

            // Check if there are any servers running
            if (stats.Servers == 0)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    "No Hangfire servers are running",
                    data: data));
            }

            // Check for excessive failed jobs
            if (stats.Failed > 100)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Hangfire has {stats.Failed} failed job(s)",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "Hangfire is healthy",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "Hangfire health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                }));
        }
    }
}

