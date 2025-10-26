using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Create tenant request
/// </summary>
public record CreateTenantRequest
{
    public required string Name { get; init; }
    public required string Identifier { get; init; }
    public string? Description { get; init; }
    public string? Email { get; init; }
    public string? ContactNumber { get; init; }
    public SubscriptionPlan SubscriptionPlan { get; init; } = SubscriptionPlan.Basic;
    public string[]? Modules { get; init; }
    public int MaxUsers { get; init; } = 10;
    public int MaxBranches { get; init; } = 1;
    public int MaxRooms { get; init; } = 100;
    public int MaxReservations { get; init; } = 1000;
    public string? Address { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public string? TimeZone { get; init; } = "WAT";
    public string? CurrencyCode { get; init; } = "NGN";
    public string? LanguageCode { get; init; } = "en";
}

public enum SubscriptionPlan
{
    Basic,
    Standard,
    Premium
}
