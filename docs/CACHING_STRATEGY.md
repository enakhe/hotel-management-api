# Distributed Caching Strategy

## Overview

The application uses Redis for distributed caching to improve performance, reduce database load, and provide fast access to frequently used data across multiple application instances.

## Caching Architecture

```
Application Layer
       ↓
  ICacheService (Interface)
       ↓
  CacheService (Implementation)
       ↓
IDistributedCache (ASP.NET Core)
       ↓
   Redis Cache
```

## Cache Service

### Basic Usage

```csharp
public class UserService
{
    private readonly ICacheService _cache;
    private readonly ApplicationDbContext _context;

    public async Task<UserDto> GetUserByIdAsync(Guid userId)
    {
        // Try to get from cache first
        var cacheKey = CacheKeys.User(userId);
        var cachedUser = await _cache.GetAsync<UserDto>(cacheKey);
        
        if (cachedUser != null)
        {
            return cachedUser;
        }

        // Not in cache, fetch from database
        var user = await _context.Users.FindAsync(userId);
        var userDto = _mapper.Map<UserDto>(user);

        // Cache for future requests
        await _cache.SetAsync(
            cacheKey, 
            userDto, 
            expiration: TimeSpan.FromMinutes(15));

        return userDto;
    }
}
```

### Using GetOrCreateAsync

```csharp
public async Task<List<BranchDto>> GetTenantBranchesAsync(Guid tenantId)
{
    var cacheKey = CacheKeys.TenantBranches(tenantId);
    
    return await _cache.GetOrCreateAsync(
        cacheKey,
        factory: async () =>
        {
            var branches = await _context.Branches
                .Where(b => b.TenantId == tenantId && b.IsActive)
                .ToListAsync();
            return _mapper.Map<List<BranchDto>>(branches);
        },
        expiration: TimeSpan.FromMinutes(60));
}
```

## Cache Key Naming Convention

### Standard Format

```
{entity}:{id}:{property}
```

**Examples**:
- `tenant:abc123:config` - Tenant configuration
- `user:xyz789:permissions` - User permissions
- `plan:def456` - Plan data

### Using CacheKeys Helper

```csharp
// Defined cache keys
var key = CacheKeys.TenantConfig(tenantId);
var key = CacheKeys.UserPermissions(userId);
var key = CacheKeys.TenantBranches(tenantId);

// Custom keys
var key = $"tenant:{tenantId}:custom_data";
```

## What to Cache

### ✅ Good Candidates for Caching

1. **Tenant Configuration**
   - Expiration: 60 minutes
   - Key: `tenant:{tenantId}:config`
   - Invalidate on: Tenant settings update

2. **User Permissions**
   - Expiration: 15 minutes
   - Key: `user:{userId}:permissions`
   - Invalidate on: Role changes, permission updates

3. **User Roles**
   - Expiration: 15 minutes
   - Key: `user:{userId}:roles`
   - Invalidate on: Role assignment changes

4. **Lookup Data** (Plans, Modules)
   - Expiration: 24 hours
   - Key: `plan:{planId}`, `module:{moduleId}`
   - Invalidate on: Updates

5. **Branch List**
   - Expiration: 30 minutes
   - Key: `tenant:{tenantId}:branches`
   - Invalidate on: Branch creation/update

### ❌ Don't Cache

1. **Real-time Data**
   - Current reservations
   - Live availability
   - Active sessions

2. **Sensitive Data**
   - Passwords (never cache)
   - Auth tokens (short-lived)
   - Payment information

3. **Frequently Changing Data**
   - Booking status
   - Real-time analytics
   - Audit logs

4. **Large Objects**
   - Binary files
   - Large reports
   - Images (use CDN/blob storage)

## Cache Expiration Strategy

### Time-Based Expiration

