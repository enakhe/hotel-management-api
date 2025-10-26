using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Request DTO for updating a plan
/// </summary>
public record UpdatePlanRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public decimal? Price { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsPopular { get; init; }
    public UpdatePlanFeatureRequest[]? Features { get; init; }
    public UpdatePlanLimitsRequest? Limits { get; init; }
    public string[]? Modules { get; init; }
}

/// <summary>
/// Request DTO for updating plan features
/// </summary>
public record UpdatePlanFeatureRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Included { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
}

/// <summary>
/// Request DTO for updating plan limits
/// </summary>
public record UpdatePlanLimitsRequest
{
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public int? MaxRooms { get; init; }
    public int? MaxReservations { get; init; }
    public int? MaxStorageGB { get; init; }
    public int? ApiRateLimit { get; init; }
    public SupportLevel? SupportLevel { get; init; }
    public decimal? SLA { get; init; }
}
