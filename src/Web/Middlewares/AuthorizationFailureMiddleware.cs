using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware to handle authorization failures and return proper JSON responses
/// </summary>
public class AuthorizationFailureMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuthorizationFailureMiddleware> _logger;

    public AuthorizationFailureMiddleware(RequestDelegate next, ILogger<AuthorizationFailureMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        // Check if the response is a 401 Unauthorized or 403 Forbidden
        if (context.Response.StatusCode == 401 || context.Response.StatusCode == 403)
        {
            // Check if the response hasn't been written to yet
            if (!context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";

                // Check if this is a SuperAdmin route
                var isSuperAdminRoute = context.Request.Path.StartsWithSegments("/cp");

                string message;
                string title;

                if (context.Response.StatusCode == 401)
                {
                    title = "Unauthorized";
                    message = isSuperAdminRoute
                        ? "Authentication required. Please log in with SuperAdmin credentials to access this resource."
                        : "Authentication required. Please log in to access this resource.";
                }
                else // 403
                {
                    title = "Forbidden";
                    message = isSuperAdminRoute
                        ? "Access denied. SuperAdmin role required to perform this action."
                        : "Access denied. You do not have permission to perform this action.";
                }

                var errorResponse = new
                {
                    statusCode = context.Response.StatusCode,
                    title = title,
                    message = message,
                    type = context.Response.StatusCode == 401
                        ? "https://tools.ietf.org/html/rfc7235#section-3.1"
                        : "https://tools.ietf.org/html/rfc7231#section-6.5.3"
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
            }
        }
    }
}
