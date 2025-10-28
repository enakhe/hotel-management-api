using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Request DTO for updating a plan
/// </summary>
public record UpdatePlanRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public string? Currency { get; init; }
    public BillingCycle? BillingCycle { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsPopular { get; init; }
    public UpdatePlanLimitsRequest? Limits { get; init; }
    public UpdatePlanModuleRequest[]? Modules { get; init; }
}

/// <summary>
/// Request DTO for updating plan modules
/// </summary>
public record UpdatePlanModuleRequest
{
    public Guid Id { get; init; }
    public bool IsRequired { get; init; } = false;
    public int DisplayOrder { get; init; } = 0;
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
