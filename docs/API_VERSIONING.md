# API Versioning Guide

## Overview

The application uses URL-based API versioning to maintain backward compatibility while allowing the API to evolve. This enables smooth transitions when introducing breaking changes.

## Versioning Strategy

### URL Segment Versioning (Primary)

```
https://api.example.com/api/v1/auth/login
https://api.example.com/api/v2/auth/login
```

**Advantages**:

- ✅ Clear and visible in URLs
- ✅ Easy to understand
- ✅ Works well with API gateways
- ✅ Cacheable

### Header Versioning (Alternative)

```http
GET /api/auth/login HTTP/1.1
X-Api-Version: 1.0
```

### Query String Versioning (Alternative)

```
https://api.example.com/api/auth/login?api-version=1.0
```

## Configuration

API versioning is configured in `DependencyInjection.cs`:

```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("X-Api-Version"),
        new QueryStringApiVersionReader("api-version"));
});
```

**Options Explained**:

- `DefaultApiVersion`: Version to use when not specified (1.0)
- `AssumeDefaultVersionWhenUnspecified`: Use default if no version provided
- `ReportApiVersions`: Add supported versions to response headers
- `ApiVersionReader`: How to read the version (URL, header, query string)

## Implementing Versioned Controllers

### Version 1.0 Controller

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginCommand command)
    {
        // v1.0 implementation
    }
}
```

### Version 2.0 Controller (Breaking Changes)

```csharp
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthV2Controller : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginV2Command command)
    {
        // v2.0 implementation with breaking changes
    }
}
```

### Supporting Multiple Versions

```csharp
[ApiController]
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult> GetUsers()
    {
        // Works for both v1.0 and v2.0
    }

    [HttpGet("details")]
    [MapToApiVersion("2.0")] // Only in v2.0
    public async Task<ActionResult> GetUserDetails()
    {
        // Only available in v2.0
    }
}
```

### Deprecating Versions

```csharp
[ApiController]
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    // v1.0 is deprecated but still functional
}
```

## Usage Examples

### Client Requests

**URL Versioning**:

```bash
# Version 1.0
curl https://api.example.com/api/v1/auth/login

# Version 2.0
curl https://api.example.com/api/v2/auth/login
```

**Header Versioning**:

```bash
curl -H "X-Api-Version: 1.0" https://api.example.com/api/auth/login
```

**Query String Versioning**:

```bash
curl "https://api.example.com/api/auth/login?api-version=1.0"
```

### Response Headers

The API returns version information in response headers:

```http
HTTP/1.1 200 OK
api-supported-versions: 1.0, 2.0
api-deprecated-versions: 1.0
```

## Version Management

### Current Versions

| Version | Status | Release Date | Deprecation Date | Sunset Date |
| ------- | ------ | ------------ | ---------------- | ----------- |
| 1.0     | Active | 2024-01-01   | -                | -           |

### Versioning Policy

1. **Major Versions** (v1, v2, v3)

   - Breaking changes
   - New version required

2. **Minor Versions** (v1.1, v1.2)

   - Backward compatible
   - Same major version

3. **Deprecation Timeline**
   - Announce deprecation: 6 months before sunset
   - Maintain support: Minimum 12 months
   - Sunset old version: After 12-18 months

### Breaking vs Non-Breaking Changes

**Breaking Changes** (require new major version):

- ❌ Removing endpoints
- ❌ Renaming properties
- ❌ Changing data types
- ❌ Changing required fields
- ❌ Changing response structure

**Non-Breaking Changes** (can be in same version):

- ✅ Adding new endpoints
- ✅ Adding new optional properties
- ✅ Adding new query parameters
- ✅ Expanding enums
- ✅ Performance improvements

## Migration Guide

### Migrating Clients from v1 to v2

**Step 1: Review Changes**

Check the changelog for breaking changes:

```
GET /api/v2/changelog
```

**Step 2: Test with New Version**

```csharp
// Test v2 endpoints
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Api-Version", "2.0");
```

**Step 3: Update Client Code**

```csharp
// v1.0
var response = await httpClient.GetAsync("/api/v1/users");

// v2.0
var response = await httpClient.GetAsync("/api/v2/users");
```

**Step 4: Deploy and Monitor**

- Deploy new client version
- Monitor error rates
- Keep v1 as fallback

## Swagger/OpenAPI Configuration

API versions appear as separate documents in Swagger:

```
/swagger/v1/swagger.json
/swagger/v2/swagger.json
```

**Swagger UI**:

- Version selector dropdown
- Separate documentation per version

## Best Practices

### 1. Version All Controllers

```csharp
// ✅ Good
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/resource")]

