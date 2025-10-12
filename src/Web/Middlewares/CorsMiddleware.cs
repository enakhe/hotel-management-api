namespace HotelManagement.Web.Middlewares;

public class CorsMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    public async Task Invoke(HttpContext httpContext)
    {
        var allowedOrigins = new[] {
            "http://localhost:3000",
            "https://localhost:3000"
        };

        var origin = httpContext.Request.Headers.Origin.ToString();

        // Handle cases where origin might be empty or null
        if (string.IsNullOrEmpty(origin))
        {
            // For development, allow requests without origin header
            if (httpContext.Request.Host.Host.Contains("localhost"))
            {
                origin = "http://localhost:3000";
            }
        }

        // Handle preflight requests first
        if (httpContext.Request.Method == "OPTIONS")
        {
            if (allowedOrigins.Contains(origin) || httpContext.Request.Host.Host.Contains("localhost"))
            {
                var allowedOrigin = allowedOrigins.Contains(origin) ? origin : "http://localhost:3000";
                httpContext.Response.Headers.AccessControlAllowOrigin = allowedOrigin;
                httpContext.Response.Headers.AccessControlAllowCredentials = "true";
                httpContext.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
                httpContext.Response.Headers.AccessControlAllowHeaders = "Origin, X-Requested-With, Content-Type, Accept, Authorization, X-Tenant-Id, X-Tenant-Identifier, X-Request-Id, X-Admin-Portal, X-App-Version, X-Debug-Timestamp, Cache-Control, Pragma";
                httpContext.Response.Headers.AccessControlAllowMethods = "GET, POST, PUT, DELETE, PATCH, OPTIONS";
                httpContext.Response.Headers.AccessControlMaxAge = "86400";
            }
            httpContext.Response.StatusCode = StatusCodes.Status200OK;
            return;
        }

        // Handle actual requests
        if (allowedOrigins.Contains(origin) || httpContext.Request.Host.Host.Contains("localhost"))
        {
            var allowedOrigin = allowedOrigins.Contains(origin) ? origin : "http://localhost:3000";
            httpContext.Response.Headers.AccessControlAllowOrigin = allowedOrigin;
            httpContext.Response.Headers.AccessControlAllowCredentials = "true";
            httpContext.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
        }

        await _next(httpContext);
    }
}
