using HotelManagement.Application.Common.DTOs.Tenant;

namespace HotelManagement.Application.Common.Interfaces.Tenant;

/// <summary>
/// Service for tenant-related operations and validation
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Validates if a tenant exists and is active
    /// </summary>
    /// <param name="tenantId">The tenant ID to validate</param>
    /// <returns>True if the tenant exists and is active</returns>
    Task<bool> IsTenantValidAsync(Guid tenantId);

    /// <summary>
    /// Gets tenant information by identifier (subdomain)
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier</param>
    /// <returns>Tenant ID if found, null otherwise</returns>
    Task<Guid?> GetTenantIdByIdentifierAsync(string tenantIdentifier);

    /// <summary>
    /// Gets tenant information by ID
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>Tenant information if found, null otherwise</returns>
    Task<TenantInfo?> GetTenantInfoAsync(Guid tenantId);
}

