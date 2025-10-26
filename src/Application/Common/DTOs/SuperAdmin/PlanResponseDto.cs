using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Plan response DTO for API responses
/// </summary>
public record PlanResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public required string Currency { get; init; }
    public BillingCycle BillingCycle { get; init; }
    public bool IsActive { get; init; }
    public bool IsPopular { get; init; }
    public string[]? Modules { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public string? UpdatedBy { get; init; }
    public required PlanFeatureResponseDto[] Features { get; init; }
    public required PlanLimitsResponseDto Limits { get; init; }
}

/// <summary>
/// Plan feature response DTO
/// </summary>
public record PlanFeatureResponseDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool Included { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
}

/// <summary>
/// Plan limits response DTO
/// </summary>
public record PlanLimitsResponseDto
{
    public Guid Id { get; init; }
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public SupportLevel SupportLevel { get; init; }
    public decimal SLA { get; init; }
}
