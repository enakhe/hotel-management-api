using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace HotelManagement.Infrastructure.HealthChecks;

/// <summary>
/// Health check for system resources (memory, disk space)
/// </summary>
public class SystemResourcesHealthCheck : IHealthCheck
{
    private readonly long _memoryThresholdBytes;
    private readonly long _diskThresholdBytes;

    public SystemResourcesHealthCheck(
        long memoryThresholdBytes = 500 * 1024 * 1024, // 500MB default
        long diskThresholdBytes = 1024 * 1024 * 1024) // 1GB default
    {
        _memoryThresholdBytes = memoryThresholdBytes;
        _diskThresholdBytes = diskThresholdBytes;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Get current process
            var process = Process.GetCurrentProcess();
            
            // Memory usage
            var workingSet = process.WorkingSet64;
            var privateMemory = process.PrivateMemorySize64;
            
            // GC memory
            var gcMemory = GC.GetTotalMemory(false);
            var gen0Collections = GC.CollectionCount(0);
            var gen1Collections = GC.CollectionCount(1);
            var gen2Collections = GC.CollectionCount(2);

            // Disk space (application drive)
            var appPath = AppContext.BaseDirectory;
            var drive = new DriveInfo(Path.GetPathRoot(appPath) ?? "C:\\");
            var availableDiskSpace = drive.AvailableFreeSpace;
            var totalDiskSpace = drive.TotalSize;
            var diskUsagePercent = ((totalDiskSpace - availableDiskSpace) * 100.0) / totalDiskSpace;

            var data = new Dictionary<string, object>
            {
                { "working_set_mb", workingSet / 1024 / 1024 },
                { "private_memory_mb", privateMemory / 1024 / 1024 },
                { "gc_memory_mb", gcMemory / 1024 / 1024 },
                { "gen0_collections", gen0Collections },
                { "gen1_collections", gen1Collections },
                { "gen2_collections", gen2Collections },
                { "available_disk_gb", availableDiskSpace / 1024 / 1024 / 1024 },
                { "total_disk_gb", totalDiskSpace / 1024 / 1024 / 1024 },
                { "disk_usage_percent", Math.Round(diskUsagePercent, 2) },
                { "cpu_count", Environment.ProcessorCount }
            };

            // Check memory threshold
            if (workingSet < _memoryThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Low available memory ({workingSet / 1024 / 1024}MB)",
                    data: data));
            }

            // Check disk threshold
            if (availableDiskSpace < _diskThresholdBytes)
            {
                return Task.FromResult(HealthCheckResult.Unhealthy(
                    $"Low disk space ({availableDiskSpace / 1024 / 1024 / 1024}GB available)",
                    data: data));
            }

            // Warning if disk usage is high
            if (diskUsagePercent > 90)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"Disk usage is high ({diskUsagePercent:F1}%)",
                    data: data));
            }

            // Warning if Gen2 collections are frequent (may indicate memory pressure)
            if (gen2Collections > 100)
            {
                return Task.FromResult(HealthCheckResult.Degraded(
                    $"High Gen2 garbage collections ({gen2Collections})",
                    data: data));
            }

            return Task.FromResult(HealthCheckResult.Healthy(
                "System resources are healthy",
                data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy(
                "System resources health check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message }
                }));
        }
    }
}

