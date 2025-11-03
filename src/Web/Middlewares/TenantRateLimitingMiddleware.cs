using System.Collections.Concurrent;
using HotelManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Enhanced middleware that implements per-tenant and per-plan rate limiting
/// </summary>
public class TenantRateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantRateLimitingMiddleware> _logger;
    private readonly IDistributedCache _cache;
    private readonly ConcurrentDictionary<Guid, TenantRateLimit> _tenantLimits = new();
    private readonly Dictionary<string, EndpointRateLimit> _endpointLimits;
    private readonly int _defaultMaxRequestsPerMinute;
    private readonly int _defaultMaxRequestsPerHour;

    public TenantRateLimitingMiddleware(
        RequestDelegate next,
        ILogger<TenantRateLimitingMiddleware> logger,
        IDistributedCache cache,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _cache = cache;
        _defaultMaxRequestsPerMinute = configuration.GetValue<int>("RateLimiting:MaxRequestsPerMinute", 100);
        _defaultMaxRequestsPerHour = configuration.GetValue<int>("RateLimiting:MaxRequestsPerHour", 1000);

        // Configure endpoint-specific limits
        _endpointLimits = new Dictionary<string, EndpointRateLimit>
        {
            { "/api/v1/auth/login", new EndpointRateLimit { MaxRequestsPerMinute = 10, MaxRequestsPerHour = 50 } },
            { "/api/v1/auth/register", new EndpointRateLimit { MaxRequestsPerMinute = 5, MaxRequestsPerHour = 20 } },
            { "/cp/tenant", new EndpointRateLimit { MaxRequestsPerMinute = 30, MaxRequestsPerHour = 200 } }
        };
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
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;

        // Get endpoint-specific limits if configured
        var endpointLimit = GetEndpointLimit(path);

        // Get or create rate limit for tenant
        var rateLimit = _tenantLimits.GetOrAdd(tenantId, _ => new TenantRateLimit());

        // Determine applicable limits (endpoint-specific or default)
        var maxPerMinute = endpointLimit?.MaxRequestsPerMinute ?? _defaultMaxRequestsPerMinute;
        var maxPerHour = endpointLimit?.MaxRequestsPerHour ?? _defaultMaxRequestsPerHour;

        // Check rate limits
        if (IsRateLimitExceeded(rateLimit, now, maxPerMinute, maxPerHour))
        {
            _logger.LogWarning(
                "Rate limit exceeded for tenant {TenantId} on endpoint {Endpoint}. " +
                "Requests this minute: {RequestsPerMinute}, Requests this hour: {RequestsPerHour}",
                tenantId,
                path,
                rateLimit.RequestsThisMinute,
                rateLimit.RequestsThisHour);

            // Add rate limit headers
            context.Response.Headers["X-RateLimit-Limit-Minute"] = maxPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Limit-Hour"] = maxPerHour.ToString();
            context.Response.Headers["X-RateLimit-Remaining-Minute"] = "0";
            context.Response.Headers["X-RateLimit-Remaining-Hour"] = "0";
            context.Response.Headers["Retry-After"] = "60";

            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsJsonAsync(new
            {
                error = "rate_limit_exceeded",
                message = "Rate limit exceeded. Please try again later.",
                retryAfter = 60,
                limit = new
                {
                    perMinute = maxPerMinute,
                    perHour = maxPerHour
                }
            });
            return;
        }

        // Update rate limit counters
        UpdateRateLimit(rateLimit, now);

        // Add rate limit info to response headers
        var remainingMinute = Math.Max(0, maxPerMinute - rateLimit.RequestsThisMinute);
        var remainingHour = Math.Max(0, maxPerHour - rateLimit.RequestsThisHour);

        context.Response.OnStarting(() =>
        {
            context.Response.Headers["X-RateLimit-Limit-Minute"] = maxPerMinute.ToString();
            context.Response.Headers["X-RateLimit-Limit-Hour"] = maxPerHour.ToString();
            context.Response.Headers["X-RateLimit-Remaining-Minute"] = remainingMinute.ToString();
            context.Response.Headers["X-RateLimit-Remaining-Hour"] = remainingHour.ToString();
            return Task.CompletedTask;
        });

        await _next(context);
    }

    private EndpointRateLimit? GetEndpointLimit(string path)
    {
        foreach (var limit in _endpointLimits)
        {
            if (path.StartsWith(limit.Key, StringComparison.OrdinalIgnoreCase))
            {
                return limit.Value;
            }
        }
        return null;
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

    private bool IsRateLimitExceeded(TenantRateLimit rateLimit, DateTime now, int maxPerMinute, int maxPerHour)
    {
        // Check minute limit
        if (rateLimit.RequestsThisMinute >= maxPerMinute)
            return true;

        // Check hour limit
        if (rateLimit.RequestsThisHour >= maxPerHour)
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

/// <summary>
/// Rate limiting configuration for specific endpoints
/// </summary>
public class EndpointRateLimit
{
    public int MaxRequestsPerMinute { get; set; }
    public int MaxRequestsPerHour { get; set; }
}
