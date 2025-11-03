using HotelManagement.Application.Common.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Distributed cache service implementation using Redis
/// </summary>
public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CacheService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CacheService(IDistributedCache cache, ILogger<CacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedData = await _cache.GetStringAsync(key, cancellationToken);

            if (string.IsNullOrEmpty(cachedData))
            {
                _logger.LogDebug("Cache miss for key: {CacheKey}", key);
                return default;
            }

            _logger.LogDebug("Cache hit for key: {CacheKey}", key);
            return JsonSerializer.Deserialize<T>(cachedData, _jsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving from cache: {CacheKey}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var serializedData = JsonSerializer.Serialize(value, _jsonOptions);

            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(30)
            };

            await _cache.SetStringAsync(key, serializedData, options, cancellationToken);

            _logger.LogDebug(
                "Cached value for key: {CacheKey}, expires in: {Expiration}",
                key,
                options.AbsoluteExpirationRelativeToNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {CacheKey}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cache key: {CacheKey}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {CacheKey}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await _cache.GetStringAsync(key, cancellationToken);
            return !string.IsNullOrEmpty(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache key existence: {CacheKey}", key);
            return false;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        // Try to get from cache
        var cachedValue = await GetAsync<T>(key, cancellationToken);

        if (cachedValue != null)
        {
            return cachedValue;
        }

        // Not in cache, create new value
        _logger.LogDebug("Cache miss for key: {CacheKey}, creating new value", key);
        var value = await factory();

        // Cache the new value
        await SetAsync(key, value, expiration, cancellationToken);

        return value;
    }

    public async Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            // Note: Pattern-based removal requires StackExchange.Redis directly
            // For IDistributedCache, we'll need to maintain a list of keys
            _logger.LogWarning(
                "Pattern-based cache removal not fully implemented for IDistributedCache. Pattern: {Pattern}",
                pattern);

            // This is a simplified implementation
            // For production, consider using StackExchange.Redis directly or maintaining key lists
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache by pattern: {Pattern}", pattern);
        }
    }

    public async Task InvalidateTenantCacheAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Invalidating cache for tenant: {TenantId}", tenantId);

            // Remove common tenant-specific cache keys
            var keysToRemove = new[]
            {
                $"tenant:{tenantId}:config",
                $"tenant:{tenantId}:permissions",
                $"tenant:{tenantId}:users",
                $"tenant:{tenantId}:branches",
                $"tenant:{tenantId}:roles"
            };

            foreach (var key in keysToRemove)
            {
                await RemoveAsync(key, cancellationToken);
            }

            _logger.LogInformation("Cache invalidated for tenant: {TenantId}", tenantId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating tenant cache: {TenantId}", tenantId);
        }
    }
}

/// <summary>
/// Cache key builder for consistent key naming
/// </summary>
public static class CacheKeys
{
    // Tenant-related keys
    public static string TenantConfig(Guid tenantId) => $"tenant:{tenantId}:config";
    public static string TenantPermissions(Guid tenantId) => $"tenant:{tenantId}:permissions";
    public static string TenantStats(Guid tenantId) => $"tenant:{tenantId}:stats";
    public static string TenantBranches(Guid tenantId) => $"tenant:{tenantId}:branches";
    public static string TenantUsers(Guid tenantId, int page, int pageSize) => 
        $"tenant:{tenantId}:users:page_{page}_{pageSize}";
    public static string TenantRooms(Guid tenantId, Guid branchId) => 
        $"tenant:{tenantId}:branch:{branchId}:rooms";
    
    // User-related keys
    public static string User(Guid userId) => $"user:{userId}";
    public static string UserPermissions(Guid userId) => $"user:{userId}:permissions";
    public static string UserRoles(Guid userId) => $"user:{userId}:roles";
    public static string UserProfile(Guid userId) => $"user:{userId}:profile";
    
    // Entity keys
    public static string Branch(Guid branchId) => $"branch:{branchId}";
    public static string Room(Guid roomId) => $"room:{roomId}";
    public static string Reservation(Guid reservationId) => $"reservation:{reservationId}";
    
    // Plan and Module keys (global/system-wide)
    public static string Plan(Guid planId) => $"plan:{planId}";
    public static string AllPlans() => "plans:all";
    public static string Module(Guid moduleId) => $"module:{moduleId}";
    public static string AllModules() => "modules:all";
    
    // Dashboard keys
    public static string DashboardData(Guid tenantId, string userId) => 
        $"tenant:{tenantId}:user:{userId}:dashboard";
    
    // Availability/booking keys
    public static string AvailableRooms(Guid branchId, DateTime date) => 
        $"branch:{branchId}:available_rooms:{date:yyyyMMdd}";
    
    public static string RoomAvailability(Guid roomId, DateTime startDate, DateTime endDate) => 
        $"room:{roomId}:availability:{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
}

