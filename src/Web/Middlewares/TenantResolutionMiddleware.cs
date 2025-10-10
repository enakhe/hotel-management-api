using HotelManagement.Application.Common.Interfaces.Tenant;
using Microsoft.AspNetCore.Http;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware that resolves the current tenant from the request
/// </summary>
public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantResolutionMiddleware> _logger;

    public TenantResolutionMiddleware(RequestDelegate next, ILogger<TenantResolutionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ITenantService tenantService)
    {
        try
        {
            // Skip tenant resolution for certain paths
            if (ShouldSkipTenantResolution(context.Request.Path))
            {
                await _next(context);
                return;
            }

            var tenantId = await ResolveTenantAsync(context, tenantService);

            if (tenantId.HasValue)
            {
                var tenantInfo = await tenantService.GetTenantInfoAsync(tenantId.Value);
                if (tenantInfo != null && tenantInfo.IsActive)
                {
                    tenantContext.SetTenant(tenantInfo.Id, tenantInfo.Identifier);
                    _logger.LogDebug("Resolved tenant: {TenantId} ({TenantIdentifier})", tenantInfo.Id, tenantInfo.Identifier);
                }
                else
                {
                    _logger.LogWarning("Tenant {TenantId} not found or inactive", tenantId);
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tenant not found or inactive");
                    return;
                }
            }
            else
            {
                // Check if this is a SuperAdmin route - allow without tenant
                if (context.Request.Path.StartsWithSegments("/cp"))
                {
                    _logger.LogDebug("SuperAdmin route - proceeding without tenant resolution");
                }
                else
                {
                    _logger.LogWarning("Could not resolve tenant from request: {Path}", context.Request.Path);
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid tenant");
                    return;
                }
            }

            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving tenant");
            context.Response.StatusCode = 500;
            await context.Response.WriteAsync("Internal server error");
        }
    }

    private static bool ShouldSkipTenantResolution(PathString path)
    {
        var skipPaths = new[]
        {
            "/health",
            "/api/health",
            "/swagger",
            "/api/swagger",
            "/error",
            "/",
            "/cp" // Skip all SuperAdmin control panel routes
        };

        return skipPaths.Any(skipPath => path.StartsWithSegments(skipPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<Guid?> ResolveTenantAsync(HttpContext context, ITenantService tenantService)
    {
        // Strategy 1: Resolve from subdomain (e.g., hotelA.myapp.com)
        var tenantId = await ResolveFromSubdomainAsync(context, tenantService);
        if (tenantId.HasValue)
            return tenantId;

        // Strategy 2: Resolve from header (X-Tenant-Id or X-Tenant-Identifier)
        tenantId = await ResolveFromHeaderAsync(context, tenantService);
        if (tenantId.HasValue)
            return tenantId;

        // Strategy 3: Resolve from query parameter (for testing/development)
        tenantId = await ResolveFromQueryAsync(context, tenantService);
        if (tenantId.HasValue)
            return tenantId;

        return null;
    }

    private async Task<Guid?> ResolveFromSubdomainAsync(HttpContext context, ITenantService tenantService)
    {
        var host = context.Request.Host.Host;

        // Skip if localhost or IP address
        if (host.Contains("localhost") || host.Contains("127.0.0.1") || System.Net.IPAddress.TryParse(host, out _))
            return null;

        // Extract subdomain (e.g., hotelA from hotelA.myapp.com)
        var parts = host.Split('.');
        if (parts.Length >= 2)
        {
            var subdomain = parts[0];
            if (!string.IsNullOrEmpty(subdomain) && subdomain != "www" && subdomain != "api")
            {
                return await tenantService.GetTenantIdByIdentifierAsync(subdomain);
            }
        }

        return null;
    }

    private async Task<Guid?> ResolveFromHeaderAsync(HttpContext context, ITenantService tenantService)
    {
        // Try X-Tenant-Id header first (GUID)
        if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader))
        {
            if (Guid.TryParse(tenantIdHeader, out var tenantId))
            {
                var isValid = await tenantService.IsTenantValidAsync(tenantId);
                return isValid ? tenantId : null;
            }
        }

        // Try X-Tenant-Identifier header (string identifier)
        if (context.Request.Headers.TryGetValue("X-Tenant-Identifier", out var tenantIdentifierHeader))
        {
            return await tenantService.GetTenantIdByIdentifierAsync(tenantIdentifierHeader!);
        }

        return null;
    }

    private async Task<Guid?> ResolveFromQueryAsync(HttpContext context, ITenantService tenantService)
    {
        // For development/testing purposes
        if (context.Request.Query.TryGetValue("tenantId", out var tenantIdQuery))
        {
            if (Guid.TryParse(tenantIdQuery, out var tenantId))
            {
                var isValid = await tenantService.IsTenantValidAsync(tenantId);
                return isValid ? tenantId : null;
            }
        }

        if (context.Request.Query.TryGetValue("tenant", out var tenantQuery))
        {
            return await tenantService.GetTenantIdByIdentifierAsync(tenantQuery!);
        }

        return null;
    }
}

