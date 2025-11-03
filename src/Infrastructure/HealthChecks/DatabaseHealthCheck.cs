using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HotelManagement.Infrastructure.HealthChecks;

/// <summary>
/// Health check for database connectivity and migration status
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ApplicationDbContext _context;

    public DatabaseHealthCheck(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if database can be connected
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            
            if (!canConnect)
            {
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to database",
                    data: new Dictionary<string, object>
                    {
                        { "connection_status", "failed" }
                    });
            }

            // Check for pending migrations
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken);
            var pendingCount = pendingMigrations.Count();

            // Get applied migrations count
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync(cancellationToken);
            var appliedCount = appliedMigrations.Count();

            var data = new Dictionary<string, object>
            {
                { "connection_status", "connected" },
                { "applied_migrations", appliedCount },
                { "pending_migrations", pendingCount },
                { "provider", _context.Database.ProviderName ?? "unknown" }
            };

            // If there are pending migrations, return degraded status
            if (pendingCount > 0)
            {
                return HealthCheckResult.Degraded(
                    $"Database has {pendingCount} pending migration(s)",
                    data: data);
            }

            return HealthCheckResult.Healthy(
                "Database is healthy",
                data: data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                });
        }
    }
}

