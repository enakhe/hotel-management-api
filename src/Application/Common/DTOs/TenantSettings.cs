namespace HotelManagement.Application.Common.DTOs;

/// <summary>
/// Tenant settings for frontend consumption
/// </summary>
public record TenantSettings
{
    public Guid TenantId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string TenantIdentifier { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string SubscriptionPlan { get; init; } = string.Empty;
    public DateTime? SubscriptionEndDate { get; init; }
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public IEnumerable<string> EnabledFeatures { get; init; } = [];
    public Dictionary<string, object> CustomSettings { get; init; } = [];
}
