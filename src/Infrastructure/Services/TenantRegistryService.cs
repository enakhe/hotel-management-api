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
                LicenseStatus = tenant.LicenseStatus,
                LicenseExpiryDate = tenant.LicenseExpiryDate,
                SubscriptionPlan = tenant.SubscriptionPlan ?? "Basic",
                TimeZone = tenant.TimeZone ?? "UTC",
                CurrencyCode = tenant.CurrencyCode ?? "USD",
                LanguageCode = tenant.LanguageCode ?? "en",
                FeatureFlags = tenant.FeatureFlags ?? "{}",
                UseSharedDatabase = tenant.UseSharedDatabase,
                DatabaseProvider = tenant.DatabaseProvider,
                MaxUsers = tenant.MaxUsers,
                MaxBranches = tenant.MaxBranches,
                MaxRooms = tenant.MaxRooms,
                MaxReservations = tenant.MaxReservations,
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

            tenant.LicenseStatus = licenseStatus;
            tenant.LicenseExpiryDate = expiryDate;
            tenant.LastLicenseCheck = DateTime.UtcNow;

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

            tenant.FeatureFlags = featureFlags;
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

            return new TenantDatabaseConfig
            {
                UseSharedDatabase = tenant.UseSharedDatabase,
                ConnectionString = tenant.DatabaseConnectionString,
                Provider = tenant.DatabaseProvider ?? "SqlServer",
                DatabaseName = ExtractDatabaseName(tenant.DatabaseConnectionString)
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

            tenant.UseSharedDatabase = config.UseSharedDatabase;
            tenant.DatabaseConnectionString = config.ConnectionString;
            tenant.DatabaseProvider = config.Provider;

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

            return tenant?.UseSharedDatabase ?? true;
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