```csharp
// Short-lived (5-15 minutes) - Frequently updated data
await _cache.SetAsync(key, value, TimeSpan.FromMinutes(15));

// Medium-lived (30-60 minutes) - Semi-static data
await _cache.SetAsync(key, value, TimeSpan.FromMinutes(30));

// Long-lived (hours/days) - Static/lookup data
await _cache.SetAsync(key, value, TimeSpan.FromHours(24));
```

### Manual Invalidation

```csharp
// Invalidate specific key
await _cache.RemoveAsync(CacheKeys.TenantConfig(tenantId));

// Invalidate all tenant data
await _cache.InvalidateTenantCacheAsync(tenantId);

// Invalidate after update
public async Task UpdateBranchAsync(BranchDto branch)
{
    await _repository.UpdateAsync(branch);
    await _cache.RemoveAsync(CacheKeys.TenantBranches(branch.TenantId));
}
```

## Cache Configuration

**appsettings.json**:
```json
{
  "Caching": {
    "DefaultExpirationMinutes": 30,
    "TenantConfigExpirationMinutes": 60,
    "UserPermissionsExpirationMinutes": 15,
    "StaticDataExpirationHours": 24,
    "EnableCaching": true
  }
}
```

**Environment-Specific**:

Development:
```json
{
  "Caching": {
    "EnableCaching": false  // Disable for debugging
  }
}
```

Production:
```json
{
  "Caching": {
    "EnableCaching": true,
    "DefaultExpirationMinutes": 30
  }
}
```

## Cache Invalidation Patterns

### 1. Write-Through Cache

```csharp
public async Task UpdateUserAsync(UpdateUserDto dto)
{
    // Update database
    await _repository.UpdateAsync(dto);
    
    // Update cache immediately
    var user = await _repository.GetByIdAsync(dto.Id);
    await _cache.SetAsync(CacheKeys.User(dto.Id), user);
}
```

### 2. Cache-Aside (Lazy Loading)

```csharp
public async Task<UserDto> GetUserAsync(Guid userId)
{
    return await _cache.GetOrCreateAsync(
        CacheKeys.User(userId),
        factory: () => _repository.GetByIdAsync(userId));
}
```

### 3. Time-Based Invalidation

```csharp
// Cache with expiration
await _cache.SetAsync(
    key, 
    value, 
    expiration: TimeSpan.FromMinutes(30));
```

### 4. Event-Based Invalidation

```csharp
// In your update handler
public async Task Handle(UpdateTenantCommand request, CancellationToken cancellationToken)
{
    await _tenantService.UpdateAsync(request);
    
    // Invalidate all tenant caches
    await _cache.InvalidateTenantCacheAsync(request.TenantId);
}
```

## Performance Optimization

### Batching Cache Operations

```csharp
public async Task<List<UserDto>> GetUsersAsync(List<Guid> userIds)
{
    var users = new List<UserDto>();
    var missingIds = new List<Guid>();

    // Try to get from cache
    foreach (var id in userIds)
    {
        var cached = await _cache.GetAsync<UserDto>(CacheKeys.User(id));
        if (cached != null)
        {
            users.Add(cached);
        }
        else
        {
            missingIds.Add(id);
        }
    }

    // Fetch missing users from database
    if (missingIds.Any())
    {
        var dbUsers = await _context.Users
            .Where(u => missingIds.Contains(u.Id))
            .ToListAsync();

        foreach (var user in dbUsers)
        {
            var dto = _mapper.Map<UserDto>(user);
            users.Add(dto);
            
            // Cache individual users
            await _cache.SetAsync(CacheKeys.User(user.Id), dto);
        }
    }

    return users;
}
```

### Cache Warming

```csharp
public class CacheWarmingService : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Warm up frequently accessed data
        await WarmTenantConfigsAsync();
        await WarmStaticDataAsync();
    }

    private async Task WarmTenantConfigsAsync()
    {
        var activeTenants = await _context.Tenants
            .Where(t => t.IsActive)
            .ToListAsync();

        foreach (var tenant in activeTenants)
        {
            await _cache.SetAsync(
                CacheKeys.TenantConfig(tenant.Id),
                tenant,
                TimeSpan.FromHours(1));
        }
    }
}
```