// ❌ Bad - No version specified
[Route("api/resource")]
```

### 2. Plan for the Future

```csharp
// Design with versioning in mind
public class UserV1Dto { } // Version-specific DTOs
public class UserV2Dto { }
```

### 3. Document Breaking Changes

Maintain a CHANGELOG.md:

```markdown
## [2.0.0] - 2024-06-01

### Breaking Changes

- Renamed `UserId` to `Id` in UserDto
- Removed `IsDeleted` property
- Changed date format to ISO 8601

### Added

- New endpoint: GET /api/v2/users/{id}/preferences

### Fixed

- Fixed pagination bug in user list
```

### 4. Communicate Deprecations

```csharp
[ApiVersion("1.0", Deprecated = true)]
[Obsolete("Use v2.0 instead. This version will be removed on 2025-01-01")]
public class UsersV1Controller : ControllerBase
{
}
```

Send deprecation headers:

```http
HTTP/1.1 200 OK
Sunset: Sat, 01 Jan 2025 00:00:00 GMT
Deprecation: true
Link: <https://api.example.com/api/v2/users>; rel="alternate"
```

### 5. Support Multiple Versions

```csharp
// Keep old version for compatibility
[ApiVersion("1.0")]
public class UsersV1Controller { }

// New version with improvements
[ApiVersion("2.0")]
public class UsersV2Controller { }
```

## Testing Versioned APIs

### Unit Tests

```csharp
[Test]
public async Task GetUsers_V1_ReturnsCorrectFormat()
{
    // Arrange
    var client = _factory.CreateClient();

    // Act
    var response = await client.GetAsync("/api/v1/users");

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    var users = await response.Content.ReadAsAsync<List<UserV1Dto>>();
    users.Should().NotBeNull();
}

[Test]
public async Task GetUsers_V2_ReturnsNewFormat()
{
    var client = _factory.CreateClient();
    var response = await client.GetAsync("/api/v2/users");
    var users = await response.Content.ReadAsAsync<List<UserV2Dto>>();
    users.Should().NotBeNull();
}
```

### Integration Tests

```csharp
[Test]
public async Task VersionHeader_IsReturned()
{
    var response = await _client.GetAsync("/api/v1/users");

    response.Headers.Should().ContainKey("api-supported-versions");
    var versions = response.Headers.GetValues("api-supported-versions").First();
    versions.Should().Contain("1.0");
}
```

## Troubleshooting

### Issue: "The API version specified in the URL is invalid"

**Cause**: Version not registered or incorrect format

**Solution**:

```csharp
// Ensure controller has version attribute
[ApiVersion("1.0")]
```

### Issue: Multiple versions conflict

**Cause**: Same endpoint defined in multiple version controllers without MapToApiVersion

**Solution**:

```csharp
[MapToApiVersion("2.0")]
[HttpGet("new-endpoint")]
public async Task<ActionResult> NewEndpoint() { }
```

### Issue: Default version not working

**Cause**: `AssumeDefaultVersionWhenUnspecified` not enabled

**Solution**:

```csharp
services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
});
```

## Monitoring API Versions

### Application Insights Query

```kusto
requests
| extend apiVersion = tostring(customDimensions.ApiVersion)
| summarize
    RequestCount = count(),
    AvgDuration = avg(duration)
  by apiVersion, name
| order by RequestCount desc
```

### Usage Analytics

Track version usage to plan deprecations:

```csharp
public class VersionUsageMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var version = context.GetRequestedApiVersion();
        _metrics.RecordVersionUsage(version);
        await _next(context);
    }
}
```

## API Lifecycle

```
[Active] → [Deprecated] → [Sunset]
  ↓           ↓              ↓
Launch    -6 months     -12 months
```

**Example Timeline**:

- **Jan 2024**: v2.0 launched, v1.0 still active
- **Jul 2024**: v1.0 marked deprecated
- **Jan 2025**: v1.0 sunset (removed)

## Client Migration Strategies

### 1. Gradual Migration

```csharp
// Use feature flags
if (_featureFlags.UseV2Api)
{
    return await _apiClient.V2.GetUsersAsync();
}
return await _apiClient.V1.GetUsersAsync();
```

### 2. Canary Deployment

- Route 10% of traffic to v2
- Monitor error rates
- Gradually increase to 100%

### 3. Blue-Green Deployment

- Deploy v2 alongside v1
- Switch traffic all at once
- Quick rollback if needed

## Related Documentation

- [API Design Guidelines](./API_DESIGN.md)
- [Breaking Changes Policy](./BREAKING_CHANGES.md)
- [Swagger Documentation](./SWAGGER.md)
