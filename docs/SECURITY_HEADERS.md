# Security Headers Guide

## Overview

The application implements comprehensive security headers to protect against common web vulnerabilities. These headers are automatically applied to all HTTP responses via the `SecurityHeadersMiddleware`.

## Implemented Security Headers

### 1. X-Frame-Options

**Purpose**: Prevents clickjacking attacks by controlling whether the site can be embedded in frames/iframes.

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnableXFrameOptions": true,
    "XFrameOptions": "DENY"
  }
}
```

**Options**:
- `DENY` - Prevents any framing (recommended for most applications)
- `SAMEORIGIN` - Allows framing only from same origin
- `ALLOW-FROM uri` - Allows framing from specific URI (deprecated)

**Default**: `DENY`

### 2. X-Content-Type-Options

**Purpose**: Prevents MIME type sniffing attacks.

**Value**: `nosniff`

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnableXContentTypeOptions": true
  }
}
```

**Default**: Enabled

### 3. X-XSS-Protection

**Purpose**: Enables the Cross-Site Scripting (XSS) filter built into browsers (legacy).

**Value**: `1; mode=block`

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnableXXssProtection": true
  }
}
```

**Default**: Enabled

**Note**: Modern browsers use CSP instead, but this provides defense-in-depth for older browsers.

### 4. Content-Security-Policy (CSP)

**Purpose**: Controls which resources the browser is allowed to load, preventing XSS and data injection attacks.

**Default Policy**:
```
default-src 'self'; 
script-src 'self' 'unsafe-inline' 'unsafe-eval'; 
style-src 'self' 'unsafe-inline'; 
img-src 'self' data: https:; 
font-src 'self' data:; 
connect-src 'self'; 
frame-ancestors 'none'; 
base-uri 'self'; 
form-action 'self'
```

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnableContentSecurityPolicy": true,
    "ContentSecurityPolicy": "default-src 'self'; ..."
  }
}
```

**Directives Explained**:
- `default-src 'self'` - Only load resources from same origin by default
- `script-src` - Controls JavaScript sources
- `style-src` - Controls CSS sources
- `img-src` - Controls image sources
- `font-src` - Controls font sources
- `connect-src` - Controls fetch/XHR/WebSocket connections
- `frame-ancestors` - Controls who can embed this site
- `base-uri` - Restricts `<base>` tag usage
- `form-action` - Controls form submission targets

**Customization for Development**:

If you need to allow specific sources (e.g., CDNs), update the policy:

```json
{
  "ContentSecurityPolicy": "default-src 'self'; script-src 'self' https://cdn.example.com; style-src 'self' https://fonts.googleapis.com"
}
```

### 5. Strict-Transport-Security (HSTS)

**Purpose**: Forces browsers to use HTTPS for all future requests.

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnableHsts": true,
    "HstsMaxAgeInSeconds": 31536000,
    "HstsIncludeSubDomains": true,
    "HstsPreload": false
  }
}
```

**Parameters**:
- `HstsMaxAgeInSeconds`: How long browsers should remember to use HTTPS (default: 1 year)
- `HstsIncludeSubDomains`: Apply to all subdomains
- `HstsPreload`: Submit to HSTS preload list (requires careful consideration)

**Value**: `max-age=31536000; includeSubDomains`

**Important**: Only applied on HTTPS requests. HTTP requests will not receive this header.

**HSTS Preload List**:
To submit your domain to the HSTS preload list:
1. Set `HstsPreload` to `true`
2. Ensure HTTPS is working on all subdomains
3. Submit at https://hstspreload.org/

### 6. Referrer-Policy

**Purpose**: Controls how much referrer information is sent with requests.

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnableReferrerPolicy": true,
    "ReferrerPolicy": "strict-origin-when-cross-origin"
  }
}
```

**Options**:
- `no-referrer` - Never send referrer
- `same-origin` - Send referrer only for same-origin requests
- `strict-origin` - Send only origin (not full URL)
- `strict-origin-when-cross-origin` - Full URL for same-origin, origin for cross-origin (recommended)

**Default**: `strict-origin-when-cross-origin`

### 7. Permissions-Policy

**Purpose**: Controls which browser features and APIs can be used (formerly Feature-Policy).

**Default Policy**:
```
accelerometer=(), camera=(), geolocation=(), gyroscope=(), 
magnetometer=(), microphone=(), payment=(), usb=()
```

**Configuration**:
```json
{
  "SecurityHeaders": {
    "EnablePermissionsPolicy": true,
    "PermissionsPolicy": "accelerometer=(), camera=(), ..."
  }
}
```

**Syntax**:
- `feature=()` - Disable feature for all origins
- `feature=(self)` - Enable only for same origin
- `feature=(self "https://example.com")` - Enable for self and specific origin
- `feature=*` - Enable for all origins (not recommended)

**Common Features**:
- `camera` - Camera access
- `microphone` - Microphone access
- `geolocation` - Location access
- `payment` - Payment Request API
- `usb` - WebUSB API
- `accelerometer`, `gyroscope`, `magnetometer` - Motion sensors

