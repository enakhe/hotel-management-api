namespace HotelManagement.Web.Middlewares;

/// <summary>
/// Middleware that adds security headers to HTTP responses
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityHeadersMiddleware> _logger;
    private readonly SecurityHeadersOptions _options;

    public SecurityHeadersMiddleware(
        RequestDelegate next,
        ILogger<SecurityHeadersMiddleware> logger,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _options = configuration.GetSection("SecurityHeaders").Get<SecurityHeadersOptions>() ?? new SecurityHeadersOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Add security headers before processing the request
        AddSecurityHeaders(context);

        await _next(context);
    }

    private void AddSecurityHeaders(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent clickjacking attacks
        if (_options.EnableXFrameOptions)
        {
            headers["X-Frame-Options"] = _options.XFrameOptions;
        }

        // Prevent MIME type sniffing
        if (_options.EnableXContentTypeOptions)
        {
            headers["X-Content-Type-Options"] = "nosniff";
        }

        // Enable XSS filter (legacy browsers)
        if (_options.EnableXXssProtection)
        {
            headers["X-XSS-Protection"] = "1; mode=block";
        }

        // Content Security Policy
        if (_options.EnableContentSecurityPolicy && !string.IsNullOrWhiteSpace(_options.ContentSecurityPolicy))
        {
            headers["Content-Security-Policy"] = _options.ContentSecurityPolicy;
        }

        // Referrer Policy
        if (_options.EnableReferrerPolicy)
        {
            headers["Referrer-Policy"] = _options.ReferrerPolicy;
        }

        // Permissions Policy (formerly Feature Policy)
        if (_options.EnablePermissionsPolicy && !string.IsNullOrWhiteSpace(_options.PermissionsPolicy))
        {
            headers["Permissions-Policy"] = _options.PermissionsPolicy;
        }

        // Strict-Transport-Security (HSTS) - only on HTTPS
        if (_options.EnableHsts && context.Request.IsHttps)
        {
            var maxAge = _options.HstsMaxAgeInSeconds;
            var includeSubDomains = _options.HstsIncludeSubDomains ? "; includeSubDomains" : "";
            var preload = _options.HstsPreload ? "; preload" : "";

            headers["Strict-Transport-Security"] = $"max-age={maxAge}{includeSubDomains}{preload}";
        }

        // Remove server information disclosure
        if (_options.RemoveServerHeader)
        {
            headers.Remove("Server");
            headers.Remove("X-Powered-By");
            headers.Remove("X-AspNet-Version");
            headers.Remove("X-AspNetMvc-Version");
        }

        _logger.LogTrace("Security headers applied to response");
    }
}

/// <summary>
/// Configuration options for security headers
/// </summary>
public class SecurityHeadersOptions
{
    public bool EnableXFrameOptions { get; set; } = true;
    public string XFrameOptions { get; set; } = "DENY";

    public bool EnableXContentTypeOptions { get; set; } = true;

    public bool EnableXXssProtection { get; set; } = true;

    public bool EnableContentSecurityPolicy { get; set; } = true;
    public string ContentSecurityPolicy { get; set; } = 
        "default-src 'self'; " +
        "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
        "style-src 'self' 'unsafe-inline'; " +
        "img-src 'self' data: https:; " +
        "font-src 'self' data:; " +
        "connect-src 'self'; " +
        "frame-ancestors 'none'; " +
        "base-uri 'self'; " +
        "form-action 'self'";

    public bool EnableReferrerPolicy { get; set; } = true;
    public string ReferrerPolicy { get; set; } = "strict-origin-when-cross-origin";

    public bool EnablePermissionsPolicy { get; set; } = true;
    public string PermissionsPolicy { get; set; } = 
        "accelerometer=(), " +
        "camera=(), " +
        "geolocation=(), " +
        "gyroscope=(), " +
        "magnetometer=(), " +
        "microphone=(), " +
        "payment=(), " +
        "usb=()";

    public bool EnableHsts { get; set; } = true;
    public int HstsMaxAgeInSeconds { get; set; } = 31536000; // 1 year
    public bool HstsIncludeSubDomains { get; set; } = true;
    public bool HstsPreload { get; set; } = false;

    public bool RemoveServerHeader { get; set; } = true;
}

