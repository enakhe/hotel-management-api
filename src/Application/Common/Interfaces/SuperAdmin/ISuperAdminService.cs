using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces.SuperAdmin;

/// <summary>
/// Service for SuperAdmin tenant management operations
/// </summary>
public interface ISuperAdminService
{
    // Tenant Lifecycle Management
    Task<CreateTenantResult> CreateTenantAsync(CreateTenantRequest request);
    Task<PaginatedResult<TenantSummary>> GetTenantsAsync(TenantListRequest request);
    Task<TenantDetail?> GetTenantDetailAsync(Guid tenantId);
    Task<bool> UpdateTenantAsync(Guid tenantId, UpdateTenantRequest request);

    // Tenant Actions
    Task<bool> LockTenantAsync(Guid tenantId, string reason);
    Task<bool> UnlockTenantAsync(Guid tenantId, string reason);
    Task<bool> SetTenantModeAsync(Guid tenantId, TenantMode mode, string reason);
    Task<bool> TerminateTenantAsync(Guid tenantId, string reason, DateTime? effectiveDate = null);

    // Data Management
    Task<ExportJobResult> ExportTenantDataAsync(Guid tenantId, ExportOptions options);
    Task<bool> PurgeTenantDataAsync(Guid tenantId, string reason);

    // Monitoring
    Task<TenantUsage?> GetTenantUsageAsync(Guid tenantId);
    Task<TenantHealth?> GetTenantHealthAsync(Guid tenantId);
}
