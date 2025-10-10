using HotelManagement.Application.Common.Interfaces.Tenant;
using System.Collections.Concurrent;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware that implements per-tenant rate limiting
/// </summary>
public class TenantRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantRateLimitingMiddleware> _logger;
    private readonly ConcurrentDictionary<Guid, TenantRateLimit> _tenantLimits = new();
    private readonly int _maxRequestsPerMinute;
    private readonly int _maxRequestsPerHour;

    public TenantRateLimitingMiddleware(
        RequestDelegate next,
        ILogger<TenantRateLimitingMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _maxRequestsPerMinute = configuration.GetValue<int>("RateLimiting:MaxRequestsPerMinute", 100);
        _maxRequestsPerHour = configuration.GetValue<int>("RateLimiting:MaxRequestsPerHour", 1000);
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        // Skip rate limiting for certain paths
        if (ShouldSkipRateLimiting(context.Request.Path))
        {
            await _next(context);
            return;
        }

        // Only apply rate limiting if tenant context is resolved
        if (!tenantContext.IsResolved)
        {
            await _next(context);
            return;
        }

        var tenantId = tenantContext.TenantId!.Value;
        var now = DateTime.UtcNow;

        // Get or create rate limit for tenant
        var rateLimit = _tenantLimits.GetOrAdd(tenantId, _ => new TenantRateLimit());

        // Check rate limits
        if (IsRateLimitExceeded(rateLimit, now))
        {
            _logger.LogWarning("Rate limit exceeded for tenant {TenantId}", tenantId);
            context.Response.StatusCode = 429; // Too Many Requests
            context.Response.Headers.Append("Retry-After", "60"); // Retry after 1 minute
            await context.Response.WriteAsync("Rate limit exceeded. Please try again later.");
            return;
        }

        // Update rate limit counters
        UpdateRateLimit(rateLimit, now);

        await _next(context);
    }

    private static bool ShouldSkipRateLimiting(PathString path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/api/health",
            "/swagger",
            "/api/swagger",
            "/error"
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsRateLimitExceeded(TenantRateLimit rateLimit, DateTime now)
    {
        // Check minute limit
        if (rateLimit.RequestsThisMinute >= _maxRequestsPerMinute)
            return true;

        // Check hour limit
        if (rateLimit.RequestsThisHour >= _maxRequestsPerHour)
            return true;

        return false;
    }

    private void UpdateRateLimit(TenantRateLimit rateLimit, DateTime now)
    {
        // Reset counters if we're in a new minute/hour
        if (now.Minute != rateLimit.LastMinute)
        {
            rateLimit.RequestsThisMinute = 0;
            rateLimit.LastMinute = now.Minute;
        }

        if (now.Hour != rateLimit.LastHour)
        {
            rateLimit.RequestsThisHour = 0;
            rateLimit.LastHour = now.Hour;
        }

        // Increment counters
        rateLimit.RequestsThisMinute++;
        rateLimit.RequestsThisHour++;
        rateLimit.LastRequestTime = now;
    }
}

/// <summary>
/// Tracks rate limiting for a specific tenant
/// </summary>
public class TenantRateLimit
{
    public int RequestsThisMinute { get; set; }
    public int RequestsThisHour { get; set; }
    public int LastMinute { get; set; }
    public int LastHour { get; set; }
    public DateTime LastRequestTime { get; set; }
}
