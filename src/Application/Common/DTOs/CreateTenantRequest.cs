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
    public Guid PlanId { get; init; }
    public Guid LicenseId { get; init; }
    public string? Address { get; init; }
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public string? TimeZone { get; init; } = "WAT";
    public string? CurrencyCode { get; init; } = "NGN";
    public string? LanguageCode { get; init; } = "en";
}
