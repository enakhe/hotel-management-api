using HotelManagement.Application.Common.Interfaces;
using System.Diagnostics;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware for logging HTTP requests with correlation ID, tenant context, and performance metrics
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ITenantContext? tenantContext = null,
        IUser? currentUser = null)
    {
        // Generate or retrieve correlation ID
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? context.TraceIdentifier;

        // Add correlation ID to response headers
        context.Response.Headers["X-Correlation-ID"] = correlationId;

        // Start timing the request
        var stopwatch = Stopwatch.StartNew();

        // Create logging scope with request context
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
            ["RequestPath"] = context.Request.Path,
            ["RequestMethod"] = context.Request.Method,
            ["TenantId"] = tenantContext?.TenantId?.ToString() ?? "none",
            ["TenantIdentifier"] = tenantContext?.TenantIdentifier ?? "none",
            ["UserId"] = currentUser?.Id ?? "anonymous",
            ["UserAgent"] = context.Request.Headers["User-Agent"].FirstOrDefault() ?? "unknown",
            ["RemoteIP"] = context.Connection.RemoteIpAddress?.ToString() ?? "unknown"
        }))
        {
            try
            {
                // Log request start
                _logger.LogInformation(
                    "HTTP {Method} {Path} started for tenant {TenantId}",
                    context.Request.Method,
                    context.Request.Path,
                    tenantContext?.TenantId?.ToString() ?? "none");

                // Process the request
                await _next(context);

                stopwatch.Stop();

                // Log successful request completion
                _logger.LogInformation(
                    "HTTP {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                // Log warning if request was slow
                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    _logger.LogWarning(
                        "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                        context.Request.Method,
                        context.Request.Path,
                        stopwatch.ElapsedMilliseconds);
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                // Log request failure
                _logger.LogError(
                    ex,
                    "HTTP {Method} {Path} failed with exception after {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}

