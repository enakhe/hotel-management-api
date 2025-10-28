using HotelManagement.Domain.Entities;

namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Detailed tenant information
/// </summary>
public record TenantDetail
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? Address { get; init; }
    public string? ContactNumber { get; init; }
    public string? Email { get; init; }
    public string TimeZone { get; init; } = string.Empty;
    public string CurrencyCode { get; init; } = string.Empty;
    public string LanguageCode { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? Region { get; init; }
    public string? Industry { get; init; }
    public bool IsActive { get; init; }
    public Plan? Plan { get; init; }
    public License? License { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActivity { get; init; }
    public TenantUsage? Usage { get; init; }
    public TenantHealth? Health { get; init; }
}
