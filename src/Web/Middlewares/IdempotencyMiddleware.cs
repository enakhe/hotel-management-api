using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Text;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware for handling idempotency keys on POST/DELETE operations
/// </summary>
public class IdempotencyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IDistributedCache? _cache;
    private readonly ILogger<IdempotencyMiddleware> _logger;

    public IdempotencyMiddleware(RequestDelegate next, IServiceProvider serviceProvider, ILogger<IdempotencyMiddleware> logger)
    {
        _next = next;
        _cache = serviceProvider.GetService<IDistributedCache>();
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip idempotency if no distributed cache is available
        if (_cache == null)
        {
            await _next(context);
            return;
        }

        // Only apply idempotency to POST and DELETE requests
        if (context.Request.Method != "POST" && context.Request.Method != "DELETE")
        {
            await _next(context);
            return;
        }

        // Check for Idempotency-Key header
        if (!context.Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyKey))
        {
            await _next(context);
            return;
        }

        var key = idempotencyKey.ToString();
        if (string.IsNullOrEmpty(key))
        {
            await _next(context);
            return;
        }

        // Create a unique cache key for this request
        var cacheKey = $"idempotency:{key}:{context.Request.Path}:{context.Request.Method}";

        try
        {
            // Check if we've already processed this request
            var cachedResponse = await _cache.GetAsync(cacheKey);
            if (cachedResponse != null)
            {
                var responseData = Encoding.UTF8.GetString(cachedResponse);
                var responseParts = responseData.Split('|', 2);

                if (responseParts.Length == 2)
                {
                    var statusCode = int.Parse(responseParts[0]);
                    var responseBody = responseParts[1];

                    context.Response.StatusCode = statusCode;
                    context.Response.Headers.Append("X-Idempotent-Response", "true");
                    await context.Response.WriteAsync(responseBody);
                    return;
                }
            }

            // Store the original response stream
            var originalBodyStream = context.Response.Body;
            using var responseBodyStream = new MemoryStream();
            context.Response.Body = responseBodyStream;

            // Process the request
            await _next(context);

            // Cache the response if it was successful
            if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
            {
                responseBodyStream.Seek(0, SeekOrigin.Begin);
                var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();
                var responseData = $"{context.Response.StatusCode}|{responseBody}";

                await _cache.SetAsync(cacheKey, Encoding.UTF8.GetBytes(responseData), new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Cache for 24 hours
                });

                _logger.LogDebug("Cached idempotent response for key {Key}", key);
            }

            // Copy the response back to the original stream
            responseBodyStream.Seek(0, SeekOrigin.Begin);
            await responseBodyStream.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling idempotency for key {Key}", key);
            await _next(context);
        }
    }
}