## Monitoring Cache Performance

### Cache Hit Ratio

```csharp
public class CacheMetrics
{
    private long _hits;
    private long _misses;

    public void RecordHit() => Interlocked.Increment(ref _hits);
    public void RecordMiss() => Interlocked.Increment(ref _misses);

    public double HitRatio => _hits + _misses == 0 
        ? 0 
        : (double)_hits / (_hits + _misses) * 100;
}
```

### Logging Cache Operations

```csharp
// Already implemented in CacheService
_logger.LogDebug("Cache hit for key: {CacheKey}", key);
_logger.LogDebug("Cache miss for key: {CacheKey}", key);
```

## Troubleshooting

### Cache Not Working

**Check Redis Connection**:
```bash
redis-cli -h localhost -p 6379 ping
# Expected: PONG
```

**Check Configuration**:
```csharp
// Verify cache is registered
var cache = services.GetService<IDistributedCache>();
Console.WriteLine($"Cache type: {cache.GetType().Name}");
```

### High Memory Usage in Redis

**Monitor Redis Memory**:
```bash
redis-cli info memory
```

**Set Max Memory**:
```bash
redis-cli config set maxmemory 2gb
redis-cli config set maxmemory-policy allkeys-lru
```

### Cache Stampede

**Problem**: Multiple requests simultaneously fetch same uncached data

**Solution**: Use SemaphoreSlim to prevent concurrent cache misses

```csharp
private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory)
{
    var value = await _cache.GetAsync<T>(key);
    if (value != null) return value;

    await _semaphore.WaitAsync();
    try
    {
        // Double-check after acquiring lock
        value = await _cache.GetAsync<T>(key);
        if (value != null) return value;

        // Create and cache
        value = await factory();
        await _cache.SetAsync(key, value);
        return value;
    }
    finally
    {
        _semaphore.Release();
    }
}
```

## Best Practices

### 1. Always Handle Cache Failures Gracefully

```csharp
try
{
    var cached = await _cache.GetAsync<T>(key);
    if (cached != null) return cached;
}
catch (Exception ex)
{
    _logger.LogWarning(ex, "Cache retrieval failed, falling back to database");
}

// Fallback to database
return await _repository.GetAsync(id);
```

### 2. Use Appropriate Expiration Times

- **Hot Data** (frequently accessed): 5-15 minutes
- **Warm Data** (regularly accessed): 30-60 minutes
- **Cold Data** (rarely changes): Hours to days

### 3. Include Tenant ID in Cache Keys

```csharp
// ✅ Good: Tenant-specific key
var key = $"tenant:{tenantId}:branches";

// ❌ Bad: Could leak data across tenants
var key = "branches";
```

### 4. Monitor Cache Performance

Track key metrics:
- Cache hit ratio (target: >80%)
- Average retrieval time
- Cache size
- Eviction rate

### 5. Implement Cache Versioning

```csharp
// Include version in key for easy invalidation
const string CACHE_VERSION = "v2";
var key = $"{CACHE_VERSION}:tenant:{tenantId}:config";

// When schema changes, just bump version
```

## Production Considerations

### High Availability

**Redis Sentinel** (automatic failover):
```
ConnectionString: "sentinel1:26379,sentinel2:26379,serviceName=mymaster"
```

**Redis Cluster** (sharding):
```
ConnectionString: "node1:6379,node2:6379,node3:6379"
```

### Security

```bash
# Enable authentication
requirepass your-strong-password

# Restrict commands
rename-command CONFIG ""
rename-command FLUSHALL ""
```

### Persistence

```bash
# AOF (Append-Only File) for durability
appendonly yes
appendfsync everysec
```

## Related Documentation

- [Redis Best Practices](https://redis.io/docs/manual/patterns/)
- [ASP.NET Core Distributed Caching](https://docs.microsoft.com/aspnet/core/performance/caching/distributed)