### 8. Server Header Removal

**Purpose**: Removes version information that could help attackers.

**Configuration**:
```json
{
  "SecurityHeaders": {
    "RemoveServerHeader": true
  }
}
```

**Headers Removed**:
- `Server`
- `X-Powered-By`
- `X-AspNet-Version`
- `X-AspNetMvc-Version`

## Request Size Limits

To prevent denial-of-service attacks and resource exhaustion:

**Maximum Request Body Size**: 100MB (configurable)

**Affected Components**:
- IIS Server
- Kestrel Server
- Form/Multipart uploads
- Model binding collections

**Configuration**: Set in `Program.cs`

```csharp
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 104857600; // 100MB
});
```

## Testing Security Headers

### Using Browser DevTools

1. Open DevTools (F12)
2. Go to Network tab
3. Refresh page
4. Click on any request
5. View "Response Headers" section

### Using curl

```bash
curl -I https://your-api-url/api/health

# Expected output includes:
# X-Frame-Options: DENY
# X-Content-Type-Options: nosniff
# Content-Security-Policy: default-src 'self'; ...
# Strict-Transport-Security: max-age=31536000; includeSubDomains
```

### Using Online Tools

- **Security Headers**: https://securityheaders.com/
- **Mozilla Observatory**: https://observatory.mozilla.org/
- **SSL Labs**: https://www.ssllabs.com/ssltest/

## Environment-Specific Configuration

### Development

```json
{
  "SecurityHeaders": {
    "EnableHsts": false,
    "ContentSecurityPolicy": "default-src 'self' 'unsafe-inline' 'unsafe-eval'"
  }
}
```

**Note**: Relaxed CSP allows for hot-reload and development tools.

### Staging

Same as production but with `HstsPreload: false`

### Production

```json
{
  "SecurityHeaders": {
    "EnableHsts": true,
    "HstsMaxAgeInSeconds": 31536000,
    "HstsIncludeSubDomains": true,
    "HstsPreload": true,
    "ContentSecurityPolicy": "default-src 'self'; script-src 'self'; ..."
  }
}
```

## Common Issues and Solutions

### Issue: CSP Blocking Swagger UI

**Symptom**: Swagger UI not loading, console shows CSP errors

**Solution**: Add Swagger domains to CSP:

```json
{
  "ContentSecurityPolicy": "default-src 'self'; script-src 'self' 'unsafe-inline'; style-src 'self' 'unsafe-inline' 'unsafe-eval'"
}
```

Or disable CSP in development:

```json
{
  "SecurityHeaders": {
    "EnableContentSecurityPolicy": false
  }
}
```

### Issue: CORS Preflight Failing

**Symptom**: OPTIONS requests failing with security header errors

**Solution**: Security headers are compatible with CORS. Ensure CORS middleware is registered after SecurityHeadersMiddleware:

```csharp
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<CorsMiddleware>();
app.UseCors("AllowSpecificOrigins");
```

### Issue: Third-Party Scripts Blocked

**Symptom**: Google Analytics, CDN resources not loading

**Solution**: Update CSP to allow trusted domains:

```json
{
  "ContentSecurityPolicy": "default-src 'self'; script-src 'self' https://www.googletagmanager.com https://cdn.example.com; img-src 'self' data: https:"
}
```

### Issue: File Downloads Failing

**Symptom**: Large file downloads interrupted

**Solution**: Increase request size limits in `Program.cs`:

```csharp
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = 524288000; // 500MB
});
```

## Security Best Practices

1. **Start Strict**: Begin with strictest policies, then relax as needed
2. **Test Thoroughly**: Test in development before deploying to production
3. **Monitor CSP Violations**: Use CSP reporting to detect issues
4. **Update Regularly**: Review and update policies as application evolves
5. **Document Exceptions**: Document why specific rules were relaxed

## CSP Reporting

To receive reports of CSP violations, add a `report-uri` or `report-to` directive:

```json
{
  "ContentSecurityPolicy": "default-src 'self'; report-uri /api/csp-report"
}
```

Then create an endpoint to handle reports:

```csharp
[HttpPost("csp-report")]
public IActionResult CspReport([FromBody] object report)
{
    _logger.LogWarning("CSP Violation: {Report}", report);
    return NoContent();
}
```

## Compliance

These security headers help meet compliance requirements for:
- **OWASP Top 10** - Protection against several top vulnerabilities
- **PCI DSS** - Payment card industry security standards
- **HIPAA** - Healthcare data protection (with additional controls)
- **GDPR** - Privacy and data protection

## Additional Resources

- [OWASP Secure Headers Project](https://owasp.org/www-project-secure-headers/)
- [MDN Web Security](https://developer.mozilla.org/en-US/docs/Web/Security)
- [Content Security Policy Reference](https://content-security-policy.com/)
- [Security Headers Scanner](https://securityheaders.com/)

