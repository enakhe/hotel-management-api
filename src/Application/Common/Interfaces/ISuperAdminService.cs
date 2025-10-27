using HotelManagement.Application.Common.DTOs.Generic;
using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.Services;
using HotelManagement.Application.Common.Interfaces.Tenant;
using HotelManagement.Application.Common.Models;

namespace HotelManagement.Application.Common.Interfaces.SuperAdmin;

/// <summary>
/// Refactored SuperAdmin service interface
/// Now uses composition pattern with domain-specific services
/// </summary>
public interface ISuperAdminService
{
    ITenantService Tenants { get; }
    IPlanService Plans { get; }
    IModuleService Modules { get; }
    ILicenseService Licenses { get; }
    ILicenseKeyService LicenseKeys { get; }
    ILimitsService Limits { get; }

    // Cross-domain operations
    Task<Result<bool>> AssignPlanToTenantAsync(Guid tenantId, Guid planId, string reason);
    Task<Result<bool>> AssignLimitsToPlanAsync(Guid planId, Guid limitsId);
    Task<Result<bool>> CreateTenantWithPlanAsync(CreateTenantWithPlanRequest request);
    Task<Result<bool>> MigrateTenantToNewPlanAsync(Guid tenantId, Guid newPlanId, string reason);

    // Analytics and reporting
    Task<Result<SuperAdminAnalyticsDto>> GetSuperAdminAnalyticsAsync();
    Task<Result<SystemHealthDto>> GetSystemHealthAsync();
    Task<Result<UsageReportDto>> GetUsageReportAsync(DateTime startDate, DateTime endDate);

    // Bulk operations
    Task<Result<BulkOperationResponse>> BulkUpdateTenantsAsync(BulkTenantUpdateRequest[] updates);
    Task<Result<BulkOperationResponse>> BulkUpdatePlansAsync(BulkPlanUpdateRequest[] updates);
    Task<Result<BulkOperationResponse>> BulkUpdateModulesAsync(BulkModuleUpdateRequest[] updates);
    Task<Result<BulkOperationResponse>> BulkUpdateLicensesAsync(Application.Common.DTOs.SuperAdmin.BulkLicenseUpdateRequest[] updates);
}

/// <summary>
/// Cross-domain request DTOs
/// </summary>
public record CreateTenantWithPlanRequest
{
    public CreateTenantRequest Tenant { get; init; } = null!;
    public Guid PlanId { get; init; }
    public Guid? OverrideLimitsId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record SuperAdminAnalyticsDto
{
    public TenantAnalyticsDto TenantAnalytics { get; init; } = null!;
    public PlanUsageDto[] PlanUsage { get; init; } = Array.Empty<PlanUsageDto>();
    public ModuleUsageDto[] ModuleUsage { get; init; } = Array.Empty<ModuleUsageDto>();
    public LicenseAnalyticsDto LicenseAnalytics { get; init; } = null!;
    public LimitsUsageDto[] LimitsUsage { get; init; } = Array.Empty<LimitsUsageDto>();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public record SystemHealthDto
{
    public int TotalTenants { get; init; }
    public int ActiveTenants { get; init; }
    public int SuspendedTenants { get; init; }
    public int TotalPlans { get; init; }
    public int ActivePlans { get; init; }
    public int TotalLicenses { get; init; }
    public int ActiveLicenses { get; init; }
    public int ExpiredLicenses { get; init; }
    public int ExpiringLicenses { get; init; }
    public DateTime LastUpdated { get; init; } = DateTime.UtcNow;
}

public record UsageReportDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalRequests { get; init; }
    public int SuccessfulRequests { get; init; }
    public int FailedRequests { get; init; }
    public decimal AverageResponseTime { get; init; }
    public Dictionary<string, int> RequestsByEndpoint { get; init; } = new();
    public Dictionary<string, int> RequestsByTenant { get; init; } = new();
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

public record BulkTenantUpdateRequest
{
    public Guid Id { get; init; }
    public UpdateTenantRequest Data { get; init; } = null!;
}

public record BulkPlanUpdateRequest
{
    public Guid Id { get; init; }
    public UpdatePlanRequest Data { get; init; } = null!;
}

public record BulkModuleUpdateRequest
{
    public Guid Id { get; init; }
    public UpdateModuleRequest Data { get; init; } = null!;
}


public enum TenantMode
{
    Active,
    Suspended,
    Maintenance,
    Locked
}
