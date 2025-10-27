using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request DTO for creating a module
/// </summary>
public record CreateModuleRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsCore { get; init; } = false;
    public string[] Dependencies { get; init; } = Array.Empty<string>();
    public required CreateModuleFeatureRequest[] Features { get; init; }
    public required CreateModulePricingRequest Pricing { get; init; }
}

/// <summary>
/// Request DTO for creating module features
/// </summary>
public record CreateModuleFeatureRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsEnabled { get; init; } = true;
    public Dictionary<string, object>? Configuration { get; init; }
}

/// <summary>
/// Request DTO for creating module pricing
/// </summary>
public record CreateModulePricingRequest
{
    public PricingType Type { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public int? MinQuantity { get; init; }
    public int? MaxQuantity { get; init; }
}

/// <summary>
/// Request DTO for updating a module
/// </summary>
public record UpdateModuleRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Category { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsCore { get; init; }
    public string[]? Dependencies { get; init; }
    public UpdateModuleFeatureRequest[]? Features { get; init; }
    public UpdateModulePricingRequest? Pricing { get; init; }
}

/// <summary>
/// Request DTO for updating module features
/// </summary>
public record UpdateModuleFeatureRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

/// <summary>
/// Request DTO for updating module pricing
/// </summary>
public record UpdateModulePricingRequest
{
    public PricingType? Type { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public int? MinQuantity { get; init; }
    public int? MaxQuantity { get; init; }
}

/// <summary>
/// Request DTO for listing modules with filtering and pagination
/// </summary>
public record ModuleListRequest
{
    public string? Query { get; init; }
    public string? Category { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsCore { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "createdAt";
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// Response DTO for module data
/// </summary>
public record ModuleResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public required string Category { get; init; }
    public bool IsActive { get; init; }
    public bool IsCore { get; init; }
    public string[] Dependencies { get; init; } = Array.Empty<string>();
    public ModuleFeatureResponseDto[] Features { get; init; } = Array.Empty<ModuleFeatureResponseDto>();
    public ModulePricingResponseDto? Pricing { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
}

/// <summary>
/// Response DTO for module features
/// </summary>
public record ModuleFeatureResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

/// <summary>
/// Response DTO for module pricing
/// </summary>
public record ModulePricingResponseDto
{
    public Guid Id { get; init; }
    public PricingType Type { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public int? MinQuantity { get; init; }
    public int? MaxQuantity { get; init; }
}

/// <summary>
/// Response DTO for plan usage analytics
/// </summary>
public record PlanUsageResponseDto
{
    public Guid PlanId { get; init; }
    public required string PlanName { get; init; }
    public int TenantCount { get; init; }
    public decimal TotalRevenue { get; init; }
    public decimal AverageTenantValue { get; init; }
    public decimal ChurnRate { get; init; }
    public DateTime LastUpdated { get; init; }
}

/// <summary>
/// Response DTO for module usage analytics
/// </summary>
public record ModuleUsageResponseDto
{
    public Guid ModuleId { get; init; }
    public required string ModuleName { get; init; }
    public int TenantCount { get; init; }
    public decimal AdoptionRate { get; init; }
    public decimal Revenue { get; init; }
    public DateTime LastUpdated { get; init; }
}
