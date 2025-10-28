using HotelManagement.Application.Common.Interfaces;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware for SuperAdmin authentication and audit logging
/// </summary>
public class SuperAdminMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SuperAdminMiddleware> _logger;

    public SuperAdminMiddleware(RequestDelegate next, ILogger<SuperAdminMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISuperAdminContext superAdminContext, ISuperAdminAuditService auditService)
    {
        // Check if this is a SuperAdmin control plane request
        if (context.Request.Path.StartsWithSegments("/cp"))
        {
            // Extract SuperAdmin information from JWT claims
            var superAdminIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("superadmin_id");
            var usernameClaim = context.User.FindFirst("preferred_username") ?? context.User.FindFirst("username");
            var mfaVerifiedClaim = context.User.FindFirst("mfa_verified");
            var sessionIdClaim = context.User.FindFirst("session_id");

            if (superAdminIdClaim != null && usernameClaim != null)
            {
                var superAdminId = Guid.Parse(superAdminIdClaim.Value);
                var username = usernameClaim.Value;
                var isMfaVerified = mfaVerifiedClaim?.Value == "true";
                var sessionId = sessionIdClaim?.Value ?? Guid.NewGuid().ToString();

                superAdminContext.SetSuperAdmin(superAdminId, username, isMfaVerified, sessionId);

                _logger.LogDebug("SuperAdmin context set: {SuperAdminId} ({Username})", superAdminId, username);
            }
            else
            {
                _logger.LogWarning("SuperAdmin claims not found in JWT token");
            }
        }

        await _next(context);

        // Log SuperAdmin actions after the request is processed
        if (context.Request.Path.StartsWithSegments("/cp") && superAdminContext.IsSuperAdmin)
        {
            await LogSuperAdminActionAsync(context, superAdminContext, auditService);
        }
    }

    private async Task LogSuperAdminActionAsync(
        HttpContext context,
        ISuperAdminContext superAdminContext,
        ISuperAdminAuditService auditService)
    {
        try
        {
            var action = GetActionFromPath(context.Request.Path, context.Request.Method);
            var targetType = GetTargetTypeFromPath(context.Request.Path);
            var targetId = GetTargetIdFromPath(context.Request.Path);

            // Extract tenant ID from path if present
            Guid? tenantId = null;
            var pathSegments = context.Request.Path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (pathSegments?.Length > 2 && pathSegments[0] == "cp" && pathSegments[1] == "tenants")
            {
                if (Guid.TryParse(pathSegments[2], out var parsedTenantId))
                {
                    tenantId = parsedTenantId;
                }
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            var userAgent = context.Request.Headers.UserAgent.ToString();

            await auditService.LogActionAsync(
                action,
                targetType,
                targetId,
                tenantId,
                null, // details
                null, // changes
                ipAddress,
                userAgent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging SuperAdmin action");
        }
    }

    private static string GetActionFromPath(PathString path, string method)
    {
        var pathSegments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments == null || pathSegments.Length < 2)
            return "Unknown";

        var resource = pathSegments[1];
        var action = pathSegments.Length > 2 ? pathSegments[^1] : "List";

        return method.ToUpper() switch
        {
            "GET" => action == "List" ? $"List{resource}" : $"Get{resource}",
            "POST" => action == "List" ? $"Create{resource}" : action,
            "PUT" => $"Update{resource}",
            "PATCH" => $"Update{resource}",
            "DELETE" => $"Delete{resource}",
            _ => "Unknown"
        };
    }

    private static string GetTargetTypeFromPath(PathString path)
    {
        var pathSegments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments == null || pathSegments.Length < 2)
            return "Unknown";

        return pathSegments[1] switch
        {
            "tenants" => "Tenant",
            "audit" => "Audit",
            _ => "Unknown"
        };
    }

    private static string? GetTargetIdFromPath(PathString path)
    {
        var pathSegments = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (pathSegments == null || pathSegments.Length < 3)
            return null;

        // Look for GUID in the path
        for (int i = 2; i < pathSegments.Length; i++)
        {
            if (Guid.TryParse(pathSegments[i], out _))
            {
                return pathSegments[i];
            }
        }

        return null;
    }
}
