using HotelManagement.Application.Common.DTOs.SuperAdmin;
using HotelManagement.Application.Common.Interfaces.SuperAdmin;
using HotelManagement.Application.Common.Models;
using HotelManagement.Application.Common.Services.LicenseKey;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Interfaces.Services;

/// <summary>
/// Plan management service interface
/// </summary>
public interface IPlanService
{
    // CRUD Operations
    Task<Result<PlanResponseDto>> CreatePlanAsync(CreatePlanRequest request);
    Task<Result<PaginatedResult<PlanResponseDto>>> GetPlansAsync(PlanListRequest request);
    Task<Result<PlanResponseDto>> GetPlanByIdAsync(Guid planId);
    Task<Result<PlanResponseDto>> UpdatePlanAsync(Guid planId, UpdatePlanRequest request);
    Task<Result<bool>> DeletePlanAsync(Guid planId);

    // Plan-Limits Management
    Task<Result<PlanResponseDto>> AssignLimitsAsync(Guid planId, Guid limitsId);
    Task<Result<PlanResponseDto>> UpdateLimitsAsync(Guid planId, UpdateLimitsRequest request);

    Task<Result<bool>> BulkUpdatePlansAsync(BulkPlanUpdateRequest[] updates);
    Task<Result<bool>> AssignModuleToPlanAsync(Guid planId, Guid moduleId);
    Task<Result<bool>> RemoveModuleFromPlanAsync(Guid planId, Guid moduleId);

    // Analytics
    Task<Result<PlanUsageDto[]>> GetPlanUsageAsync();
}

/// <summary>
/// Module management service interface
/// </summary>
public interface IModuleService
{
    // CRUD Operations
    Task<Result<ModuleResponseDto>> CreateModuleAsync(CreateModuleRequest request);
    Task<Result<PaginatedResult<ModuleResponseDto>>> GetModulesAsync(ModuleListRequest request);
    Task<Result<ModuleResponseDto>> GetModuleByIdAsync(Guid moduleId);
    Task<Result<ModuleResponseDto>> UpdateModuleAsync(Guid moduleId, UpdateModuleRequest request);
    Task<Result<bool>> DeleteModuleAsync(Guid moduleId);

    // Plan-Module Relationships
    Task<Result<bool>> AssignModuleToPlanAsync(Guid planId, Guid moduleId);
    Task<Result<bool>> RemoveModuleFromPlanAsync(Guid planId, Guid moduleId);

    // Analytics
    Task<Result<ModuleUsageDto[]>> GetModuleUsageAsync();
}

/// <summary>
/// License management service interface
/// </summary>
public interface ILicenseService
{
    // CRUD Operations
    Task<Result<LicenseResponseDto>> CreateLicenseAsync(CreateLicenseRequest request);
    Task<Result<PaginatedResult<LicenseResponseDto>>> GetLicensesAsync(LicenseListRequest request);
    Task<Result<LicenseResponseDto>> GetLicenseByIdAsync(Guid licenseId);
    Task<Result<LicenseResponseDto>> GetLicenseByKeyAsync(string licenseKey);
    Task<Result<LicenseResponseDto>> UpdateLicenseAsync(Guid licenseId, UpdateLicenseRequest request);
    Task<Result<bool>> DeleteLicenseAsync(Guid licenseId);

    // License Management Operations
    Task<Result<LicenseResponseDto>> RenewLicenseAsync(Guid licenseId, LicenseRenewalRequest request);
    Task<Result<LicenseResponseDto>> TransferLicenseAsync(Guid licenseId, LicenseTransferRequest request);
    Task<Result<LicenseResponseDto>> SuspendLicenseAsync(Guid licenseId, string reason);
    Task<Result<LicenseResponseDto>> RevokeLicenseAsync(Guid licenseId, string reason);
    Task<Result<LicenseResponseDto>> ActivateLicenseAsync(Guid licenseId, string reason);

    // License Validation
    Task<Result<LicenseValidationResponse>> ValidateLicenseAsync(LicenseValidationRequest request);

    // Analytics
    Task<Result<LicenseAnalyticsDto>> GetLicenseAnalyticsAsync();
    Task<Result<LicenseUsageDto[]>> GetLicenseUsageAsync();
    Task<Result<LicenseResponseDto[]>> GetExpiringLicensesAsync(int days);

    // Bulk Operations
    //Task<Result<bool>> BulkUpdateLicensesAsync(BulkLicenseUpdateRequest[] updates);
    Task<Result<bool>> BulkRenewLicensesAsync(BulkLicenseRenewalRequest[] renewals);
    Task<Result<bool>> BulkSuspendLicensesAsync(BulkLicenseSuspensionRequest[] suspensions);
}

/// <summary>
/// License key generation service interface
/// </summary>
public interface ILicenseKeyService
{
    Task<Result<string>> GenerateLicenseKeyAsync(HotelManagement.Application.Common.Services.LicenseKey.LicenseKeyGenerationOptions options);
    Task<Result<LicenseKeyValidationResult>> ValidateLicenseKeyFormatAsync(string key, LicenseKeyFormat format, bool includeChecksum = true);
    Task<Result<bool>> IsLicenseKeyUniqueAsync(string key);
    Task<Result<string>> MaskLicenseKeyForDisplayAsync(string key);
    Task<Result<string>> FormatLicenseKeyForDisplayAsync(string key);
}

/// <summary>
/// Limits management service interface
/// </summary>
public interface ILimitsService
{
    // CRUD Operations
    Task<Result<LimitsResponseDto>> CreateLimitsAsync(CreateLimitsRequest request);
    Task<Result<PaginatedResult<LimitsResponseDto>>> GetLimitsAsync(LimitsListRequest request);
    Task<Result<LimitsResponseDto>> GetLimitsByIdAsync(Guid limitsId);
    Task<Result<LimitsResponseDto>> UpdateLimitsAsync(Guid limitsId, UpdateLimitsRequest request);
    Task<Result<bool>> DeleteLimitsAsync(Guid limitsId);

    // Template Management
    Task<Result<LimitsResponseDto>> CreateFromTemplateAsync(Guid templateId, string name);
    Task<Result<LimitsTemplateDto[]>> GetLimitsTemplatesAsync();
    Task<Result<LimitsResponseDto>> CreateTemplateAsync(CreateLimitsTemplateRequest request);

    // Usage Analytics
    Task<Result<LimitsUsageDto[]>> GetLimitsUsageAsync();
}
