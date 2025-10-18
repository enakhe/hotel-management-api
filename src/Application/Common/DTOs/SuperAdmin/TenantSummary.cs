using HotelManagement.Domain.Entities.Configuration;

namespace HotelManagement.Application.Common.DTOs.SuperAdmin;

/// <summary>
/// Tenant summary for list views
/// </summary>
public record TenantSummary
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Identifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string LicenseStatus { get; init; } = string.Empty;
    public string SubscriptionPlan { get; init; } = string.Empty;
    public string? Country { get; init; }
    public string? Region { get; init; }
    public ICollection<TenantFeature>? Modules { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastActivity { get; init; }
    public int UserCount { get; init; }
    public int BranchCount { get; init; }
}
