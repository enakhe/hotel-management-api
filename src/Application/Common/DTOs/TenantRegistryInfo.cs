using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Tenant registry information
/// </summary>
public record TenantRegistryInfo
{
    public Guid TenantId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public LicenseStatus LicenseStatus { get; init; }
    public DateTime? LicenseExpiryDate { get; init; }
    public string SubscriptionPlan { get; init; } = string.Empty;
    public string TimeZone { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public string FeatureFlags { get; init; } = string.Empty;
    public bool UseSharedDatabase { get; init; }
    public string? DatabaseProvider { get; init; }
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public string Country { get; init; } = string.Empty;
    public string Region { get; init; } = string.Empty;
}
