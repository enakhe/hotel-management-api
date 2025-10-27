using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;
using HotelManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace HotelManagement.Infrastructure.Services;

/// <summary>
/// Service for managing the tenant registry and tenant-specific configurations
/// </summary>
public class TenantRegistryService : ITenantRegistryService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TenantRegistryService> _logger;

    public TenantRegistryService(ApplicationDbContext context, ILogger<TenantRegistryService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<TenantRegistryInfo?> GetTenantRegistryAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return null;

            return new TenantRegistryInfo
            {
                TenantId = tenant.Id,
                Name = tenant.Name,
                Identifier = tenant.Identifier,
                IsActive = tenant.IsActive,
                TimeZone = tenant.TimeZone ?? "UTC",
                CurrencyCode = tenant.CurrencyCode ?? "USD",
                LanguageCode = tenant.LanguageCode ?? "en",
                Country = tenant.Country ?? "",
                Region = tenant.Region ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant registry for {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> UpdateLicenseStatusAsync(Guid tenantId, LicenseStatus licenseStatus, DateTime? expiryDate = null)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

            // Note: License status is now managed through the License entity
            // This method might need to be updated to work with the License relationship
            // For now, we'll just update the tenant's LastModified timestamp
            tenant.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating license status for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> UpdateFeatureFlagsAsync(Guid tenantId, string featureFlags)
    {
        try
        {
            // Validate JSON format
            JsonDocument.Parse(featureFlags);

            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

            // Note: Feature flags are now managed through the Plan and Module relationships
            // This method might need to be updated to work with the new architecture
            // For now, we'll just update the tenant's LastModified timestamp
            tenant.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON format for feature flags: {FeatureFlags}", featureFlags);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating feature flags for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<TenantDatabaseConfig?> GetDatabaseConfigAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            if (tenant == null)
                return null;

            // Note: Database configuration properties no longer exist on Tenant entity
            // This method might need to be updated to work with the new architecture
            // For now, return a default configuration
            return new TenantDatabaseConfig
            {
                UseSharedDatabase = true, // Default to shared database
                ConnectionString = "", // Would need to be configured elsewhere
                Provider = "SqlServer", // Default provider
                DatabaseName = tenant.Identifier // Use tenant identifier as database name
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database config for tenant {TenantId}", tenantId);
            return null;
        }
    }

    public async Task<bool> UpdateDatabaseConfigAsync(Guid tenantId, TenantDatabaseConfig config)
    {
        try
        {
            var tenant = await _context.Tenants.FindAsync(tenantId);
            if (tenant == null)
                return false;

            // Note: Database configuration properties no longer exist on Tenant entity
            // This method might need to be updated to work with the new architecture
            // For now, we'll just update the tenant's LastModified timestamp
            tenant.LastModified = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating database config for tenant {TenantId}", tenantId);
            return false;
        }
    }

    public async Task<bool> ShouldUseSharedDatabaseAsync(Guid tenantId)
    {
        try
        {
            var tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId);

            // Note: UseSharedDatabase property no longer exists on Tenant entity
            // Default to shared database for now
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking database usage for tenant {TenantId}", tenantId);
            return true; // Default to shared database on error
        }
    }

    private static string? ExtractDatabaseName(string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return null;

        // Simple extraction - in production, use proper connection string parsing
        var parts = connectionString.Split(';');
        var databasePart = parts.FirstOrDefault(p => p.StartsWith("Database=", StringComparison.OrdinalIgnoreCase) ||
                                                   p.StartsWith("Initial Catalog=", StringComparison.OrdinalIgnoreCase));

        return databasePart?.Split('=').LastOrDefault();
    }
}
