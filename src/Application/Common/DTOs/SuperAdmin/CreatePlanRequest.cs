using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Create plan request DTO
/// </summary>
public record CreatePlanRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public required string Currency { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public bool IsActive { get; init; } = true;
    public bool IsPopular { get; init; } = false;
    public required CreatePlanFeatureRequest[] Features { get; init; }
    public required CreatePlanLimitsRequest Limits { get; init; }
    public required string[] Modules { get; init; }
}

/// <summary>
/// Create plan feature request DTO
/// </summary>
public record CreatePlanFeatureRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool Included { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
}

/// <summary>
/// Create plan limits request DTO
/// </summary>
public record CreatePlanLimitsRequest
{
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public SupportLevel SupportLevel { get; init; }
    public decimal SLA { get; init; }
}
