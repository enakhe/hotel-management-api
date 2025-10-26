using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Core.ModuleManagement.Commands;
using HotelManagement.Application.Core.PlanManagement.Commands;
using HotelManagement.Domain.Entities.Configuration;
using HotelManagement.Domain.Entities.SuperAdmin;

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

    // Plan Management
    Task<Result<PlanResponseDto>> CreatePlanAsync(CreatePlanRequest request);
    Task<Result<PaginatedResult<PlanResponseDto>>> GetPlansAsync(PlanListRequest request);
    Task<Result<PlanResponseDto>> GetPlanByIdAsync(Guid planId);
    Task<Result<PlanResponseDto>> UpdatePlanAsync(Guid planId, UpdatePlanRequest request);
    Task<Result<bool>> DeletePlanAsync(Guid planId);

    // Module Management
    Task<Result<ModuleResponseDto>> CreateModuleAsync(CreateModuleRequest request);
    Task<Result<PaginatedResult<ModuleResponseDto>>> GetModulesAsync(ModuleListRequest request);
    Task<Result<ModuleResponseDto>> GetModuleByIdAsync(Guid moduleId);
    Task<Result<ModuleResponseDto>> UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request);
    Task<Result<bool>> DeleteModuleAsync(Guid moduleId);

    // Plan-Module Relationships
    Task<Result<bool>> AssignModuleToPlanAsync(Guid planId, Guid moduleId);
    Task<Result<bool>> RemoveModuleFromPlanAsync(Guid planId, Guid moduleId);

    // Bulk Operations
    Task<Result<bool>> BulkUpdatePlansAsync(HotelManagement.Application.Core.PlanManagement.Commands.BulkPlanUpdateRequest[] updates);
    Task<Result<bool>> BulkUpdateModulesAsync(HotelManagement.Application.Core.ModuleManagement.Commands.BulkModuleUpdateRequest[] updates);

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
