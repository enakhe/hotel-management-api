using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces.Tenant;

/// <summary>
/// Service for managing the tenant registry and tenant-specific configurations
/// </summary>
public interface ITenantRegistryService
{
    /// <summary>
    /// Gets tenant registry information including license status and feature flags
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>Tenant registry information</returns>
    Task<TenantRegistryInfo?> GetTenantRegistryAsync(Guid tenantId);

    /// <summary>
    /// Updates tenant license status
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="licenseStatus">New license status</param>
    /// <param name="expiryDate">License expiry date</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateLicenseStatusAsync(Guid tenantId, LicenseStatus licenseStatus, DateTime? expiryDate = null);

    /// <summary>
    /// Updates tenant feature flags
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="featureFlags">JSON string of feature flags</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateFeatureFlagsAsync(Guid tenantId, string featureFlags);

    /// <summary>
    /// Gets tenant database configuration for potential DB-per-tenant migration
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>Database configuration</returns>
    Task<TenantDatabaseConfig?> GetDatabaseConfigAsync(Guid tenantId);

    /// <summary>
    /// Updates tenant database configuration
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="config">Database configuration</param>
    /// <returns>True if updated successfully</returns>
    Task<bool> UpdateDatabaseConfigAsync(Guid tenantId, TenantDatabaseConfig config);

    /// <summary>
    /// Checks if tenant should use shared database or dedicated database
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>True if should use shared database</returns>
    Task<bool> ShouldUseSharedDatabaseAsync(Guid tenantId);
}
