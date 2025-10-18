using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Entities.Configuration;

namespace HotelManagement.Application.Common.Interfaces.SuperAdmin;

/// <summary>
/// Service for SuperAdmin tenant management operations
/// </summary>
public interface ISuperAdminService
{
    // Tenant Lifecycle Management
    Task<Result<TenantSummary>> CreateTenantAsync(CreateTenantRequest request);
    Task<Result<PaginatedResult<TenantSummary>>> GetTenantsAsync(TenantListRequest request);
    Task<Result<TenantDetail>> GetTenantDetailAsync(Guid tenantId);
    Task<Result<bool>> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request);

    // Tenant Actions
    Task<Result<bool>> LockTenantAsync(Guid tenantId, string reason);
    Task<Result<bool>> UnlockTenantAsync(Guid tenantId, string reason);
    Task<Result<bool>> SetTenantModeAsync(Guid tenantId, TenantMode mode, string reason);
    Task<Result<bool>> TerminateTenantAsync(Guid tenantId, string reason, DateTime? effectiveDate = null);

    // Data Management
    Task<Result<ExportJobResult>> ExportTenantDataAsync(Guid tenantId, ExportOptions options);
    Task<Result<bool>> PurgeTenantDataAsync(Guid tenantId, string reason);

    // Monitoring
    Task<Result<TenantUsage>> GetTenantUsageAsync(Guid tenantId);
    Task<Result<TenantHealth>> GetTenantHealthAsync(Guid tenantId);
}
