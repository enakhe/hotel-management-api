using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.DTOs.Tenant;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;

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

    Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantRequest request);

    Task<Result<PaginatedResult<TenantSummary>>> GetTenantsAsync(TenantListRequest request);

    Task<Result<TenantDetail>> GetTenantDetailAsync(Guid tenantId);

    Task<Result<bool>> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request);

    Task<Result<bool>> LockTenantAsync(Guid tenantId, string reason);

    Task<Result<bool>> UnlockTenantAsync(Guid tenantId, string reason);

    Task<Result<bool>> SetTenantModeAsync(Guid tenantId, TenantMode mode, string reason);

    Task<Result<bool>> TerminateTenantAsync(Guid tenantId, string reason, DateTime? effectiveDate = null);

    Task<Result<ExportJobResult>> ExportTenantDataAsync(Guid tenantId, ExportOptions options);

    Task<Result<bool>> PurgeTenantDataAsync(Guid tenantId, string reason);

    Task<Result<TenantUsage>> GetTenantUsageAsync(Guid tenantId);

    Task<Result<TenantHealth>> GetTenantHealthAsync(Guid tenantId);
}

